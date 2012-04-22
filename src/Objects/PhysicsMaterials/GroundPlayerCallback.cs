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
            if (contact.Body0.Type == (int)PhysicsManager.BodyTypes.PLAYER
                || contact.Body0.Type == (int)PhysicsManager.BodyTypes.NPC)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body0.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose.m_name == "fly" && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        controler.ChangePoseTo("normal");
                        Console.WriteLine(controler.m_Pose.m_name);
                    }
                    controler.m_Onground = 0;
                }
            }
            else if (contact.Body1.Type == (int)PhysicsManager.BodyTypes.PLAYER
                || contact.Body1.Type == (int)PhysicsManager.BodyTypes.NPC)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body1.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose.m_name == "fly" && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        controler.ChangePoseTo("normal");
                        Console.WriteLine(controler.m_Pose.m_name);
                    }
                    controler.m_Onground = 0;
                }
            }
            return;
        }
    }
}
