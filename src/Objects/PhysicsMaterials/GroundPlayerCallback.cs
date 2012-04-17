using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.PhysicsMaterials
{
    class GroundPlayerCallback : MogreNewt.ContactCallback
    {
        public GroundPlayerCallback()
        {
            System.Console.WriteLine(": ");
        }

        public override void UserProcess(MogreNewt.ContactJoint contact, float timestep, int threadIndex)
        {
            if (contact.Body0.Type == (int)PhysicsManager.BodyTypes.PLAYER)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body0.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose == controler.m_myPoses["fly"] && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        controler.m_Pose = controler.m_myPoses["normal"];
                        controler.m_MainBody.LinearDamping = 1.0f;
                        Console.WriteLine("NORMAL");
                    }
                    controler.m_Onground = 0;
                }
            }
            else if (contact.Body1.Type == (int)PhysicsManager.BodyTypes.PLAYER)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body1.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose == controler.m_myPoses["fly"] && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        controler.m_Pose = controler.m_myPoses["normal"];
                        controler.m_MainBody.LinearDamping = 1.0f;
                        Console.WriteLine("NORMAL");
                    }
                    controler.m_Onground = 0;
                }
            }
            return;
        }
    }
}
