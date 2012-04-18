using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;
using Tachycardia.Objects;

namespace Tachycardia.Objects
{
    class Barrel : GameObject
    {//przy loaderze istotna jest juz tylko logika i fizyka obiektow
        public MogreNewt.Body m_Body;
        public Vector3 m_HeadOffset;
        public Entity m_Entity;
        public SceneNode m_Node;

        public Barrel(string meshName, float mass)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.1f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.m_SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.SetPosition(0f, 0f, 0f);
            SetPhysics(m_Entity, m_Node, mass);
        }

        public Barrel(string meshName, float mass, Vector3 v)
        {
            m_Entity = Core.Singleton.m_SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.Scale(0.5f, 0.5f, 0.5f);
            m_Node.Scale(new Vector3(0.3f,0.3f,0.3f));
            // m_Node.Rotate(new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0f, 0f, 1f) ));
            m_Node.SetPosition(v.x, v.y, v.z);
            SetPhysics(m_Entity, m_Node, mass);
        }

        public void SetPhysics(Mogre.Entity entity, Mogre.SceneNode node, float mass)
        {
            MogreNewt.ConvexCollision collision = new MogreNewt.CollisionPrimitives.Cylinder(
                Core.Singleton.m_NewtonWorld,

                Core.Singleton.m_PhysicsManager.getCollisionCylinderRadius(entity, node),
                Core.Singleton.m_PhysicsManager.getCollisionCylinderHeight(entity, node),
                new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0f, 0f, 1f)),
                Core.Singleton.GetUniqueBodyId()
                );

            Mogre.Vector3 inertia, offset;
            collision.CalculateInertialMatrix(out inertia, out offset);
            inertia *= mass;

            m_Body = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, collision, true);
            m_Body.AttachNode(node);
            m_Body.SetMassMatrix(mass, inertia);
            m_Body.SetPositionOrientation(node.Position + new Vector3(0, 1, 0), node.Orientation);
            m_Body.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Metal");
        }

        public override void Update()
        { }
    }
}
