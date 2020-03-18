using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Interface.Modules;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment.Entities
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
        [JsonIgnore] public CameraViewmodelQueue CameraQueue { get; set; }
        [JsonIgnore] public AnimationQueue AnimationQueue => Entity.AnimationQueue;

        public CuriosityPlayer(ulong discordId, CuriosityEntity entity)
        {
            DiscordId = discordId;
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
        }

        public void ShowNotification(string message, bool blinking = false)
        {
            Screen.ShowNotification(message, blinking);
        }
    }
}
