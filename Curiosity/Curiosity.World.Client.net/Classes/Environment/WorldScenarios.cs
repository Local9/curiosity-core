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

        static List<string> ScenarioGroups = new List<string>()
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
            //"GRAPESEED_PLANES",
            "KORTZ_SECURITY",
            "LOST_BIKERS",
            //"LSA_Planes",
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
            //"SANDY_PLANES",
            "SCRAP_SECURITY",
            "SEW_MACHINE",
            "SOLOMON_GATE",
            //"Triathlon_1",
            //"Triathlon_2",
            //"Triathlon_2_Start",
            //"Triathlon_3",
            //"Triathlon_3_Start",
        };

        static List<string> ScenarioTypes = new List<string>()
        {
            "WORLD_MOUNTAIN_LION_REST",
            "WORLD_MOUNTAIN_LION_WANDER",
            "DRIVE",
            "WORLD_VEHICLE_POLICE_BIKE",
            "WORLD_VEHICLE_POLICE_CAR",
            "WORLD_VEHICLE_POLICE_NEXT_TO_CAR",
            "WORLD_VEHICLE_DRIVE_SOLO",
            "WORLD_VEHICLE_BIKER",
            "WORLD_VEHICLE_DRIVE_PASSENGERS",
            "WORLD_VEHICLE_SALTON_DIRT_BIKE",
            "WORLD_VEHICLE_BICYCLE_MOUNTAIN",
            "PROP_HUMAN_SEAT_CHAIR",
            "WORLD_VEHICLE_ATTRACTOR",
            "WORLD_HUMAN_LEANING",
            "WORLD_HUMAN_HANG_OUT_STREET",
            "WORLD_HUMAN_DRINKING",
            "WORLD_HUMAN_SMOKING",
            "WORLD_HUMAN_GUARD_STAND",
            "WORLD_HUMAN_CLIPBOARD",
            "WORLD_HUMAN_HIKER",
            "WORLD_VEHICLE_EMPTY",
            "WORLD_VEHICLE_BIKE_OFF_ROAD_RACE",
            "WORLD_HUMAN_PAPARAZZI",
            "WORLD_VEHICLE_PARK_PERPENDICULAR_NOSE_IN",
            "WORLD_VEHICLE_PARK_PARALLEL",
            "WORLD_VEHICLE_CONSTRUCTION_SOLO",
            "WORLD_VEHICLE_CONSTRUCTION_PASSENGERS",
            "WORLD_VEHICLE_TRUCK_LOGS",
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
            foreach (string scenario in ScenarioGroups)
            {
                if (Function.Call<bool>(Hash.DOES_SCENARIO_GROUP_EXIST, scenario)
                    && !Function.Call<bool>(Hash.IS_SCENARIO_GROUP_ENABLED, scenario))
                {
                    Function.Call<bool>(Hash.SET_SCENARIO_GROUP_ENABLED, scenario, true);
                }

                if (DoesScenarioGroupExist(scenario) && !IsScenarioGroupEnabled(scenario))
                    SetScenarioGroupEnabled(scenario, true);
            }

            foreach(string type in ScenarioTypes)
            {
                Function.Call(Hash.SET_SCENARIO_TYPE_ENABLED, type, true);
                SetScenarioTypeEnabled(type, true);
            }

            Debug.WriteLine("WorldScenarios -> Loaded");

        }
    }
}
