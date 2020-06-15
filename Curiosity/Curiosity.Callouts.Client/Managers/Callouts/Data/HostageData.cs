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
                },
                Guards = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(1671.809f, -74.20051f, 173.4963f), 120.7903f ),
                    new Tuple<Vector3, float>( new Vector3(1668.851f, -69.07045f, 173.5114f), 135.1386f ),
                    new Tuple<Vector3, float>( new Vector3(1662.848f, -28.89268f, 173.7748f), 109.1476f ),
                    new Tuple<Vector3, float>( new Vector3(1665.733f, -0.4518342f, 173.7751f), 289.996f ),
                    new Tuple<Vector3, float>( new Vector3(1640.632f, 20.70308f, 173.7744f), 338.3163f ),
                    new Tuple<Vector3, float>( new Vector3(1645.217f, 20.78804f, 173.7744f), 343.6192f ),
                    new Tuple<Vector3, float>( new Vector3(1662.544f, -3.019243f, 166.1182f), 29.75872f ),
                    new Tuple<Vector3, float>( new Vector3(1660.352f, 5.324343f, 166.1182f), 183.8288f ),
                },
                Snipers = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(1661.223f, 36.43375f, 179.8763f), 301.0259f ),
                    new Tuple<Vector3, float>( new Vector3(1662.174f, -30.35659f, 182.7696f), 170.3308f ),
                    new Tuple<Vector3, float>( new Vector3(1658.894f, -66.81844f, 178.6644f), 85.56298f ),
                },
                Wanders = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(1671.246f, -43.60514f, 173.771f), 2.846863f ),
                    new Tuple<Vector3, float>( new Vector3(1665.694f, -15.00139f, 173.7745f), 19.3666f ),
                    new Tuple<Vector3, float>( new Vector3(1645.912f, 14.74555f, 173.7745f), 14.06115f ),
                }
            };

            Situations.Add(landActDam);

            HostageDataModel pacificStandard = new HostageDataModel()
            {
                StoreName = "Pacific Standard",
                Location = new Vector3(235.6577f, 216.9228f, 106.2868f),
                PatrolZone = PatrolZone.City,
                SpawnRadius = 100f,
                MissionRadius = 100f,
                BlipScale = .5f,
                Hostages = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(263.2645f, 220.7315f, 101.6833f), 172.6615f ),
                    new Tuple<Vector3, float>( new Vector3(254.2221f, 224.1971f, 106.2869f), 153.1367f ),
                    new Tuple<Vector3, float>( new Vector3(248.1087f, 226.165f, 106.2873f), 152.5928f ),
                    new Tuple<Vector3, float>( new Vector3(242.8457f, 227.2699f, 106.2872f), 152.1703f ),
                    new Tuple<Vector3, float>( new Vector3(233.6464f, 219.9191f, 110.2827f), 61.93121f ),
                    new Tuple<Vector3, float>( new Vector3(243.1119f, 210.6941f, 110.283f), 18.85863f ),
                    new Tuple<Vector3, float>( new Vector3(254.4889f, 209.0891f, 110.283f), 341.6508f ),
                    new Tuple<Vector3, float>( new Vector3(261.4947f, 204.1163f, 110.2874f), 25.54833f ),
                },
                Guards = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(236.3161f, 212.9918f, 106.2868f), 56.04658f ),
                    new Tuple<Vector3, float>( new Vector3(233.3001f, 220.6271f, 106.2868f), 183.7656f ),
                    new Tuple<Vector3, float>( new Vector3(239.1385f, 223.1199f, 106.2868f), 260.8069f ),
                    new Tuple<Vector3, float>( new Vector3(251.5542f, 208.3407f, 106.2868f), 35.13206f ),
                    new Tuple<Vector3, float>( new Vector3(263.1232f, 209.5521f, 106.2832f), 159.4815f ),
                    new Tuple<Vector3, float>( new Vector3(259.5376f, 210.5749f, 106.2832f), 247.1622f ),
                    new Tuple<Vector3, float>( new Vector3(259.0273f, 222.4877f, 106.2853f), 146.3903f ),
                    new Tuple<Vector3, float>( new Vector3(266.0105f, 220.1533f, 110.283f), 69.43592f ),
                    new Tuple<Vector3, float>( new Vector3(263.0029f, 224.0629f, 101.6833f), 199.8399f ),
                    new Tuple<Vector3, float>( new Vector3(255.1471f, 227.4406f, 101.6833f), 242.3134f ),
                    new Tuple<Vector3, float>( new Vector3(253.9521f, 207.4929f, 110.283f), 346.3055f ),
                    new Tuple<Vector3, float>( new Vector3(235.2369f, 217.0184f, 110.2828f), 283.4188f ),
                    new Tuple<Vector3, float>( new Vector3(236.1823f, 224.7399f, 110.2828f), 266.6903f ),
                    
                },
                Wanders = new List<Tuple<Vector3, float>>
                {
                    new Tuple<Vector3, float>( new Vector3(242.3272f, 218.7228f, 106.2868f), 248.3813f ),
                    new Tuple<Vector3, float>( new Vector3(253.7576f, 213.2547f, 106.2868f), 246.8687f ),
                    new Tuple<Vector3, float>( new Vector3(259.0129f, 209.7595f, 110.283f), 70.91381f ),
                    new Tuple<Vector3, float>( new Vector3(261.1353f, 214.6259f, 110.283f), 341.6583f ),
                }
            };

            Situations.Add(pacificStandard);
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
