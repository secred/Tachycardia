using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class Core
    {
        protected static Core Instance = null;

        public static Core Singleton()
        {
            if (Instance == null)
                Instance = new Core();
            return Instance;
        }

        protected Core()
        {
            m_Config = null;
            //!config
            m_StateManager = null;

            m_Root = null;
            m_RenderWnd = null;
            m_Viewport = null;
            m_Log = null;
            m_Timer = null;

            m_InputMgr = null;
            m_Keyboard = null;
            m_Mouse = null;

            m_Update = 1.0f / 120;
            m_Elapsed = 0.0f;

            /*
            for (int i = 0; i < 256; ++i)
                m_keyStates[i] = false;
            */
        }

        public Boolean Start()
        {
            //GameState.create(m_stateManager, "GameState");
            //MenuState.create(m_stateManager, "MenuState");
            //PauseState.create(m_stateManager, "PauseState");
            m_StateManager.PushState("MenuState");
            m_StateManager.Start();
            return true;
        }

        public Boolean Init(String wndTitle, MOIS.KeyListener keyListener = null, MOIS.MouseListener mouseListener = null)
        {

#if DEBUG
	            m_ResourcesCfg = "../../res/resources_d.cfg";
	            m_PluginsCfg = "../../res/plugins_d.cfg";
#else
            m_ResourcesCfg = "../../res/resources.cfg";
            m_luginsCfg = "../../res/plugins.cfg";
#endif

            //m_ResourcesCfg = "../../res/resources.cfg";
            //m_luginsCfg = "../../res/plugins.cfg";

            SetupLog();

            GetLog().LogMessage("Initialize SECred Framework...");

            m_Root = new Mogre.Root(m_PluginsCfg);
            if (!m_Root.ShowConfigDialog())
            {
                return false;
            }
            m_RenderWnd = m_Root.Initialise(true, wndTitle);


            GetRoot().AddFrameListener(this);

            SetupViewport();
            GetLog().LogMessage("ViewPort ready.");

            SetupInputSystem(keyListener, mouseListener);
            GetLog().LogMessage("InputSystem ready.");

            SetupResources();
            GetLog().LogMessage("Resources ready.");

            Mogre.TextureManager.Singleton.DefaultNumMipmaps = 5;
            Mogre.ResourceGroupManager.Singleton.InitialiseAllResourceGroups();

            GetLog().LogMessage("TrayManager ready.");

            m_Timer = new Mogre.Timer();
            m_Timer.Reset();
            GetLog().LogMessage("Timer ready.");

            //config
            //m_config = new Config( "../../res/game_config.xml" );
            GetLog().LogMessage("ConfigManager ready.");

            //!config	
            m_StateManager = new StateManager();
            GetLog().LogMessage("StateManager ready.");
            /*
	        //CEgui initialization
	        mainRenderer = &CEGUI.OgreRenderer.bootstrapSystem();
	        // set the default resource groups to be used
	        CEGUI.Imageset.setDefaultResourceGroup((CEGUI.utf8*)"Imagesets");
	        CEGUI.Font.setDefaultResourceGroup((CEGUI.utf8*)"Fonts");
	        CEGUI.Scheme.setDefaultResourceGroup((CEGUI.utf8*)"Schemes");
	        CEGUI.WidgetLookManager.setDefaultResourceGroup((CEGUI.utf8*)"LookNFeel");
	        CEGUI.WindowManager.setDefaultResourceGroup((CEGUI.utf8*)"Layouts");
	        //setup default group for validation schemas
	        CEGUI.XMLParser* parser = CEGUI.System.getSingleton().getXMLParser();
	        if (parser->isPropertyPresent((CEGUI.utf8*)"SchemaDefaultResourceGroup"))
		        parser->setProperty((CEGUI.utf8*)"SchemaDefaultResourceGroup", (CEGUI.utf8*)"schemas");
	        //wybranie stylu CEgui
	        CEGUI.SchemeManager.getSingleton().create((CEGUI.utf8*)"SECred.scheme");
	        //kursor
	        CEGUI.System.getSingleton().setDefaultMouseCursor((CEGUI.utf8*)"SECred", (CEGUI.utf8*)"MouseArrow");
	        //Tooltips - jeszcze nie wiem czy bedziemy tego uzywac
	        CEGUI.System.getSingleton().setDefaultTooltip((CEGUI.utf8*)"SECred/Tooltip");
	        
            GetLog().LogMessage("CEgui has been initiliazed!");
            */
            m_RenderWnd.IsActive = true;

            //MissionsCreator missions;

            GetLog().LogMessage("SECred Framework initialized!");
            return true;

        }

        public Boolean FrameRenderingQueued(Mogre.FrameEvent evt)
        {
            if (m_StateManager.GetActiveState() != null)
                m_StateManager.GetActiveState().UpdateFrameEvent(evt);

            double time = evt.timeSinceLastFrame;
            ///// Bullet time tymczasowo wyci?ty
            //if( m_keyStates[OIS.KC_B] )
            //	time = 0.3 * evt.timeSinceLastFrame;
            //else
            //	time = 1.0 * evt.timeSinceLastFrame;

            double tElapsed = m_Elapsed += time;
            double tUpdate = m_Update;

            // loop through and update as many times as necessary (up to 10 times maximum).
            if ((tElapsed > tUpdate) && (tElapsed < (tUpdate * 10)))
            {
                while (tElapsed > tUpdate)
                {
                    //for( std.list<TimeObserver *>.iterator it = m_TimeObservers.begin(); it != m_TimeObservers.end(); it++ )
                    //	(*it)->update( tUpdate );
                    m_StateManager.Update(tUpdate);
                    tElapsed -= tUpdate;
                }
            }
            else
            {
                if (tElapsed < tUpdate)
                {
                    // not enough time has passed this loop, so ignore for now.
                }
                else
                {
                    // too much time has passed (would require more than 10 updates!), so just update once and reset.
                    // this often happens on the first frame of a game, where assets and other things were loading, then
                    // the elapsed time since the last drawn frame is very long.
                    //for( std.list<TimeObserver *>.iterator it = m_TimeObservers.begin(); it != m_TimeObservers.end(); it++ )
                    //	(*it)->update( tUpdate );
                    m_StateManager.Update(tUpdate);
                    tElapsed = 0.0f; // reset the elapsed time so we don't become "eternally behind".
                }
            }

            Core.Singleton().m_Elapsed = tElapsed;
            Core.Singleton().m_Update = tUpdate;

            //wstrzykiwanie uplywu czasu do CEgui
            //CEGUI.System.getSingleton().injectTimePulse(evt.timeSinceLastFrame);

            return true;
        }

        public Boolean KeyPressed(MOIS.KeyEvent keyEventRef)
        {
            m_StateManager.KeyPressed(keyEventRef);

            m_KeyStates[keyEventRef.key] = true;

            return true;
        }

        public Boolean KeyReleased(MOIS.KeyEvent keyEventRef)
        {
            if (keyEventRef.key == MOIS.KeyCode.KC_KANJI || keyEventRef.key == MOIS.KeyCode.KC_GRAVE || keyEventRef.key == MOIS.KeyCode.KC_TAB)
                /*{
                    if (Console.getSingleton().isVisible())
                        Console.getSingleton().hide();
                    else
                        Console.getSingleton().show();
                }*/
                m_StateManager.KeyReleased(keyEventRef);
            m_KeyStates[keyEventRef.key] = false;
            return true;
        }

        public Boolean MouseMoved(MOIS.MouseEvent evt)
        {
            m_StateManager.MouseMoved(evt);

            return true;
        }

        public Boolean MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            m_StateManager.MousePressed(evt, id);
            return true;
        }

        public Boolean MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            m_StateManager.MouseReleased(evt, id);
            return true;
        }

        public Config GetConfig()
        {
            return m_Config;
        }

        public StateManager GetStateManager()
        {
            return m_StateManager;
        }

        public Mogre.Root GetRoot()
        {
            return m_Root;
        }

        public Mogre.RenderWindow GetRenderWindow()
        {
            return m_RenderWnd;
        }

        public Mogre.Viewport GetViewport()
        {
            return m_Viewport;
        }

        public Mogre.Log GetLog()
        {
            return m_Log;
        }

        public Mogre.Timer GetTimer()
        {
            return m_Timer;
        }

        public MOIS.InputManager GetInputMgr()
        {
            return m_InputMgr;
        }

        public MOIS.Keyboard GetKeyboard()
        {
            return m_Keyboard;
        }

        public MOIS.Mouse GetMouse()
        {
            return m_Mouse;
        }

        //public CEGUI.MogreRenderer		    getMogreRenderer();

        public Boolean GetKeyState(MOIS.KeyCode keyCode)
        {
            return m_KeyStates[keyCode];
        }

        private void SetupLog()
        {
            Mogre.LogManager logMgr = new Mogre.LogManager();
            m_Log = Mogre.LogManager.Singleton.CreateLog("OgreLogfile.log", true, true, false);
            m_Log.SetDebugOutputEnabled(true);
        }
        private void SetupViewport()
        {
            m_Viewport = m_RenderWnd.AddViewport(null);
            m_Viewport.BackgroundColour = new Mogre.ColourValue(0.5f, 0.5f, 0.5f, 1.0f);
        }
        private void SetupInputSystem(MOIS.KeyListener pKeyListener, MOIS.MouseListener pMouseListener)
        {
            ulong hWnd = 0;
	        MOIS.ParamList paramList = new MOIS.ParamList();
	        paramList.Insert( "WINDOW", Mogre.StringConverter.ToString(hWnd));

	        m_InputMgr = MOIS.InputManager.CreateInputSystem(paramList);
	        m_Keyboard = (MOIS.Keyboard) m_InputMgr.CreateInputObject( MOIS.Type.OISKeyboard, true);
            m_Mouse = (MOIS.Mouse)m_InputMgr.CreateInputObject(MOIS.Type.OISMouse, true);

            //m_mouse.MouseState.height = m_renderWnd.height;
            //m_mouse.MouseState.width  = m_renderWnd.width;

            //if(pKeyListener == 0)
            //    m_Keyboard->setEventCallback(this);
            //else
            //    m_Keyboard->setEventCallback(pKeyListener);
            
            //if(pMouseListener == 0)
            //    m_Mouse->setEventCallback(this);
            //else
            //    m_Mouse->setEventCallback(pMouseListener);
        }


        private void SetupResources()
        {

            String secName, typeName, archName;
            Mogre.ConfigFile cf;
            cf.Load(m_ResourcesCfg, "", false);

            Mogre.ConfigFile.SectionIterator seci = cf.GetSectionIterator();
            while (seci.HasMoreElements())
            {
                secName = seci.PeekNextKey();
                Mogre.ConfigFile.SettingsMultiMap settings = seci.GetNext();
                Mogre.ConfigFile.SettingsMultiMap.Iterator i;
                for (i = settings.Begin(); i != settings.End(); ++i)
                {
                    typeName = i.Key;
                    archName = i.Value;
                    Mogre.ResourceGroupManager.Singleton.AddResourceLocation(archName, typeName, secName);
                }
            }
            /*
	        new SoundDict();
	        SoundDict.Singleton().SetDirectory( "../../res/sfx/" );
	        SoundDict.Singleton().LoadAndInsert( "button.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/die_01.wav" );
	        SoundDict.Singleton().LoadAndInsert( "impact/box_01.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/jump_01.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/step_concrete_01.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/step_concrete_02.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/step_concrete_03.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/step_concrete_04.wav" );
	        SoundDict.Singleton().LoadAndInsert( "player/step_concrete_05.wav" );
	        SoundDict.Singleton().LoadAndInsert( "ambient/siren_01.wav" );
	        SoundDict.Singleton().Init();
            */
        }




        private Config m_Config;
        private StateManager m_StateManager;

        private Mogre.Root m_Root;
        private Mogre.RenderWindow m_RenderWnd;
        private Mogre.Viewport m_Viewport;
        private Mogre.Log m_Log;
        private Mogre.Timer m_Timer;

        private MOIS.InputManager m_InputMgr;
        private MOIS.Keyboard m_Keyboard;
        private MOIS.Mouse m_Mouse;

        //CEgui - renderer dla CEgui
        //private CEGUI.MogreRenderer         mainRenderer;

        private String m_ResourcesCfg;
        private String m_PluginsCfg;

        private Double m_Update;
        private Double m_Elapsed;

        private Dictionary<MOIS.KeyCode, Boolean> m_KeyStates;

    }
}
