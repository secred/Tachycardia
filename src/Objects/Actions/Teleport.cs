using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.Actions
{
    class Teleport : BaseAction
    {
        //teleport destination
        public Mogre.Vector3 m_TeleportTo;
        public Teleport(Mogre.Vector3 destination)
        {
            m_TeleportTo = destination;
        }
        
        //launch method
        public override void Go()
        {
            //getting pointer to player
            Tachycardia.Character _Player = (Tachycardia.Character)Tachycardia.Core.Singleton.m_ObjectManager.Find("player");
            //run any graphics effect, blur or smth
            //Tachycardia.Core.Singleton.m_Camera.DoSmth();
            //move player to:
            _Player.m_Control.m_MainBody.SetPositionOrientation(m_TeleportTo, Mogre.Quaternion.IDENTITY);
            _Player.m_Control.m_SecondBody.SetPositionOrientation(m_TeleportTo + new Mogre.Vector3(0,1,0), Mogre.Quaternion.IDENTITY);
            _Player.m_Control.m_MainBody.Velocity = new Mogre.Vector3(0, 0, 0);
            _Player.m_Control.m_SecondBody.Velocity = new Mogre.Vector3(0, 0, 0);
        }
    }
}
