﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Utils;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.Interactions.VehicleInteractions;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net.Enums.Patrol;
using Curiosity.Shared.Client.net.Extensions;
using System.Collections.Generic;

namespace Curiosity.Missions.Client.Scripts.Interactions.PedInteractions
{
    class ArrestInteractions
    {
        static public async void InteractionArrestInit(InteractivePed interactivePed)
        {
            List<string> resp = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!" };

            int resistExitChance = Utility.RANDOM.Next(30);

            if (interactivePed.IsUnderTheInfluence)
            {
                resistExitChance = Utility.RANDOM.Next(15, 30);
            }

            if (interactivePed.IsCarryingStolenItems)
            {
                resistExitChance = Utility.RANDOM.Next(25, 30);
            }

            if (interactivePed.Ped.IsInVehicle())
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Out of the vehicle! Now!");
                if (resistExitChance >= 25)
                {
                    Wrappers.Helpers.ShowSuspectSubtitle(resp[Utility.RANDOM.Next(resp.Count)]);
                    await PluginManager.Delay(1000);
                    interactivePed.Ped.CurrentVehicle.TrafficStopVehicleFlee(interactivePed.Ped);
                }
                else
                {
                    while (interactivePed.Ped.IsInVehicle())
                    {
                        interactivePed.Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                        await PluginManager.Delay(100);
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
                    Wrappers.Helpers.ShowSuspectSubtitle(resp[Utility.RANDOM.Next(resp.Count)]);
                    interactivePed.Ped.Weapons.Give(WeaponHash.Pistol, 10, true, true);
                    interactivePed.Ped.DropsWeaponsOnDeath = false;
                    interactivePed.Ped.Task.ShootAt(Game.PlayerPed);
                    await PluginManager.Delay(5000);
                    interactivePed.Ped.Task.FleeFrom(Game.PlayerPed);
                }
                else if (resistExitChance >= 25)
                {
                    Wrappers.Helpers.ShowSuspectSubtitle(resp[Utility.RANDOM.Next(resp.Count)]);
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
            await PluginManager.Delay(0);
            API.SetBlockingOfNonTemporaryEvents(ped.Handle, true);
            ped.CanRagdoll = true;
            ped.Task.PlayAnimation("random@arrests", "idle_2_hands_up", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await PluginManager.Delay(4000);
            ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_idle", 8.0f, -1, AnimationFlags.StayInEndFrame);
            ped.Weapons.RemoveAll();
            ped.Task.PlayAnimation("random@arrests@busted", "enter", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await PluginManager.Delay(500);
            ped.Task.PlayAnimation("random@arrests@busted", "idle_a", 8.0f, -1, (AnimationFlags)9);
        }

        static async void AnimationPedFreed(Ped ped)
        {
            await PluginManager.Delay(0);
            ped.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await PluginManager.Delay(2000);
            ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
            await PluginManager.Delay(3000);
            ped.Task.ClearSecondary();
            ped.CanRagdoll = true;
            PluginManager.TriggerEvent("curiosity:setting:group:leave", ped.Handle, Game.PlayerPed.PedGroup.Handle);
        }

        static public async void InteractionHandcuff(InteractivePed interactivePed)
        {
            await PluginManager.Delay(0);

            if (interactivePed == null) return;

            if (interactivePed.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (!interactivePed.IsHandcuffed) // if kneeling... then cuff them
            {
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                interactivePed.Ped.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);
                float position = interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "idle_a") ? 0.3f : 0.65f;
                API.AttachEntityToEntity(interactivePed.Ped.Handle, Game.PlayerPed.Handle, 11816, 0.0f, position, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await PluginManager.Delay(2000);
                interactivePed.Detach();
                Game.PlayerPed.Task.ClearSecondary();

                if (interactivePed.Ped.IsPlayingAnim("random@arrests@busted", "idle_a"))
                {
                    interactivePed.Ped.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
                    await PluginManager.Delay(1000);
                    interactivePed.Ped.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
                }

                API.SetEnableHandcuffs(interactivePed.Ped.Handle, true);

                interactivePed.IsHandcuffed = true;
                Game.PlayerPed.IsPositionFrozen = false;

                PluginManager.TriggerEvent("curiosity:interaction:handcuffs", interactivePed.Ped.Handle, true);
                PluginManager.TriggerEvent("curiosity:setting:group:join", interactivePed.Ped.Handle);
            }
            else if (interactivePed != null)
            {
                Game.PlayerPed.IsPositionFrozen = true;
                Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                API.AttachEntityToEntity(interactivePed.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.65f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await PluginManager.Delay(2000);
                interactivePed.Detach();
                interactivePed.Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();
                interactivePed.Ped.Task.ClearAll();
                interactivePed.Ped.CanRagdoll = true;
                API.SetBlockingOfNonTemporaryEvents(interactivePed.Ped.Handle, false);
                Game.PlayerPed.IsPositionFrozen = false;
                interactivePed.IsHandcuffed = false;

                PluginManager.TriggerEvent("curiosity:interaction:handcuffs", interactivePed.Handle, false);
                PluginManager.TriggerEvent("curiosity:setting:group:leave", interactivePed.Ped.Handle);
            }
            else
            {
                Wrappers.Helpers.ShowSimpleNotification("~r~You must be looking at the suspect.");
                while (interactivePed.IsHandcuffed)
                {
                    await PluginManager.Delay(0);
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
                VehicleSeat seat = VehicleSeat.LeftRear;

                if (PluginManager.CurrentVehicle.IsSeatFree(VehicleSeat.RightRear))
                {
                    seat = VehicleSeat.RightRear;
                }
                else if (PluginManager.CurrentVehicle.IsSeatFree(VehicleSeat.Passenger))
                {
                    seat = VehicleSeat.Passenger;
                }
                else
                {
                    Screen.ShowNotification("~r~Unable to find a free seat.");
                    return;
                }

                while (!interactivePed.Ped.IsInVehicle())
                {
                    await PluginManager.Delay(500);
                    interactivePed.Ped.Task.EnterVehicle(PluginManager.CurrentVehicle, seat);
                }

                interactivePed.Ped.SetConfigFlag(292, true);
            }
        }

        internal static async void InteractionRelease(InteractivePed interactivePed)
        {
            if (PluginManager.speechType == SpeechType.NORMAL)
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Alright, you're free to go.");
            }
            else
            {
                Wrappers.Helpers.ShowOfficerSubtitle("Get out of here before I change my mind.");
            }
            List<string> DriverResponse = new List<string>() { "Okay, thanks.", "Thanks.", "Thank you officer, have a nice day!", "Thanks, bye!", "I'm free to go? Okay, bye!" };
            if (interactivePed.Attitude >= 50 && interactivePed.Attitude < 80)
            {
                DriverResponse = new List<string>() { "Alright.", "Okay.", "Good.", "Okay, bye.", "Okay, goodbye officer.", "Later.", "Bye bye.", "Until next time." };
            }
            else if (interactivePed.Attitude >= 80 && interactivePed.Attitude < 95)
            {
                DriverResponse = new List<string>() { "Bye, asshole...", "Ugh.. Finally.", "Damn cops...", "Until next time.", "Its about time, pig" };
            }
            await PluginManager.Delay(2000);
            Wrappers.Helpers.ShowSuspectSubtitle(DriverResponse[Utility.RANDOM.Next(DriverResponse.Count)]);

            await PluginManager.Delay(0);
            PluginManager.TriggerEvent("curiosity:interaction:released", interactivePed.Ped.Handle);
        }

        internal static async void InteractionIssueWarning(InteractivePed interactivePed)
        {
            List<string> OfficerResponse;
            List<string> DriverResponse = new List<string>() { "Thanks.", "Thank you officer.", "Okay, thank you.", "Okay, thank you officer.", "Thank you so much!", "Alright, thanks!", "Yay! Thank you!", "I'll be more careful next time!", "Sorry about that!" }; ;

            if (PluginManager.speechType == SpeechType.NORMAL)
            {
                OfficerResponse = new List<string>() { "You can go, but don't do it again.", "Don't make me pull you over again!", "Have a good day. Be a little more careful next time.", "I'll let you off with a warning this time." };
            }
            else
            {
                OfficerResponse = new List<string>() { "Don't do that again.", "Don't make me pull you over again!", "I'll let you go this time.", "I'll let you off with a warning this time." };
            }

            if (interactivePed.Attitude >= 50 && interactivePed.Attitude < 80)
            {
                DriverResponse = new List<string>() { "Thanks... I guess...", "Yeah, whatever.", "Finally.", "Ugh..", };
            }
            else if (interactivePed.Attitude >= 80 && interactivePed.Attitude < 95)
            {
                DriverResponse = new List<string>() { "Uh huh, bye.", "Yeah, whatever.", "Finally.", "Ugh..", "Prick." };
            }
            else if (interactivePed.Attitude >= 95)
            {
                DriverResponse = new List<string>() { "Troublesum said fuck you too buddy!", "Yea, well don't kill yourself trying" };
            }

            Wrappers.Helpers.ShowOfficerSubtitle(OfficerResponse[Utility.RANDOM.Next(OfficerResponse.Count)]);
            await PluginManager.Delay(2000);
            Wrappers.Helpers.ShowSuspectSubtitle(DriverResponse[Utility.RANDOM.Next(DriverResponse.Count)]);
            await PluginManager.Delay(2000);

            await PluginManager.Delay(0);
            PluginManager.TriggerEvent("curiosity:interaction:released", interactivePed.Ped.Handle);
        }
    }
}
