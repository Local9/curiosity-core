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

    class SafeZonePlayer
    {
        Client client = Client.GetInstance();
        CitizenFX.Core.Player _player;

        public SafeZonePlayer(CitizenFX.Core.Player player)
        {
            _player = player;
            client.RegisterTickHandler(DisableCollision);
        }

        async Task DisableCollision()
        {
            await Client.Delay(0);

            _player.Character.Opacity = 180;

            if (_player.Character.IsInVehicle())
                _player.Character.CurrentVehicle.Opacity = 180;

            _player.Character.SetNoCollision(Game.PlayerPed, false);
            Game.Player.Character.SetNoCollision(_player.Character, false);

            if (_player.Character.IsInVehicle())
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    _player.Character.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, false);
                }
                else
                {
                    _player.Character.CurrentVehicle.SetNoCollision(Game.PlayerPed, false);
                }
            }
        }

        public void EnableCollision()
        {
            client.DeregisterTickHandler(DisableCollision);

            _player.Character.ResetOpacity();

            _player.Character.SetNoCollision(Game.PlayerPed, true);
            Game.Player.Character.SetNoCollision(_player.Character, true);

            if (_player.Character.IsInVehicle())
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    _player.Character.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
                }
                else
                {
                    _player.Character.CurrentVehicle.SetNoCollision(Game.PlayerPed, true);
                }
                _player.Character.CurrentVehicle.ResetOpacity();
            }
        }
    }

    class SafeZone
    {
        static Client client = Client.GetInstance();

        static Vector3 pos1 = new Vector3(-1095.472f, -880.6858f, -1f);
        static Vector3 pos2 = new Vector3(-1033.078f, -840.2169f, 10f);

        static AreaBox areaBox = new AreaBox();
        static int Opacity = 180;

        static Dictionary<int, SafeZonePlayer> safeZonePlayer = new Dictionary<int, SafeZonePlayer>();

        static bool IsInArea = false;

        static Dictionary<Vector3, AreaBox> safeZones = new Dictionary<Vector3, AreaBox>();

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
                    Game.PlayerPed.CurrentVehicle.Opacity = Opacity;
                }

                List<CitizenFX.Core.Player> playerPeds = Client.players.ToList().Select(m => m).Where(m => m.Character.Position.DistanceToSquared(Game.PlayerPed.Position) < 15f).ToList();

                foreach (CitizenFX.Core.Player pedInSafeZone in playerPeds)
                {
                    if (pedInSafeZone == Game.Player) continue;
                    if (safeZonePlayer.ContainsKey(pedInSafeZone.Handle)) continue;
                    safeZonePlayer.Add(pedInSafeZone.Handle, new SafeZonePlayer(pedInSafeZone));
                }
            }

            foreach (CitizenFX.Core.Player player in Client.players)
            {
                if (player == Game.Player) continue;
                if (safeZonePlayer.ContainsKey(player.Handle))
                {
                    safeZonePlayer[player.Handle].EnableCollision();
                    safeZonePlayer.Remove(player.Handle);
                };
            }

            Game.PlayerPed.ResetOpacity();

            if (Game.PlayerPed.IsInVehicle())
                Game.PlayerPed.CurrentVehicle.ResetOpacity();

            client.DeregisterTickHandler(SafeZonePeds);
            safeZonePlayer.Clear();
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
