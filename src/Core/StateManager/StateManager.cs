using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class StateManager : IStateListener
    {
        List<State>                     m_ActiveStateStack;
        //std::vector<State>				m_ActiveStateStack;
	    Dictionary<String, State>	    m_States;
	    bool							m_Shutdown;

        public StateManager()
        {
            m_ActiveStateStack = new List<State>();
            m_States = new Dictionary<string,State>();
	        m_Shutdown = false;
        }

        ~StateManager()
        {
            while(m_ActiveStateStack.Count > 0)
	        {
                State st = m_ActiveStateStack[0];
                st.Exit();
                m_ActiveStateStack.Remove(st);
	        }
	        m_States.Clear();
        }

        public void CreateState(String stateName)
        {

        }

        public void RegisterState(String stateName, State state)
        {
	        try
	        {
		        m_States.Add( stateName, state );		
	        }
	        catch(Exception e)
	        {
                throw new Exception("Error while trying to add new state", e);
	        }
        }
        
        public  State FindByName(String stateName)
        {
            if (m_States.ContainsKey(stateName))
                return m_States[stateName];
            throw new Exception("StateManager.FindByName: " + stateName + " doesn't exist!");
        }

        public void Start()
        {
	        //changeState(state);

	        //dTime dT = dTime::CreateBySeconds(1);
	        //int startTime = 0;

            Core core = Core.Singleton();
	
	        while(!m_Shutdown)
	        {
		        if(core.GetRenderWindow().IsClosed)
                    m_Shutdown = true;

		        Mogre.WindowEventUtilities.MessagePump();

                if (core.GetRenderWindow().IsActive)
		        {
			        //startTime = Core.getSingletonPtr()->getTimer()->getMillisecondsCPU();

                    //if (core.GetKeyboard())       //  maybe
                        core.GetKeyboard().Capture();

                    //if (core.GetMouse())      //  maybe
                        core.GetMouse().Capture();
			
			        //m_ActiveStateStack[0].update( dT );      // nie wiem do czego to sluzylo, ale bylo wykomentowane
                    //Core.Singleton().Update( dT );     // nie wiem do czego to sluzylo, ale bylo wykomentowane

                    //if (core.GetRoot())       //  maybe
                        core.GetRoot().RenderOneFrame();
			
			        //dT = (long) Core.getSingletonPtr()->getTimer()->getMillisecondsCPU() - startTime;
		        }
		        else
		        {
        //#if OGRE_PLATFORM == OGRE_PLATFORM_WIN32
        //            Sleep(1000);
        //#else
        //            sleep(1);
        //#endif
		        }
	        }

	        core.GetLog().LogMessage("Main loop quit");
        }

        public  void ChangeState(State state)
        {
	        if(m_ActiveStateStack.Count > 0)
	        {
                m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
	        }
            m_ActiveStateStack.Insert(0, state);
            Init(state);
            state.Enter();
        }

        public  void ChangeState(String stateName)
        {
            ChangeState(FindByName(stateName));
        }

        public  bool PushState(State state)
        {
	        if(m_ActiveStateStack.Count > 0)
	        {
		        if(!m_ActiveStateStack[0].Pause())
			        return false;
	        }
	        m_ActiveStateStack.Insert(0, state);
	        Init(state);
            state.Enter();
	        return true;
        }

        public  bool PushState( String  stateName)
        {
            return PushState(FindByName(stateName));
        }

        public  void PopState()
        {
	        if(m_ActiveStateStack.Count > 0)
	        {
		        m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
	        }
	        if(m_ActiveStateStack.Count > 0)
	        {
		        Init(m_ActiveStateStack[0]);
		        m_ActiveStateStack[0].Resume();
	        }
	        else
		        Shutdown();
        }

        public  void PopAllAndPushState(State state)
        {
	        while(m_ActiveStateStack.Count > 0)
	        {
		        m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
	        }
	        PushState(state);
        }

        public  void PauseState()
        {
	        if(m_ActiveStateStack.Count > 0)
	        {
		        m_ActiveStateStack[0].Pause();
	        }
 
	        if(m_ActiveStateStack.Count > 1)
	        {
		        Init(m_ActiveStateStack[1]);
                m_ActiveStateStack[1].Resume();
	        }
        }

        public  void Shutdown()
        {
	        m_Shutdown = true;
        }

        public void Init(State state)
        {
            //Core.getSingletonPtr()->getTrayMgr()->setListener(state);
            //Core.getSingletonPtr()->getRenderWnd()->resetStatistics();

            //Core.Singleton().GetTrayMgr().SetListener(state);     // zarzucony na rzecz CEgui
            Core.Singleton().GetRenderWindow().ResetStatistics();
        }

        public State GetActiveState()
        {
	        if( m_ActiveStateStack.Count > 0)
		        return m_ActiveStateStack[0];
		    return null;
        }

        public void Update(  Double  dt )
        {
	        if( m_ActiveStateStack.Count > 0)
		        m_ActiveStateStack[0].Update( dt );
        }

        public bool KeyPressed(MOIS.KeyEvent keyEventRef)
        {
	        if(m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].KeyPressed(keyEventRef);
            return false;
        }
        public bool KeyReleased(MOIS.KeyEvent keyEventRef)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].KeyReleased(keyEventRef);
            return false;
        }
        public bool MouseMoved(MOIS.MouseEvent evt)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MouseMoved(evt);
            return false;
        }
        public bool MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MousePressed(evt, id);
            return false;
        }
        public bool MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MouseReleased(evt, id);
            return false;
        }

    }
}
