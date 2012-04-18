using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mogre;
using MogreNewt;

namespace Tachycardia
{
    class IntroState : State
    {
        public override void Enter()
        {
            base.Enter();
            Init();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void Resume()
        {
            base.Resume();
            Init();
        }

        public override bool KeyPressed(MOIS.KeyEvent keyEventRef)
        {
            switch (keyEventRef.key)
            {
                case MOIS.KeyCode.KC_SPACE:
                    PushState("Game");
                    break;
                case MOIS.KeyCode.KC_ESCAPE:
                    if(Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_RSHIFT))
                        PopState();
                    break;
            }
            return true;
        }

        /*
         *  Urr, whatever. Sample usage of IntroState:
         **/

        private void Init()
        {
            Character player = (Character)Core.Singleton.m_ObjectManager.Find("player");
            //Character player = (Character)Core.Singleton.m_ObjectManager.Find("bot1");
            
            Core.Singleton.m_GameCamera.Character = player;
            Core.Singleton.m_GameCamera.Distance = 8;
            Core.Singleton.m_GameCamera.Angle = new Degree(20);
            Core.Singleton.m_GameCamera.Cam2();

            Core.Log("\n\nINTRO  INSTRUCTION:\n\nSPACE  -  Enter Game State\nESCAPE  -  Exit Application\n\n");

            angDest = -1;
            angTime = 0;
        }

        private int angDest;
        private int angTime;

        public override void Update()
        {
            if (angDest > 0)
            {
                Core.Singleton.m_GameCamera.Angle += new Degree(0.015f);
                Core.Singleton.m_GameCamera.Distance += 0.015f;
            }
            else
            {
                Core.Singleton.m_GameCamera.Angle -= new Degree(0.015f);
                Core.Singleton.m_GameCamera.Distance -= 0.015f;
            }
            angTime++;
            if (angTime > 400)
            {
                angDest = angDest > 0 ? -1 : 1;
                angTime = 0;
            }
        }
    }
}
