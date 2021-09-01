using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Server.Commands.Impl
{
    public class PedCommands : CommandContext
    {
        public override string[] Aliases { get; set; } = { "ped" };
        public override string Title { get; set; } = "Server Ped Commands";
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER };

        static Dictionary<int, DateTime> activePeds = new Dictionary<int, DateTime>();

        [CommandInfo(new[] { "p" })]
        public class WorldTextToSpeech : ICommand
        {
            public async void On(CuriosityUser user, Player player, List<string> arguments)
            {
                if (arguments.Count == 0)
                {
                    ChatManager.OnServerMessage(player, $"Missing argument.");
                    return;
                }

                int playerServerId = 0;

                if (!int.TryParse(arguments[0], out playerServerId))
                {
                    ChatManager.OnServerMessage(player, $"Player ID is incorrect.");
                    return;
                }

                Player playerToAttack = PluginManager.PlayersList[playerServerId];

                if (playerToAttack is null)
                {
                    ChatManager.OnServerMessage(player, $"Player was not found.");
                    return;
                }

                Vector3 offset = playerToAttack.Character.Position;
                offset.X = offset.X + 50f;

                int pedHandle = await CreateFxPed(offset);
                await BaseScript.Delay(100);

                int weapon = GetHashKey("WEAPON_KNIFE");

                GiveWeaponToPed(pedHandle, (uint)weapon, 1, true, true);

                TaskCombatPed(pedHandle, playerToAttack.Character.Handle, 0, 16);

                activePeds.Add(pedHandle, DateTime.UtcNow.AddMinutes(1));
            }
        }

        private static async Task<int> CreateFxPed(Vector3 pos)
        {
            int pedHash = GetHashKey("u_m_y_zombie_01");
            int pedHandle = CreatePed((int)PedType.PED_TYPE_MISSION, (uint)pedHash, pos.X, pos.Y, pos.Z, 0f, true, true);
            
            if (pedHandle == 0)
            {
                Logger.Debug($"Possible OneSync is Disabled");
                return -1;
            }

            DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

            while (!DoesEntityExist(pedHandle))
            {
                await BaseScript.Delay(0);

                if (maxWaitTime < DateTime.UtcNow) break;
            }

            if (!DoesEntityExist(pedHandle))
            {
                Logger.Debug($"Failed to create entity in timely manner.");
                return -1;
            }

            return pedHandle;
        }

        [TickHandler]
        private async Task OnMonitorPed()
        {
            if (activePeds.Count == 0)
            {
                await BaseScript.Delay(10000);
                return;
            }

            Dictionary<int, DateTime> copyPeds = new Dictionary<int, DateTime>(activePeds);
            foreach(KeyValuePair<int, DateTime> kvp in copyPeds)
            {
                int ped = kvp.Key;
                DateTime timeToDelete = kvp.Value;

                if (DateTime.UtcNow > timeToDelete)
                    goto DeletePed;

            DeletePed:
                if (DoesEntityExist(ped))
                {
                    DeleteEntity(ped);
                    copyPeds.Remove(ped);
                }
            }

            await BaseScript.Delay(10000);
        }

    }
}
