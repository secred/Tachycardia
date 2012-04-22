﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
    class GameState : State
    {
        Character m_Player;

        public override void Enter()
        {
            base.Enter();
            m_Player = (Character) Core.Singleton.m_ObjectManager.Find("player");
            //m_Player = (Character)Core.Singleton.m_ObjectManager.Find("bot1");
            Core.Singleton.m_GameCamera.Character = m_Player;
            Core.Singleton.m_GameCamera.Distance = 4;
            Core.Singleton.m_GameCamera.Angle = new Degree(20);
            Core.Singleton.m_GameCamera.Cam1();

            Core.Log("\n\nGAME  INSTRUCTION:\n\nW/A/D  -  Player controller\nW+LSHIFT  -  Run\nESCAPE  -  Exit Game\n\n");
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override bool KeyPressed(MOIS.KeyEvent keyEventRef)
        {
            switch (keyEventRef.key)
            {
                case MOIS.KeyCode.KC_ESCAPE:
                    PopState();
                    break;
                case MOIS.KeyCode.KC_1:
                    Core.Singleton.m_GameCamera.Cam1();
                    break;
                case MOIS.KeyCode.KC_2:
                    Core.Singleton.m_GameCamera.Cam2();
                    break;
                case MOIS.KeyCode.KC_3:
                    Core.Singleton.m_GameCamera.Cam3();
                    break;
                case MOIS.KeyCode.KC_4:
                    Core.Singleton.m_GameCamera.Cam4();
                    break;
                /* controler */
                case MOIS.KeyCode.KC_W:
                    m_Player.m_Control.ForwardButtonPressed();
                    break;
                case MOIS.KeyCode.KC_S:
                    m_Player.m_Control.BackwardButtonPressed();
                    break;
                case MOIS.KeyCode.KC_A:
                    m_Player.m_Control.LeftButtonPressed();
                    break;
                case MOIS.KeyCode.KC_D:
                    m_Player.m_Control.RightButtonPressed();
                    break;
                case MOIS.KeyCode.KC_LSHIFT:
                    m_Player.m_Control.BoostButtonPressed();
                    break;
                case MOIS.KeyCode.KC_SPACE:
                    m_Player.m_Control.JumpButtonPressed();
                    break;
                case MOIS.KeyCode.KC_C:
                    m_Player.m_Control.CrouchButtonPressed();
                    break;
                case MOIS.KeyCode.KC_X:
                    Console.WriteLine("Pozycja X = " + Core.Singleton.m_Camera.Position.x + " Y = " + Core.Singleton.m_Camera.Position.y + " Z  = " + Core.Singleton.m_Camera.Position.z);
                    break;

            }
            return true;
        }

        public override bool KeyReleased(MOIS.KeyEvent keyEventRef)
        {
            switch (keyEventRef.key)
            {
                /* Klawisze dla controlera */
                case MOIS.KeyCode.KC_W:
                    m_Player.m_Control.ForwardButtonReleased();
                    break;
                case MOIS.KeyCode.KC_S:
                    m_Player.m_Control.BackwardButtonReleased();
                    break;
                case MOIS.KeyCode.KC_A:
                    m_Player.m_Control.LeftButtonReleased();
                    break;
                case MOIS.KeyCode.KC_D:
                    m_Player.m_Control.RightButtonReleased();
                    break;
                case MOIS.KeyCode.KC_LSHIFT:
                    m_Player.m_Control.BoostButtonReleased();
                    break;
            }
            return true;
        }

        public override void Update()
        {

        }
            
    }
}
