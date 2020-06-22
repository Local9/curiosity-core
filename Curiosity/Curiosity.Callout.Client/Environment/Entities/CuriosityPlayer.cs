using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Callout.Client.Environment.Entities.Models;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callout.Client.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] protected Player Player => Game.Player;
        [JsonIgnore] public CuriosityUser User { get; set; }
        [JsonIgnore] public CuriosityEntity Entity { get; set; }
        [JsonIgnore] public CuriosityCharacter Character { get; set; }
        [JsonIgnore] public SoundSystem Sound { get; set; }
        [JsonIgnore] public int LocalHandle => Game.Player.Handle;
        public ulong DiscordId { get; set; }
        public int Handle { get; set; }
        public string Name { get; set; }
        public DateTime Joined { get { return User.DateCreated; } }
        public Role Role { get { return User.Role; } }
        public string RoleDescription { get { return $"{User.Role}"; } }
        [JsonIgnore] public CameraViewmodelQueue CameraQueue { get; set; }
        [JsonIgnore] public AnimationQueue AnimationQueue => Entity.AnimationQueue;

        public CuriosityPlayer(ulong discordId, CuriosityEntity entity)
        {
            DiscordId = discordId;
            Entity = entity;
            Sound = new SoundSystem();
            CameraQueue = new CameraViewmodelQueue();
        }

        public async Task CommitModel(Model model)
        {
            model.Request();

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(10);
            }

            API.SetPlayerModel(LocalHandle, (uint)model.Hash);
        }

        public void ShowNotification(string message, bool blinking = false)
        {
            Screen.ShowNotification(message, blinking);
        }
    }
}
