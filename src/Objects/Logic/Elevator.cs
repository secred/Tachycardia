using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;
using Tachycardia.Objects;


namespace Tachycardia.Objects
{
    class Elevator : GameObject
    {
        #region Fields

        public MogreNewt.Body m_Body;
        public Vector3 m_HeadOffset;
        public Entity m_Entity;
        public SceneNode m_Node;
        public float m_Direction=1;
        public int caunter = 0;
        #endregion

        #region Moving methods

        public void ElevatorControl()
        {
            if (m_Body.Position.y >= 30 && m_Direction == 1)
            {
                caunter++;
                //m_Direction = 0.1f;
                if (caunter >= 200)
                {
                    m_Direction = -1;
                    caunter = 0;
                }
            }
            if (m_Body.Position.y <= -5 && m_Direction == -1)
            {
                caunter++;
                if (caunter >= 1000)
                {
                    m_Direction = 1;
                    caunter = 0;
                }
            } /*if (m_Body.Position.y < -13 && m_Direction == -1)
                m_Direction = 1;*/
        }

        #endregion

        #region Constructors

        public Elevator(string meshName,float mass)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.1f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.SetPosition(0f, 0f, 0f);
            SetPhysics(m_Entity, m_Node, mass);
        }

        public Elevator(string meshName, float mass, Vector3 v, bool flaga)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.1f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.Rotate(new Mogre.Quaternion(new Mogre.Radian(Mogre.Math.RadiansToDegrees(20)), new Mogre.Vector3(0f, 1f, 0f)));
            m_Node.SetPosition(v.x, v.y, v.z);
            SetPhysics(m_Entity, m_Node, mass);
        }

        public Elevator(string meshName, Vector3 v)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.1f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.SetPosition(v.x, v.y, v.z);
            SetPhysics(m_Entity, m_Node);
        }

        #endregion

        public Elevator(string meshName, Vector3 v, bool flaga)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.1f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);
            m_Node.SetPosition(v.x, v.y, v.z);
            SetPhysics(m_Entity, m_Node);
        }

        public void SetPhysics(Mogre.Entity entity, Mogre.SceneNode node, float mass)
        {
            MogreNewt.ConvexCollision collision = new MogreNewt.CollisionPrimitives.Box(
                Core.Singleton.NewtonWorld,
                Core.Singleton.PhysicsManager.getCollisionBoxSize(entity, node),
               // Core.Singleton.PhysicsManager.getCollisionCylinderRadius(entity, node),
               // Core.Singleton.PhysicsManager.getCollisionCylinderHeight(entity, node),
               // new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0f, 0f, 1f)),
                Core.Singleton.GetUniqueBodyId()
                );

            Mogre.Vector3 inertia, offset;
            collision.CalculateInertialMatrix(out inertia, out offset);
            inertia *= mass;

            m_Body = new MogreNewt.Body(Core.Singleton.NewtonWorld, collision, true);
            m_Body.AttachNode(node);
            m_Body.SetMassMatrix(mass, inertia);
            m_Body.SetPositionOrientation( node.Position + new Vector3(0,1,0) , node.Orientation);
            m_Body.MaterialGroupID = Core.Singleton.PhysicsManager.getMaterialID("Ground");
        }


        public void SetPhysics(Mogre.Entity entity, Mogre.SceneNode node)
        {
            MogreNewt.ConvexCollision collision = new MogreNewt.CollisionPrimitives.Cylinder(
                Core.Singleton.NewtonWorld,
                Core.Singleton.PhysicsManager.getCollisionCylinderRadius(entity, node),
                Core.Singleton.PhysicsManager.getCollisionCylinderHeight(entity, node),
                new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0f, 0f, 1f)),
                Core.Singleton.GetUniqueBodyId()
                );

            Mogre.Vector3 inertia, offset;
            collision.CalculateInertialMatrix(out inertia, out offset);
            

            m_Body = new MogreNewt.Body(Core.Singleton.NewtonWorld, collision, true);
            m_Body.AttachNode(node);
            
            m_Body.SetPositionOrientation(node.Position + new Vector3(0, 1, 0), node.Orientation);
            m_Body.MaterialGroupID = Core.Singleton.PhysicsManager.getMaterialID("Ground");
        }

        public override void Update()
        { }

        public void CallBack(MogreNewt.Body body, float tomeStep, int index)
        {
            ElevatorControl();
            body.AddForce(new Vector3(0f, m_Direction * 3f, 0f) * body.Mass);
            
        }
    }
}
