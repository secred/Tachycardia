using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.PhysicsMaterials
{
    class MetalMetalCallback : MogreNewt.ContactCallback
    {
        public MetalMetalCallback()
        {
            System.Console.WriteLine("metal metal kolizja: ");
        }

        public override void UserProcess(MogreNewt.ContactJoint contact, float timestep, int threadIndex)
        {
            System.Console.WriteLine("metal metal kolizja: ");
            return;
        }
    }
}
