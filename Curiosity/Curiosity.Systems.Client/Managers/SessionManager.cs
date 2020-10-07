using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Environment.Entities.Modules.Impl;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Systems.Client.Managers
{
    public class SessionManager : Manager<SessionManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("session:reload", new EventCallback(metadata =>
            {
                var entity = Game.PlayerPed.Handle;
                var session = metadata.Find<int>(1);

                var decors = new EntityDecorModule
                {
                    Id = entity,
                };

                decors.Set("Session", session);

                Session.Reload();

                return this;
            }));
        }
    }
}