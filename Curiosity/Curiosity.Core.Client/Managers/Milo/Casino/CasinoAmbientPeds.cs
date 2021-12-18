using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.Casino
{
    internal class CasinoAmbientPeds
    {
        static List<Ped> peds = new List<Ped>();
        static bool isRunning = false;

        public async static void Init()
        {
            try
            {
                if (peds.Count == 0 && !isRunning)
                {
                    isRunning = true;
                    Logger.Info("Init Ambient Peds");

                    // These are all in the wrong location

                    Ped ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "U_F_M_CasinoCash_01", new Vector3(950.214f, 33.151f, 70.839f), 57.052f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Doorman_01", new Vector3(955.619f, 70.179f, 69.433f), 190.937f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(920.726f, 45.883f, 71.073f), 276.635f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(933.62f, 41.631f, 80.089f), 56.923f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(938.974f, 27.776f, 70.834f), 13.513f, "WORLD_HUMAN_WINDOW_SHOP_BROWSE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(947.994f, 45.233f, 70.638f), 220.199f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(969.729f, 57.35f, 70.233f), 23.352f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(998.848f, 45.308f, 68.833f), 43.086f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(978.838f, 69.432f, 69.233f), 237.674f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(973.283f, 41.411f, 70.233f), 184.428f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(996.982f, 37.33f, 69.233f), 45.32f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(984.436f, 36.494f, 69.233f), 314.327f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(994.234f, 65.098f, 68.833f), 151.299f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(989.222f, 71.748f, 69.233f), 161.745f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(974.901f, 34.128f, 69.833f), 1.995f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(970.651f, 65.43f, 69.833f), 192.896f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(920.48f, 46.759f, 71.073f), 277.117f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(933.011f, 40.807f, 80.089f), 54.913f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(929.086f, 34.707f, 80.089f), 328.4f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(949.615f, 25.983f, 70.834f), 53.202f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "S_F_Y_Casino_01", new Vector3(939.71f, 47.066f, 71.279f), 4164.51f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "S_M_Y_Casino_01", new Vector3(929.79f, 37.618f, 71.274f), 35.262f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "s_m_y_valet_01", new Vector3(925.235f, 50.921f, 80.106f), 55.601f, "WORLD_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_paparazzi_01", new Vector3(949.985f, 54.927f, 70.433f), 172.611f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_tourist_02", new Vector3(943.266f, 64.802f, 69.833f), 267.054f, "WORLD_HUMAN_STAND_MOBILE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_vinewood_02", new Vector3(943.723f, 65.612f, 69.833f), 173.907f, "WORLD_HUMAN_HANG_OUT_STREET");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_tom", new Vector3(942.68f, 38.882f, 70.834f), 154.872f, "WORLD_HUMAN_WINDOW_SHOP_BROWSE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_jimmyboston", new Vector3(942.057f, 38.012f, 70.834f), 329.05f, "WORLD_HUMAN_MOBILE_FILM_SHOCKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_carbuyer", new Vector3(929.239f, 28.97f, 70.834f), 281.783f, "WORLD_HUMAN_MOBILE_FILM_SHOCKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_y_clubcust_03", new Vector3(928.8f, 29.857f, 70.834f), 283.934f, "WORLD_HUMAN_HANG_OUT_STREET");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_gurk", new Vector3(934.337f, 37.499f, 71.279f), 168.193f, "WORLD_HUMAN_HANG_OUT_STREET");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_lazlow_2", new Vector3(935.38f, 37.579f, 71.279f), 180.215f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_y_clubcust_03", new Vector3(927.904f, 41.925f, 71.274f), 179.576f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_y_bevhills_01", new Vector3(953.208f, 61.601f, 69.833f), 339.76f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_vinewood_04", new Vector3(953.94f, 61.392f, 69.833f), 344.716f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_scdressy_01", new Vector3(955.158f, 61.174f, 69.833f), 356.136f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_genhot_01", new Vector3(965.489f, 72.251f, 69.833f), 196.752f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_malibu_01", new Vector3(964.468f, 71.471f, 69.833f), 221.392f, "WORLD_HUMAN_STAND_MOBILE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_vinewood_04", new Vector3(987.52f, 57.652f, 68.833f), 204.379f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "s_m_m_hairdress_01", new Vector3(986.732f, 57.113f, 68.866f), 222.62f, "WORLD_HUMAN_STAND_MOBILE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "ig_taostranslator", new Vector3(982.362f, 46.761f, 69.238f), 0.99f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "u_f_m_miranda_02", new Vector3(969.109f, 46.507f, 69.833f), 82.089f, "WORLD_HUMAN_STAND_MOBILE");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_carbuyer", new Vector3(969.066f, 47.366f, 69.833f), 83.4f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "cs_dale", new Vector3(968.666f, 45.528f, 69.833f), 81.227f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "ig_taostranslator", new Vector3(961.332f, 52.542f, 69.833f), 203.484f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_y_hipster_02", new Vector3(988.817f, 49.03f, 68.833f), 352.551f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "u_f_m_miranda_02", new Vector3(989.748f, 49.294f, 68.832f), 6.127f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_scdressy_01", new Vector3(962.446f, 52.816f, 69.833f), 207.774f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_prolhost_01", new Vector3(945.779f, 22.762f, 70.279f), 44.244f, "PROP_HUMAN_SEAT_CHAIR");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_scdressy_01", new Vector3(945.09f, 21.745f, 70.279f), 51.433f, "PROP_HUMAN_SEAT_DECKCHAIR_DRINK");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_bevhills_04", new Vector3(940.96f, 18.807f, 70.305f), 17.935f, "PROP_HUMAN_SEAT_DECKCHAIR_DRINK");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_stlat_02", new Vector3(941.913f, 19.163f, 70.288f), 18.65f, "PROP_HUMAN_SEAT_CHAIR_FOOD");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_scdressy_01", new Vector3(932.517f, 18.882f, 70.313f), 7.469f, "PROP_HUMAN_SEAT_CHAIR_UPRIGHT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_bevhills_04", new Vector3(933.335f, 19.048f, 70.33f), 357.385f, "PROP_HUMAN_SEAT_BENCH_DRINK_BEER");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_malibu_01", new Vector3(934.378f, 19.097f, 70.339f), 357.293f, "PROP_HUMAN_SEAT_DECKCHAIR_DRINK");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_bevhills_04", new Vector3(937.486f, 29.38f, 70.537f), 202.034f, "PROP_HUMAN_SEAT_BENCH_DRINK_BEER");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "a_m_m_malibu_01", new Vector3(938.916f, 29.585f, 70.534f), 179.067f, "PROP_HUMAN_SEAT_BENCH_DRINK_BEER");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVMALE, "ig_taostranslator", new Vector3(935.392f, 27.821f, 70.834f), 326.627f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_genhot_01", new Vector3(935.98f, 28.242f, 70.834f), 233.585f, "PROP_HUMAN_STAND_IMPATIENT");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_genhot_01", new Vector3(962.431f, 51.656f, 69.833f), 29.831f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_genhot_01", new Vector3(951.392f, 36.326f, 70.838f), 101.098f, "WORLD_HUMAN_PARTYING");
                    peds.Add(ped);
                    ped = await CreateCasinoPed(PedType.PED_TYPE_CIVFEMALE, "a_f_y_genhot_01", new Vector3(950.274f, 37.131f, 70.838f), 201.608f, "WORLD_HUMAN_SMOKING");
                    peds.Add(ped);

                    Logger.Info($"Init Ambient Peds: {peds.Count}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Init Ambient Peds");
            }
        }

        public static void Dispose()
        {
            if (peds.Count == 0) return;
            foreach(Ped ped in peds.ToArray())
            {
                if (ped.Exists())
                    ped.Dispose();
            }
            peds.Clear();
            isRunning = false;
        }

        static async Task<Ped> CreateCasinoPed(PedType pedType, string pedModel, Vector3 position, float heading, string scenario)
        {
            Model model = new Model(pedModel);
            await model.Request(10000);

            int pedHandle = CreatePed((int)pedType, (uint)model.Hash, position.X, position.Y, position.Z, heading, false, false);

            Ped ped = new Ped(pedHandle);
            ped.IsPositionFrozen = true;
            ped.IsInvincible = true;
            ped.BlockPermanentEvents = true;
            ped.Task.StartScenario(scenario, position);

            model.MarkAsNoLongerNeeded();
            return ped;
        }
    }
}
