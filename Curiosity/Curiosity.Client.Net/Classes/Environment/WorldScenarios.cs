using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment
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
            "WORLD_HUMAN_AA_COFFEE",
            "WORLD_HUMAN_AA_SMOKE",
            "WORLD_HUMAN_BINOCULARS",
            "WORLD_HUMAN_BUM_FREEWAY",
            "WORLD_HUMAN_BUM_SLUMPED",
            "WORLD_HUMAN_BUM_STANDING",
            "WORLD_HUMAN_BUM_WASH",
            "WORLD_HUMAN_CAR_PARK_ATTENDANT",
            "WORLD_HUMAN_CHEERING",
            "WORLD_HUMAN_CLIPBOARD",
            "WORLD_HUMAN_CONST_DRILL",
            "WORLD_HUMAN_COP_IDLES",
            "WORLD_HUMAN_DRINKING",
            "WORLD_HUMAN_DRUG_DEALER",
            "WORLD_HUMAN_DRUG_DEALER_HARD",
            "WORLD_HUMAN_MOBILE_FILM_SHOCKING",
            "WORLD_HUMAN_GARDENER_LEAF_BLOWER",
            "WORLD_HUMAN_GARDENER_PLANT",
            "WORLD_HUMAN_GOLF_PLAYER",
            "WORLD_HUMAN_GUARD_PATROL",
            "WORLD_HUMAN_GUARD_STAND",
            "WORLD_HUMAN_HAMMERING",
            "WORLD_HUMAN_HANG_OUT_STREET",
            "WORLD_HUMAN_HIKER_STANDING",
            "WORLD_HUMAN_HUMAN_STATUE",
            "WORLD_HUMAN_JANITOR",
            "WORLD_HUMAN_JOG_STANDING",
            "WORLD_HUMAN_LEANING",
            "WORLD_HUMAN_MAID_CLEAN",
            "WORLD_HUMAN_MUSCLE_FLEX",
            "WORLD_HUMAN_MUSCLE_FREE_WEIGHTS",
            "WORLD_HUMAN_MUSICIAN",
            "WORLD_HUMAN_PAPARAZZI",
            "WORLD_HUMAN_PARTYING",
            "WORLD_HUMAN_PICNIC",
            "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS",
            "WORLD_HUMAN_PROSTITUTE_LOW_CLASS",
            "WORLD_HUMAN_PUSH_UPS",
            "WORLD_HUMAN_SEAT_LEDGE",
            "WORLD_HUMAN_SEAT_STEPS",
            "WORLD_HUMAN_SEAT_WALL",
            "WORLD_HUMAN_SECURITY_SHINE_TORCH",
            "WORLD_HUMAN_SIT_UPS",
            "WORLD_HUMAN_SMOKING",
            "WORLD_HUMAN_SMOKING_POT",
            "WORLD_HUMAN_STAND_FIRE",
            "WORLD_HUMAN_STAND_FISHING",
            "WORLD_HUMAN_STAND_IMPATIENT",
            "WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT",
            "WORLD_HUMAN_STAND_MOBILE",
            "WORLD_HUMAN_STAND_MOBILE_UPRIGHT",
            "WORLD_HUMAN_STRIP_WATCH_STAND",
            "WORLD_HUMAN_STUPOR",
            "WORLD_HUMAN_SUNBATHE",
            "WORLD_HUMAN_SUNBATHE_BACK",
            "WORLD_HUMAN_TENNIS_PLAYER",
            "WORLD_HUMAN_TOURIST_MAP",
            "WORLD_HUMAN_TOURIST_MOBILE",
            "WORLD_HUMAN_VEHICLE_MECHANIC",
            "WORLD_HUMAN_WELDING",
            "WORLD_HUMAN_WINDOW_SHOP_BROWSE",
            "WORLD_HUMAN_YOGA",
            "PROP_HUMAN_ATM",
            "PROP_HUMAN_BBQ",
            "PROP_HUMAN_BUM_BIN",
            "PROP_HUMAN_BUM_SHOPPING_CART",
            "PROP_HUMAN_MUSCLE_CHIN_UPS",
            "PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY",
            "PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON",
            "PROP_HUMAN_PARKING_METER",
            "PROP_HUMAN_SEAT_ARMCHAIR",
            "PROP_HUMAN_SEAT_BAR",
            "PROP_HUMAN_SEAT_BENCH",
            "PROP_HUMAN_SEAT_BUS_STOP_WAIT",
            "PROP_HUMAN_SEAT_CHAIR",
            "PROP_HUMAN_SEAT_CHAIR_UPRIGHT",
            "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER",
            "PROP_HUMAN_SEAT_COMPUTER",
            "PROP_HUMAN_SEAT_DECKCHAIR",
            "PROP_HUMAN_SEAT_DECKCHAIR_DRINK",
            "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS",
            "PROP_HUMAN_SEAT_MUSCLE_BENCH_PRESS_PRISON",
            "PROP_HUMAN_SEAT_STRIP_WATCH",
            "PROP_HUMAN_SEAT_SUNLOUNGER",
            "PROP_HUMAN_STAND_IMPATIENT",
            "CODE_HUMAN_CROSS_ROAD_WAIT",
            "CODE_HUMAN_MEDIC_KNEEL",
            "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
            "CODE_HUMAN_MEDIC_TIME_OF_DEATH",
            "CODE_HUMAN_POLICE_CROWD_CONTROL",
            "CODE_HUMAN_POLICE_INVESTIGATE",
            "WORLD_VEHICLE_ATTRACTOR",
            "WORLD_VEHICLE_AMBULANCE",
            "WORLD_VEHICLE_BICYCLE_BMX",
            "WORLD_VEHICLE_BICYCLE_BMX_BALLAS",
            "WORLD_VEHICLE_BICYCLE_BMX_FAMILY",
            "WORLD_VEHICLE_BICYCLE_BMX_HARMONY",
            "WORLD_VEHICLE_BICYCLE_BMX_VAGOS",
            "WORLD_VEHICLE_BICYCLE_MOUNTAIN",
            "WORLD_VEHICLE_BICYCLE_ROAD",
            "WORLD_VEHICLE_BIKE_OFF_ROAD_RACE",
            "WORLD_VEHICLE_BIKER",
            "WORLD_VEHICLE_BOAT_IDLE",
            "WORLD_VEHICLE_BOAT_IDLE_ALAMO",
            "WORLD_VEHICLE_BOAT_IDLE_MARQUIS",
            "WORLD_VEHICLE_BOAT_IDLE_MARQUIS",
            "WORLD_VEHICLE_BROKEN_DOWN",
            "WORLD_VEHICLE_BUSINESSMEN",
            "WORLD_VEHICLE_HELI_LIFEGUARD",
            "WORLD_VEHICLE_CLUCKIN_BELL_TRAILER",
            "WORLD_VEHICLE_CONSTRUCTION_SOLO",
            "WORLD_VEHICLE_CONSTRUCTION_PASSENGERS",
            "WORLD_VEHICLE_DRIVE_PASSENGERS",
            "WORLD_VEHICLE_DRIVE_PASSENGERS_LIMITED",
            "WORLD_VEHICLE_DRIVE_SOLO",
            "WORLD_VEHICLE_FARM_WORKER",
            "WORLD_VEHICLE_FIRE_TRUCK",
            "WORLD_VEHICLE_EMPTY",
            "WORLD_VEHICLE_MARIACHI",
            "WORLD_VEHICLE_MECHANIC",
            "WORLD_VEHICLE_MILITARY_PLANES_BIG",
            "WORLD_VEHICLE_MILITARY_PLANES_SMALL",
            "WORLD_VEHICLE_PARK_PARALLEL",
            "WORLD_VEHICLE_PARK_PERPENDICULAR_NOSE_IN",
            "WORLD_VEHICLE_PASSENGER_EXIT",
            "WORLD_VEHICLE_POLICE_BIKE",
            "WORLD_VEHICLE_POLICE_CAR",
            "WORLD_VEHICLE_POLICE",
            "WORLD_VEHICLE_POLICE_NEXT_TO_CAR",
            "WORLD_VEHICLE_QUARRY",
            "WORLD_VEHICLE_SALTON",
            "WORLD_VEHICLE_SALTON_DIRT_BIKE",
            "WORLD_VEHICLE_SECURITY_CAR",
            "WORLD_VEHICLE_STREETRACE",
            "WORLD_VEHICLE_TOURBUS",
            "WORLD_VEHICLE_TOURIST",
            "WORLD_VEHICLE_TANDL",
            "WORLD_VEHICLE_TRACTOR",
            "WORLD_VEHICLE_TRACTOR_BEACH",
            "WORLD_VEHICLE_TRUCK_LOGS",
            "WORLD_VEHICLE_TRUCKS_TRAILERS",
            "WORLD_VEHICLE_DISTANT_EMPTY_GROUND"
        };

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));

            // client.RegisterTickHandler(OnDensity);
        }

        private static async Task OnDensity()
        {
            API.SetScenarioPedDensityMultiplierThisFrame(1, 1);
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
                if (API.DoesScenarioGroupExist(scenario)
                    && !API.IsScenarioGroupEnabled(scenario))
                {
                    API.SetScenarioGroupEnabled(scenario, true);
                }
                API.ResetScenarioGroupsEnabled();
            }

            foreach (string type in ScenarioTypes)
            {
                API.SetScenarioTypeEnabled(type, true);
                API.ResetScenarioTypesEnabled();
            }

            Debug.WriteLine("WorldScenarios -> Loaded");

            //client.RegisterTickHandler(OnScenarioToSetup);
        }

        //static bool KoreanSpawned = false;

        //static async Task OnScenarioToSetup()
        //{
        //    TimeSpan timeSpan = new TimeSpan(TimeSync.currentServerHours, TimeSync.currentServerMinutes, 0);
        //    TimeSpan startTime = new TimeSpan(20, 0, 0);
        //    TimeSpan endTime = new TimeSpan(5, 0, 0);

        //    if (timeSpan.IsTimeOfDayBetween(startTime, endTime) && !KoreanSpawned)
        //    {
        //        Vector3 pos = new Vector3(-763.5086f, -919.8815f, 19.20683f);
        //        Vector3 position = Game.PlayerPed.Position;

        //        float distance = API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, pos.X, pos.Y, pos.Z, false);

        //        if (distance > 200f) return;

        //        Model model = PedHash.ChiBoss01GMM;
        //        await model.Request(10000);

        //        while (!model.IsLoaded)
        //            await BaseScript.Delay(100);

        //        Ped ped = await World.CreatePed(model, pos, -1.504403f);

        //        ped.Task.StartScenario("WORLD_HUMAN_SMOKING", pos);

        //        KoreanSpawned = true;

        //        await BaseScript.Delay(1000);
        //    }
        //}
    }
}
