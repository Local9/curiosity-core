using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Scripts.Interactions.VehicleInteractions;

namespace Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions
{
    class ArrestInteractions
    {
        static public async void InteractionArrestInit(InteractivePed interactivePed)
        {
            List<string> resp = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!" };

            int resistExitChance = Client.Random.Next(30);

            if (interactivePed.IsUnderTheInfluence)
            {
                resistExitChance = Client.Random.Next(15, 30);
            }

            if (interactivePed.IsCarryingStolenItems)
            {
                resistExitChance = Client.Random.Next(25, 30);
            }

            if (interactivePed.Ped.IsInVehicle())
            {
                Helpers.ShowOfficerSubtitle("Out of the vehicle! Now!");
                if (resistExitChance >= 25)
                {
                    Helpers.ShowDriverSubtitle(resp[Client.Random.Next(resp.Count)]);
                    await Client.Delay(1000);
                    interactivePed.Ped.CurrentVehicle.TrafficStopVehicleFlee(interactivePed.Ped);
                }
                else
                {
                    while (interactivePed.Ped.IsInVehicle())
                    {
                        interactivePed.Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                        await Client.Delay(100);
                    }
                    AnimationPedOnKnees(interactivePed.Ped);
                }
            }
            else if (
                       interactivePed.Ped.IsPlayingAnim("random@arrests", "idle_2_hands_up")
                       || interactivePed.Ped.IsPlayingAnim("random@arrests", "kneeling_arrest_idle")
                       || interactivePed.Ped.IsPlayingAnim("random@arrests", "kneeling_arrest_get_up")
                       || interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "enter")
                       || interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "exit")
                       || interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "exit")
                       )
            {
                // do nothing
            }
            else if (interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "idle_a"))
            {
                AnimationPedFreed(interactivePed.Ped);
            }
            else
            {
                if (resistExitChance >= 28)
                {
                    Helpers.ShowDriverSubtitle(resp[Client.Random.Next(resp.Count)]);
                    interactivePed.Ped.Weapons.Give(WeaponHash.Pistol, 10, true, true);
                    interactivePed.Ped.DropsWeaponsOnDeath = false;
                    interactivePed.Ped.Task.ShootAt(Game.PlayerPed);
                    await Client.Delay(5000);
                    interactivePed.Ped.Task.FleeFrom(Game.PlayerPed);
                }
                else if (resistExitChance >= 25)
                {
                    Helpers.ShowDriverSubtitle(resp[Client.Random.Next(resp.Count)]);
                    interactivePed.Ped.Task.FleeFrom(Game.PlayerPed);
                }
                else
                {
                    AnimationPedOnKnees(interactivePed.Ped);
                }
            }
        }

        static async void AnimationPedOnKnees(Ped ped)
        {
            await Client.Delay(0);
            API.SetBlockingOfNonTemporaryEvents(ped.Handle, true);
            ped.CanRagdoll = true;
            ped.Task.PlayAnimation("random@arrests", "idle_2_hands_up", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(4000);
            ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_idle", 8.0f, -1, AnimationFlags.StayInEndFrame);
            ped.Weapons.RemoveAll();
            ped.Task.PlayAnimation("random@arrests@busted", "enter", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(500);
            ped.Task.PlayAnimation("random@arrests@busted", "idle_a", 8.0f, -1, (AnimationFlags)9);
        }

        static async void AnimationPedFreed(Ped ped)
        {
            await Client.Delay(0);
            ped.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(2000);
            ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
            await Client.Delay(3000);
            ped.Task.ClearSecondary();
            ped.CanRagdoll = true;
        }

        static public async void InteractionHandcuff(InteractivePed interactivePed)
        {
            int playerGroupId = API.GetPedGroupIndex(Game.PlayerPed.Handle);
            await Client.Delay(0);

            if (interactivePed == null) return;

            if (interactivePed.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (!interactivePed.IsHandcuffed) // if kneeling... then cuff them
            {
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                interactivePed.Ped.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);
                float position = interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "idle_a") ? 0.3f : 0.65f;
                API.AttachEntityToEntity(interactivePed.Handle, Game.PlayerPed.Handle, 11816, 0.0f, position, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await Client.Delay(2000);
                interactivePed.Detach();
                Game.PlayerPed.Task.ClearSecondary();

                if (interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "idle_a"))
                {
                    interactivePed.Ped.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    await Client.Delay(1000);
                    interactivePed.Ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
                }

                API.SetPedAsGroupMember(interactivePed.Handle, playerGroupId);
                API.SetEnableHandcuffs(interactivePed.Handle, true);
                API.SetPedCanTeleportToGroupLeader(interactivePed.Handle, playerGroupId, true);

                interactivePed.IsHandcuffed = true;
                Game.PlayerPed.IsPositionFrozen = false;
            }
            else if (interactivePed != null)
            {
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                API.AttachEntityToEntity(interactivePed.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.65f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await Client.Delay(2000);
                interactivePed.Ped.LeaveGroup();
                interactivePed.Detach();
                interactivePed.Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();
                interactivePed.Ped.Task.ClearAll();
                interactivePed.Ped.CanRagdoll = false;
                API.SetBlockingOfNonTemporaryEvents(interactivePed.Handle, false);
                Game.PlayerPed.IsPositionFrozen = false;
                interactivePed.IsHandcuffed = false;
            }
            else
            {
                Helpers.ShowSimpleNotification("~r~You must be looking at the suspect.");
                while (interactivePed.IsHandcuffed)
                {
                    await Client.Delay(0);
                    if (interactivePed == null)
                    {
                        interactivePed.IsHandcuffed = false;
                    }
                    else
                    {
                        if (interactivePed.IsAlive && interactivePed.IsHandcuffed)
                        {
                            interactivePed.Ped.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);
                        }
                    }
                }
            }
        }

        static public async void InteractionPutInVehicle(InteractivePed interactivePed)
        {
            if (interactivePed.Ped.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (interactivePed.Ped.IsInVehicle())
            {
                interactivePed.Ped.SetConfigFlag(292, false);
                interactivePed.Ped.Task.LeaveVehicle();
                return;
            }
            else
            {
                if (Client.CurrentVehicle.PassengerCount >= 2)
                {
                    Screen.ShowNotification("~r~Too many passengers.");
                    return;
                }

                if (Client.CurrentVehicle.PassengerCount == 1)
                {
                    interactivePed.Ped.Task.EnterVehicle(Client.CurrentVehicle, VehicleSeat.RightRear);
                }
                else
                {
                    interactivePed.Ped.Task.EnterVehicle(Client.CurrentVehicle, VehicleSeat.LeftRear);
                }

                while (!interactivePed.Ped.IsInVehicle())
                {
                    await Client.Delay(0);
                }
                interactivePed.Ped.SetConfigFlag(292, true);
            }
        }
    }
}
