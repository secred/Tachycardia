using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
	sealed class Core
	{
		public Mogre.Log m_Log;
		public Mogre.Root m_Root;
		public Mogre.RenderWindow m_RenderWindow;
		public Mogre.SceneManager m_SceneManager;
		public Mogre.Camera m_Camera;
		public Mogre.Viewport m_Viewport;
		public MOIS.Keyboard m_Keyboard;
		public MOIS.Mouse m_Mouse;
		public MOIS.InputManager m_InputManager;
		public MogreNewt.World m_NewtonWorld;
		public MogreNewt.Debugger m_NewtonDebugger;
		public MHydrax.MHydrax m_Hydrax;

		public Map m_CurrentMap;
		public GameCamera m_GameCamera;
		public ObjectManager m_ObjectManager;
		public StateManager m_StateManager;
        public PhysicsManager m_PhysicsManager;

		bool m_Shutdown;

		int m_BodyId;

		public const float m_FixedFPS = 60;
		public const float m_FixedTime = 1.0f / m_FixedFPS;

		float m_TimeAccumulator = 0;

		float m_GFXFPSAccumulator = 0;
		int m_GFXFPS = 0;
		int m_GFXFPSStored = 0;
		float m_PHYSUPSAccumulator = 0;
		int m_PHYSUPS = 0;
		int m_PHYSUPSStored = 0;

		public static void Log(string message)
		{
			Console.WriteLine("Core.Log: " + message);
			Singleton.m_Log.LogMessage("Core.Log: " + message);
		}

		public bool Initialise()
		{
			m_Shutdown = false;


			new Mogre.LogManager();
			m_Log = Mogre.LogManager.Singleton.CreateLog("OgreLogfile.log", true, true, false);

			m_Root = new Root();
			ConfigFile cf = new ConfigFile();
			cf.Load("Resources.cfg", "\t:=", true);

			ConfigFile.SectionIterator seci = cf.GetSectionIterator();

			while (seci.MoveNext())
			{
				ConfigFile.SettingsMultiMap settings = seci.Current;
				foreach (KeyValuePair<string, string> pair in settings)
					ResourceGroupManager.Singleton.AddResourceLocation(pair.Value, pair.Key, seci.CurrentKey);
			}

			if (!m_Root.RestoreConfig())    // comment this line to view configuration dialog box
				if (!m_Root.ShowConfigDialog())
					return false;

			m_RenderWindow = m_Root.Initialise(true);
			ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

			m_SceneManager = m_Root.CreateSceneManager(SceneType.ST_GENERIC);
			m_Camera = m_SceneManager.CreateCamera("MainCamera");
			m_Viewport = m_RenderWindow.AddViewport(m_Camera);
			m_Camera.NearClipDistance = 0.1f;
			m_Camera.FarClipDistance = 1000.0f;

			MOIS.ParamList pl = new MOIS.ParamList();
			IntPtr windowHnd;
			m_RenderWindow.GetCustomAttribute("WINDOW", out windowHnd);
			pl.Insert("WINDOW", windowHnd.ToString());

			m_InputManager = MOIS.InputManager.CreateInputSystem(pl);

			m_Keyboard = (MOIS.Keyboard)m_InputManager.CreateInputObject(MOIS.Type.OISKeyboard, true);
			m_Mouse = (MOIS.Mouse)m_InputManager.CreateInputObject(MOIS.Type.OISMouse, true);

			m_NewtonWorld = new World();
            m_NewtonWorld.SetWorldSize(new AxisAlignedBox(-500, -500, -500, 500, 500, 500)); 
			m_NewtonDebugger = new Debugger(m_NewtonWorld);
			m_NewtonDebugger.Init(m_SceneManager);

			m_GameCamera = new GameCamera();
			m_ObjectManager = new ObjectManager();
			m_StateManager = new StateManager();
            m_PhysicsManager = new PhysicsManager();

            /*
             * To co tu mamy nalezy przeniesc jak najszybciej do ogitora
             * Materialy dla kazdej mapy moga byc inne inne parametry inne powiazania itp wiec tworzone
             * sa oddzielnie dla kazdej nowej mapy, a potem przy obiektach konkretny material jest przypsiywany
             * przy starcie ladowania physicsmanager powinien byc wyczyszczony
             */

            //inicjalizacja konkretnych obiektow, w sumie tylko nadanie nazw w dictionary
            m_PhysicsManager.addMaterial("Metal");
            //podstawowe
            m_PhysicsManager.addMaterial("Ground");
            m_PhysicsManager.addMaterial("Trigger");
            m_PhysicsManager.addMaterial("Player");

            //laczenie materialow w pary
            //dla kazdej mapy rozne powiazania (zapewne beda sie powtarzac ale im wieksza ilosc
            //materialow tym wieksze obciazenie dla silnika wiec nalezy to ograniczac
            //dla kazdej pary materialow mozna ustawic rozne parametry kolizji
            //jak tarcie, elastycznosc, miekkosc kolizji oraz callback ktory jest wywolywany przy kolizji
            m_PhysicsManager.addMaterialPair("Metal", "Metal");
            m_PhysicsManager.getMaterialPair("MetalMetal").SetDefaultFriction(1.5f, 1.4f);
            m_PhysicsManager.setPairCallback("MetalMetal", "MetalCallback");

            //obowiazkowe
            m_PhysicsManager.addMaterialPair("Trigger", "Player");//wyzwalacze pozycja obowiazkowa
            m_PhysicsManager.setPairCallback("TriggerPlayer", "TriggerCallback");
            
            //obowiazkowe
            m_PhysicsManager.addMaterialPair("Ground", "Player");//ground material podstawowy
            m_PhysicsManager.getMaterialPair("GroundPlayer").SetDefaultElasticity(0);
            m_PhysicsManager.setPairCallback("GroundPlayer", "GroundPlayerCallback");
            /*
                        * Koniec inicjalizacji materialow ktora musi sie znalezc w opisie mapy, dla niekumatych w ogitorze. 
                        */
            
            CreateOgitorScene();
			//CreateScene();

			/*
			 *  Rejestracja listenerów  
			 **/
			m_Root.FrameRenderingQueued += new FrameListener.FrameRenderingQueuedHandler(Update);

			m_Keyboard.KeyPressed += new MOIS.KeyListener.KeyPressedHandler(KeyPressed);
			m_Keyboard.KeyReleased += new MOIS.KeyListener.KeyReleasedHandler(KeyReleased);

			m_Keyboard.KeyPressed += new MOIS.KeyListener.KeyPressedHandler(m_StateManager.KeyPressed);
			m_Keyboard.KeyReleased += new MOIS.KeyListener.KeyReleasedHandler(m_StateManager.KeyReleased);
			m_Mouse.MouseMoved += new MOIS.MouseListener.MouseMovedHandler(m_StateManager.MouseMoved);
			m_Mouse.MousePressed += new MOIS.MouseListener.MousePressedHandler(m_StateManager.MousePressed);
			m_Mouse.MouseReleased += new MOIS.MouseListener.MouseReleasedHandler(m_StateManager.MouseReleased);

			return true;
		}

		public void Go()
		{
			/*
			 *  Odpalenie głównej pętli renderującej Ogra:
			 **/
			m_Root.StartRendering();
		}

		public void Shutdown()
		{
			m_Shutdown = true;
		}

		private bool Update(Mogre.FrameEvent evt)
		{
			if (m_RenderWindow.IsClosed)
				return false;

			if (m_Shutdown)
				return false;

			try
			{
				m_TimeAccumulator += evt.timeSinceLastFrame;
				m_GFXFPSAccumulator += evt.timeSinceLastFrame;
				m_GFXFPS++;

				while (m_TimeAccumulator >= m_FixedTime)
				{
					m_PHYSUPSAccumulator += m_FixedTime;
					m_PHYSUPS++;

					m_Keyboard.Capture();
					m_Mouse.Capture();

					m_StateManager.Update();

					// not sure where exactly should be these lines below (State Updates?)    ~MDZ
					m_ObjectManager.Update();
					m_NewtonWorld.Update(m_FixedTime);
					m_GameCamera.Update();

					m_TimeAccumulator -= m_FixedTime;

					if (m_PHYSUPSAccumulator >= 1.0f)
					{
						m_PHYSUPSStored = m_PHYSUPS;
						m_PHYSUPS = 0;
						m_PHYSUPSAccumulator -= 1.0f;
					}
				}

				if (m_GFXFPSAccumulator >= 1.0f)
				{
					m_GFXFPSStored = m_GFXFPS;
					m_GFXFPS = 0;
					m_GFXFPSAccumulator -= 1.0f;
					Console.WriteLine("FPS: " + m_GFXFPSStored + " (gfx), " + m_PHYSUPSStored + " (phys)");
				}

				WindowEventUtilities.MessagePump();

				if (m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_F3))
					m_NewtonDebugger.ShowDebugInformation();
				else
					m_NewtonDebugger.HideDebugInformation();

				return true;
			}
			catch (Exception e)
			{
				System.Console.WriteLine(e);
				m_Shutdown = true;
				return false;
			}
		}
		
		private void AttachHydrax()
        {
            m_Log.LogMessage("Hydrax initialization...");
            m_Hydrax = new MHydrax.MHydrax(m_SceneManager, m_Camera, m_Viewport);
            MHydrax.MProjectedGrid grid = new MHydrax.MProjectedGrid(m_Hydrax, new MHydrax.MPerlin(), new Plane(new Vector3(0, 1, 0), new Vector3()), MHydrax.MMaterialManager.MNormalMode.NM_VERTEX);
            m_Hydrax.SetModule(grid);
            m_Hydrax.LoadCfg("../scenes/Default/Hydrax/Hydrax.hdx");
            m_Hydrax.Create();
            m_Log.LogMessage("Hydrax initialized.");
        }
		
		private void CreateOgitorScene()
		{
			m_Log.LogMessage("Creating Default scene...");
			m_CurrentMap = new Map();

			m_Log.LogMessage("Loading scene data...");
			Helper.DotSceneLoader dsl = new Helper.DotSceneLoader();
			dsl.ParseDotScene("default.scene", "Scenes", m_SceneManager);
			m_Log.LogMessage("Scene data loaded.");

            // AttachHydrax();
            
			/*m_Log.LogMessage("Creating character profile...");
			CharacterProfile profile = new CharacterProfile();
			profile.m_BodyMass = 70;
			profile.m_BodyScaleFactor = new Vector3(1.5f, 1, 1.5f);
			profile.m_HeadOffset = new Vector3(0, 0.8f, 0);
			profile.m_MeshName = "Man.mesh";
			m_Log.LogMessage("Character profile created.");

			m_Log.LogMessage("Creating player...");
			Character player = new Character(profile);
			player.SetPosition(new Vector3(0, 2, 0));
			m_ObjectManager.Add("player", player);
			m_Log.LogMessage("Player created.");*/
            
            //z pominieciem profilu, jezeli mi ktos przedstawi jaki byl glebszy sens tego to wrocimy
            m_Log.LogMessage("Creating player...");
            Character player = new Character("Man.mesh", 70);//tworzenie grafiki
            player.SetPosition(new Vector3(0, 3, 0));
            m_ObjectManager.Add("player", player);
			m_Log.LogMessage("Player created.");
            
			m_Log.LogMessage("Adding light...");
			Light light = Core.Singleton.m_SceneManager.CreateLight();
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(1, -3, 1).NormalisedCopy;
			light.DiffuseColour = new ColourValue(0.2f, 0.2f, 0.2f);
			m_Log.LogMessage("Light added.");

			m_SceneManager.ShadowTechnique = ShadowTechnique.SHADOWTYPE_TEXTURE_MODULATIVE;
			m_Log.LogMessage("Default scene created.");


            //overlays
            //m_Log.LogMessage("Creating an overlay...");
            //var overlay = OverlayManager.Singleton.GetByName("TestScriptOverlay");
            //overlay.Show();

            //overlaye na klawisz: Enter i R wlacza, T wylacza
            m_Keyboard.KeyPressed += new MOIS.KeyListener.KeyPressedHandler(KeyPressedHandler);

            
        }
        /*
		private void CreateScene()
		{
			m_CurrentMap = new Map();
			m_CurrentMap.SetGraphicsMesh("Level.mesh");
			m_CurrentMap.SetCollisionMesh("LevelCol.mesh");

			CharacterProfile profile = new CharacterProfile();
			profile.m_BodyMass = 70;
			profile.m_BodyScaleFactor = new Vector3(1.5f, 1, 1.5f);
			profile.m_HeadOffset = new Vector3(0, 0.8f, 0);
			profile.m_MeshName = "Man.mesh";
			//profile.m_WalkSpeed = 0.5f;   // temporarly defined in Character.cs   ~ MDZ

			Character player = new Character(profile);
			player.SetPosition(new Vector3(0, 2, 0));
			m_ObjectManager.Add("player", player);

			Light light = Core.Singleton.m_SceneManager.CreateLight();
			light.Type = Light.LightTypes.LT_DIRECTIONAL;
			light.Direction = new Vector3(1, -3, 1).NormalisedCopy;
			light.DiffuseColour = new ColourValue(0.2f, 0.2f, 0.2f);

			m_SceneManager.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;
		}*/

        //overlaye na przycisk
        //hello world - ENTER
        //red - R
        //hello world na wierzch - V
        //wylacz - T
        //broken screen - B
        public bool KeyPressedHandler(MOIS.KeyEvent keyEventRef)
        {
            if (keyEventRef.key == MOIS.KeyCode.KC_RETURN)
            {
                var messageBox = OverlayManager.Singleton.GetOverlayElement("HelloWorldOverlay/MessageBox");
                messageBox.Left = (m_RenderWindow.Width - messageBox.Width) / 2;
                messageBox.Top = (m_RenderWindow.Height - messageBox.Height) / 2;

                var messageBody = OverlayManager.Singleton.GetOverlayElement("HelloWorldOverlay/MessageBox/Body");
                messageBody.Caption = "Ssij palke :*";

                OverlayManager.Singleton.GetByName("HelloWorldOverlay").Show();
            }

            if (keyEventRef.key == MOIS.KeyCode.KC_R)
            {
                OverlayManager.Singleton.GetByName("Red").Show();
            }

            if (keyEventRef.key == MOIS.KeyCode.KC_B)
            {
                OverlayManager.Singleton.GetByName("BrokenScreen").Show();
            }

            if (keyEventRef.key == MOIS.KeyCode.KC_T)
            {
                OverlayManager.Singleton.GetByName("Red").Hide();
                OverlayManager.Singleton.GetByName("HelloWorldOverlay").Hide();
                OverlayManager.Singleton.GetByName("BrokenScreen").Hide();
            }

            if (keyEventRef.key == MOIS.KeyCode.KC_V)
            {
                OverlayManager.Singleton.GetByName("Red").ZOrder -= 50;
            }

            return true;
        }

		public bool KeyPressed(MOIS.KeyEvent keyEventRef)
		{
			return true;
		}
		public bool KeyReleased(MOIS.KeyEvent keyEventRef)
		{
			return true;
		}

		public int GetUniqueBodyId()
		{
			return m_BodyId++;
		}

		static Core instance;

		Core()
		{
		}

		static Core()
		{
			instance = new Core();
		}

		public static Core Singleton
		{
			get
			{
				return instance;
			}
		}
	}
}

//        public Boolean Init(String wndTitle/*, skoro to static to po co przekazywac? MOIS.KeyListener.KeyPressedHandler keyListener/* = null, MOIS.MouseListener.MouseMovedHandler mouseListener = null*/)
//        {

//#if DEBUG
//                m_ResourcesCfg = "../../res/resources_d.cfg";
//                m_PluginsCfg = "../../res/plugins_d.cfg";
//#else
//            m_ResourcesCfg = "../../res/resources.cfg";
//            m_PluginsCfg = "../../res/plugins.cfg";
//#endif

//            //m_ResourcesCfg = "../../res/resources.cfg";
//            //m_luginsCfg = "../../res/plugins.cfg";

//            SetupLog();

//            GetLog().LogMessage("Initialize SECred Framework...");

//            m_Root = new Mogre.Root(m_PluginsCfg);
//            if (!m_Root.ShowConfigDialog())
//            {
//                return false;
//            }
//            m_RenderWnd = m_Root.Initialise(true, wndTitle);


//            //?????GetRoot().AddFrameListener(this);

//            SetupViewport();
//            GetLog().LogMessage("ViewPort ready.");

//            SetupInputSystem(/*keyListener, mouseListener*/);
//            GetLog().LogMessage("InputSystem ready.");

//            SetupResources();
//            GetLog().LogMessage("Resources ready.");

//            Mogre.TextureManager.Singleton.DefaultNumMipmaps = 5;
//            Mogre.ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

//            GetLog().LogMessage("TrayManager ready.");

//            m_Timer = new Mogre.Timer();
//            m_Timer.Reset();
//            GetLog().LogMessage("Timer ready.");

//            //config
//            //m_config = new Config( "../../res/game_config.xml" );
//            GetLog().LogMessage("ConfigManager ready.");

//            //!config	
//            m_StateManager = new StateManager();
//            GetLog().LogMessage("StateManager ready.");
//            /*
//            //CEgui initialization
//            mainRenderer = &CEGUI.OgreRenderer.bootstrapSystem();
//            // set the default resource groups to be used
//            CEGUI.Imageset.setDefaultResourceGroup((CEGUI.utf8*)"Imagesets");
//            CEGUI.Font.setDefaultResourceGroup((CEGUI.utf8*)"Fonts");
//            CEGUI.Scheme.setDefaultResourceGroup((CEGUI.utf8*)"Schemes");
//            CEGUI.WidgetLookManager.setDefaultResourceGroup((CEGUI.utf8*)"LookNFeel");
//            CEGUI.WindowManager.setDefaultResourceGroup((CEGUI.utf8*)"Layouts");
//            //setup default group for validation schemas
//            CEGUI.XMLParser* parser = CEGUI.System.getSingleton().getXMLParser();
//            if (parser->isPropertyPresent((CEGUI.utf8*)"SchemaDefaultResourceGroup"))
//                parser->setProperty((CEGUI.utf8*)"SchemaDefaultResourceGroup", (CEGUI.utf8*)"schemas");
//            //wybranie stylu CEgui
//            CEGUI.SchemeManager.getSingleton().create((CEGUI.utf8*)"SECred.scheme");
//            //kursor
//            CEGUI.System.getSingleton().setDefaultMouseCursor((CEGUI.utf8*)"SECred", (CEGUI.utf8*)"MouseArrow");
//            //Tooltips - jeszcze nie wiem czy bedziemy tego uzywac
//            CEGUI.System.getSingleton().setDefaultTooltip((CEGUI.utf8*)"SECred/Tooltip");

//            GetLog().LogMessage("CEgui has been initiliazed!");
//            */
//            m_RenderWnd.IsActive = true;

//            //MissionsCreator missions;

//            GetLog().LogMessage("SECred Framework initialized!");
//            return true;

//        }

/*
 * 
 *      BIG, NOT EXISTING FRAGMENT OF CODE. It was useless.     ~MDZ
 * 
 * /

//        private void SetupResources()
//        {
//            // Load resource paths from config file
//            var cf = new Mogre.ConfigFile();
//            cf.Load(m_ResourcesCfg, "\t:=", true);

//            // Go through all sections & settings in the file
//            var seci = cf.GetSectionIterator();
//            while (seci.MoveNext())
//            {
//                foreach (var pair in seci.Current)
//                {
//                    Mogre.ResourceGroupManager.Singleton.AddResourceLocation(
//                        pair.Value, pair.Key, seci.CurrentKey);
//                }
//            }

//            /*
//            String secName, typeName, archName;
//            Mogre.ConfigFile cf;
//            cf.Load(m_ResourcesCfg, "", false);

//            Mogre.ConfigFile.SectionIterator seci = cf.GetSectionIterator();

            
//            while (seci.HasMoreElements())
//            {
//                secName = seci.PeekNextKey();
//                Mogre.ConfigFile.SettingsMultiMap settings = seci.GetNext();
//                Mogre.ConfigFile.SettingsMultiMap.Iterator i;
//                for (i = settings.Begin(); i != settings.End(); ++i)
//                {
//                    typeName = i.Key;
//                    archName = i.Value;
//                    Mogre.ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
//                }
//            }*/
//            /*
//            new SoundDict();
//            SoundDict.Singleton().SetDirectory( "../../res/sfx/" );
//            SoundDict.Singleton().LoadAndInsert( "button.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/die_01.wav" );
//            SoundDict.Singleton().LoadAndInsert( "impact/box_01.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/jump_01.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/step_concrete_01.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/step_concrete_02.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/step_concrete_03.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/step_concrete_04.wav" );
//            SoundDict.Singleton().LoadAndInsert( "player/step_concrete_05.wav" );
//            SoundDict.Singleton().LoadAndInsert( "ambient/siren_01.wav" );
//            SoundDict.Singleton().Init();
//            */
//        }
