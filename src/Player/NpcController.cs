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
            m_MainBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("NPC");
            m_SecondBody.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("NPC");
        }
        
        public override void Update()
        {
            m_MissionSteps--;

            if (m_MissionSteps <= 0)
            {
                switch (Core.Singleton.Rand.Next() % 3)
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

            if ((m_MissionType & GO_AHEAD) > 0)
            {
                m_Pose.willForward();
            }
            if ((m_MissionType & GO_LEFT) > 0)
            {
                m_Pose.willTurnLeft();
            }
            if ((m_MissionType & GO_RIGHT) > 0)
            {
                m_Pose.willTurnRight();
            }

        }
    }
}
