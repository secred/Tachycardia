using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia
{
    class PhysicsManager
    {
        public enum BodyTypes
        {
            PLAYER,
            TRIGGER
        };

        Dictionary<string, MogreNewt.MaterialID> m_Materials;
        Dictionary<string, MogreNewt.MaterialPair> m_MaterialsPair;
        Dictionary<string, MogreNewt.ContactCallback> m_PairCallback;

        public PhysicsManager()
        {
            m_Materials = new Dictionary<string, MogreNewt.MaterialID>();
            m_MaterialsPair = new Dictionary<string, MogreNewt.MaterialPair>();
            m_PairCallback = new Dictionary<string, MogreNewt.ContactCallback>();
        }

        public void addMaterial(string mat_name)
        {
            //PROFILER__WORLDMANAGER_ADDMATERIAL
            m_Materials.Add(mat_name, new MogreNewt.MaterialID(Core.Singleton.m_NewtonWorld));
        }

        public MogreNewt.MaterialID getMaterialID(string mat_name)
        {
            return m_Materials[mat_name];
        }

        public void addMaterialPair(string mat_name0, string mat_name1)
        {
            //PROFILER__WORLDMANAGER_ADDMATERIALPAIR
            m_MaterialsPair.Add(mat_name0 + mat_name1, new MogreNewt.MaterialPair(Core.Singleton.m_NewtonWorld, m_Materials[mat_name0], m_Materials[mat_name1]));
        }

        public MogreNewt.MaterialPair getMaterialPair(string pair_name)
        {
            //PROFILER__WORLDMANAGER_GETMATERIALPAIR
            return m_MaterialsPair[pair_name];
        }

        public void setPairCallback(string pair_name, string callback_class)
        {
            //PROFILER__WORLDMANAGER_SETPAIRCALLBACK
            if (callback_class == "metalCallback")
            {
                m_PairCallback[pair_name] = new Tachycardia.Objects.PhysicsMaterials.MetalMetalCallback();
                m_MaterialsPair[pair_name].SetContactCallback(m_PairCallback[pair_name]);
            }
            else if (callback_class == "TriggerCallback")
            {
                m_PairCallback[pair_name] = new Tachycardia.Objects.PhysicsMaterials.TriggerCallback();
                m_MaterialsPair[pair_name].SetContactCallback(m_PairCallback[pair_name]);
            }
        }


        public Mogre.Vector3 getCollisionBoxSize(Mogre.Entity entity, Mogre.SceneNode Node)
        {
            //PROFILER__LOGICCOMPONENT_GETCOLLISIONBOXSIZE
            return Node.GetScale() * entity.BoundingBox.Size;
        }

        public Mogre.Vector3 getCollisionEllipsoidSize(Mogre.Entity entity, Mogre.SceneNode Node)
        {
            //PROFILER__LOGICCOMPONENT_GETCOLLISIONELLIPSOIDSIZE

            return Node.GetScale() * entity.BoundingBox.Size / 2;
        }

        public float getCollisionCylinderRadius(Mogre.Entity entity, Mogre.SceneNode Node)
        {
            //PROFILER__LOGICCOMPONENT_GETCOLLISIONCYLINDERRADIUS
            Mogre.Vector3 size = Node.GetScale() * entity.BoundingBox.Size / 2;
            return size.x;
        }

        public float getCollisionCylinderHeight(Mogre.Entity entity, Mogre.SceneNode Node)
        {
            //PROFILER__LOGICCOMPONENT_GETCOLLISIONCYLINDERHEIGHT
            Mogre.Vector3 size = Node.GetScale() * entity.BoundingBox.Size;
            return size.y;
        }


    }
}

