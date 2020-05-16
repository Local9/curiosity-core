using CitizenFX.Core;
using Curiosity.Tools.Server.net.Helpers;
using System;
using System.Linq;

namespace Curiosity.Tools.Server.net
{
    public class PlayerController : ServerAccessor
    {
        public PlayerController(Server server) : base(server)
        {
            Server.RegisterEventHandler("curiosity:Tools:Player:Bring", new Action<Player, int, float, float, float>(OnPlayerBring));
        }

        private void OnPlayerBring([FromSource] Player source, int serverId, float x, float y, float z)
        {
            try
            {
                var target = Players.FirstOrDefault(p => p.Handle == $"{serverId}");
                if (target == null)
                {
                    Log.Warn($"Player {source.Name} (net:{source.Handle}) tried to bring non-existent player from handle {serverId}.");
                    return;
                }

                Log.Verbose($"Player {source.Name} (net:{source.Handle}) brought player {target.Name} (net:{target.Handle})");
                target.TriggerEvent("curiosity:Tools:Player:Bring", int.Parse(source.Handle), x, y, z);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}
