using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Scripts.PedCreators;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using CitizenFX.Core;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Helpers;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Shared.Client.net.Models;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Missions.Client.net.Scripts.Police
{
    class ArrestPed
    {
        static Client client = Client.GetInstance();
        
        static Ped ArrestedPed;
        static public Ped PedInHandcuffs;
        static public bool IsPedBeingArrested = false;
        static public bool IsPedCuffed = false;
        static public bool IsPedGrabbed = false;

        static List<BlipData> jailHouseBlips = new List<BlipData>();
        static List<Marker> jailHouseMarkers = new List<Marker>();

        public static async void Setup()
        {
            MarkerHandler.Init();
            MarkerHandler.HideAllMarkers = false;
            
            await Client.Delay(100);
            Static.Relationships.SetupRelationShips();

            await Client.Delay(100);
            // kill it incase it doubles
            client.DeregisterTickHandler(OnTaskArrestPed);
            client.RegisterTickHandler(OnTaskArrestPed);

            client.DeregisterTickHandler(OnTaskJailPed);
            client.RegisterTickHandler(OnTaskJailPed);

            await Client.Delay(100);
            SetupJailHouses();

            Screen.ShowNotification($"~b~Arrests~s~: ~g~Enabled");
        }

        public static void Dispose()
        {
            MarkerHandler.HideAllMarkers = true;
            MarkerHandler.Dispose();

            jailHouseBlips.ForEach(m => BlipHandler.RemoveBlip(m.BlipId));

            client.DeregisterTickHandler(OnTaskArrestPed);
            client.DeregisterTickHandler(OnTaskJailPed);
            Screen.ShowNotification("~b~Arrests~s~: ~r~Disabled");
        }

        static async void SetupJailHouses()
        {
            if (jailHouseMarkers.Count > 0)
                jailHouseMarkers.Clear();

            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 65, 105, 225);

            jailHouseMarkers.Add(new Marker("~b~Bolingbroke Penitentiary\n~w~Jail Arrested Ped", new Vector3(1690.975f, 2592.581f, 44.71336f), c));
            jailHouseMarkers.Add(new Marker("~b~Paleto Bay PD\n~w~Jail Arrested Ped", new Vector3(-449.3008f, 6012.623f, 30.71638f), c));
            jailHouseMarkers.Add(new Marker("~b~Vespucci PD\n~w~Jail Arrested Ped", new Vector3(-1113.08f, -848.6609f, 12.4414f), c));
            jailHouseMarkers.Add(new Marker("~b~Eastbourn PD\n~w~Jail Arrested Ped", new Vector3(-583.1123f, -146.7518f, 38.23016f), c));
            jailHouseMarkers.Add(new Marker("~b~La Mesa\n~w~Jail Arrested Ped", new Vector3(830.4728f, -1310.793f, 27.13673f), c));
            jailHouseMarkers.Add(new Marker("~b~Rancho\n~w~Jail Arrested Ped", new Vector3(370.3031f, -1608.2098f, 28.2919f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9393f, -1001.6194f, 23.9148f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9213f, -997.9607f, 23.9148f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(460.7617f, -994.2283f, 23.9148f), c));

            await Client.Delay(100);

            foreach(Marker m in jailHouseMarkers)
            {
                int id = jailHouseBlips.Count + 1;
                jailHouseBlips.Add(new BlipData(id, "Jail", m.Position, BlipSprite.Handcuffs, BlipCategory.Unknown, BlipColor.Blue, true));
                MarkerHandler.MarkersAll.Add(m);
            }

            foreach (BlipData b in jailHouseBlips)
            {
                BlipHandler.AddBlip(b);
            }
        }

        static async Task OnTaskJailPed()
        {
            await Task.FromResult(0);
            Marker marker = MarkerHandler.GetActiveMarker();
            if (marker == null)
            {
                await Client.Delay(500);
            }
            else
            {
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    if (ArrestedPed == null)
                    {
                        Screen.ShowNotification($"~o~You have no one to arrest, don't waste my time.");
                        return;
                    }

                    if (!IsPedCuffed)
                    {
                        List<string> vs = new List<string> { $"~o~WHY AREN'T THEY CUFFED!", "~o~Handcuff them you idoit!", "~r~WHAT IS YOUR MAJOR MALFUNCTION! PUT ON THE CUFFS!!!", "Cuff them, fecking muppet!" };
                        Screen.ShowNotification(vs[Client.Random.Next(vs.Count)]);
                        return;
                    }


                    ArrestedPedData arrestedPedData = new ArrestedPedData();
                    arrestedPedData.IsAllowedToBeArrested = TrafficStop.CanDriverBeArrested;

                    arrestedPedData.IsDrunk = TrafficStop.IsDriverUnderTheInfluence;
                    arrestedPedData.IsDrugged = TrafficStop.IsDriverUnderTheInfluenceOfDrugs;
                    arrestedPedData.IsDrivingStolenCar = TrafficStop.HasVehicleBeenStolen;
                    arrestedPedData.IsCarryingIllegalItems = TrafficStop.IsCarryingIllegalItems;


                }
            }
        }

        static async Task OnTaskArrestPed()
        {
            await Task.FromResult(0);
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                if (Game.PlayerPed.IsAiming)
                {
                    int entityHandle = 0;
                    if (API.GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entityHandle))
                    {
                        if (entityHandle == 0) return;

                        if (API.GetEntityType(entityHandle) == 1 && API.GetPedType(entityHandle) != 28)
                        {
                            ArrestedPed = new Ped(entityHandle);
                        }
                    }

                    if (ArrestedPed == null) return;
                    if (ArrestedPed.IsDead) return;

                    IsPedBeingArrested = true;

                    if (ArrestedPed.Position.Distance(Game.PlayerPed.Position) <= 15)
                    {
                        API.RequestAnimDict("mp_arresting");
                        Helpers.LoadAnimation("random@arrests");
                        Helpers.LoadAnimation("random@arrests@busted");

                        if (ArrestedPed.Handle == TrafficStop.StoppedDriver.Handle)
                        {
                            Helpers.ShowOfficerSubtitle("Out of the vehicle! Now!");
                            int resistExitChance = Client.Random.Next(30);

                            if (TrafficStop.IsDriverUnderTheInfluence)
                            {
                                resistExitChance = Client.Random.Next(15, 30);
                            }

                            if (TrafficStop.HasVehicleBeenStolen)
                            {
                                resistExitChance = Client.Random.Next(25, 30);
                            }

                            if (resistExitChance >= 25)
                            {
                                List<string> resp = new List<string>() { "No way!", "Fuck off!", "Not today!", "Shit!", "Uhm.. Nope.", "Get away from me!", "Pig!", "No.", "Never!" };
                                Helpers.ShowDriverSubtitle(resp[Client.Random.Next(resp.Count)]);
                                await Client.Delay(1000);
                                TrafficStop.TrafficStopVehicleFlee(TrafficStop.TargetVehicle, TrafficStop.StoppedDriver);
                            }
                            else
                            {
                                while (ArrestedPed.IsInVehicle())
                                {
                                    ArrestedPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                                    await Client.Delay(100);
                                }
                                // ARREST
                                ArrestingPed();
                            }
                        }
                        else
                        {
                            while (ArrestedPed.IsInVehicle())
                            {
                                ArrestedPed.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                                await Client.Delay(100);
                            }
                            // ARREST
                            ArrestingPed();
                        }
                    }
                    else if (
                        ArrestedPed.IsPlayingAnim("random@arrests", "idle_2_hands_up")
                        || ArrestedPed.IsPlayingAnim("random@arrests", "kneeling_arrest_idle")
                        || ArrestedPed.IsPlayingAnim("random@arrests", "kneeling_arrest_get_up")
                        || ArrestedPed.IsPlayingAnim("random@arrests@busted", "enter")
                        || ArrestedPed.IsPlayingAnim("random@arrests@busted", "exit")
                        || ArrestedPed.IsPlayingAnim("random@arrests@busted", "exit")
                        )
                    {
                        // do nothing
                    }
                    else if (ArrestedPed.IsPlayingAnim("random@arrests@busted", "idle_a"))
                    {
                        // free
                        FreeingPed();
                    }
                    else
                    {
                        // arrest
                        ArrestingPed();
                    }
                }
            }
        }

        static async void ArrestingPed()
        {
            await Client.Delay(0);
            if (ArrestedPed == null) return;

            API.SetBlockingOfNonTemporaryEvents(ArrestedPed.Handle, true);
            ArrestedPed.CanRagdoll = true;
            ArrestedPed.Task.PlayAnimation("random@arrests", "idle_2_hands_up", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(4000);
            ArrestedPed.Task.PlayAnimation("random@arrests", "kneeling_arrest_idle", 8.0f, -1, AnimationFlags.StayInEndFrame);
            ArrestedPed.Weapons.RemoveAll();
            ArrestedPed.Task.PlayAnimation("random@arrests@busted", "enter", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(500);
            ArrestedPed.Task.PlayAnimation("random@arrests@busted", "idle_a", 8.0f, -1, (AnimationFlags)9);
        }

        static async void FreeingPed()
        {
            await Client.Delay(0);
            if (ArrestedPed == null) return;
            ArrestedPed.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
            await Client.Delay(2000);
            ArrestedPed.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
            await Client.Delay(3000);
            ArrestedPed.Task.ClearSecondary();
            ArrestedPed.CanRagdoll = true;
        }

        // MENU INTERACTIONS
        // Handcuff
        static public async void InteractionHandcuff()
        {
            int playerGroupId = API.GetPedGroupIndex(Game.PlayerPed.Handle);
            await Client.Delay(0);
            PedInHandcuffs = ArrestedPed ?? Game.PlayerPed.GetPedInFront();
            if (PedInHandcuffs == null) return;

            if (PedInHandcuffs.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (PedInHandcuffs.IsPlayingAnim("random@arrests@busted", "idle_a")) // if kneeling... then cuff them
            {
                Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                PedInHandcuffs.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);
                API.AttachEntityToEntity(PedInHandcuffs.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.3f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await Client.Delay(2000);
                PedInHandcuffs.Detach();
                Game.PlayerPed.Task.ClearSecondary();
                PedInHandcuffs.Task.PlayAnimation("random@arrests@busted", "exit", 8.0f, -1, AnimationFlags.StayInEndFrame);
                await Client.Delay(1000);
                PedInHandcuffs.Task.PlayAnimation("random@arrests", "kneeling_arrest_get_up", 8.0f, -1, AnimationFlags.CancelableWithMovement);
                API.SetPedAsGroupMember(PedInHandcuffs.Handle, playerGroupId);
                API.SetEnableHandcuffs(PedInHandcuffs.Handle, true);
                API.SetPedCanTeleportToGroupLeader(PedInHandcuffs.Handle, playerGroupId, true);
                IsPedCuffed = true;
            }
            else if (PedInHandcuffs != null)
            {
                PedInHandcuffs.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                API.AttachEntityToEntity(PedInHandcuffs.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.65f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                await Client.Delay(2000);
                PedInHandcuffs.LeaveGroup();
                PedInHandcuffs.Detach();
                PedInHandcuffs.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();
                PedInHandcuffs.Task.ClearAll();
                PedInHandcuffs.CanRagdoll = false;
                API.SetBlockingOfNonTemporaryEvents(PedInHandcuffs.Handle, false);
            }
            else
            {
                Helpers.ShowSimpleNotification("~r~You must be looking at the suspect.");
                while (IsPedCuffed)
                {
                    await Client.Delay(0);
                    if (PedInHandcuffs == null)
                    {
                        IsPedCuffed = false;
                    }
                    else
                    {
                        if (PedInHandcuffs.IsAlive && IsPedCuffed)
                        {
                            PedInHandcuffs.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);
                        }
                    }
                }
            }
        }

        // Secure In Players Vehicle
        static public void InteractionPutInVehicle()
        {
            Ped ped = ArrestedPed ?? Game.PlayerPed.GetPedInFront();
            if (ped.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (ped.IsInVehicle()) return;

            ped.Task.EnterVehicle(Client.CurrentVehicle, VehicleSeat.Passenger);
        }
        // Remove from Players Vehicle
        static public void InteractionRemoveFromVehicle()
        {
            Ped ped = ArrestedPed ?? Game.PlayerPed.GetPedInFront();
            if (ped.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (!ped.IsInVehicle()) return;

            ped.Task.LeaveVehicle();
        }

        // Grab Ped
        static public void InteractionGrab()
        {
            Ped grabbedPed = ArrestedPed ?? Game.PlayerPed.GetPedInFront();

            if (grabbedPed.Position.Distance(Game.PlayerPed.Position) > 3) return;

            if (!IsPedGrabbed)
            {
                IsPedGrabbed = true;
                API.AttachEntityToEntity(grabbedPed.Handle, Game.PlayerPed.Handle, 11816, -0.3f, 0.4f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
            }
            else if (IsPedGrabbed)
            {
                grabbedPed.Detach();
                IsPedGrabbed = false;
            }
        }

        // Make Ped Kneel
        static void InteractionKneel()
        {

        }

    }
}
