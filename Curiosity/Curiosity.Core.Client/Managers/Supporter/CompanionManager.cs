using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Exceptions;
using Curiosity.Core.Client.Extensions;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using ClientUtility = Curiosity.Core.Client.Utils.Utility;

namespace Curiosity.Core.Client.Managers.Supporter
{
    public class CompanionManager : Manager<CompanionManager>
    {
        const string ANIM_DICT_TRICKS = "creatures@rottweiler@tricks@";
        const string ANIM_SIT_ENTER = "sit_enter";

        Ped companionPed;

        public override void Begin()
        {
            
        }

        public async void CreateCompanion(string companionHash)
        {
            try
            {
                await DeleteCurrentCompanion();

                await BaseScript.Delay(1000);

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

                SetNetworkIdExistsOnAllMachines(companionPed.NetworkId, true);
                SetNetworkIdCanMigrate(companionPed.NetworkId, true);

                companionPed.Task.ClearAll();

                await companionPed.FadeIn();

                await BaseScript.Delay(100);

                companionPed.Health = 200;
                companionPed.Armor = 100;
                companionPed.CanSufferCriticalHits = false;
                companionPed.CanRagdoll = false;
                companionPed.CanBeTargetted = false;

                if (!companionPed.IsHuman) PlayAnimalVocalization(companionPed.Handle, 3, "BARK");

                await BaseScript.Delay(100);

                if (companionPed.IsInGroup)
                    companionPed.LeaveGroup();

                PedGroup playerPedGroup = Cache.PedGroup;

                if (playerPedGroup is not null)
                    Logger.Debug($"Current ped group is {playerPedGroup.Handle}");

                if (playerPedGroup is null)
                    playerPedGroup = new PedGroup();

                if (!playerPedGroup.Contains(Game.PlayerPed))
                {
                    playerPedGroup.Add(Game.PlayerPed, true);
                    Logger.Debug($"Added player as group leader");
                }

                if (!playerPedGroup.Contains(companionPed))
                {
                    playerPedGroup.Add(companionPed, false);
                    Logger.Debug($"Added companion as group member");
                }

                companionPed.RelationshipGroup = Game.PlayerPed.RelationshipGroup;
                companionPed.NeverLeavesGroup = true;

                await BaseScript.Delay(100);

                if (companionPed.PedGroup is not null)
                {
                    Logger.Debug($"Companion Group {companionPed.PedGroup.Handle}");
                    SetPedCanTeleportToGroupLeader(companionPed.Handle, playerPedGroup.Handle, true);
                }

                NotificationManager.GetModule().Info($"Your campanion will defend you.");

                Blip companionBlip = companionPed.AttachBlip();
                companionBlip.Color = BlipColor.Blue;
                companionBlip.Scale = 0.7f;
                companionBlip.Name = "Compainion";

                SetPedToInformRespectedFriends(companionPed.Handle, 20f, 20);
                SetPedToInformRespectedFriends(Game.PlayerPed.Handle, 20f, 20);

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
            Dispose();
            await BaseScript.Delay(100);
            if (companionPed is not null)
            {
                if (companionPed.Exists())
                {
                    Logger.Debug($"DeleteCurrentCompanion: Companion Ped Exists");
                    Blip companionBlip = companionPed.AttachedBlip;
                    if (companionBlip is not null)
                    {
                        if (companionBlip.Exists()) companionBlip.Delete();

                        Logger.Debug($"DeleteCurrentCompanion: Companion Ped Blip Removing");
                    }
                    await BaseScript.Delay(100);

                    companionPed.LeaveGroup();

                    if (companionPed.IsHuman) companionPed.PlayAmbientSpeech("GENERIC_BYE", SpeechModifier.Standard);

                    await BaseScript.Delay(500);
                    await companionPed.FadeOut();
                    await BaseScript.Delay(500);
                    companionPed.Delete();
                    companionPed.MarkAsNoLongerNeeded();
                    EventSystem.Send("entitiy:delete", companionPed.NetworkId);
                }
                await BaseScript.Delay(500);

                companionPed = null;
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

                        PedGroup currentPedGroup = Game.PlayerPed.PedGroup;
                        currentPedGroup.SeparationRange = 300f;

                        if (!currentPedGroup.Contains(Game.PlayerPed)) currentPedGroup.Add(Game.PlayerPed, true);
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
            }
            catch (Exception ex)
            {
                DeleteCurrentCompanion();
                NotificationManager.GetModule().Error($"Companion has been removed.");
            }
        }
    }
}
