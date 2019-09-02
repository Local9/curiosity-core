using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;

namespace Curiosity.World.Client.net.Classes.Environment
{
    class WorldScenarios
    {
        static bool ScenariosSetup = false;
        static Client client = Client.GetInstance();

        static List<string> Scenarios = new List<string>()
        {
            "ALAMO_PLANES",
             "ARMENIAN_CATS",
             "ARMY_GUARD",
             "ARMY_HELI",
             "ATTRACT_PAP",
             "BLIMP",
             "CHINESE2_HILLBILLIES",
             "Chinese2_Lunch",
             "Cinema_Downtown",
             "Cinema_Morningwood",
             "Cinema_Textile",
             "City_Banks",
             "Countryside_Banks",
             "DEALERSHIP",
             "FIB_GROUP_1",
             "FIB_GROUP_2",
             "GRAPESEED_PLANES",
             "Grapeseed_Planes",
             "KORTZ_SECURITY",
             "LOST_BIKERS",
             "LSA_Planes",
             "MOVIE_STUDIO_SECURITY",
             "MP_POLICE",
             "Observatory_Bikers",
             "POLICE_POUND1",
             "POLICE_POUND2",
             "POLICE_POUND3",
             "POLICE_POUND4",
             "POLICE_POUND5",
             "PRISON_TOWERS",
             "QUARRY",
             "SANDY_PLANES",
             "SCRAP_SECURITY",
             "SEW_MACHINE",
             "SOLOMON_GATE",
             "Triathlon_1",
             "Triathlon_2",
             "Triathlon_2_Start",
             "Triathlon_3",
             "Triathlon_3_Start",
        };

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            Setup();
        }

        static void OnPlayerSpawned(dynamic dynData)
        {
            Setup();
        }

        static void Setup()
        {
            foreach (string scenario in Scenarios)
            {
                if (Function.Call<bool>(Hash.DOES_SCENARIO_GROUP_EXIST, scenario)
                    && !Function.Call<bool>(Hash.IS_SCENARIO_GROUP_ENABLED, scenario))
                {
                    Function.Call<bool>(Hash.SET_SCENARIO_GROUP_ENABLED, scenario);
                }
            }

            Debug.WriteLine("WorldScenarios -> Loaded");

        }
    }
}
