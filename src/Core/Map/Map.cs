using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
    class Map
    {
        Entity m_GraphicsEntity;
        SceneNode m_GraphicsNode;

        Entity m_CollisionEntity;
        SceneNode m_CollisionNode;

        public Body m_Body;

        public void SetGraphicsMesh(String meshFile)
        {
            m_GraphicsNode =
                Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();
            m_GraphicsEntity = Core.Singleton.m_SceneManager.CreateEntity(meshFile);
            m_GraphicsNode.AttachObject(m_GraphicsEntity);
            m_GraphicsEntity.CastShadows = false;
        }

        public void SetCollisionMesh(String meshFile)
        {
            m_CollisionNode =
                Core.Singleton.m_SceneManager.RootSceneNode.CreateChildSceneNode();
            m_CollisionEntity = Core.Singleton.m_SceneManager.CreateEntity(meshFile);
            m_CollisionNode.AttachObject(m_CollisionEntity);

            m_CollisionNode.SetVisible(false);

             MogreNewt.CollisionPrimitives.TreeCollisionSceneParser collision = 
                 new MogreNewt.CollisionPrimitives.TreeCollisionSceneParser(
                Core.Singleton.m_NewtonWorld);
             collision.ParseScene(m_CollisionNode, true, 1);
             m_Body = new Body(Core.Singleton.m_NewtonWorld, collision);
            collision.Dispose();
            m_Body.AttachNode(m_CollisionNode);
        }
    }
}
