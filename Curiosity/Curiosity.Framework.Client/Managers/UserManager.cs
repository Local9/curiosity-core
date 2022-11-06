using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user => Session.User; // refactor
        Vehicle _vehicle;
        Vector3 _position = new Vector3(0, 0, 0);
        float _heading = 0f;
        
        public async override void Begin()
        {
            Event("onResourceStop", new Action<string>(OnResourceStop));

            bool playerSpawned = Game.Player.State.Get("player:spawned") ?? false;
            if (!playerSpawned)
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

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            
            Logger.Debug($"Resource '{GetCurrentResourceName()}' Stopping.");

            Vector3 pos = Game.PlayerPed.Position;

            SetResourceKvpFloat("pos:x", pos.X);
            SetResourceKvpFloat("pos:y", pos.Y);
            SetResourceKvpFloat("pos:z", pos.Z);
            SetResourceKvpFloat("pos:h", Game.PlayerPed.Heading);

            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
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

            CharacterCreatorManager characterCreatorManager = CharacterCreatorManager.GetModule();
            // characterCreatorManager.OnCreateNewCharacter(new CharacterSkin());

            Game.Player.ChangeModel(PedHash.FreemodeMale01);
            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.Position = _position;
            Game.PlayerPed.Heading = _heading;

            LoadTransition.OnDownAsync();

            GameInterface.Hud.FadeIn(1000);
            ScreenInterface.EnableHud();

            await BaseScript.Delay(3000);

            int vehicleHash = GetResourceKvpInt("vehicle:last:model");
            
            if (vehicleHash == 0)
                vehicleHash = GetHashKey("tenf");

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

            Game.Player.State.Set("player:spawned", true, true);

            Logger.Info($"User Database: [{_user.Handle}] {_user.Username}#{_user.UserID} with {_user.Characters.Count} Character(s)");

            await BaseScript.Delay(10000);
        }
    }
}
