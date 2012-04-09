using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects
{
    class Trigger : GameObject
    {
        //more than one? vector?
        public Tachycardia.Objects.Actions.BaseAction m_action;

        MogreNewt.Body m_Body;
        MogreNewt.ConvexCollision m_collision;

        public Trigger(string _shapename, Mogre.Vector3 _shapesize)
        {

            switch (_shapename)
            {
                case "box":
                    m_collision = new MogreNewt.CollisionPrimitives.Box(
                                    Core.Singleton.m_NewtonWorld,
                                    _shapesize,
                                    new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0, 0, 1)),
                                    Core.Singleton.GetUniqueBodyId()
                                    );
                    break;
                case "ellipsoid":
                    m_collision = new MogreNewt.CollisionPrimitives.Ellipsoid(
                                    Core.Singleton.m_NewtonWorld,
                                    _shapesize,
                                    new Mogre.Quaternion(new Mogre.Radian(1.57f), new Mogre.Vector3(0, 0, 1)),
                                    Core.Singleton.GetUniqueBodyId()
                                    );
                    break;
            }

            m_collision.IsTriggerVolume = true;
            m_Body = new MogreNewt.Body(Core.Singleton.m_NewtonWorld, m_collision, true);
            m_Body.UserData = this;
            m_Body.Type = (int)PhysicsManager.BodyTypes.TRIGGER;
            m_Body.MaterialGroupID = Core.Singleton.m_PhysicsManager.getMaterialID("Trigger");
        }

        public void SetPosition(Mogre.Vector3 position)
        {
            m_Body.SetPositionOrientation(position, Mogre.Quaternion.IDENTITY);
        }

        public override void Update()
        {
        }
    }
}
