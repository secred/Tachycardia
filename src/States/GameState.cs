using System;
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
            m_Player = (Character) Core.Singleton.ObjectManager.Find("player");
            //m_Player = (Character)Core.Singleton.ObjectManager.Find("bot1");
            Core.Singleton.GameCamera.Character = m_Player;
            Core.Singleton.GameCamera.Distance = 4;
            Core.Singleton.GameCamera.Angle = new Degree(20);
            Core.Singleton.GameCamera.Cam1();

            Core.Singleton.SoundDict.PlayBGM();

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
                    Core.Singleton.GameCamera.Cam1();
                    break;
                case MOIS.KeyCode.KC_2:
                    Core.Singleton.GameCamera.Cam2();
                    break;
                case MOIS.KeyCode.KC_3:
                    Core.Singleton.GameCamera.Cam3();
                    break;
                case MOIS.KeyCode.KC_4:
                    Core.Singleton.GameCamera.Cam4();
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
                    Console.WriteLine("Pozycja X = " + Core.Singleton.Camera.Position.x + " Y = " + Core.Singleton.Camera.Position.y + " Z  = " + Core.Singleton.Camera.Position.z);
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
        
        public float camy;
        public override bool MouseMoved(MOIS.MouseEvent evt)
        {
            camy += (float)evt.state.Y.rel / 5.0f;
            //Core.Singleton.m_GameCamera.Angle = camy;
            Core.Singleton.m_GameCamera.orientation *= Mogre.Vector3.UNIT_Z.GetRotationTo(new Vector3((float)evt.state.X.rel / -5 * Core.m_FixedTime, 0, 1.0f).NormalisedCopy);

            Console.WriteLine("X" + evt.state.X.abs);
            Console.WriteLine("Z" + evt.state.Z.abs);

            return true;
        }
        public override void Update()
        {

        }
            
    }
}
