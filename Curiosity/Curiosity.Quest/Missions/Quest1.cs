using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using System.Drawing;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;
using Curiosity.MissionManager.Client.Diagnostics;

namespace Curiosity.Quest.Missions
{
    [MissionInfo("Intro Quest", "quest1", -543.9988f, -157.8393f, 38.54123f, MissionType.Halloween, true, "None")]
    public class Quest1 : Mission
    {
        Vector3 _start = new Vector3(-543.9988f, -157.8393f, 38.54123f);
        Vector3 _scale = new Vector3(1f, 1f, 1f);

        NUIMarker questMarker1;
        NUIMarker questMarker2;
        NUIMarker questMarker3;
        NUIMarker questMarker4;
        NUIMarker questMarker5;
        NUIMarker questMarker6;
        NUIMarker questMarker7;
        NUIMarker questMarker8;

        MissionPhase missionPhase = MissionPhase.START;
        int _soundId = -1;

        public override async void Start()
        {
            MissionInitiated();
        }

        void MissionInitiated()
        {
            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            Pass();
        }
    }

    enum MissionPhase
    {
        START,
        END
    }
}
