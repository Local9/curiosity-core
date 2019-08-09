using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Helper.Area;
using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    class SafeZone
    {
        static Client client = Client.GetInstance();

        static Vector3 pos1 = new Vector3(-1095.472f, -880.6858f, -1f);
        static Vector3 pos2 = new Vector3(-1033.078f, -840.2169f, 10f);

        static AreaBox areaBox = new AreaBox();
        static List<CitizenFX.Core.Vehicle> vehicles = new List<CitizenFX.Core.Vehicle>();
        static List<CitizenFX.Core.Player> playerPeds = new List<CitizenFX.Core.Player>();
        static int Opacity = 180;

        static bool IsInArea = false;
        static bool IsInOwnVehicle = false;

        public static void Init()
        {
            areaBox.Angle = 10f;
            areaBox.Pos1 = pos1;
            areaBox.Pos2 = pos2;
            areaBox.Identifier = $"{SpawnLocations.VespucciPD}";

            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action(OnEnter));
            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action(OnExit));

            client.RegisterTickHandler(IsInSafeZone);
        }

        static async Task SafeZonePeds()
        {
            while (IsInArea)
            {
                await Client.Delay(0);
                int PlayerHandle = Game.PlayerPed.Handle;
                Game.PlayerPed.Opacity = Opacity;

                if (Game.PlayerPed.IsInVehicle())
                {
                    //if (Game.PlayerPed.CurrentVehicle.Driver.Handle == Game.PlayerPed.Handle)
                    //{
                    //    if (!Game.PlayerPed.CurrentVehicle.PreviouslyOwnedByPlayer)
                    //        Client.CurrentVehicle = Game.PlayerPed.CurrentVehicle;
                    //}

                    Game.PlayerPed.CurrentVehicle.Opacity = Opacity;
                }

                try
                {
                    playerPeds = Client.players.ToList().Select(m => m).Where(m => areaBox.CoordsInside(m.Character.Position)).ToList();
                    playerPeds.Remove(Game.Player);

                    foreach (CitizenFX.Core.Player pedInSafeZone in playerPeds)
                    {
                        pedInSafeZone.Character.Opacity = Opacity;
                        API.SetEntityNoCollisionEntity(PlayerHandle, pedInSafeZone.Character.Handle, false);

                        if (pedInSafeZone.Character.IsInVehicle())
                        {
                            pedInSafeZone.Character.CurrentVehicle.Opacity = Opacity;
                            API.SetEntityNoCollisionEntity(PlayerHandle, pedInSafeZone.Character.CurrentVehicle.Handle, false);
                        }

                        if (Game.PlayerPed.IsInVehicle())
                        {
                            API.SetEntityNoCollisionEntity(Game.PlayerPed.CurrentVehicle.Handle, pedInSafeZone.Character.Handle, false);

                            if (pedInSafeZone.Character.IsInVehicle())
                                API.SetEntityNoCollisionEntity(Game.PlayerPed.CurrentVehicle.Handle, pedInSafeZone.Character.CurrentVehicle.Handle, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"SAFEZONE -> {ex.Message}");
                }
            }

            foreach (CitizenFX.Core.Player pedInSafeZone in Client.players)
            {
                pedInSafeZone.Character.ResetOpacity();
                API.SetEntityNoCollisionEntity(Game.PlayerPed.Handle, pedInSafeZone.Character.Handle, true);

                if (pedInSafeZone.Character.IsInVehicle())
                {
                    pedInSafeZone.Character.CurrentVehicle.ResetOpacity();
                    API.SetEntityNoCollisionEntity(Game.PlayerPed.Handle, pedInSafeZone.Character.CurrentVehicle.Handle, true);
                }

                if (Game.PlayerPed.IsInVehicle())
                {
                    API.SetEntityNoCollisionEntity(Game.PlayerPed.CurrentVehicle.Handle, pedInSafeZone.Character.Handle, true);

                    if (pedInSafeZone.Character.IsInVehicle())
                        API.SetEntityNoCollisionEntity(Game.PlayerPed.CurrentVehicle.Handle, pedInSafeZone.Character.CurrentVehicle.Handle, true);
                }
            }

            Game.PlayerPed.ResetOpacity();

            if (Game.PlayerPed.IsInVehicle())
                Game.PlayerPed.CurrentVehicle.ResetOpacity();

            client.DeregisterTickHandler(SafeZonePeds);
        }

        static async Task IsInSafeZone()
        {
            await Task.FromResult(0);
            if (Game.Player.IsAlive)
            {
                areaBox.Check();

                bool registeredTick = false;

                while (IsInArea)
                {
                    await Client.Delay(0);
                    if (!registeredTick)
                    {
                        client.RegisterTickHandler(SafeZonePeds);
                        registeredTick = true;
                    }
                    areaBox.Check();
                }
            }
        }

        //static async Task IsLeavingCar()
        //{
        //    await Task.FromResult(0);
        //    if (IsInOwnVehicle)
        //    {
        //        if (API.IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
        //        {
        //            API.NetworkFadeOutEntity(Client.CurrentVehicle.Handle, false, true);

        //            await Client.Delay(1000);

        //            Client.CurrentVehicle.Delete();

        //            Client.CurrentVehicle = null;
        //        }
        //    }
        //}

        public static void OnEnter()
        {
            IsInArea = true;
        }

        public static void OnExit()
        {
            IsInArea = false;
        }
    }
}
