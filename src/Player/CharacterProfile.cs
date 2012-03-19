using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;

namespace Tachycardia
{
    class CharacterProfile
    {
        public String m_MeshName;
        public float m_BodyMass;
        public float m_WalkSpeed;
        public Vector3 m_BodyScaleFactor;
        public Vector3 m_HeadOffset;

        public CharacterProfile Clone()
        {
            return (CharacterProfile)MemberwiseClone();
        }
    }
}
