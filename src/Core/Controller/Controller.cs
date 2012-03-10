using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class Controller
    {
        bool            m_IsJumping;
        //<-----------FullObject*     ptr2;

        //<-----------WorldManager*   m_pWorldMgr;

        Controller(/* WorldManager * worldMgr, FullObject* ptr*/ )
        {
            /*//<-----------PROFILER__CONTROLLER_CONTROLLER
            ptr2 = ptr;

            ptr->getBody()->setPositionOrientation( ptr->getBody()->getPosition(), Ogre::Quaternion::IDENTITY);

            ptr2->refreshPlayerController();

            ptr->getBody()->setPositionOrientation( ptr->getBody()->getPosition(), ptr->getBody()->getOrientation());
        	
            m_pWorldMgr = worldMgr;*/
        }

        ~Controller()
        {
            //<-----------PROFILER__CONTROLLER_DESTRUCTOR
        }
        public bool keyPressed(MOIS.KeyEvent keyEventRef)
        {
            //<-----------PROFILER__CONTROLLER_KEYPRESSED
	        if( keyEventRef.key == MOIS.KeyCode.KC_W )
	        {
                //<-----------ptr2->setMission( "GoAhead" );
	        }
	        else if( keyEventRef.key == MOIS.KeyCode.KC_S )
	        {
	        /*	ObjectMission * missionS = new ObjectMission( "GoBack" );
		        missionS->addState( ObjectState::GOBACK );*/
		        //<-----------ptr2->setMission( "GoBack" );
	        }
	        else if( keyEventRef.key == MOIS.KeyCode.KC_R )
	        {
		        //ObjectMission * mission = new ObjectMission( "Rush" );
		        //mission->addState( ObjectState::FASTRUN, 3.0f, true );
		        //mission->addState( ObjectState::IDLE, 0.8f, true );
		        //mission->addState( ObjectState::GOAHEAD, 0.2f );
		        //mission->addState( ObjectState::GOBACK, 0.3f );
		        //mission->addState( ObjectState::GOAHEAD, 0.4f );
		        //mission->addState( ObjectState::GOBACK, 0.5f );
		        //mission->addState( ObjectState::FASTRUN, 1.2f );
		        ////mission->useUpdateCallbacks( true );
                //<-----------ptr2->setMission( "Rush" );
	        }
	        else if( keyEventRef.key == MOIS.KeyCode.KC_A )
	        {
                //<-----------ptr2->setMission( "GoLeft" );
	        }
	        else if( keyEventRef.key == MOIS.KeyCode.KC_D )
	        {
                //<-----------ptr2->setMission( "GoRight" );
	        }
	        else if ( keyEventRef.key == MOIS.KeyCode.KC_SPACE )
	        {
		        //ObjectMission * mission2 = new ObjectMission( "Jump" );															// czas animacji ma si?
		        //mission2->addState( ObjectState::JUMP);//, ptr2->getEntity("Ninja")->getAnimationState("Jump")->getLength(), true);	// dostosowac do czasu
                //<-----------ptr2->setMission( "Jump" );																					// spadania. NIE odwrotnie
	        }
	        else if ( keyEventRef.key == MOIS.KeyCode.KC_LCONTROL)
	        {
                //<-----------if(ptr2->getMission()->getName() == "Run")
                //<-----------ptr2->setMission( "Slizg" );			
	        }
	        else if ( keyEventRef.key == MOIS.KeyCode.KC_LSHIFT )
	        {
                //<-----------if(frameworkPtr()->getKeyState(MOIS.KeyCode.KC_W) == true)
                //<-----------ptr2->setMission( "Run" );			
	        }
	        return false;
        }

        public bool keyReleased(MOIS.KeyEvent keyEventRef)
        {
            //<-----------PROFILER__CONTROLLER_KEYRELEASED
            //<-----------if(keyEventRef.key == MOIS.KeyCode.KC_LSHIFT && frameworkPtr()->getKeyState(MOIS.KeyCode.KC_W) == true)
		        //<-----------ptr2->setMission("GoAhead");



            if ((keyEventRef.key == MOIS.KeyCode.KC_W/* && ptr2->getMission()->getName()=="GoAhead"*/)
		        || (keyEventRef.key == MOIS.KeyCode.KC_S/* && ptr2->getMission()->getName()=="GoBack"*/) 
		        || (keyEventRef.key == MOIS.KeyCode.KC_A/* && ptr2->getMission()->getName()=="GoLeft"*/)
		        || (keyEventRef.key == MOIS.KeyCode.KC_D/* && ptr2->getMission()->getName()=="GoRight"*/)
		        || (keyEventRef.key == MOIS.KeyCode.KC_W/* && ptr2->getMission()->getName()=="Run"*/))
			 /*ptr2->getMission()->nextState()*/;
	        return false;
        }

        public bool  mouseMoved(MOIS.MouseEvent evt){
            //<-----------PROFILER__CONTROLLER_MOUSEMOVED
        		
	        /*if(evt.state.X.rel>0 && evt.state.Y.rel<10)
		        ptr2->physSetHeading( ptr2->physGetHeading() - Mogre.Radian(3.14 * m_pWorldMgr->getDt()*frameworkPtr()->getConfig()->getMouseSpeed()) );
	        else if(evt.state.X.rel<0 && evt.state.Y.rel<10)
                ptr2->physSetHeading(ptr2->physGetHeading() + Mogre.Radian(3.14 * m_pWorldMgr->getDt() * frameworkPtr()->getConfig()->getMouseSpeed()));
            */
	        return false;
        }
    }
}
