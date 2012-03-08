using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class Core
    {
        public static Core Singleton()
        {
            return new Core();
        }

        public Core()
        {
            m_config = 0;
            //!config
            m_stateManager = 0;

            m_root = null;
            m_renderWnd = null;
            m_viewport = null;
            m_log = null;
            m_timer = null;

            m_inputMgr = null;
            m_keyboard = null;
            m_mouse = null;

            m_update = 1.0f / 120;
            m_elapsed = 0.0f;

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
            m_stateManager->pushState("MenuState");
            m_stateManager->start();
            return true;
        }

        public Boolean Init(String wndTitle, MOIS.KeyListener keyListener = null, MOIS.MouseListener mouseListener = null)
        {

#if DEBUG
	            m_resourcesCfg = "../../res/resources_d.cfg";
	            m_pluginsCfg = "../../res/plugins_d.cfg";
#else
            m_ResourcesCfg = "../../res/resources.cfg";
            m_luginsCfg = "../../res/plugins.cfg";
#endif

            //m_ResourcesCfg = "../../res/resources.cfg";
            //m_luginsCfg = "../../res/plugins.cfg";

            SetupLog();

            GetLog().LogMessage("Initialize SECred Framework...");

            m_root = new Mogre.Root(m_pluginsCfg);
            if (!m_root.ShowConfigDialog())
            {
                return false;
            }
            m_renderWnd = m_root.Initialise(true, wndTitle);


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

            m_timer = new Mogre.Timer();
            m_timer.Reset();
            GetLog().LogMessage("Timer ready.");

            //config
            //m_config = new Config( "../../res/game_config.xml" );
            GetLog().LogMessage("ConfigManager ready.");

            //!config	
            //m_stateManager = new StateManager();
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
            m_renderWnd.IsActive = true;

            //MissionsCreator missions;

            GetLog().LogMessage("SECred Framework initialized!");
            return true;

        }

        public Boolean FrameRenderingQueued(Mogre.FrameEvent evt)
        {
            if (m_stateManager->getActiveState())
                m_stateManager->getActiveState()->updateFrameEvent(evt);

            double time = evt.timeSinceLastFrame;
            ///// Bullet time tymczasowo wyci?ty
            //if( m_keyStates[OIS.KC_B] )
            //	time = 0.3 * evt.timeSinceLastFrame;
            //else
            //	time = 1.0 * evt.timeSinceLastFrame;

            double tElapsed = m_elapsed += time;
            double tUpdate = m_update;

            // loop through and update as many times as necessary (up to 10 times maximum).
            if ((tElapsed > tUpdate) && (tElapsed < (tUpdate * 10)))
            {
                while (tElapsed > tUpdate)
                {
                    //for( std.list<TimeObserver *>.iterator it = m_TimeObservers.begin(); it != m_TimeObservers.end(); it++ )
                    //	(*it)->update( tUpdate );
                    m_stateManager->update(tUpdate);
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
                    m_stateManager->update(tUpdate);
                    tElapsed = 0.0f; // reset the elapsed time so we don't become "eternally behind".
                }
            }

            Core.Singleton().m_elapsed = tElapsed;
            Core.Singleton().m_update = tUpdate;

            //wstrzykiwanie uplywu czasu do CEgui
            //CEGUI.System.getSingleton().injectTimePulse(evt.timeSinceLastFrame);

            return true;
        }

        public Boolean KeyPressed(MOIS.KeyEvent keyEventRef)
        {
            m_stateManager.keyPressed(keyEventRef);

            m_keyStates[keyEventRef.key] = true;

            return true;
        }

        public Boolean KeyReleased(MOIS.KeyEvent keyEventRef)
        {
            if (keyEventRef.key == MOIS.KeyCode.KC_KANJI || keyEventRef.key == MOIS.KeyCode.KC_GRAVE || keyEventRef.key == OIS.KC_TAB)
                /*{
                    if (Console.getSingleton().isVisible())
                        Console.getSingleton().hide();
                    else
                        Console.getSingleton().show();
                }*/
                m_stateManager.keyReleased(keyEventRef);
            m_keyStates[keyEventRef.key] = false;
            return true;
        }

        public Boolean MouseMoved(MOIS.MouseEvent evt)
        {
            m_stateManager.mouseMoved(evt);

            return true;
        }

        public Boolean MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            m_stateManager.mousePressed(evt, id);
            return true;
        }

        public Boolean MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            m_stateManager.mouseReleased(evt, id);
            return true;
        }

        public Config GetConfig()
        {
            return m_config;
        }

        public StateManager GetStateManager()
        {
            return m_stateManager;
        }

        public Mogre.Root GetRoot()
        {
            return m_root;
        }

        public Mogre.RenderWindow GetRenderWnd()
        {
            return m_renderWnd;
        }

        public Mogre.Viewport GetViewport()
        {
            return m_viewport;
        }

        public Mogre.Log GetLog()
        {
            return m_log;
        }

        public Mogre.Timer GetTimer()
        {
            return m_timer;
        }

        public MOIS.InputManager GetInputMgr()
        {
            return m_inputMgr;
        }

        public MOIS.Keyboard GetKeyboard()
        {
            return m_keyboard;
        }

        public MOIS.Mouse GetMouse()
        {
            return m_mouse;
        }

        //public CEGUI.MogreRenderer		    getMogreRenderer();

        public Boolean GetKeyState(MOIS.KeyCode keyCode)
        {
            return m_keyStates[keyCode];
        }

        private void SetupLog()
        {
            Mogre.LogManager logMgr = new Mogre.LogManager();
            m_log = Mogre.LogManager.Singleton.CreateLog("OgreLogfile.log", true, true, false);
            m_log.SetDebugOutputEnabled(true);
        }
        private void SetupViewport()
        {
            m_viewport = m_renderWnd.AddViewport(null);
            m_viewport.BackgroundColour = new Mogre.ColourValue(0.5f, 0.5f, 0.5f, 1.0f);
        }
        private void SetupInputSystem(MOIS.KeyListener pKeyListener, MOIS.MouseListener pMouseListener)
        {
            ulong hWnd = 0;
	        MOIS.ParamList paramList;
	        paramList.Insert( "WINDOW", Mogre.StringConverter.ToString(hWnd));

	        m_inputMgr = MOIS.InputManager.CreateInputSystem(paramList);
	        m_keyboard = (MOIS.Keyboard) m_inputMgr.CreateInputObject( MOIS.Type.OISKeyboard, true);
	        m_mouse = (MOIS.Mouse) m_inputMgr.CreateInputObject(MOIS.Type.OISMouse, true));
	        /*m_mouse.MouseState.height = m_renderWnd.height;
	        m_mouse.MouseState.width  = m_renderWnd.width;
	        
	        if(pKeyListener == 0)
		        m_Keyboard->setEventCallback(this);
	        else
		        m_Keyboard->setEventCallback(pKeyListener);
            
	        if(pMouseListener == 0)
		        m_Mouse->setEventCallback(this);
	        else
		        m_Mouse->setEventCallback(pMouseListener);
             */
        }


        private void SetupResources()
        {

            String secName, typeName, archName;
            Mogre.ConfigFile cf;
            cf.Load(m_resourcesCfg, "", false);

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




        private Config m_config;
        private StateManager m_stateManager;

        private Mogre.Root m_root;
        private Mogre.RenderWindow m_renderWnd;
        private Mogre.Viewport m_viewport;
        private Mogre.Log m_log;
        private Mogre.Timer m_timer;

        private MOIS.InputManager m_inputMgr;
        private MOIS.Keyboard m_keyboard;
        private MOIS.Mouse m_mouse;

        //CEgui - renderer dla CEgui
        //private CEGUI.MogreRenderer         mainRenderer;

        private String m_resourcesCfg;
        private String m_pluginsCfg;

        private Double m_update;
        private Double m_elapsed;

        private Dictionary<MOIS.KeyCode, Boolean> m_keyStates;

    }
}
