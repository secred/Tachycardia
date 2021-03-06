﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
    class Character : GameObject
    {
        /*w kontrolerze
        public enum CharacterState
        {
            IDLE,
            WALK,
            BACKWALK,
            RUN,
            CROUCH,
            CROUCH_RUN,
            CROUCH_BACK,
            CROUCH_IDLE,
            JUMP,
            CRAWL

        };
        */
        public Entity m_Entity;
        public SceneNode m_Node;
   //    public CharacterState m_State;

        //dostarczane przez kontroler
        //public Quaternion m_Orientation;
        
        public PlayerController m_Control;//kontroler
        public Vector3 m_HeadOffset;

        public Character(string meshName, float mass)
            : this(meshName, mass, false)
        {
        }

        public Character(string meshName, float mass/*CharacterProfile profile*/, bool npc)
        {//tworzy grafike playera i podczepia mu kontroler, obsluguje animacje i uaktualnia kontroler
            m_HeadOffset = new Vector3(0, 0.8f, 0);
            //headoffset powinien byc chyba zmienny dla croucha itp
            m_Entity = Core.Singleton.SceneManager.CreateEntity(meshName);
            m_Node = Core.Singleton.SceneManager.RootSceneNode.CreateChildSceneNode();
            m_Node.AttachObject(m_Entity);

            if (npc)
                m_Control = new NpcController(m_Node, m_Entity, mass);
            else
                m_Control = new PlayerController(m_Node, m_Entity, mass);
        }

        public void SetPosition(Vector3 position)
        {
            m_Control.m_MainBody.SetPositionOrientation(position, Mogre.Quaternion.IDENTITY);
        }

        public override void Update()
        {
            AnimationState idleAnimation = m_Entity.GetAnimationState("Idle");
            AnimationState walkAnimation = m_Entity.GetAnimationState("Walk");

            float animationCorrector = 0.3f;

            //tutaj sprawdzana jest informacja w jakim stanie obecnie znajduje sie player
            //odczytane z controlera
            switch (m_Control.m_State)
            {
                case PlayerController.CharacterState.IDLE:
                    walkAnimation.Enabled = false;
                    idleAnimation.Enabled = true;
                    idleAnimation.Loop = true;
                    idleAnimation.AddTime(Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector);
                    break;
                case PlayerController.CharacterState.WALK:
                    idleAnimation.Enabled = false;
                    walkAnimation.Enabled = true;
                    walkAnimation.Loop = true;
                    walkAnimation.AddTime(Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector);
                    if (walkAnimation.TimePosition / walkAnimation.Length > 0.5f
                        && (walkAnimation.TimePosition - (Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector)) / walkAnimation.Length < 0.5f
                        || (walkAnimation.TimePosition - (Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector)) / walkAnimation.Length < 0.0f)
                    {
                        Core.Singleton.SoundDict.Play("player/step_gravel_0" + new Random().Next(1, 4) + ".wav", m_Control.m_MainBody.Position);
                    }
                    break;
                case PlayerController.CharacterState.RUN:
                    idleAnimation.Enabled = false;
                    walkAnimation.Enabled = true;
                    walkAnimation.Loop = true;
                    walkAnimation.AddTime(Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector);
                    if (walkAnimation.TimePosition / walkAnimation.Length > 0.5f
                        && (walkAnimation.TimePosition - (Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector)) / walkAnimation.Length < 0.5f
                        || (walkAnimation.TimePosition - (Core.m_FixedTime * m_Control.m_MainBody.Velocity.Length * animationCorrector)) / walkAnimation.Length < 0.0f)
                    {
                        Core.Singleton.SoundDict.Play("player/step_gravel_0" + new Random().Next(1, 4) + ".wav", m_Control.m_MainBody.Position);
                    }
                    break;
            }
        }

    }
}
