using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.PhysicsMaterials
{
    class NPCNPCCallback : MogreNewt.ContactCallback
    {
        public NPCNPCCallback()
        {
            
        }

        public override void UserProcess(MogreNewt.ContactJoint contact, float timestep, int threadIndex)
        {
            
            //contact.Body0.AddForce(-contact.Body0.Velocity * new Mogre.Vector3(1, 0, 1) * 400 * contact.Body0.Mass);
            //contact.Body1.AddForce(-contact.Body1.Velocity * new Mogre.Vector3(1, 0, 1) * 400 * contact.Body1.Mass);

            return;
        }
    }
}
