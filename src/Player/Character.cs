using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
    class Character : GameObject
    {
        public enum CharacterState
        {
            IDLE,
            WALK,
            BACKWALK,
            RUN
        };

        public Entity m_Entity;
        public SceneNode m_Node;
        public Body m_Body;

        public CharacterProfile m_Profile;
        public CharacterState m_State;

        public Quaternion m_Orientation;
        public Vector3 m_Velocity;

        public Character(CharacterProfile profile)
        {
            m_Profile = profile.Clone();

            m_Orientation = Quaternion.IDENTITY;

            m_Entity = Core.Singleton.m_SceneManager.CreateEntity(m_Profile.m_MeshName);
            m_Node = Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);

            Vector3 scaledSize = m_Entity.BoundingBox.HalfSize * m_Profile.m_BodyScaleFactor;

            ConvexCollision collision = new MogreNewt.CollisionPrimitives.Capsule(
                Core.Singleton.m_NewtonWorld,
                System.Math.Min(scaledSize.x, scaledSize.z),
                scaledSize.y * 2,
                Vector3.UNIT_X.GetRotationTo(Vector3.UNIT_Y),
                Core.Singleton.GetUniqueBodyId());

            Vector3 inertia, offset;
            collision.CalculateInertialMatrix(out inertia, out offset);
            inertia *= m_Profile.m_BodyMass;

            m_Body = new Body(Core.Singleton.m_NewtonWorld, collision, true);
            m_Body.AttachNode(m_Node);
            m_Body.SetMassMatrix(m_Profile.m_BodyMass, inertia);
            m_Body.AutoSleep = false;

            m_Body.Transformed += BodyTransformCallback;
            m_Body.ForceCallback += BodyForceCallback;

            Joint upVector = new MogreNewt.BasicJoints.UpVector(
            Core.Singleton.m_NewtonWorld, m_Body, Vector3.UNIT_Y);

            collision.Dispose();
        }

        void BodyTransformCallback(Body sender, Quaternion orientation, Vector3 position, int threadIndex)
        {
            m_Node.Position = position;
            m_Node.Orientation = m_Orientation;
        }

        public void BodyForceCallback(Body body, float timeStep, int threadIndex)
        {
            Vector3 force = (m_Velocity - m_Body.Velocity * new Vector3(1, 0, 1))
              * m_Profile.m_BodyMass * Core.m_FixedFPS;
            m_Body.AddForce(force);
        }

        public void SetPosition(Vector3 position)
        {
            m_Body.SetPositionOrientation(position, m_Orientation);
        }

        public override void Update()
        {
            AnimationState idleAnimation = m_Entity.GetAnimationState("Idle");
            AnimationState walkAnimation = m_Entity.GetAnimationState("Walk");

            float animationCorrector = 0.3f;

            switch (m_State)
            {
                case CharacterState.IDLE:
                    m_Profile.m_WalkSpeed = 0.0f;
                    m_Velocity = Vector3.ZERO;
                    walkAnimation.Enabled = false;
                    idleAnimation.Enabled = true;
                    idleAnimation.Loop = true;
                    idleAnimation.AddTime(Core.m_FixedTime * m_Profile.m_WalkSpeed * animationCorrector);
                    break;
                case CharacterState.WALK:
                    m_Profile.m_WalkSpeed = 3.0f;
                    m_Velocity = m_Orientation * Vector3.UNIT_Z * m_Profile.m_WalkSpeed;
                    idleAnimation.Enabled = false;
                    walkAnimation.Enabled = true;
                    walkAnimation.Loop = true;
                    walkAnimation.AddTime(Core.m_FixedTime * m_Profile.m_WalkSpeed * animationCorrector);
                    break;
                case CharacterState.RUN:
                    m_Profile.m_WalkSpeed = 8.0f;
                    m_Velocity = m_Orientation * Vector3.UNIT_Z * m_Profile.m_WalkSpeed;
                    idleAnimation.Enabled = false;
                    walkAnimation.Enabled = true;
                    walkAnimation.Loop = true;
                    walkAnimation.AddTime(Core.m_FixedTime * m_Profile.m_WalkSpeed * animationCorrector);
                    break;
            }
        }

    }
}
