using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.EventWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Managers.Callouts.Data
{
    class HostageData : BaseScript
    {
        public static List<HostageDataModel> Situations = new List<HostageDataModel>();
        
        public HostageData()
        {
            EventHandlers[Events.Native.Client.OnClientResourceStart.Path]
                += Events.Native.Client.OnClientResourceStart.Action += resourceName =>
                {
                    if (resourceName != API.GetCurrentResourceName()) return;

                    SetupSituationData();
                };
        }

        void SetupSituationData()
        {
            if (Situations.Count() > 0)
                Situations.Clear();

            HostageDataModel clintonAve = new HostageDataModel()
            {
                StoreName = "24/7, Clinton Ave",
                Location = new Vector3(375.6602f, 325.6703f, 103.5664f),
                PatrolZone = PatrolZone.City,
                SpawnRadius = 150f,
                MissionRadius = 100f,
                BlipScale = .5f,
                Guards = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(375.6602f, 325.6703f, 103.5664f), 255.8121f ),
                    new Tuple<Vector3, float>( new Vector3(381.166f, 327.2303f, 103.5664f), 109.4753f ),
                    new Tuple<Vector3, float>( new Vector3(381.946f, 358.5238f, 102.5128f), 84.95505f ),
                    new Tuple<Vector3, float>( new Vector3(365.0694f, 254.3395f, 112.8926f), 355.4359f ),
                },
                Hostages = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(379.5025f,332.2108f,103.5664f), 257.2952f ),
                }
            };

            Situations.Add(clintonAve);

            HostageDataModel landActDam = new HostageDataModel()
            {
                StoreName = "Land Act Dam",
                Location = new Vector3(1667.245f, -22.89022f, 173.7747f),
                PatrolZone = PatrolZone.City,
                SpawnRadius = 150f,
                MissionRadius = 100f,
                BlipScale = .5f,
                Hostages = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(1666.029f, 0.9760635f, 166.118f), 109.3163f ),
                    new Tuple<Vector3, float>( new Vector3(1657.144f, 4.828178f, 166.118f), 253.8788f ),
                    new Tuple<Vector3, float>( new Vector3(1661.5f, -25.59821f, 173.7747f), 41.3812f ),
                },
                Vehicles = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(1670.741f, -73.97673f, 173.4063f), 124.1436f ),
                    new Tuple<Vector3, float>( new Vector3(1643.784f, 20.3597f, 173.7745f), 351.5334f ),
                }
            };

            // Situations.Add(landActDam);
        }
    }

    class HostageDataModel
    {
        public Vector3 Location;
        public string StoreName;
        public PatrolZone PatrolZone;
        public float SpawnRadius = 150f;
        public float MissionRadius = 100f;
        public float BlipScale = 1.0f;
        public List<Tuple<Vector3, float>> Guards = new List<Tuple<Vector3, float>>();
        public List<Tuple<Vector3, float>> Snipers = new List<Tuple<Vector3, float>>();
        public List<Tuple<Vector3, float>> Wanders = new List<Tuple<Vector3, float>>();
        public List<Tuple<Vector3, float>> Hostages = new List<Tuple<Vector3, float>>();
        public List<Tuple<Vector3, float>> Vehicles = new List<Tuple<Vector3, float>>();
    }
}
