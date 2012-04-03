using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class StateManager //: IStateListener   // a potrzebny ten interfejs komus do szczescia? ~MDZ
    {
        List<State>                 m_ActiveStateStack;
        Dictionary<String, State>   m_States;

        public StateManager()
        {
            m_ActiveStateStack = new List<State>();
            m_States = new Dictionary<string, State>();
        }

        ~StateManager()
        {
            while (m_ActiveStateStack.Count > 0)
            {
                State st = m_ActiveStateStack[0];
                st.Exit();
                m_ActiveStateStack.Remove(st);
            }
            m_States.Clear();
        }

        public void RegisterState(String stateName, State state)
        {
            try
            {
                if (!m_States.ContainsKey(stateName))
                {
                    m_States.Add(stateName, state);
                    state.Initialize(this, stateName);
                }
                else
                    throw new Exception("State `"+stateName+"` already registered!");
            }
            catch (Exception e)
            {
                throw new Exception("Error while trying to add new state", e);
            }
        }

        public State FindByName(String stateName)
        {
            if (m_States.ContainsKey(stateName))
                return m_States[stateName];
            throw new Exception("StateManager.FindByName: " + stateName + " doesn't exist!");
        }

        public void ChangeState(State state)
        {
            if (m_ActiveStateStack.Count > 0)
            {
                m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
            }
            m_ActiveStateStack.Insert(0, state);
            Init(state);
            state.Enter();
        }

        public void ChangeState(String stateName)
        {
            ChangeState(FindByName(stateName));
        }

        public bool PushState(State state)
        {
            if (m_ActiveStateStack.Count > 0)
            {
                if (!m_ActiveStateStack[0].Pause())
                    return false;
            }
            m_ActiveStateStack.Insert(0, state);
            Init(state);
            state.Enter();
            return true;
        }

        public bool PushState(String stateName)
        {
            return PushState(FindByName(stateName));
        }

        public void PopState()
        {
            if (m_ActiveStateStack.Count > 0)
            {
                m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
            }
            if (m_ActiveStateStack.Count > 0)
            {
                Init(m_ActiveStateStack[0]);
                m_ActiveStateStack[0].Resume();
            }
            else
                Core.Singleton.Shutdown();
        }

        public void PopAllAndPushState(State state)
        {
            while (m_ActiveStateStack.Count > 0)
            {
                m_ActiveStateStack[0].Exit();
                m_ActiveStateStack.Remove(m_ActiveStateStack[0]);
            }
            PushState(state);
        }

        public void PauseState()
        {
            if (m_ActiveStateStack.Count > 0)
            {
                m_ActiveStateStack[0].Pause();
            }

            if (m_ActiveStateStack.Count > 1)
            {
                Init(m_ActiveStateStack[1]);
                m_ActiveStateStack[1].Resume();
            }
        }
        
        public State GetActiveState()
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0];
            return null;
        }

        public void Init(State state)
        {
            Core.Singleton.m_RenderWindow.ResetStatistics();
        }

        public void Update()
        {
            if (m_ActiveStateStack.Count > 0)
                m_ActiveStateStack[0].Update();
        }

        public bool KeyPressed(MOIS.KeyEvent keyEventRef)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].KeyPressed(keyEventRef);
            return true;
        }
        public bool KeyReleased(MOIS.KeyEvent keyEventRef)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].KeyReleased(keyEventRef);
            return true;
        }
        public bool MouseMoved(MOIS.MouseEvent evt)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MouseMoved(evt);
            return true;
        }
        public bool MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MousePressed(evt, id);
            return true;
        }
        public bool MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id)
        {
            if (m_ActiveStateStack.Count > 0)
                return m_ActiveStateStack[0].MouseReleased(evt, id);
            return true;
        }

    }
}
