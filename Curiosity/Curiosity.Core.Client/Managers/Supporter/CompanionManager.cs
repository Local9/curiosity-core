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
using CitizenFX.Core.UI;

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

                await BaseScript.Delay(100);

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

                companionPed = await World.CreatePed(model, spawn, Game.PlayerPed.Heading);
                model.MarkAsNoLongerNeeded();

                companionPed.Task.ClearAll();
                companionPed.LeaveGroup();

                await companionPed.FadeIn();

                await BaseScript.Delay(100);

                companionPed.RelationshipGroup = Game.PlayerPed.RelationshipGroup;
                companionPed.NeverLeavesGroup = true;

                companionPed.Health = 200;
                companionPed.Armor = 100;
                companionPed.CanSufferCriticalHits = false;
                companionPed.CanRagdoll = false;
                companionPed.CanBeTargetted = false;
                companionPed.IsPersistent = true;

                Blip companionBlip = companionPed.AttachBlip();
                companionBlip.Color = BlipColor.Blue;
                companionBlip.Scale = 0.7f;
                companionBlip.Name = "Compainion";

                if (!companionPed.IsHuman) PlayAnimalVocalization(companionPed.Handle, 3, "BARK");

                PedGroup playerGroup = Game.PlayerPed.PedGroup;

                if (playerGroup is null)
                {
                    int playerGroupId = GetPedGroupIndex(Game.PlayerPed.Handle);
                    playerGroup = new PedGroup(playerGroupId);
                }

                playerGroup.FormationType = FormationType.Default;
                playerGroup.SeparationRange = 2.14748365E+09f; // inifinity

                playerGroup.Add(Game.PlayerPed, true);
                playerGroup.Add(companionPed, false);

                SetGroupFormationSpacing(playerGroup.Handle, 1f, 0.9f, 3f);

                SetPedCanTeleportToGroupLeader(companionPed.Handle, playerGroup.Handle, true);
                NotificationManager.GetModule().Info($"Your campanion will defend you.");

                SetPedToInformRespectedFriends(companionPed.Handle, 20f, 20);
                SetPedToInformRespectedFriends(Cache.PlayerPed.Handle, 20f, 20);

                Instance.AttachTickHandler(OnCompanionMasterTick);
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

                Dispose();
            }
        }

        private void Dispose()
        {
            Instance.DetachTickHandler(OnCompanionMasterTick);
        }

        private async Task OnCompanionMasterTick()
        {
            if (companionPed is null) Dispose();
            if (!companionPed.Exists()) Dispose();

            try
            {
                if (Game.PlayerPed.IsInVehicle() && !companionPed.IsInVehicle())
                {
                    await companionPed.FadeOut();
                    companionPed.Task.WarpIntoVehicle(Game.PlayerPed.CurrentVehicle, VehicleSeat.Any);

                    if (!companionPed.IsHuman)
                    {
                        await BaseScript.Delay(500);
                        PlayAnimalVocalization(companionPed.Handle, 3, "WHINE");
                        await BaseScript.Delay(500);
                        companionPed.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_ENTER, 1f, -1, AnimationFlags.StayInEndFrame);
                    }

                    await companionPed.FadeIn();
                }

                if (!Game.PlayerPed.IsInVehicle() && companionPed.IsInVehicle())
                {
                    companionPed.SetConfigFlag(292, false);
                    if (!companionPed.IsHuman)
                    {
                        await companionPed.FadeOut();
                        Vehicle vehicle = companionPed.CurrentVehicle;
                        companionPed.Position = vehicle.GetOffsetPosition(new Vector3(2f, 0f, 0f));
                        companionPed.Task.ClearAll();
                        await companionPed.FadeIn();
                    }
                }

                if (Game.PlayerPed.IsInVehicle() && companionPed.IsInVehicle())
                {
                    companionPed.SetConfigFlag(292, true);
                }

                if (companionPed.IsDead && Game.PlayerPed.IsInRangeOf(companionPed.Position, 2.5f) && !Game.PlayerPed.IsInVehicle())
                {
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to revive companion.");

                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        await companionPed.FadeOut();
                        await BaseScript.Delay(500);
                        companionPed.Resurrect();
                        companionPed.Health = 200;
                        companionPed.Armor = 100;
                        await companionPed.FadeIn();

                        PedGroup currentPedGroup = Cache.PlayerPed.PedGroup;
                        currentPedGroup.SeparationRange = 300f;

                        if (!currentPedGroup.Contains(Cache.PlayerPed)) currentPedGroup.Add(Cache.PlayerPed, true);
                        if (!currentPedGroup.Contains(companionPed)) currentPedGroup.Add(companionPed, false);

                        await BaseScript.Delay(500);
                    }
                }

                if (Game.PlayerPed.IsDead && companionPed.IsAlive && !Game.PlayerPed.IsInVehicle())
                {
                    while (companionPed.Position.Distance(Game.PlayerPed.Position) > 2f)
                    {
                        companionPed.Task.GoTo(Game.PlayerPed);
                        await BaseScript.Delay(500);
                    }
                    GameEventTigger.GetModule().Respawn(Cache.Player);
                    await BaseScript.Delay(3000);
                }

                if (!companionPed.IsInGroup)
                {
                    DeleteCurrentCompanion();
                    NotificationManager.GetModule().Info($"Your companion has left you.");
                }
            }
            catch (Exception ex)
            {
                DeleteCurrentCompanion();
                NotificationManager.GetModule().Error($"Companion has been removed.");
            }
        }
    }
}
