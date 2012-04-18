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
        public const int STAND = 16;

        protected int m_MissionSteps = 0;
        protected int m_MissionType = 0;
        
        public NpcController(Mogre.Node node, Mogre.Entity entity, float mass)
            : base(node, entity, mass)
        {
            m_MainBody.Type = (int)PhysicsManager.BodyTypes.NPC;
            m_MainBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Metal");
            m_SecondBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Metal");
        }
        
        public override void Update()
        {
            m_MissionSteps--;

            if (m_MissionSteps <= 0)
            {
                switch (Core.Singleton.Rand.Next() % 2)
                {
                    case 0:
                        m_MissionSteps = Core.Singleton.Rand.Next(5, 10);
                        if (Core.Singleton.Rand.Next() % 2 == 0)
                            m_MissionType = GO_LEFT;
                        else
                            m_MissionType = GO_RIGHT;
                        if (Core.Singleton.Rand.Next() % 2 == 0)
                            m_MissionType += GO_AHEAD;
                        break;
                    case 1:
                        m_MissionSteps = Core.Singleton.Rand.Next(20, 60);
                        m_MissionType = GO_AHEAD;
                        break;
                    case 2:
                        m_MissionSteps = Core.Singleton.Rand.Next(20, 60);
                        m_MissionType = STAND;
                        break;
                }
            }

            bool activateidle = true;
            bool turning = false;

            if ((m_MissionType & GO_AHEAD) > 0)
            {
                m_Pose.willForward();
                activateidle = false;
                m_backward = false;
            }
            if ((m_MissionType & GO_LEFT) > 0)
            {
                m_Pose.willTurnLeft();
                turning = true;
            }
            if ((m_MissionType & GO_RIGHT) > 0)
            {
                m_Pose.willTurnRight();
                turning = true;
            }

            //zeroanie wektoras kretu
            //if (!turning)
            {
                //m_GoTo = m_LookingAt;
            }

            //proba zatrzymania typa w miejscu w ktorym ostatnio puscil klawisze
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
    }
}
