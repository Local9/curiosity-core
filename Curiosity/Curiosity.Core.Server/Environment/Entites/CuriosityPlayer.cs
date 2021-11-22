using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Newtonsoft.Json;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] public Player Player;
        [JsonIgnore] int StateBagHandler;

        public bool IsPassive = false;

        public CuriosityPlayer(Player player)
        {
            Player = player;
            StateBagHandler = AddStateBagChangeHandler(string.Empty, $"player:{player.Handle}", new Action<string, string, dynamic, int, bool>(OnStateChange));
        }

        private void OnStateChange(string key, string bag, dynamic bagValue, int reserved, bool replicated)
        {
            if (PluginManager.IsDebugging)
            {
                string msg = $"OnStateChange" +
                    $"key: {key}" +
                    $"bag: {bag}" +
                    $"reserved: {reserved}" +
                    $"replicated: {reserved}" +
                    $"bagValue: {JsonConvert.SerializeObject(bagValue)}";
                Logger.Debug(msg);
            }
        }
    }
}
