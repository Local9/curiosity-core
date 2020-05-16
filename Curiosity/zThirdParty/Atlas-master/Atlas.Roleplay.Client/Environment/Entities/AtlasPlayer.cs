using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Interface.Modules;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment.Entities
{
    public class AtlasPlayer
    {
        [JsonIgnore] protected Player Player => Game.Player;
        [JsonIgnore] public AtlasUser User { get; set; }
        public string SteamId { get; set; }
        public int Handle { get; set; }
        [JsonIgnore] public int LocalHandle => Game.Player.Handle;
        public string Name { get; set; }
        [JsonIgnore] public AtlasEntity Entity { get; set; }
        [JsonIgnore] public AtlasCharacter Character { get; set; }
        [JsonIgnore] public SoundSystem Sound { get; set; }
        [JsonIgnore] public AnimationQueue AnimationQueue => Entity.AnimationQueue;
        [JsonIgnore] public CameraViewmodelQueue CameraQueue { get; set; }

        public AtlasPlayer(string steamId, AtlasEntity entity)
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

            API.SetPlayerModel(LocalHandle, (uint)model.Hash);

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