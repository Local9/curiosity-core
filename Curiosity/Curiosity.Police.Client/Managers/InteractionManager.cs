using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Police.Client.Interface;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Police.Client.Managers
{
    internal class InteractionManager : Manager<InteractionManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("police:suspect:jail", new AsyncEventCallback(async metadata =>
            {
                await ScreenInterface.FadeOut(500);
                await BaseScript.Delay(1000);

                Game.PlayerPed.Position = new Vector3(1669.281f, 2565.229f, 45.56486f);
                PlaceObjectOnGroundProperly(Game.PlayerPed.Handle);

                await BaseScript.Delay(500);
                await ScreenInterface.FadeIn(2000);

                return null;
            }));
        }
    }
}
