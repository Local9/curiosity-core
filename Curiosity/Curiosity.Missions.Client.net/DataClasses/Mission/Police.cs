using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class Police
    {
        static Client client = Client.GetInstance();

        static public Dictionary<int, DataClasses.Mission.MissionData> storesCity = new Dictionary<int, DataClasses.Mission.MissionData>();
        static public Dictionary<int, DataClasses.Mission.MissionData> storesCountry = new Dictionary<int, DataClasses.Mission.MissionData>();
        static public Dictionary<int, DataClasses.Mission.MissionData> storesRural = new Dictionary<int, DataClasses.Mission.MissionData>();

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(PlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            SetupStores();
        }

        static void PlayerSpawned()
        {
            SetupStores();
        }

        static async void SetupStores()
        {
            if (storesCity.Count > 0)
            {
                storesCity.Clear();
                storesCountry.Clear();
                storesRural.Clear();

                Debug.WriteLine("Mission Lists Cleared");
            }

            await Task.FromResult(0);

            //// City
            InitCityClintonAve();
            InitCityDavisAve();
            InitCityLittleSeoul();
            InitCityMurrietaHeights();
            InitCityWestMirrorDrive();
            InitCityProsperityStreet();
            InitCitySanAndreasAve();
            InitCityStrawberry();
            //// County
            InitCountryGrandSenoraDesertScoops();
            InitCountryGrandSenoraDesertTwentyFour();
            InitCountryGrapeseed();
            InitCountryHarmony();
            InitCountryMountChiliad();
            InitCountrySandyShoresLiquorAce();
            InitCountrySandyShoresTwentyFour();
            //// Rural
            InitRuralBanhamCanyonRobsLiquor();
            InitRuralBanhamCanyonTwentyFour();
            InitRuralChumash();
            InitRuralRichmanGlen();
            InitRuralTataviamMountains();

            Debug.WriteLine("Mission Setup Completed");
        }

        #region City Missions

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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 257.2952f,
                    SpawnPoint = new Vector3(379.5025f,332.2108f,103.5664f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(1, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Clinton Ave",
                Location = new Vector3(375.6602f, 325.6703f, 103.5664f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 260.355f,
                    SpawnPoint = new Vector3(-41.81285f, -1749.345f, 29.42101f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(2, new DataClasses.Mission.MissionData
            {
                Name = "LTD Garage, Davis Ave",
                Location = new Vector3(-53.7861f, -1757.661f, 29.43897f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 310.407f,
                    SpawnPoint = new Vector3(-708.0243f, -904.0414f, 19.21559f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(3, new DataClasses.Mission.MissionData
            {
                Name = "LTD, Palomino Ave & Ginger St",
                Location = new Vector3(-711.6313f, -918.0067f, 19.21452f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 340.7079f,
                    SpawnPoint = new Vector3(1161.371f, -313.7611f, 69.20514f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(4, new DataClasses.Mission.MissionData
            {
                Name = "LTD, West Mirror Drive",
                Location = new Vector3(1159.815f, -327.377f, 69.21338f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 22.91358f,
                    SpawnPoint = new Vector3(1126.472f, -980.517f, 45.41566f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(5, new DataClasses.Mission.MissionData
            {
                Name = "Rob's Liquor, Vespucci Blvd",
                Location = new Vector3(1142.1f, -980.7694f, 46.20402f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 42.99305f,
                    SpawnPoint = new Vector3(-1484.027f, -375.3083f, 40.16343f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(6, new DataClasses.Mission.MissionData
            {
                Name = "Rob's Liquor, Prosperity St",
                Location = new Vector3(-1491.354f, -384.2097f, 40.08645f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 286.0046f,
                    SpawnPoint = new Vector3(-1220.106f, -907.9452f, 12.32635f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(7, new DataClasses.Mission.MissionData
            {
                Name = "Rob's Liquor, San Andreas Ave",
                Location = new Vector3(-1226.846f, -901.4744f, 12.28888f),
                MissionGangOne = pedData,
                Hostages = hostages
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

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 359.4656f,
                    SpawnPoint = new Vector3(29.65214f, -1343.586f, 29.49703f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCity.Add(8, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Elgin Ave",
                Location = new Vector3(29.32283f, -1349.734f, 29.32919f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        #endregion

        #region Country Missions

        static void InitCountryGrandSenoraDesertScoops()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 38.5031f,
                    SpawnPoint = new Vector3(1168.959f, 2706.064f, 38.1577f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly02AMM,
                    SpawnHeading = 347.5388f,
                    SpawnPoint = new Vector3(1163.829f, 2705.987f, 38.15771f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 241.8593f,
                    SpawnPoint = new Vector3(1168.946f, 2718.437f, 37.15754f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly02AMM,
                    SpawnHeading = 29.6417f,
                    SpawnPoint = new Vector3(1196.343f, 2656.701f, 40.77607f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SniperRifle,
                    VisionDistance = 500f
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 352.3094f,
                    SpawnPoint = new Vector3(1169.322f, 2714.104f, 38.15771f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(9, new DataClasses.Mission.MissionData
            {
                Name = "Scoops Liquor Barn",
                Location = new Vector3(1166.458f, 2702.856f, 38.17914f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountryGrandSenoraDesertTwentyFour()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 185.1518f,
                    SpawnPoint = new Vector3(2676.935f, 3284.402f, 55.24114f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly02AMM,
                    SpawnHeading = 179.1626f,
                    SpawnPoint = new Vector3(2679.578f, 3287.912f, 55.24113f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 71.15457f,
                    SpawnPoint = new Vector3(2673.681f, 3286.66f, 55.24115f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly02AMM,
                    SpawnHeading = 153.1552f,
                    SpawnPoint = new Vector3(2670.178f, 3290.69f, 55.24052f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 24.69863f,
                    SpawnPoint = new Vector3(2679.345f, 3288.867f, 55.24117f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(10, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Grand Senora Desert",
                Location = new Vector3(2683.266f, 3281.896f, 55.24052f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountryGrapeseed()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 130.5826f,
                    SpawnPoint = new Vector3(1703.9f, 4927.225f, 42.06367f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 135.3811f,
                    SpawnPoint = new Vector3(1707.202f, 4929.813f, 42.06368f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 311.5807f,
                    SpawnPoint = new Vector3(1707.086f, 4920.181f, 42.06368f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 207.6736f,
                    SpawnPoint = new Vector3(1670.252f, 4976.064f, 42.31182f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 78.98377f,
                    SpawnPoint = new Vector3(1697.059f, 4923.034f, 42.06368f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(11, new DataClasses.Mission.MissionData
            {
                Name = "LTD Garage, Grapeseed",
                Location = new Vector3(1698.641f, 4929.917f, 42.0781f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountryHarmony()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 317.2984f,
                    SpawnPoint = new Vector3(542.8696f, 2667.01f, 42.15653f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 290.3221f,
                    SpawnPoint = new Vector3(542.2476f, 2666.537f, 42.15654f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hillbilly01AMM,
                    SpawnHeading = 209.0979f,
                    SpawnPoint = new Vector3(545.2713f, 2663.984f, 42.15653f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 307.2429f,
                    SpawnPoint = new Vector3(544.3661f, 2669.405f, 48.94073f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 153.3502f,
                    SpawnPoint = new Vector3(547.7152f, 2663.135f, 42.15653f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(12, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Harmony",
                Location = new Vector3(543.722f, 2674.618f, 42.15475f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountryMountChiliad()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippie01AFY,
                    SpawnHeading = 66.84532f,
                    SpawnPoint = new Vector3(1735.893f, 6410.622f, 35.03723f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippy01AMY,
                    SpawnHeading = 73.89735f,
                    SpawnPoint = new Vector3(1736.645f, 6414.626f, 35.03722f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippie01AFY,
                    SpawnHeading = 324.7849f,
                    SpawnPoint = new Vector3(1735.074f, 6419.245f, 35.03723f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippy01AMY,
                    SpawnHeading = 163.317f,
                    SpawnPoint = new Vector3(1729.552f, 6426.299f, 34.28769f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 57.55748f,
                    SpawnPoint = new Vector3(1729.78f, 6418.804f, 35.03723f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(13, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Mount Chiliad",
                Location = new Vector3(1730.532f, 6410.807f, 35.00065f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountrySandyShoresLiquorAce()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Lost01GMY,
                    SpawnHeading = 67.03603f,
                    SpawnPoint = new Vector3(1397.863f, 3605.385f, 34.98093f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Lost01GFY,
                    SpawnHeading = 119.1938f,
                    SpawnPoint = new Vector3(1397.829f, 3606.384f, 34.98093f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Lost02GMY,
                    SpawnHeading = 98.14623f,
                    SpawnPoint = new Vector3(1394.403f, 3612.635f, 34.98092f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Lost03GMY,
                    SpawnHeading = 195.5031f,
                    SpawnPoint = new Vector3(1396.078f, 3608.911f, 38.94193f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 358.3886f,
                    SpawnPoint = new Vector3(1394.78f, 3613.35f, 34.98093f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(14, new DataClasses.Mission.MissionData
            {
                Name = "Liquor ACE, Sandy Shores",
                Location = new Vector3(1394.598f, 3598.536f, 34.99088f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitCountrySandyShoresTwentyFour()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippie01,
                    SpawnHeading = 203.0614f,
                    SpawnPoint = new Vector3(1961.589f, 3744.87f, 32.34375f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippie01AFY,
                    SpawnHeading = 100.3628f,
                    SpawnPoint = new Vector3(1964.858f, 3746.902f, 32.34375f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippy01AMY,
                    SpawnHeading = 36.21674f,
                    SpawnPoint = new Vector3(1960.116f, 3748.457f, 32.34378f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Hippie01AFY,
                    SpawnHeading = 203.0665f,
                    SpawnPoint = new Vector3(1960.161f, 3747.839f, 36.43888f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 295.6968f,
                    SpawnPoint = new Vector3(1961.463f, 3749.31f, 32.34375f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesCountry.Add(15, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Sandy Shores",
                Location = new Vector3(1965.68f, 3739.752f, 32.32018f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        #endregion

        #region Rural Missions

        static void InitRuralBanhamCanyonRobsLiquor()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk01GMY,
                    SpawnHeading = 161.5946f,
                    SpawnPoint = new Vector3(-2968.926f, 392.5486f, 15.04331f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk02GMY,
                    SpawnHeading = 269.9462f,
                    SpawnPoint = new Vector3(-2970.986f, 393.072f, 15.04331f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk01GMY,
                    SpawnHeading = 170.5265f,
                    SpawnPoint = new Vector3(-2959.125f, 387.2958f, 14.04316f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk02GMY,
                    SpawnHeading = 161.1103f,
                    SpawnPoint = new Vector3(-2952.538f, 388.8123f, 15.08122f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 340.6716f,
                    SpawnPoint = new Vector3(-2965.421f, 392.5959f, 15.04331f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesRural.Add(16, new DataClasses.Mission.MissionData
            {
                Name = "Rob's Liquor, Banham Canyon",
                Location = new Vector3(-2974.973f, 390.8409f, 15.03081f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitRuralBanhamCanyonTwentyFour()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Surfer01AMY,
                    SpawnHeading = 239.2633f,
                    SpawnPoint = new Vector3(-3043.355f, 586.9014f, 7.908933f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Surfer01AMY,
                    SpawnHeading = 214.9556f,
                    SpawnPoint = new Vector3(-3044.415f, 591.0453f, 7.90893f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Surfer01AMY,
                    SpawnHeading = 97.11764f,
                    SpawnPoint = new Vector3(-3047.269f, 585.941f, 7.90893f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Surfer01AMY,
                    SpawnHeading = 109.0769f,
                    SpawnPoint = new Vector3(-3043.736f, 586.5539f, 11.97686f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 112.1929f,
                    SpawnPoint = new Vector3(-3046.771f, 582.85f, 7.90893f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesRural.Add(17, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Banham Canyon",
                Location = new Vector3(-3036.43f, 589.7015f, 7.811507f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitRuralChumash()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk01GMY,
                    SpawnHeading = 207.4452f,
                    SpawnPoint = new Vector3(-3245.716f, 1004.078f, 12.83071f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk02GMY,
                    SpawnHeading = 188.4922f,
                    SpawnPoint = new Vector3(-3244.494f, 1008.814f, 12.83071f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk01GMY,
                    SpawnHeading = 78.84269f,
                    SpawnPoint = new Vector3(-3249.636f, 1004.747f, 12.83071f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.StrPunk02GMY,
                    SpawnHeading = 114.785f,
                    SpawnPoint = new Vector3(-3180.201f, 1042.482f, 27.66457f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SniperRifle,
                    VisionDistance = 500f
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 277.1693f,
                    SpawnPoint = new Vector3(-3248.413f, 1002.209f, 12.83071f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesRural.Add(18, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Chumash",
                Location = new Vector3(-3238.816f, 1004.304f, 12.45766f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitRuralRichmanGlen()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Stwhi02AMY,
                    SpawnHeading = 321.5045f,
                    SpawnPoint = new Vector3(-1826.983f, 790.7717f, 138.2379f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Stwhi02AMY,
                    SpawnHeading = 339.4079f,
                    SpawnPoint = new Vector3(-1829.74f, 788.985f, 138.3019f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Stwhi02AMY,
                    SpawnHeading = 101.7553f,
                    SpawnPoint = new Vector3(-1827.409f, 798.4881f, 138.1647f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Stwhi02AMY,
                    SpawnHeading = 16.38768f,
                    SpawnPoint = new Vector3(-1837.479f, 794.6126f, 138.6669f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 26.31407f,
                    SpawnPoint = new Vector3(-1825.169f, 800.7675f, 138.1038f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesRural.Add(19, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Richman Glen",
                Location = new Vector3(-1819.346f, 786.3083f, 137.9569f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        static void InitRuralTataviamMountains()
        {
            List<DataClasses.Mission.MissionPedData> pedData = new List<DataClasses.Mission.MissionPedData>
            {
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 210.8072f,
                    SpawnPoint = new Vector3(2553.855f, 384.7222f, 108.6229f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Pistol
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 192.3992f,
                    SpawnPoint = new Vector3(2554.359f, 389.3066f, 108.623f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.Revolver
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 71.06248f,
                    SpawnPoint = new Vector3(2550.206f, 384.5594f, 108.623f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.SawnOffShotgun
                },
                new DataClasses.Mission.MissionPedData {
                    Model = PedHash.Farmer01AMM,
                    SpawnHeading = 264.2936f,
                    SpawnPoint = new Vector3(2543.925f, 398.2904f, 108.6162f),
                    Alertness = Extensions.Alertness.FullyAlert,
                    Difficulty = Extensions.Difficulty.BringItOn,
                    Weapon = WeaponHash.PumpShotgun
                },
            };

            List<MissionPedData> hostages = new List<MissionPedData>()
            {
                new MissionPedData
                {
                    Model = PedHash.ShopKeep01,
                    SpawnHeading = 0.9818109f,
                    SpawnPoint = new Vector3(2550.074f, 386.6728f, 108.623f),
                    IsHostage = true,
                    Alertness = Extensions.Alertness.Nuetral
                }
            };

            storesRural.Add(20, new DataClasses.Mission.MissionData
            {
                Name = "24/7, Tataviam Mountains",
                Location = new Vector3(2561.057f, 385.2407f, 108.6211f),
                MissionGangOne = pedData,
                Hostages = hostages
            });
        }

        #endregion
    }
}
