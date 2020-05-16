using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Missions.Client.net.Classes.PlayerClient;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Scripts
{
    class NpcHandler
    {
        public static bool IsMenuVisible = false;
        public static bool IsPerformingCpr = false;
        public static bool IsCoronerCalled = false;

        static Client client = Client.GetInstance();
        static ConcurrentDictionary<int, InteractivePed> NpcNetworkIds = new ConcurrentDictionary<int, InteractivePed>();

        static Ped recruitedPed;

        static public void Init()
        {
            client.RegisterTickHandler(OnNpcInteraction);

            client.RegisterTickHandler(OnPoliceNpcRecruit);
        }

        public async static Task OnPoliceNpcRecruit()
        {
            // Screen.ShowSubtitle($"Group Members: {Game.PlayerPed.PedGroup.MemberCount}");
            // List<Ped> peds = World.GetAllPeds().Select(x => x).Where(p => p.Position.Distance(Game.PlayerPed.Position) < 3f).ToList();

            if (!ClientInformation.IsTrusted()) return;

            Ped pedInFront = Game.PlayerPed.GetPedInFront();

            //if (pedInFront != null)
            //{

            //    if (
            //        pedInFront.Model.Hash != (int)PedHash.Cop01SFY
            //        || pedInFront.Model.Hash != (int)PedHash.Cop01SMY
            //        //|| pedInFront.Model.Hash != (int)PedHash.Ranger01SFY
            //        //|| pedInFront.Model.Hash != (int)PedHash.Ranger01SMY
            //        //|| pedInFront.Model.Hash != (int)PedHash.Sheriff01SFY
            //        //|| pedInFront.Model.Hash != (int)PedHash.Sheriff01SMY
            //        )
            //    {
            //        return;
            //    }
            //}

            if (pedInFront != null && !Game.PlayerPed.IsInVehicle())
            {
                if (
                    !Decorators.GetBoolean(pedInFront.Handle, Client.DECOR_NPC_ACTIVE_TRAFFIC_STOP)
                    && !Decorators.GetBoolean(pedInFront.Handle, Decorators.DECOR_GROUP_MEMBER)
                    && !Decorators.GetBoolean(pedInFront.Handle, Client.DECOR_PED_MISSION)
                    && !Decorators.GetBoolean(pedInFront.Handle, Decorators.DECOR_PED_INTERACTIVE))
                {
                    Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to recruit ped");

                    if (Game.IsControlJustPressed(0, Control.Context))
                    {
                        if (Game.PlayerPed.PedGroup.MemberCount == 1)
                        {
                            Screen.ShowNotification($"~r~Cannot recruit more");
                            return;
                        }

                        pedInFront.Recruit(Game.PlayerPed);

                        recruitedPed = pedInFront;

                        Screen.ShowNotification($"~g~Ped Recruited");
                        await Client.Delay(5000);
                        return;
                    }
                }
                
                if (Decorators.GetBoolean(pedInFront.Handle, Decorators.DECOR_GROUP_MEMBER))
                {
                    Ped ped = Game.PlayerPed.PedGroup.GetMember(0);

                    if (ped != null)
                    {
                        if (ped == recruitedPed)
                        {
                            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to remove ped");

                            if (Game.IsControlJustPressed(0, Control.Context))
                            {
                                pedInFront.LeaveParty();
                                Game.PlayerPed.LeaveGroup();
                                Decorators.Set(pedInFront.Handle, Decorators.DECOR_GROUP_MEMBER, false);
                                Screen.ShowNotification($"~g~Ped has left party");
                                await Client.Delay(5000);
                            }
                        }
                    }
                }
            }

            if (recruitedPed != null)
            {
                if (Game.PlayerPed.IsInVehicle() && !recruitedPed.IsInVehicle() && recruitedPed.Position.Distance(Game.PlayerPed.Position) > 50f)
                {
                    recruitedPed.Fade(true);
                    await Client.Delay(500);
                    recruitedPed.Task.WarpIntoVehicle(Game.PlayerPed.CurrentVehicle, VehicleSeat.Any);
                    await Client.Delay(500);
                    recruitedPed.Fade(false);

                    await Client.Delay(5000);
                }
            }
        }

        private static async Task OnNpcInteraction()
        {
            List<Ped> closePeds = World.GetAllPeds().Select(p => p).Where(x => x.Position.Distance(Game.PlayerPed.Position) < 50f).ToList();

            if (NpcNetworkIds.Count > 0)
            {
                List<Ped> copy = new List<Ped>(closePeds);

                foreach (Ped ped in copy)
                {
                    if (NpcNetworkIds.ContainsKey(ped.NetworkId))
                    {
                        if (ped.Position.Distance(Game.PlayerPed.Position) <= 20)
                        {
                            if (ped.Position.VDist(Game.PlayerPed.Position) <= 120f)
                            {
                                if (CanDisplayMenu(ped))
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu");

                                    if (Game.IsControlPressed(0, Control.Context))
                                    {
                                        Menus.PedInteractionMenu.MenuBase.Open(NpcNetworkIds[ped.NetworkId]);
                                    }
                                }

                                if (Game.PlayerPed.IsAiming && ped.IsAlive && !Game.PlayerPed.IsInVehicle())
                                {
                                    if (ped.Position.Distance(Game.PlayerPed.Position) > 2f && ped.Position.Distance(Game.PlayerPed.Position) <= 20f)
                                    {
                                        int entityHandle = 0;
                                        Ped pedBeingAimedAt = null;
                                        if (API.GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entityHandle))
                                        {
                                            if (entityHandle == 0) return;

                                            if (API.GetEntityType(entityHandle) == 1 && API.GetPedType(entityHandle) != 28)
                                            {
                                                pedBeingAimedAt = new Ped(entityHandle);
                                            }
                                        }

                                        if (pedBeingAimedAt != null)
                                        {
                                            if (ped == pedBeingAimedAt)
                                            {
                                                if (ped.IsInVehicle())
                                                {
                                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to exit their vehicle.");
                                                }
                                                else
                                                {
                                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to get on their knees.");
                                                }

                                                if (Game.IsControlJustPressed(0, Control.Context))
                                                {
                                                    ArrestInteractions.InteractionArrestInit(NpcNetworkIds[ped.NetworkId]);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void AddNpc(int networkId, InteractivePed interactivePed)
        {
            NpcNetworkIds.GetOrAdd(networkId, interactivePed);
        }

        public static void RemoveNpc(int networkId)
        {
            NpcNetworkIds.TryRemove(networkId, out InteractivePed p);
        }

        private static bool CanDisplayMenu(Ped ped)
        {
            try
            {
                if (!NpcNetworkIds.ContainsKey(ped.NetworkId)) return false;

                if (Menus.PedInteractionMenu.MenuBase.MainMenu != null)
                    IsMenuVisible = Menus.PedInteractionMenu.MenuBase.AnyMenuVisible();

                if (ped.IsInVehicle())
                {
                    return ped.Position.Distance(Game.PlayerPed.Position) <= 4f && !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle();
                }

                return ped.Position.Distance(Game.PlayerPed.Position) <= 2f && !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle();
            }
            catch (Exception ex)
            {
                Log.Error("InteractivePed -> CanDisplayMenu");
                return false;
            }
        }
    }
}
