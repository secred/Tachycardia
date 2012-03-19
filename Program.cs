using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mogre;

namespace Tachycardia
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.Singleton.Initialise();
            Core.Singleton.m_CurrentMap = new Map();
            Core.Singleton.m_CurrentMap.SetGraphicsMesh("Level.mesh");
            Core.Singleton.m_CurrentMap.SetCollisionMesh("LevelCol.mesh");

            CharacterProfile profile = new CharacterProfile();
            profile.m_BodyMass = 70;
            profile.m_BodyScaleFactor = new Vector3(1.5f, 1, 1.5f);
            profile.m_HeadOffset = new Vector3(0, 0.8f, 0);
            profile.m_MeshName = "Man.mesh";
            profile.m_WalkSpeed = 2.5f;

            Character player = new Character(profile);
            player.SetPosition(new Vector3(0, 2, 0));
            Core.Singleton.m_ObjectManager.Add(player);

            Core.Singleton.m_GameCamera.Character = player;
            Core.Singleton.m_GameCamera.Distance = 4;
            Core.Singleton.m_GameCamera.Angle = new Degree(20);

            Light light = Core.Singleton.m_SceneManager.CreateLight();
            light.Type = Light.LightTypes.LT_DIRECTIONAL;
            light.Direction = new Vector3(1, -3, 1).NormalisedCopy;
            light.DiffuseColour = new ColourValue(0.2f, 0.2f, 0.2f);

            Core.Singleton.m_SceneManager.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;

            while (true)
            {
                Quaternion rotation = new Quaternion();
                rotation.FromAngleAxis(new Degree(120 * Core.Singleton.m_TimeStep), Vector3.UNIT_Y);
                Core.Singleton.Update();

                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_ESCAPE))
                    break;

                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                    player.m_Orientation *= Vector3.UNIT_Z.GetRotationTo(new Vector3(0.01f, 0, 1.0f).NormalisedCopy);
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_D))
                    player.m_Orientation *= Vector3.UNIT_Z.GetRotationTo(new Vector3(-0.01f, 0, 1.0f).NormalisedCopy);
                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_W))
                    player.m_State = Character.CharacterState.WALK;
                else
                    player.m_State = Character.CharacterState.IDLE;

                if (Core.Singleton.m_Keyboard.IsKeyDown(MOIS.KeyCode.KC_F3))
                    Core.Singleton.m_NewtonDebugger.ShowDebugInformation();
                else
                    Core.Singleton.m_NewtonDebugger.HideDebugInformation();
            }
             
        }
    }
}
