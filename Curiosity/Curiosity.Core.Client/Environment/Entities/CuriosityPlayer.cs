using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface.Modules;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Environment.Entities
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

        public void DisableHud()
        {
            API.DisplayRadar(false);

            HeadupDisplay.GetModule().IsDisabled = true;
        }

        public void EnableHud()
        {
            API.DisplayRadar(true);

            HeadupDisplay.GetModule().IsDisabled = false;

            if (User.IsDeveloper)
            {
                NoClipManager.NoClipInstance.Init();
            }
            else
            {
                NoClipManager.NoClipInstance.Dispose();
            }

            PlayerOptions.SetPlayerPassive(Cache.Player.User.IsPassive);

            Session.ForceLoaded = true;
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
