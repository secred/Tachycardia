using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    abstract class State
    {
        protected string            m_StateName;
        protected StateManager      m_StateManager;

        public State() { }
        public void Initialize(StateManager mngr, string name)
        {
            m_StateManager = mngr;
            m_StateName = name;
        }

        public virtual void Enter() { Core.Log("Entering to state `"+m_StateName+"`..."); }
        public virtual void Exit() { Core.Log("Exiting from state `" + m_StateName + "`..."); }
        public virtual bool Pause() { Core.Log("Pausing state `" + m_StateName + "`..."); return true; }
        public virtual void Resume() { Core.Log("Resuming state `" + m_StateName + "`..."); }
        public virtual void Update() { }

        public virtual bool KeyPressed(MOIS.KeyEvent keyEventRef) { return true; }
        public virtual bool KeyReleased(MOIS.KeyEvent keyEventRef) { return true; }

        public virtual bool MouseMoved(MOIS.MouseEvent evt) { return true; }
        public virtual bool MousePressed(MOIS.MouseEvent evt, MOIS.MouseButtonID id) { return true; }
        public virtual bool MouseReleased(MOIS.MouseEvent evt, MOIS.MouseButtonID id) { return true; }

        protected State FindByName(String stateName) { return m_StateManager.FindByName(stateName); }
        protected void ChangeState(State state) { m_StateManager.ChangeState(state); }
        protected void ChangeState(String stateName) { m_StateManager.ChangeState(stateName); }
        protected bool PushState(State state) { return m_StateManager.PushState(state); }
        protected bool PushState(String stateName) { return m_StateManager.PushState(stateName); }
        protected void PopState() { m_StateManager.PopState(); }
        protected void PopAllAndPushState(State state) { m_StateManager.PopAllAndPushState(state); }

    }
}
