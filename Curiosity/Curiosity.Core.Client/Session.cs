using Curiosity.Core.Client.Environment.Entities.Modules.Impl;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.Core.Client
{
    public class Session
    {
        public static bool HasJoinedSession { get; set; }
        public static bool CreatingCharacter { get; set; }
        public static int LastSession { get; set; }

        public static bool ForceLoaded = false;

        public static bool IsWanted => Game.Player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

        public static async Task Loading()
        {
            while (true)
            {
                if (ForceLoaded) goto DownWait;

                if (Cache.Player?.Character is not null && Cache.Entity is not null) goto DownWait;

                await BaseScript.Delay(1000);
            }

        DownWait:
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

            //var voice = VoiceChat.GetModule();
            //voice.Channel = session;
            //voice.Commit();
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