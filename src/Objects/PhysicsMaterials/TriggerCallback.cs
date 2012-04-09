using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.PhysicsMaterials
{
    class TriggerCallback : MogreNewt.ContactCallback
    {
        public TriggerCallback()
        {
            System.Console.WriteLine("Triggerready: ");
        }

        public override void UserProcess(MogreNewt.ContactJoint contact, float timestep, int threadIndex)
        {
            if ( contact.Body0.Type == (int)PhysicsManager.BodyTypes.TRIGGER )
            {
                Tachycardia.Objects.Actions.BaseAction action = (Tachycardia.Objects.Actions.BaseAction)contact.Body0.UserData;
                action.Go();
            }
            else if ( contact.Body1.Type == (int)PhysicsManager.BodyTypes.TRIGGER )
            {
                Tachycardia.Objects.Actions.BaseAction action = (Tachycardia.Objects.Actions.BaseAction)contact.Body1.UserData;
                action.Go();
            }
            return;
        }
    }
}
