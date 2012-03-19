//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Tachycardia
//{
//    //class StateRegister<T>
//    //    where T : State
//    //{
//    //    static void Create(IStateListener listener, String name)
//    //    {
//    //        T myState = new T();
//    //        myState.Parent = listener;
//    //        listener.RegisterState(name, myState);
//    //    }   
//    //}

//    abstract class State
//    {

//        protected State() { }

//        //static void create(StateListener parent, String name)
//        //{
//        //    T* myState = new T();
//        //    myState.Parent = parent;
//        //    parent.RegisterState(name, myState);
//        //}

//        //public static void Create(StateListener parent, String name){}    // tez chyba niepotrzebne

//        //void destroy(){}	// chyba niepotrzebne

//        public abstract void Enter();
//        public abstract void Exit();
//        public virtual bool Pause(){return true;}
//        public virtual void Resume(){}
//        public virtual void Update( Double dt ) {}
	
//        //void updateFrameEvent(Mogre.FrameEvent evt) { FrameEvent = new Mogre.FrameEvent(evt); }
//        public void UpdateFrameEvent(Mogre.FrameEvent evt) { FrameEvent = evt; }
	
//        public virtual bool KeyPressed( MOIS.KeyEvent keyEventRef) { return true; }
//        public virtual bool KeyReleased( MOIS.KeyEvent keyEventRef) { return true; }
	
//        public virtual bool MouseMoved( MOIS.MouseEvent evt) { return true; }
//        public virtual bool MousePressed( MOIS.MouseEvent evt, MOIS.MouseButtonID id) { return true; }
//        public virtual bool MouseReleased( MOIS.MouseEvent evt, MOIS.MouseButtonID id) { return true; }
 
//        protected State     FindByName( String  stateName){return Parent.FindByName(stateName);}
//        protected void      ChangeState(State state){Parent.ChangeState(state);}
//        protected void      ChangeState( String  stateName){Parent.ChangeState(stateName);}
//        protected bool      PushState(State state){return Parent.PushState(state);}
//        protected bool      PushState(String stateName) { return Parent.PushState(stateName); }
//        protected void      PopState() { Parent.PopState(); }
//        protected void      Shutdown() { Parent.Shutdown(); }
//        protected void      PopAllAndPushState(State state) { Parent.PopAllAndPushState(state); }

//        protected IStateListener         Parent;
//        protected Mogre.FrameEvent		FrameEvent;
//        protected Mogre.Camera			Camera;
 
//        //protected WorldManager			pWorldManager;
	
//    }
//}
