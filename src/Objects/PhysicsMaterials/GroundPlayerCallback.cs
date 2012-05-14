using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tachycardia.Objects.PhysicsMaterials
{
    class GroundPlayerCallback : MogreNewt.ContactCallback
    {
        public GroundPlayerCallback()
        {
            System.Console.WriteLine(": ");
        }

        public void killtest(Mogre.Vector3 position)
        {
            Mogre.OverlayManager.Singleton.GetByName("BrokenScreen").Show();
            Mogre.MaterialPtr mat = Mogre.MaterialManager.Singleton.GetByName("BrokenScreen");
            Mogre.TextureUnitState tus = mat.GetTechnique(0).GetPass(0).GetTextureUnitState(0);
            mat.GetTechnique(0).GetPass(0).SetSceneBlending(Mogre.SceneBlendType.SBT_TRANSPARENT_ALPHA);
            tus.SetAlphaOperation(Mogre.LayerBlendOperationEx.LBX_MODULATE, Mogre.LayerBlendSource.LBS_MANUAL, Mogre.LayerBlendSource.LBS_TEXTURE, 1);
            Core.Singleton.SoundDict.Play("die_01.wav", position);
        }

        public override void UserProcess(MogreNewt.ContactJoint contact, float timestep, int threadIndex)
        {
            if (contact.Body0.Type == (int)PhysicsManager.BodyTypes.PLAYER
                || contact.Body0.Type == (int)PhysicsManager.BodyTypes.NPC)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body0.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose.m_name == "fly" && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        if (controler.m_MainBody.Velocity.y < -8)
                            killtest( controler.m_MainBody.Position );
                        controler.ChangePoseTo("normal");
                        Console.WriteLine(controler.m_Pose.m_name);
                    }
                    controler.m_Onground = 0;
                }
            }
            else if (contact.Body1.Type == (int)PhysicsManager.BodyTypes.PLAYER
                || contact.Body1.Type == (int)PhysicsManager.BodyTypes.NPC)
            {
                Tachycardia.PlayerController controler = (Tachycardia.PlayerController)contact.Body1.UserData;
                if (controler != null)
                {
                    if (controler.m_Pose.m_name == "fly" && controler.m_jumpLimit < Core.m_FixedFPS / 2)
                    {
                        if (controler.m_MainBody.Velocity.y < -8)
                            killtest( controler.m_MainBody.Position );
                        controler.ChangePoseTo("normal");
                        Console.WriteLine(controler.m_Pose.m_name);
                    }
                    controler.m_Onground = 0;
                }
            }
            return;
        }
    }
}
