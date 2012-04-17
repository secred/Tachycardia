using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;


namespace Tachycardia
{
    class GameCamera
    {
        public Character Character;
        public Degree Angle;
        public float Distance;
        public SceneNode m_Node;
        public int m_Type;
        public float m_Tight;
        public Vector3 m_Height;

        public GameCamera()
        {
            float mass = 1;
            m_Node = Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();

        }

        public void Update()
        {
            Vector3 offset =
            Character.m_Control.m_LookingAt * (-Vector3.UNIT_Z +
                (Vector3.UNIT_Y)
                ).NormalisedCopy * Distance;

            Vector3 head = Character.m_Node.Position + Character.m_HeadOffset;
            Vector3 desiredPosition = head + offset;

            Vector3 newPosition = (Core.Singleton.m_Camera.Position +
                    (desiredPosition - Core.Singleton.m_Camera.Position) * m_Tight);

            m_Node.SetPosition(newPosition.x, newPosition.y, newPosition.z);

            Core.Singleton.m_Camera.Position = m_Node.Position + m_Height;

            Core.Singleton.m_Camera.LookAt(head);
        }

        public void Cam1()
        {
            m_Type = 0;
            Distance = 1.0f;
            m_Tight = 0.05f;
            m_Height = new Vector3(0, -0.035f, 0);
        }
        public void Cam2()
        {
            m_Type = 1;
            Distance = 4.0f;
            m_Tight = 0.5f;
            m_Height = new Vector3(0, 1.0f, 0);
        }
        public void Cam3()
        {
            m_Type = 2;
            Distance = 0.0f;
            m_Tight = 0.05f;
            m_Height = new Vector3(0, 0.5f, 0);
        }
        public void Cam4()
        {
            m_Type = 3;
            Distance = 4.0f;
            m_Tight = 0.1f;
            m_Height = new Vector3(0, 0, 0);
        }
    }
}
