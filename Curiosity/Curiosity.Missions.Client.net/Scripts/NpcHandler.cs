using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.Interactions.PedInteractions;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.Scripts
{
    class NpcHandler
    {
        public static bool IsMenuVisible = false;
        public static bool IsPerformingCpr = false;
        public static bool IsCoronerCalled = false;

        static PluginManager PluginInstance => PluginManager.Instance;
        static ConcurrentDictionary<int, InteractivePed> NpcNetworkIds = new ConcurrentDictionary<int, InteractivePed>();

        static public void Init()
        {
            PluginInstance.RegisterTickHandler(OnNpcInteraction);
        }

        private static async Task OnNpcInteraction()
        {
            List<Ped> closePeds = World.GetAllPeds().Select(p => p).Where(x => x.Position.Distance(Game.PlayerPed.Position) < 20f).ToList();

            if (NpcNetworkIds.Count > 0)
            {
                List<Ped> copy = new List<Ped>(closePeds);

                foreach (Ped ped in copy)
                {
                    if (NpcNetworkIds.ContainsKey(ped.Handle))
                    {
                        if (ped.Position.Distance(Game.PlayerPed.Position) <= 10f)
                        {
                            if (ped.Position.VDist(Game.PlayerPed.Position) <= 100f)
                            {
                                if (CanDisplayMenu(ped))
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu");

                                    if (Game.IsControlPressed(0, Control.Context))
                                    {
                                        Menus.PedInteractionMenu.MenuBase.Open(NpcNetworkIds[ped.Handle]);
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
                                                    ArrestInteractions.InteractionArrestInit(NpcNetworkIds[ped.Handle]);
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

        public static void AddNpc(int handle, InteractivePed interactivePed)
        {
            NpcNetworkIds.GetOrAdd(handle, interactivePed);
        }

        public static void RemoveNpc(int handle)
        {
            NpcNetworkIds.TryRemove(handle, out InteractivePed p);
        }

        private static bool CanDisplayMenu(Ped ped)
        {
            try
            {
                if (!NpcNetworkIds.ContainsKey(ped.Handle)) return false;

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
