using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Scripts.Mission
{
    class PoliceStores
    {
        static Dictionary<int, DataClasses.Mission.Store> stores = new Dictionary<int, DataClasses.Mission.Store>();

        static public void Init()
        {
            InitCityClintonAve();
            InitCityDavisAve();
            InitCityLittleSeoul();
            InitCityMurrietaHeights();
            InitCityWestMirrorDrive();
            InitCityProsperityStreet();
            InitCitySanAndreasAve();
            InitCityStrawberry();
        }

        static void InitCityClintonAve()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 255.8121f,
                    SpawnPoint = new Vector3(375.6602f, 325.6703f, 103.5664f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 109.4753f,
                    SpawnPoint = new Vector3(381.166f, 327.2303f, 103.5664f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 84.95505f,
                    SpawnPoint = new Vector3(381.946f, 358.5238f, 102.5128f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol,
                    VisionDistance = 100f
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 355.4359f,
                    SpawnPoint = new Vector3(365.0694f, 254.3395f, 112.8926f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SniperRifle,
                    VisionDistance = 500f
                },
            };

            stores.Add(1, new DataClasses.Mission.Store
            {
                Name = "24/7, Clinton Ave",
                Location = new Vector3(375.6602f, 325.6703f, 103.5664f),
                missionPeds = pedData
            });
        }

        static void InitCityDavisAve()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 190.1924f,
                    SpawnPoint = new Vector3(-47.62278f, -1753.076f, 29.42101f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 202.6338f,
                    SpawnPoint = new Vector3(-52.41798f, -1748.562f, 29.42101f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 29.03807f,
                    SpawnPoint = new Vector3(-43.02503f, -1749.319f, 29.42101f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 111.3776f,
                    SpawnPoint = new Vector3(-38.14207f, -1747.355f, 29.19765f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MicroSMG
                },
            };

            stores.Add(2, new DataClasses.Mission.Store
            {
                Name = "LTD Garage, Davis Ave",
                Location = new Vector3(-53.7861f, -1757.661f, 29.43897f),
                missionPeds = pedData
            });
        }

        static void InitCityLittleSeoul()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 223.6788f,
                    SpawnPoint = new Vector3(-709.2447f, -910.5724f, 19.2156f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 266.0481f,
                    SpawnPoint = new Vector3(-716.8518f, -910.9249f, 19.21559f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 67.95396f,
                    SpawnPoint = new Vector3(-708.9248f, -904.6361f, 19.21561f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MicroSMG
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon02GMM,
                    SpawnHeading = 160.6611f,
                    SpawnPoint = new Vector3(-693.8491f, -901.0219f, 23.6719f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MicroSMG
                },
            };

            stores.Add(3, new DataClasses.Mission.Store
            {
                Name = "LTD, Palomino Ave & Ginger St",
                Location = new Vector3(-711.6313f, -918.0067f, 19.21452f),
                missionPeds = pedData
            });
        }

        static void InitCityWestMirrorDrive()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dockwork01SMY,
                    SpawnHeading = 224.5752f,
                    SpawnPoint = new Vector3(1160.791f, -320.9355f, 69.20513f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dockwork01SMM,
                    SpawnHeading = 269.7378f,
                    SpawnPoint = new Vector3(1153.717f, -321.0354f, 69.20515f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dockwork01SMY,
                    SpawnHeading = 70.36984f,
                    SpawnPoint = new Vector3(1160.362f, -314.3562f, 69.20513f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MicroSMG
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dockwork01SMM,
                    SpawnHeading = 201.9093f,
                    SpawnPoint = new Vector3(1160.738f, -311.3483f, 69.27756f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MiniSMG
                },
            };

            stores.Add(4, new DataClasses.Mission.Store
            {
                Name = "LTD, West Mirror Drive",
                Location = new Vector3(1159.815f, -327.377f, 69.21338f),
                missionPeds = pedData
            });
        }

        static void InitCityMurrietaHeights()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dealer01SMY,
                    SpawnHeading = 130.9778f,
                    SpawnPoint = new Vector3(1138.608f, -978.605f, 46.41584f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dealer01SMY,
                    SpawnHeading = 85.26283f,
                    SpawnPoint = new Vector3(1138.97f, -983.6699f, 46.41584f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dealer01SMY,
                    SpawnHeading = 349.2951f,
                    SpawnPoint = new Vector3(1126.959f, -981.006f, 45.41562f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dealer01SMY,
                    SpawnHeading = 201.9093f,
                    SpawnPoint = new Vector3(1126.27f, -991.4229f, 45.95441f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.MiniSMG
                },
            };

            stores.Add(5, new DataClasses.Mission.Store
            {
                Name = "Rob's Liquor, Vespucci Blvd",
                Location = new Vector3(1142.1f, -980.7694f, 46.20402f),
                missionPeds = pedData
            });
        }

        static void InitCityProsperityStreet()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.BikerChic,
                    SpawnHeading = 351.4111f,
                    SpawnPoint = new Vector3(-1487.207f, -382.805f, 40.16343f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.BikerChic,
                    SpawnHeading = 340.9902f,
                    SpawnPoint = new Vector3(-1490.947f, -379.5471f, 40.16344f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.BikerChic,
                    SpawnHeading = 209.9628f,
                    SpawnPoint = new Vector3(-1479.647f, -374.1291f, 39.1634f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Lost01GFY,
                    SpawnHeading = 213.494f,
                    SpawnPoint = new Vector3(-1471.456f, -363.1623f, 40.11281f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            stores.Add(6, new DataClasses.Mission.Store
            {
                Name = "Rob's Liquor, Prosperity St",
                Location = new Vector3(-1491.354f, -384.2097f, 40.08645f),
                missionPeds = pedData
            });
        }

        static void InitCitySanAndreasAve()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Eastsa01AMM,
                    SpawnHeading = 233.6525f,
                    SpawnPoint = new Vector3(-1224.359f, -905.7396f, 12.32636f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Eastsa01AFY,
                    SpawnHeading = 174.2641f,
                    SpawnPoint = new Vector3(-1222.542f, -903.4592f, 12.32636f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Eastsa01AMY,
                    SpawnHeading = 116.6096f,
                    SpawnPoint = new Vector3(-1219.176f, -915.4348f, 11.32633f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Eastsa03AFY,
                    SpawnHeading = 269.7748f,
                    SpawnPoint = new Vector3(-1178.778f, -902.0923f, 13.58387f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            stores.Add(7, new DataClasses.Mission.Store
            {
                Name = "Rob's Liquor, San Andreas Ave",
                Location = new Vector3(-1226.846f, -901.4744f, 12.28888f),
                missionPeds = pedData
            });
        }

        static void InitCityStrawberry()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.AlDiNapoli,
                    SpawnHeading = 180.1658f,
                    SpawnPoint = new Vector3(29.5332f, -1346.235f, 29.49703f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Dealer01SMY,
                    SpawnHeading = 96.20592f,
                    SpawnPoint = new Vector3(32.30225f, -1343.495f, 29.49702f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.ChiGoon01GMM,
                    SpawnHeading = 345.2916f,
                    SpawnPoint = new Vector3(28.82755f, -1340.169f, 29.49705f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Eastsa03AFY,
                    SpawnHeading = 320.6067f,
                    SpawnPoint = new Vector3(27.83668f, -1313.568f, 29.52451f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            stores.Add(8, new DataClasses.Mission.Store
            {
                Name = "24/7, Elgin Ave",
                Location = new Vector3(29.32283f, -1349.734f, 29.32919f),
                missionPeds = pedData
            });
        }
    }
}
