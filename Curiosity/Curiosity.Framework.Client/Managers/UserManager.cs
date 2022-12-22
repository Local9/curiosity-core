using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Managers.Events;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using System.Drawing;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user => Session.User; // refactor
        bool _playerSpawned = false;
        Vehicle _vehicle;
        Vector3 _position = new Vector3(0, 0, 0);
        float _heading = 0f;

        Camera _camera;

        public async override void Begin()
        {
            Event("onResourceStop", new Action<string>(OnResourceStop));

            InternalGameEvents.PlayerJoined += OnPlayerJoined;

            _playerSpawned = Game.Player.State.Get("player:spawned") ?? false;
            if (!_playerSpawned)
            {
                Logger.Debug($"Player not spawned, waiting for spawn");
                GameInterface.Hud.FadeOut(0);
                await LoadTransition.OnUpAsync();
                ScreenInterface.DisableHud();
                ScreenInterface.CloseLoadingScreen();
                ScreenInterface.StartLoadingMessage("PM_WAIT");
            }
            

            // Get last position
            float x = GetResourceKvpFloat("pos:x");
            float y = GetResourceKvpFloat("pos:y");
            float z = GetResourceKvpFloat("pos:z");
            _position = new Vector3(x, y, z);
            _heading = GetResourceKvpFloat("pos:h");

            OnRequestCharactersAsync();
        }

        private async void OnPlayerJoined()
        {
            
        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            
            Logger.Debug($"Resource '{GetCurrentResourceName()}' Stopping.");

            Vector3 pos = Game.PlayerPed.Position;

            SetResourceKvpFloat("pos:x", pos.X);
            SetResourceKvpFloat("pos:y", pos.Y);
            SetResourceKvpFloat("pos:z", pos.Z - 1f);
            SetResourceKvpFloat("pos:h", Game.PlayerPed.Heading);

            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                Entity entity = vehicle.GetEntityAttachedTo();
                entity?.Delete();

                SetResourceKvpInt("vehicle:last:model", vehicle.Model.Hash);
            }

            if (_vehicle != null)
            {
                if (_vehicle.Exists())
                {
                    _vehicle.Delete();
                    Logger.Debug($"Vehicle Deleted");
                }
            }
        }

        public async Task OnRequestCharactersAsync()
        {
            User user = await EventDispatcher.Get<User>("user:active", Game.Player.ServerId);

            if (user is null)
            {
                Logger.Error($"No user was returned from the server");
                return;
            }

            Session.User = user;

            //CharacterCreatorManager characterCreatorManager = CharacterCreatorManager.GetModule();
            //characterCreatorManager.OnCreateNewCharacter(new CharacterSkin());

            RegisterCommand("paint", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (!Game.PlayerPed.IsInVehicle())
                {
                    Logger.Debug($"Player is not in a vehicle");
                    return;
                }

                if (args.Count == 0)
                {
                    Logger.Debug($"No color specified");
                    return;
                }

                string paint = args[0].ToString();

                if ("prim" != paint && "sec" != paint)
                {
                    Logger.Debug($"Invalid paint type specified: {paint}");
                    return;
                }

                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                vehicle.Mods.InstallModKit();

                int red = 0;
                int green = 0;
                int blue = 0;

                if (int.TryParse(args[1].ToString(), out int _red))
                    red = _red;

                if (int.TryParse(args[2].ToString(), out int _green))
                    green = _green;

                if (int.TryParse(args[3].ToString(), out int _blue))
                    blue = _blue;

                Color color = Color.FromArgb(255, red, green, blue);

                if ("prim" == paint)
                {
                    Logger.Debug($"Painting vehicle primary color to {color}");
                    vehicle.Mods.CustomPrimaryColor = color;
                    return;
                }
                if ("sec" == paint)
                {
                    Logger.Debug($"Setting secondary color to {color}");
                    vehicle.Mods.CustomSecondaryColor = color;
                    return;
                }

                Logger.Debug($"Invalid paint type specified");

            }), false);


            await Game.Player.ChangeModel(PedHash.FreemodeMale01);
            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.Position = _position;
            Game.PlayerPed.Heading = _heading;

            // Test Code
            int vehicleHash = GetResourceKvpInt("vehicle:last:model");

            if (vehicleHash == 0)
                vehicleHash = GetHashKey("pbus2");

            _vehicle = await World.CreateVehicle(vehicleHash, Game.PlayerPed.Position, Game.PlayerPed.Heading);

            if (_vehicle != null)
            {
                if (_vehicle.Exists())
                {
                    DecorSetInt(_vehicle.Handle, "Player_Vehicle", -1);
                    Game.PlayerPed.SetIntoVehicle(_vehicle, VehicleSeat.Driver);
                    Logger.Debug($"Vehicle Created");
                }
            }

            Screen.Fading.FadeIn(0);

            if (!_playerSpawned)
                await LoadTransition.OnDownAsync();

            Game.PlayerPed.Weapons.Give(WeaponHash.AdvancedRifle, 999, true, true);
            Game.PlayerPed.Weapons.Give(WeaponHash.Firework, 999, true, true);

            Game.Player.State.Set("player:spawned", true, true);

            Instance.AttachTickHandler(OnTestSomeShit);
        }

        int fov = 0;

        private async Task OnTestSomeShit()
        {
            if (Game.IsControlJustPressed(0, Control.MultiplayerInfo))
            {
                ScaleformUI.ScaleformUI.RankBarInstance.SetScores(0, 800, 40, 500, 1);
                await ScaleformUI.ScaleformUI.CountdownInstance.Start(hudColor: HudColor.HUD_COLOUR_ENEMY);
                ScaleformUI.ScaleformUI.RankBarInstance.SetScores(0, 800, 500, 500, 1);
            }   
        }
    }
}
