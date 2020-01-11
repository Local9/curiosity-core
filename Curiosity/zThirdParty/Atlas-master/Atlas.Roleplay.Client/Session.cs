using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client
{
    public class Session
    {
        public static bool HasJoinedRP { get; set; }
        public static bool CreatingCharacter { get; set; }
        public static int LastSession { get; set; }

        public static async Task Loading()
        {
            while (true)
            {
                if (Cache.Player?.Character != null && Cache.Entity != null) break;

                await BaseScript.Delay(100);
            }

            await new CharacterManager.LoadTransition().DownWait();
        }

        public static bool IsSpawnHost()
        {
            return API.GetNumberOfPlayers() == 1;
        }

        public static void Join(int session)
        {
            var entity = Cache.Entity;
            var decors = new EntityDecorModule
            {
                Id = entity.Id,
                Entity = entity
            };

            decors.Set("Session", session);

            Reload(session);

            EventSystem.GetModule().Send("event:global", "session:reload", API.GetPlayerServerId(API.PlayerId()),
                session);

            LastSession = session;
        }

        public static void Reload(int session = 0)
        {
            var entity = Cache.Entity;

            if (session == 0)
            {
                var decors = new EntityDecorModule
                {
                    Id = entity.Id,
                    Entity = entity
                };

                session = decors.GetInteger("Session");
            }

            for (var i = 0; i < AtlasPlugin.MaximumPlayers; i++)
            {
                var ped = API.GetPlayerPed(i);

                if (!API.DoesEntityExist(ped) || ped == API.GetPlayerPed(-1)) continue;

                var decors = new EntityDecorModule
                {
                    Id = ped
                };

                Modify(ped, session == decors.GetInteger("Session"));
            }

            var voice = VoiceChat.GetModule();

            voice.Channel = session;
            voice.Commit();
        }

        private static void Modify(int ped, bool toggle)
        {
            API.SetEntityNoCollisionEntity(ped, API.GetPlayerPed(-1), toggle);
            API.SetEntityCollision(ped, toggle, toggle);
            API.SetPedCurrentWeaponVisible(ped, toggle, false, false, false);

            if (toggle) API.SetEntityLocallyVisible(ped);
            else API.SetEntityLocallyInvisible(ped);
        }
    }
}