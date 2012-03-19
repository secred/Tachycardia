//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Tachycardia
//{
//    class StateManager : IStateListener
//    {
//        List<State>                     ActiveStateStack;
//        //std::vector<State>				ActiveStateStack;
//        Dictionary<String, State>	    States;
//        bool							Shutdown;

//        public StateManager()
//        {
//            ActiveStateStack = new List<State>();
//            States = new Dictionary<string,State>();
//            Shutdown = false;
//        }

//        ~StateManager()
//        {
//            while(ActiveStateStack.Count > 0)
//            {
//                State st = ActiveStateStack[0];
//                st.Exit();
//                ActiveStateStack.Remove(st);
//            }
//            States.Clear();
//        }

//        public void CreateState(String stateName)
//        {

//        }

//        public void RegisterState(String stateName, State state)
//        {
//            try
//            {
//                States.Add( stateName, state );		
//            }
//            catch(Exception e)
//            {
//                throw new Exception("Error while trying to add new state", e);
//            }
//        }
        
//        public  State FindByName(String stateName)
//        {
//            if (States.ContainsKey(stateName))
//                return States[stateName];
//            throw new Exception("StateManager.FindByName: " + stateName + " doesn't exist!");
//        }

//        public void Start()
//        {
//            //changeState(state);

//            //dTime dT = dTime::CreateBySeconds(1);
//            //int startTime = 0;

//            Core core = Core.Singleton();
	
//            while(!Shutdown)
//            {
//                if(core.GetRenderWindow().IsClosed)
//                    Shutdown = true;

//                Mogre.WindowEventUtilities.MessagePump();

//                if (core.GetRenderWindow().IsActive)
//                {
//                    //startTime = Core.getSingletonPtr()->getTimer()->getMillisecondsCPU();

//                    //if (core.GetKeyboard())       //  maybe
//                        core.GetKeyboard().Capture();

//                    //if (core.GetMouse())      //  maybe
//                        core.GetMouse().Capture();
			
//                    //ActiveStateStack[0].update( dT );      // nie wiem do czego to sluzylo, ale bylo wykomentowane
//                    //Core.Singleton().Update( dT );     // nie wiem do czego to sluzylo, ale bylo wykomentowane

//                    //if (core.GetRoot())       //  maybe
//                        core.GetRoot().RenderOneFrame();
			
//                    //dT = (long) Core.getSingletonPtr()->getTimer()->getMillisecondsCPU() - startTime;
//                }
//                else
//                {
//        //#if OGRE_PLATFORM == OGRE_PLATFORWIN32
//        //            Sleep(1000);
//        //#else
//        //            sleep(1);
//        //#endif
//                }
//            }

//            core.GetLog().LogMessage("Main loop quit");
//        }

//        public  void ChangeState(State state)
//        {
//            if(ActiveStateStack.Count > 0)
//            {
//                ActiveStateStack[0].Exit();
//                ActiveStateStack.Remove(ActiveStateStack[0]);
//            }
//            ActiveStateStack.Insert(0, state);
//            Init(state);
//            state.Enter();
//        }

//        public  void ChangeState(String stateName)
//        {
//            ChangeState(FindByName(stateName));
//        }

//        public  bool PushState(State state)
//        {
//            if(ActiveStateStack.Count > 0)
//            {
//                if(!ActiveStateStack[0].Pause())
//                    return false;
//            }
//            ActiveStateStack.Insert(0, state);
//            Init(state);
//            state.Enter();
//            return true;
//        }

//        public  bool PushState( String  stateName)
//        {
//            return PushState(FindByName(stateName));
//        }

//        public  void PopState()
//        {
//            if(ActiveStateStack.Count > 0)
//            {
//                ActiveStateStack[0].Exit();
//                ActiveStateStack.Remove(ActiveStateStack[0]);
//            }
//            if(ActiveStateStack.Count > 0)
//            {
//                Init(ActiveStateStack[0]);
//                ActiveStateStack[0].Resume();
//            }
//            else
//                Shutdown();
//        }

//        public  void PopAllAndPushState(State state)
//        {
//            while(ActiveStateStack.Count > 0)
//            {
//                ActiveStateStack[0].Exit();
//                ActiveStateStack.Remove(ActiveStateStack[0]);
//            }
//            PushState(state);
//        }

//        public  void PauseState()
//        {
//            if(ActiveStateStack.Count > 0)
//            {
//                ActiveStateStack[0].Pause();
//            }
 
//            if(ActiveStateStack.Count > 1)
//            {
//                Init(ActiveStateStack[1]);
//                ActiveStateStack[1].Resume();
//            }
//        }

//        public  void Shutdown()
//        {
//            Shutdown = true;
//        }

//        public void Init(State state)
//        {
//            //Core.getSingletonPtr()->getTrayMgr()->setListener(state);
//            //Core.getSingletonPtr()->getRenderWnd()->resetStatistics();

//            //Core.Singleton().GetTrayMgr().SetListener(state);     // zarzucony na rzecz CEgui
//            Core.Singleton().GetRenderWindow().ResetStatistics();
//        }

//        public State GetActiveState()
//        {
//            if( ActiveStateStack.Count > 0)
//                return ActiveStateStack[0];
//            return null;
//        }

//        public void Update(  Double  dt )
//        {
//            if( ActiveStateStack.Count > 0)
//                ActiveStateStack[0].Update( dt );
//        }

//        public bool KeyPressed(MOIS.KeyEvent keyEventRef)
//        {
//            if(ActiveStateStack.Count > 0)
//                return ActiveStateStack[0].KeyPressed(keyEventRef);
//            return false;
//        }
//        public bool KeyReleased(MOIS.KeyEvent keyEventRef)
//        {
//            if (ActiveStateStack.Count > 0)
//                return ActiveStateStack[0].KeyReleased(keyEventRef);
//            return false;
//        }
//        public bool MouseMoved(MOIS.MouseEvent evt)
//        {
//            if (ActiveStateStack.Count > 0)
//                return ActiveStateStack[0].MouseMoved(evt);
//            return false;
//        }
//        public bool MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
//        {
//            if (ActiveStateStack.Count > 0)
//                return ActiveStateStack[0].MousePressed(evt, id);
//            return false;
//        }
//        public bool MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
//        {
//            if (ActiveStateStack.Count > 0)
//                return ActiveStateStack[0].MouseReleased(evt, id);
//            return false;
//        }

//    }
//}
