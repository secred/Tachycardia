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

        
        //launch method
        public override void Go()
        {
            //getting pointer to player
            Tachycardia.Character _Player = (Tachycardia.Character)Tachycardia.Core.Singleton.m_ObjectManager.Find("player");
            //run any graphics effect, blur or smth
            //Tachycardia.Core.Singleton.m_Camera.DoSmth();
            //move player to:
            _Player.m_Control.m_MainBody.SetPositionOrientation(m_TeleportTo, Mogre.Quaternion.IDENTITY);
        }
    }
}
