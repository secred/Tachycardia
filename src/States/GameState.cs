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

            }
            return true;
        }

        public override void Update()
        {
            /*
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_ESCAPE))
                Core.Singleton.Shutdown();

            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                m_Player.m_Orientation *= Vector3.UNIT_Z.GetRotationTo(new Vector3(5.0f * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_D))
                m_Player.m_Orientation *= Vector3.UNIT_Z.GetRotationTo(new Vector3(-5.0f * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);
            if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_W))
            {
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
                    m_Player.m_State = Character.CharacterState.RUN;
                else
                    m_Player.m_State = Character.CharacterState.WALK;
            }
            else
                m_Player.m_State = Character.CharacterState.IDLE;
*/
        }
            
    }
}
