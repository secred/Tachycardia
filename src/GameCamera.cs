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
        public float Distance;
        public Degree Angle;

        public void Update()
        {
            Vector3 offset =
            Character.m_Node.Orientation * (-Vector3.UNIT_Z +
                (Vector3.UNIT_Y * (float)System.Math.Tan(Angle.ValueRadians))
                ).NormalisedCopy * Distance;

            Vector3 head = Character.m_Node.Position + Character.m_Profile.m_HeadOffset;
            Vector3 desiredPosition = head + offset;

            Core.Singleton.m_Camera.Position +=
            (desiredPosition - Core.Singleton.m_Camera.Position) * 0.1f;
            Core.Singleton.m_Camera.LookAt(head);     
        }

    }
}
