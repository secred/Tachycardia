using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects
{
    class Barrel
    {//przy loaderze istotna jest juz tylko logika i fizyka obiektow
        MogreNewt.Body m_Body;
        
        public void SetPhysics(Mogre.Entity entity, Mogre.SceneNode node, float mass)
        {
            MogreNewt.ConvexCollision collision = new MogreNewt.CollisionPrimitives.Cylinder(
                Core.Singleton.m_NewtonWorld,
                Core.Singleton.m_PhysicsManager.getCollisionCylinderRadius(entity, node),
                Core.Singleton.m_PhysicsManager.getCollisionCylinderHeight(entity, node),
                new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0, 0, 1)),
                Core.Singleton.GetUniqueBodyId()
                );

            Mogre.Vector3 inertia, offset;
            collision.CalculateInertialMatrix(out inertia, out offset);
            inertia *= mass;

            m_Body = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, collision, true);
            m_Body.AttachNode(node);
            m_Body.SetMassMatrix(mass, inertia);
            m_Body.SetPositionOrientation(node.Position, node.Orientation);
            m_Body.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Metal");
        }
    }
}
