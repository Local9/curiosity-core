using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.System.Client.Interface.Modules;
using Curiosity.System.Library.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Curiosity.System.Client.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] protected Player Player => Game.Player;
        [JsonIgnore] public CuriosityUser User { get; set; }
        public string SteamId { get; set; }
        public int Handle { get; set; }
        [JsonIgnore] public int LocalHandle => Game.Player.Handle;
        public string Name { get; set; }
        [JsonIgnore] public CuriosityEntity Entity { get; set; }
        [JsonIgnore] public CuriosityCharacter Character { get; set; }
        [JsonIgnore] public SoundSystem Sound { get; set; }
        [JsonIgnore] public AnimationQueue AnimationQueue => Entity.AnimationQueue;
        [JsonIgnore] public CameraViewmodelQueue CameraQueue { get; set; }

        public CuriosityPlayer(string steamId, CuriosityEntity entity)
        {
            SteamId = steamId;
            Entity = entity;
            Sound = new SoundSystem();
            CameraQueue = new CameraViewmodelQueue();
        }

        public void DisableHud()
        {
            API.DisplayRadar(false);

            HeadupDisplay.GetModule().IsDisabled = true;
        }

        public void EnableHud()
        {
            API.DisplayRadar(true);

            HeadupDisplay.GetModule().IsDisabled = false;
        }

        public async Task CommitModel(Model model)
        {
            model.Request();

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(10);
            }

            API.SetPlayerModel(LocalHandle, (uint) model.Hash);

            Entity.Id = Game.PlayerPed.Handle;
        }

        public void ShowNotification(string message)
        {
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentSubstringPlayerName(message);
            API.DrawNotification(false, true);
        }
    }
}