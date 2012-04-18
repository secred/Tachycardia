using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class NpcController : PlayerController
    {
        public const int GO_AHEAD = 1;
        public const int GO_LEFT = 2;
        public const int GO_RIGHT = 4;
        public const int JUMP = 8;
        public const int RUN = 16;

        protected int m_MissionSteps = 0;
        protected int m_MissionType = 0;
        
        public NpcController(Mogre.Node node,Mogre.Entity entity,float mass)
            : base(node, entity, mass)
        {
            m_MainBody.Type = (int)PhysicsManager.BodyTypes.NPC;
            m_MainBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("NPC");
            m_SecondBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("NPC");
        }

        //position buffor
       /* public override void BodyTransformCallback(MogreNewt.Body sender, Mogre.Quaternion orientation, Mogre.Vector3 position, int threadIndex)
        {//calling each physics iteration
            //odczytywac ze stanu wysokosc kamery
            m_PlayerNode.Position = position + _position;

            //obracanie zgodnie z kierunkiem ruchu lub jezeli 0 to z kierunkiem goto
            if(m_MainBody.Velocity.Length < 0.1f)
            {
                m_State = CharacterState.IDLE;
                m_PlayerNode.Orientation = m_GoTo;
            }
            else
            {
                if (m_Pose == m_myPoses["normal"])
                {
                    if (m_MainBody.Velocity.Length > 3.5)
                        m_State = CharacterState.RUN;
                    else
                        m_State = CharacterState.WALK;
                }
                else if (m_Pose == m_myPoses["fly"])
                {
                    m_State = CharacterState.JUMP;
                }

                Mogre.Vector3 xyVector = new Mogre.Vector3(0,0,1);
                Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1,0,1);
                m_PlayerNode.Orientation = xyVector.GetRotationTo(velocityxy);
            }
        }
        */
        public override void BodyForceCallback(MogreNewt.Body body, float timeStep, int threadIndex)
        {//calling each physics iteration

            //jezeli niedawno skoczyl to obniz
            //to zmiany bo jak zeskoczymy z czegos to nadal mozna bedzie podskoczycz lecac
            if (m_jumpLimit > 0)
                m_jumpLimit--;

            if (m_adrenaline > 1)
                m_adrenaline -= 0.01f * timeStep;
            else
                m_adrenaline = 1;
            Update();
            
        }
        
        public override void Update()
        {
            m_MissionSteps--;

            if (m_MissionSteps <= 0)
            {
                if ((m_MissionType & RUN) > 0)
                    m_Pose.m_MaxSpd /= 1.5f;
                switch (Core.Singleton.Rand.Next() % 10)
                {
                    case 0:
                    case 1:
                    case 2:
                        m_MissionSteps = Core.Singleton.Rand.Next(5, 10);
                        if (Core.Singleton.Rand.Next() % 2 == 0)
                            m_MissionType = GO_LEFT;
                        else
                            m_MissionType = GO_RIGHT;
                        if (Core.Singleton.Rand.Next() % 2 == 0)
                            m_MissionType += GO_AHEAD;
                        break;
                    case 3:
                    case 4:
                    case 5:
                        m_MissionSteps = Core.Singleton.Rand.Next(20, 60);
                        m_MissionType = GO_AHEAD;
                        break;
                    case 6:
                        m_MissionSteps = Core.Singleton.Rand.Next(30, 120);
                        m_MissionType = RUN;
                        m_Pose.m_MaxSpd *= 1.5f;
                        break;
                    default:
                        m_MissionSteps = Core.Singleton.Rand.Next(20, 60);
                        m_MissionType = 0;
                        break;
                }
            }

            //m_GoTo = m_LookingAt;
            bool activateidle = true;

            if ((m_MissionType & GO_AHEAD) > 0)
            {
                m_Pose.willForward();
                activateidle = false;
            }
            if ((m_MissionType & RUN) > 0)
            {
                m_Pose.willForward();
                activateidle = false;
            }
            if ((m_MissionType & GO_LEFT) > 0)
            {
                m_Pose.willTurnLeft();
            }
            if ((m_MissionType & GO_RIGHT) > 0)
            {
                m_Pose.willTurnRight();
            }

            if (activateidle == true && m_Pose == m_myPoses["normal"])
            {
                if (!m_MainBody.Velocity.IsZeroLength)
                {//nie zatrzymal sie jeszcze to go zatrzymujemy
                    Mogre.Vector3 xyVector = new Mogre.Vector3(0, 0, 1);
                    Mogre.Vector3 velocityxy = m_MainBody.Velocity * new Mogre.Vector3(1, 0, 1);
                    Mogre.Quaternion ForceDirection = xyVector.GetRotationTo(velocityxy);
                    Mogre.Vector3 StoppingForce = -ForceDirection * Mogre.Vector3.UNIT_Z * m_MainBody.Mass * 6;
                    m_MainBody.AddForce(-StoppingForce);
                }
            }
        }

        protected override void initLogicStates()
        {
            m_myPoses = new Dictionary<string, LogicState>();

            //m_adrenaline
            m_adrenaline = 1;
            //sila wyskoku
            m_jumpForce = 6;
            //pozycja wyprostowana
            LogicState normal = new Normal();//LogicState();
            normal.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("normal", normal);//dodanie obiektu

            //crouch
            LogicState crouch = new Crouch();
            crouch.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("crouch", crouch);//dodanie obiektu

            //fly
            LogicState fly = new Fly();
            fly.m_Control = this;//przyda sie uchwyt do swojego kontrolera
            m_myPoses.Add("fly", fly);//dodanie obiektu

        }
    }
}
