using CitizenFX.Core.Native;
using Curiosity.Interface.Client.Environment.Entities.Modules.Impl;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Interface.Client.Managers
{
    public class SessionManager : Manager<SessionManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("session:reload", new EventCallback(metadata =>
            {
                var entity = API.GetPlayerPed(API.GetPlayerFromServerId(metadata.Find<int>(0)));
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