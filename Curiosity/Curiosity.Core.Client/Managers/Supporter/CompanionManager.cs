using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using ClientUtility = Curiosity.Core.Client.Utils.Utility;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Exceptions;
using Curiosity.Core.Client.Diagnostics;

namespace Curiosity.Core.Client.Managers.Supporter
{
    public class CompanionManager : Manager<CompanionManager>
    {
        #region Animations
        const string ANIM_DICT_IDLE = "idle_a";
        const string ANIM_RETRIEVER_IDLE = "creatures@retriever@amb@world_dog_sitting@idle_a";
        const string ANIM_ROTTWEILER_IDLE = "creatures@rottweiler@amb@world_dog_sitting@idle_a";
        const string ANIM_DICT_MOVE = "creatures@rottweiler@move";
        const string ANIM_DICT_TRICKS = "creatures@rottweiler@tricks@";
        const string ANIM_PETTING_PLAYER = "petting_franklin";
        const string ANIM_PETTING_PET = "petting_chop";
        const string ANIM_SIT_ENTER = "sit_enter";
        const string ANIM_SIT_LOOP = "sit_loop";
        const string ANIM_SIT_EXIT = "sit_exit";
        #endregion

        WeaponHash thrownProjectile;
        WeaponHash weaponBall = WeaponHash.Ball;

        static Ped companionPed;

        int companionState = 0;

        public override void Begin()
        {
            
        }

        public async void CreateCompanion(string companionHash)
        {
            try
            {
                await DeleteCurrentCompanion();

                if (companionPed is not null) throw new CitizenFxException($"Current companion has not been removed.");

                Model model = await ClientUtility.LoadModel(companionHash);
                Vector3 offset = new Vector3(2f, 0f, 0f);
                Vector3 spawn = Cache.PlayerPed.GetOffsetPosition(offset);
                float groundZ = spawn.Z;
                Vector3 groundNormal = Vector3.Zero;

                if (GetGroundZAndNormalFor_3dCoord(spawn.X, spawn.Y, spawn.Z, ref groundZ, ref groundNormal))
                {
                    spawn.Z = groundZ;
                }

                companionPed = await World.CreatePed(model, spawn, Cache.PlayerPed.Heading);
                await companionPed.FadeIn();

                companionPed.RelationshipGroup = Instance.PlayerRelationshipGroup;
                companionPed.NeverLeavesGroup = true;

                companionPed.CanSufferCriticalHits = false;
                companionPed.CanBeTargetted = false;
                companionPed.IsPersistent = true;

                Blip companionBlip = companionPed.AttachBlip();
                companionBlip.Color = BlipColor.Blue;
                companionBlip.Scale = 0.7f;
                companionBlip.Name = "Compainion";

                PedGroup playerGroup = Cache.PlayerPed.PedGroup;
                playerGroup.FormationType = FormationType.Default;
                playerGroup.SeparationRange = 2f;

                SetPedCanTeleportToGroupLeader(companionPed.Handle, playerGroup.Handle, true);
                NotificationManager.GetModule().Info($"Your campanion will defend you.");
            }
            catch (CitizenFxException cfxEx)
            {
                NotificationManager.GetModule().Error(cfxEx.Message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Create Companion");
            }
        }

        public async Task DeleteCurrentCompanion()
        {
            if (companionPed is not null)
            {
                if (companionPed.Exists())
                {
                    Blip companionBlip = companionPed.AttachedBlip;
                    if (companionBlip is not null)
                    {
                        if (companionBlip.Exists()) companionBlip.Delete();
                    }

                    companionPed.LeaveGroup();
                    companionPed.IsPersistent = false;

                    if (companionPed.IsHuman) companionPed.PlayAmbientSpeech("GENERIC_BYE", SpeechModifier.Standard);

                    await BaseScript.Delay(500);
                    await companionPed.FadeOut();
                    EventSystem.Send("entitiy:delete", companionPed.NetworkId);
                    if (companionPed.Exists()) companionPed.Delete();
                    companionPed = null;
                }
            }
        }
    }
}
