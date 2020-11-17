using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Particle", "devPart", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class Particle : Mission
    {
        public async override void Start()
        {
            Mission.CreateParticleAtLocation("core", "exp_grd_flare", Players[0].Character.Position.Around(1f, 1f));

            //int duration = 10000;
            //Control control = Control.Context;
            //Notify.CustomControl("Press the key you fool, got 10 seconds to do it.", true, control: control, timeToDisplay: duration);

            //int gameTimer = API.GetGameTimer();
            //while((API.GetGameTimer() - gameTimer) < duration)
            //{
            //    if (Game.IsControlJustPressed(0, control))
            //    {
            //        Notify.Success("Weldone monkey boy!");
            //        return;
            //    }

            //    await BaseScript.Delay(0);
            //}
            

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {

        }
    }
}
