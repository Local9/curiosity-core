using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.Utils;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Exceptions;
using Curiosity.Missions.Client.Managers;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.Scripts.PedCreators;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Helper.Area;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Mission.PoliceMissions
{
    class HumainLabs
    {
        private const string TRIGGER1 = "hlTrigger1";
        private const string TRIGGER2 = "hlTrigger2";
        private const string TRIGGER3 = "hlTrigger3";
        private const string TRIGGER4 = "hlTrigger4";
        private const string TRIGGER5 = "hlTrigger5";
        private const string TRIGGER6 = "hlTrigger6";
        private const string TRIGGER7 = "hlTrigger7";
        private const string TRIGGER8 = "hlTrigger8";
        private const string TRIGGER9 = "hlTrigger9";
        private const string TRIGGER10 = "hlTrigger10";
        private const string MISSIONTRIGGER = "hlTriggerMissionLocation";
        static PluginManager PluginInstance => PluginManager.Instance;

        static ConcurrentDictionary<string, AreaSphere> _missionTriggers = new ConcurrentDictionary<string, AreaSphere>();

        static Vector3 _location = Vector3.Zero;
        static Blip _blip;

        static bool DebugAreas = false;
        static bool MissionActive = false;
        static bool HasSpawnedInitialNpcs = false;

        public static void Init()
        {
            API.RegisterCommand("hlmission", new Action<int, List<object>, string>(CommandHlMission), false);
            API.RegisterCommand("boss", new Action<int, List<object>, string>(BossTest), false);
            API.RegisterCommand("zombie", new Action<int, List<object>, string>(OnZombieCommand), false);
            API.RegisterCommand("makePed", new Action<int, List<object>, string>(OnMakePed), false);

            PluginInstance.RegisterEventHandler("curiosity:missions:player:spawn", new Action(CreateMission));
            PluginInstance.RegisterEventHandler("curiosity:missions:player:invalid", new Action(InvalidMission));
            PluginInstance.RegisterEventHandler("curiosity:missions:player:clean", new Action(CleanMission));

            PluginInstance.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action<string, dynamic>(OnAreaEnter));
            PluginInstance.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action<string, dynamic>(OnAreaExit));

            PluginInstance.RegisterEventHandler("curiosity:Client:Player:Environment:DrawAreas", new Action<bool>(OnDrawAreas));
        }

        private static async void OnMakePed(int arg1, List<object> arg2, string arg3)
        {
            Vector3 offset = Game.PlayerPed.Position + new Vector3(0f, 3f, 0f);

            Model model = PedHash.Acult02AMY;
            model.Request(10000);

            while (!model.IsLoaded)
            {
                await PluginManager.Delay(100);
            }

            int pedId = API.CreatePed((int)PedTypes.PED_TYPE_CRIMINAL, (uint)model.Hash, offset.X, offset.Y, offset.Z, 0f, false, false);
        }

        private static async void OnZombieCommand(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!ClientInformation.IsTrusted()) return;

                int numberToSpawn = Utility.RANDOM.Next(3, 8);

                int runs = arguments.Count > 0 ? int.Parse($"{arguments[0]}") : 1;

                if (runs > 10)
                    runs = 10;

                for (int r = 0; r < runs; r++)
                {
                    await PluginManager.Delay(50);
                    for (int i = 0; i < numberToSpawn; i++)
                    {
                        await PluginManager.Delay(50);
                        CreateZombie();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{ex.InnerException}");
            }
        }

        static async void CreateZombie()
        {
            float rnd = Utility.RANDOM.Next(-15, 15);
            float rnd2 = Utility.RANDOM.Next(-15, 15);
            Vector3 offset = Game.PlayerPed.Position + new Vector3(rnd, rnd2, 0f);
            float groundPosZ = 0f;

            if (API.GetGroundZFor_3dCoord(offset.X, offset.Y, offset.Z, ref groundPosZ, false))
            {
                offset.Z = groundPosZ;
            }

            List<dynamic> pedHashList = PedModelListUniqueFemale;

            if (Utility.RANDOM.NextBool(50))
            {
                pedHashList = PedModelListUniqueMale;
            }

            if (Utility.RANDOM.NextBool(10))
            {
                pedHashList = PedModelListSpecialMale;
            }

            if (Utility.RANDOM.NextBool(10))
            {
                pedHashList = PedModelListSpecialFemale;
            }

            object pedHash = pedHashList[Utility.RANDOM.Next(0, pedHashList.Count - 1)];

            await CreateZombiePed(offset.X, offset.Y, offset.Z, Game.PlayerPed.Heading, API.GetHashKey($"{pedHash}"));
        }

        static List<dynamic> PedModelListSpecialMale = new List<object>() {
            "s_m_m_ammucountry",
            "s_m_m_autoshop_01",
            "s_m_m_autoshop_02",
            "s_m_m_cntrybar_01",
            "s_m_m_dockwork_01",
            "s_m_m_doctor_01",
            "s_m_m_gaffer_01",
            "s_m_m_gardener_01",
            "s_m_m_gentransport",
            "s_m_m_hairdress_01",
            "s_m_m_janitor",
            "s_m_m_lathandy_01",
            "s_m_m_lifeinvad_01",
            "s_m_m_linecook",
            "s_m_m_lsmetro_01",
            "s_m_m_mariachi_01",
            "s_m_m_migrant_01",
            "s_m_m_movalien_01",
            "s_m_m_movprem_01",
            "s_m_m_movspace_01",
            "s_m_m_paramedic_01",
            "s_m_m_pilot_01",
            "s_m_m_pilot_02",
            "s_m_m_postal_01",
            "s_m_m_postal_02",
            "s_m_m_scientist_01",
            "s_m_m_strperf_01",
            "s_m_m_strpreach_01",
            "s_m_m_strvend_01",
            "s_m_m_trucker_01",
            "s_m_m_ups_01",
            "s_m_m_ups_02",
            "s_m_o_busker_01",
            "s_m_y_airworker",
            "s_m_y_ammucity_01",
            "s_m_y_autopsy_01",
            "s_m_y_barman_01",
            "s_m_y_baywatch_01",
            "s_m_y_busboy_01",
            "s_m_y_chef_01",
            "s_m_y_clown_01",
            "s_m_y_construct_01",
            "s_m_y_construct_02",
            "s_m_y_dealer_01",
            "s_m_y_devinsec_01",
            "s_m_y_dockwork_01",
            "s_m_y_dwservice_01",
            "s_m_y_dwservice_02",
            "s_m_y_factory_01",
            "s_m_y_fireman_01",
            "s_m_y_garbage",
            "s_m_y_grip_01",
            "s_m_y_mime",
            "s_m_y_pestcont_01",
            "s_m_y_pilot_01",
            "s_m_y_prismuscl_01",
            "s_m_y_prisoner_01",
            "s_m_y_robber_01",
            "s_m_y_shop_mask",
            "s_m_y_strvend_01",
            "s_m_y_valet_01",
            "s_m_y_waiter_01",
            "s_m_y_winclean_01",
            "s_m_y_xmech_01",
            "s_m_y_xmech_02"
        };

        static List<dynamic> PedModelListSpecialFemale = new List<object>() {
            "s_f_m_fembarber",
            "s_f_m_maid_01",
            "s_f_m_shop_high",
            "s_f_m_sweatshop_01",
            "s_f_y_airhostess_01",
            "s_f_y_bartender_01",
            "s_f_y_baywatch_01",
            "s_f_y_factory_01",
            "s_f_y_hooker_01",
            "s_f_y_hooker_02",
            "s_f_y_hooker_03",
            "s_f_y_migrant_01",
            "s_f_y_movprem_01",
            "s_f_y_scrubs_01",
            "s_f_y_shop_low",
            "s_f_y_shop_mid",
            "s_f_y_stripperlite",
            "s_f_y_stripper_01",
            "s_f_y_stripper_02",
            "s_f_y_sweatshop_01"
        };

        static List<dynamic> PedModelListUniqueMale = new List<object>() {
            "u_m_m_aldinapoli",
            "u_m_m_bankman",
            "u_m_m_bikehire_01",
            "u_m_m_fibarchitect",
            "u_m_m_filmdirector",
            "u_m_m_glenstank_01",
            "u_m_m_griff_01",
            "u_m_m_jesus_01",
            "u_m_m_jewelthief",
            "u_m_m_markfost",
            "u_m_m_partytarget",
            "u_m_m_prolsec_01",
            "u_m_m_promourn_01",
            "u_m_m_rivalpap",
            "u_m_m_spyactor",
            "u_m_m_willyfist",
            "u_m_o_finguru_01",
            "u_m_o_taphillbilly",
            "u_m_o_tramp_01",
            "u_m_y_abner",
            "u_m_y_antonb",
            "u_m_y_babyd",
            "u_m_y_baygor",
            "u_m_y_burgerdrug_01",
            "u_m_y_chip",
            "u_m_y_cyclist_01",
            "u_m_y_fibmugger_01",
            "u_m_y_guido_01",
            "u_m_y_gunvend_01",
            "u_m_y_hippie_01",
            "u_m_y_imporage",
            "u_m_y_justin",
            "u_m_y_mani",
            "u_m_y_militarybum",
            "u_m_y_paparazzi",
            "u_m_y_party_01",
            "u_m_y_pogo_01",
            "u_m_y_prisoner_01",
            "u_m_y_proldriver_01",
            "u_m_y_rsranger_01",
            "u_m_y_sbike",
            "u_m_y_staggrm_01",
            "u_m_y_tattoo_01",
            "u_m_y_zombie_01"
        };

        static List<dynamic> PedModelListUniqueFemale = new List<object>() {
            "u_f_m_corpse_01",
            "u_f_m_miranda",
            "u_f_m_promourn_01",
            "u_f_o_moviestar",
            "u_f_o_prolhost_01",
            "u_f_y_bikerchic",
            "u_f_y_comjane",
            // "u_f_y_corpse_01",
            // "u_f_y_corpse_02",
            "u_f_y_hotposh_01",
            "u_f_y_jewelass_01",
            "u_f_y_mistress",
            "u_f_y_poppymich",
            "u_f_y_princess",
            "u_f_y_spyactress"
        };

        private static async void BossTest(int playerHandle, List<object> arguments, string raw)
        {
            if (!ClientInformation.IsTrusted()) return;

            Vector3 offset = Game.PlayerPed.Position + new Vector3(0f, 3f, 0f);

            await CreatePed(offset.X, offset.Y, offset.Z, Game.PlayerPed.Heading, API.GetHashKey("u_m_y_juggernaut_01"));
            //await CreatePed(offset.X, offset.Y, offset.Z, Game.PlayerPed.Heading, API.GetHashKey("u_m_y_juggernaut_01"));
            //await CreatePed(offset.X, offset.Y, offset.Z, Game.PlayerPed.Heading, API.GetHashKey("u_m_y_juggernaut_01"));
            //await CreatePed(offset.X, offset.Y, offset.Z, Game.PlayerPed.Heading, API.GetHashKey("u_m_y_juggernaut_01"));

            //Model model = API.GetHashKey("u_m_y_juggernaut_01");
            //await model.Request(10000);
            //Ped ped = await World.CreatePed(model, offset);
            //model.MarkAsNoLongerNeeded();

            //int type = 1;

            //if (arguments.Count > 0)
            //    type = int.Parse($"{arguments[0]}");

            //if (type == 1)
            //{
            //    SetPedPropIndex(ped.Handle, 0, 0, 0, false);
            //    SetPedComponentVariation(ped.Handle, 0, 0, 1, 0);
            //    SetPedComponentVariation(ped.Handle, 3, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 8, 0, 1, 0);
            //    SetPedComponentVariation(ped.Handle, 10, 0, 1, 0);
            //    return;
            //}

            //if (type == 2)
            //{
            //    SetPedPropIndex(ped.Handle, 0, 0, 0, false);
            //    SetPedComponentVariation(ped.Handle, 0, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 3, 0, 1, 0);
            //    SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 8, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 10, 0, 0, 0);
            //    return;
            //}

            //if (type == 3)
            //{
            //    ClearPedProp(ped.Handle, 0);
            //    SetPedComponentVariation(ped.Handle, 0, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 3, 0, 1, 0);
            //    SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 8, 0, 0, 0);
            //    SetPedComponentVariation(ped.Handle, 10, 0, 0, 0);
            //    return;
            //}


        }

        private static void CommandHlMission(int playerHandle, List<object> arguments, string raw)
        {
            if (!PlayerManager.IsDeveloper) return;

            if (arguments.Count > 0)
                DebugAreas = true;

            CreateMission();
        }

        private static void OnAreaExit(string identifier, dynamic data)
        {
            if (!identifier.Contains("hlTrigger")) return;

            if (identifier == MISSIONTRIGGER)
            {
                // CLEAR
            }
        }

        private static async void OnAreaEnter(string identifier, dynamic data)
        {
            try
            {
                if (!identifier.Contains("hlTrigger")) return;

                if (DebugAreas)
                    Screen.ShowNotification($"Trigger: {identifier}");

                if (identifier == MISSIONTRIGGER)
                {
                    // _missionTriggers.TryRemove(MISSIONTRIGGER, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN

                    if (HasSpawnedInitialNpcs) return;
                    HasSpawnedInitialNpcs = true;

                    await CreatePed(3611.027f, 3728.94f, 29.68939f, 308.7826f);
                    await CreatePed(3623.014f, 3728.33f, 28.69011f, 355.1138f);
                    await CreatePed(3609.912f, 3745.168f, 28.6901f, 304.7286f);
                    await CreatePed(3605.438f, 3735.31f, 28.69009f, 325.1797f);
                    await CreatePed(3610.265f, 3713.552f, 29.6894f, 319.5776f);
                    await CreatePed(3601.898f, 3707.934f, 29.6894f, 6.761765f);
                }

                if (identifier == TRIGGER1)
                {
                    await CreatePed(3600.922f, 3716.216f, 29.68941f, 160.6745f);
                    await CreatePed(3598.575f, 3714.219f, 29.6894f, 183.3985f);

                    _missionTriggers.TryRemove(TRIGGER1, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER2)
                {
                    CheckTriggerIsRemoved(TRIGGER1);

                    await CreatePed(3596.407f, 3718.502f, 29.68941f, 328.133f);
                    await CreatePed(3593.634f, 3706.342f, 29.68941f, 236.0788f);

                    _missionTriggers.TryRemove(TRIGGER2, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER3)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);

                    await CreatePed(3597.94f, 3690.484f, 28.8214f, 20.90239f);
                    await CreatePed(3594.008f, 3695.081f, 28.82139f, 43.89056f);
                    await CreatePed(3602.753f, 3688.477f, 28.82139f, 49.60534f);

                    _missionTriggers.TryRemove(TRIGGER3, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER4)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);

                    await CreatePed(3585.394f, 3691.344f, 27.12185f, 258.396f);
                    await CreatePed(3567.439f, 3700.764f, 28.12148f, 186.5913f);
                    await CreatePed(3560.268f, 3696.225f, 30.12151f, 253.0495f);
                    await CreatePed(3590.57f, 3685.909f, 27.62151f, 260.2165f);
                    await CreatePed(3582.299f, 3694.177f, 27.12185f, 228.9425f);

                    _missionTriggers.TryRemove(TRIGGER4, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER5)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);

                    await CreatePed(3558.471f, 3666.603f, 28.12189f, 341.537f);
                    await CreatePed(3562.876f, 3683.196f, 28.12189f, 321.4918f);
                    await CreatePed(3560.752f, 3678.526f, 28.12187f, 55.01857f);
                    await CreatePed(3551.098f, 3664.566f, 28.12189f, 208.1347f);

                    _missionTriggers.TryRemove(TRIGGER5, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER6)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);

                    await CreatePed(3545.969f, 3645.399f, 28.12189f, 352.4461f);
                    await CreatePed(3552.627f, 3656.957f, 28.12189f, 91.69083f);

                    _missionTriggers.TryRemove(TRIGGER6, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER7)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);

                    await CreatePed(3534.463f, 3672.313f, 28.12114f, 133.2211f);
                    await CreatePed(3530.158f, 3673.195f, 28.12114f, 172.5429f);
                    await CreatePed(3547.29f, 3641.687f, 28.12189f, 70.07915f);
                    await CreatePed(3531.382f, 3650.277f, 27.52158f, 274.3347f);

                    _missionTriggers.TryRemove(TRIGGER7, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER8)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);

                    _missionTriggers.TryRemove(TRIGGER8, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER9)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);
                    CheckTriggerIsRemoved(TRIGGER8);

                    await CreatePed(3535.261f, 3672.048f, 20.99179f, 263.4342f);
                    await CreatePed(3524.487f, 3681.816f, 20.99179f, 172.2374f);
                    await CreatePed(3527.599f, 3693.367f, 20.99179f, 83.60178f);

                    _missionTriggers.TryRemove(TRIGGER9, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER10)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);
                    CheckTriggerIsRemoved(TRIGGER8);
                    CheckTriggerIsRemoved(TRIGGER9);

                    await CreatePed(3524.206f, 3711.529f, 20.99178f, 171.4173f, API.GetHashKey("u_m_y_juggernaut_01"));
                    await CreatePed(3528.076f, 3711.494f, 20.99179f, 180.5266f);
                    await CreatePed(3521.714f, 3712.648f, 20.99179f, 200.29f);

                    _missionTriggers.TryRemove(TRIGGER10, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }
            }
            catch (InvalidOrderException ex)
            {
                // KILL THE MISSION
                KillTheMission(true);
            }
        }

        private static void KillTheMission(bool teleportPlayers = false)
        {
            // remove all triggers

            if (_missionTriggers.ContainsKey(TRIGGER1))
                _missionTriggers.TryRemove(TRIGGER1, out AreaSphere t1);
            if (_missionTriggers.ContainsKey(TRIGGER2))
                _missionTriggers.TryRemove(TRIGGER2, out AreaSphere t2);
            if (_missionTriggers.ContainsKey(TRIGGER3))
                _missionTriggers.TryRemove(TRIGGER3, out AreaSphere t3);
            if (_missionTriggers.ContainsKey(TRIGGER4))
                _missionTriggers.TryRemove(TRIGGER4, out AreaSphere t4);
            if (_missionTriggers.ContainsKey(TRIGGER5))
                _missionTriggers.TryRemove(TRIGGER5, out AreaSphere t5);
            if (_missionTriggers.ContainsKey(TRIGGER6))
                _missionTriggers.TryRemove(TRIGGER6, out AreaSphere t6);
            if (_missionTriggers.ContainsKey(TRIGGER7))
                _missionTriggers.TryRemove(TRIGGER7, out AreaSphere t7);
            if (_missionTriggers.ContainsKey(TRIGGER8))
                _missionTriggers.TryRemove(TRIGGER8, out AreaSphere t8);
            if (_missionTriggers.ContainsKey(TRIGGER9))
                _missionTriggers.TryRemove(TRIGGER9, out AreaSphere t9);
            if (_missionTriggers.ContainsKey(TRIGGER10))
                _missionTriggers.TryRemove(TRIGGER10, out AreaSphere t10);

            ClearAreaOfPeds(_location.X, _location.Y, _location.Z, 300f, 1);
            ClearAreaOfCops(_location.X, _location.Y, _location.Z, 300f, 0);
            ClearAreaOfProjectiles(_location.X, _location.Y, _location.Z, 300f, true);

            if (teleportPlayers)
            {
                GetPlayersInArea().ForEach(p =>
                {
                    string encoded = "";
                    BaseScript.TriggerServerEvent("curiosity:Server:Mission:MovePlayer", encoded);
                });
            }
        }

        private static void CheckTriggerIsRemoved(string trigger)
        {
            if (_missionTriggers.ContainsKey(trigger))
            {
                throw new InvalidOrderException("Invalid order of triggers");
            }
        }

        static void OnDrawAreas(bool state)
        {
            if (!PlayerManager.IsDeveloper) return;

            DebugAreas = state;
        }

        private static void InvalidMission()
        {
            CleanMission();
            PluginManager.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");
        }

        private static void CleanMission()
        {
            if (_blip != null)
            {
                if (_blip.Exists())
                {
                    _blip.Delete();
                }
            }
        }

        private static void AddTrigger(string identifier, Vector3 position, float radius = 5f)
        {
            AddTrigger(identifier, position, Color.FromArgb(255, 0, 0), radius);
        }

        private static void AddTrigger(string identifier, Vector3 position, System.Drawing.Color color, float radius = 5f)
        {
            AreaSphere areaSphere = new AreaSphere();
            areaSphere.Pos = position;
            areaSphere.Radius = radius;
            areaSphere.Identifier = identifier;
            areaSphere.Color = color;
            _missionTriggers.GetOrAdd(areaSphere.Identifier, areaSphere);
        }

        private static async void CreateMission()
        {
            await BaseScript.Delay(0);

            _location = new Vector3(3611.552f, 3720.873f, 29.68941f);

            ClearAreaOfPeds(_location.X, _location.Y, _location.Z, 300f, 1);
            ClearAreaOfCops(_location.X, _location.Y, _location.Z, 300f, 0);
            ClearAreaOfProjectiles(_location.X, _location.Y, _location.Z, 300f, true);

            AddTrigger(MISSIONTRIGGER, _location, Color.FromArgb(0, 255, 0), 250f);

            AddTrigger(TRIGGER1, new Vector3(3611.552f, 3720.873f, 29.68941f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER2, new Vector3(3599.848f, 3716.641f, 29.68941f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER3, new Vector3(3589.642f, 3707.065f, 29.68545f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER4, new Vector3(3597.859f, 3690.435f, 28.82138f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER5, new Vector3(3569.469f, 3694.422f, 28.12245f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER6, new Vector3(3555.890f, 3676.128f, 28.12187f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER7, new Vector3(3547.526f, 3645.309f, 28.12189f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER8, new Vector3(3529.248f, 3654.237f, 27.52158f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER9, new Vector3(3540.52f, 3675.612f, 20.99179f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER10, new Vector3(3524.037f, 3690.067f, 20.9918f), Color.FromArgb(0, 0, 255));

            Alert();
            CreateMissionBlip();

            PluginInstance.RegisterTickHandler(OnTriggerCheck);
            PluginInstance.RegisterTickHandler(OnCompleteCheck);
        }

        private static async Task OnTriggerCheck()
        {
            await Task.FromResult(0);

            ConcurrentDictionary<string, AreaSphere> copy = _missionTriggers;

            foreach (KeyValuePair<string, AreaSphere> pair in copy)
            {
                pair.Value.Check();

                if (DebugAreas)
                {
                    NativeWrappers.Draw3DText(pair.Value.Pos.X, pair.Value.Pos.Y, pair.Value.Pos.Z, pair.Value.Identifier);
                    pair.Value.Draw();
                }
            }
        }

        private static async Task OnCompleteCheck()
        {
            await Task.FromResult(0);
            //if (AreAllPedsAreCleared())
            //{
            //    CleanMission();

            //    List<Player> players = GetPlayersInArea();


            //    string json = "";
            //    BaseScript.TriggerServerEvent("curiosity:Server:Missions:CompletedGroupMission", Encode.StringToBase64(json));
            //}
        }

        private static void Alert()
        {
            PluginManager.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 3", $"Humain Labs", "Humain labs is being raided!", 2);
            PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
            SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Utility.RANDOM.Next(1, 3)} ATTENTION_ALL_UNITS/ATTENTION_ALL_UNITS_0{Utility.RANDOM.Next(1, 5)} WE_HAVE/WE_HAVE_0{Utility.RANDOM.Next(1, 3)} CRIMES/CRIME_GUNFIRE_0{Utility.RANDOM.Next(1, 4)} CONJUNCTIVES/AT_0{Utility.RANDOM.Next(1, 3)} AREAS/AREA_HUMANE_LABS");
        }

        private static void CreateMissionBlip()
        {
            _blip = World.CreateBlip(_location);
            _blip.Sprite = BlipSprite.BigCircle;
            _blip.Scale = 0.5f;
            _blip.Color = (BlipColor)5;
            _blip.Alpha = 126;
            _blip.ShowRoute = true;
            _blip.Priority = 9;
            _blip.IsShortRange = true;

            SetBlipDisplay(_blip.Handle, 5);
        }

        private static async Task CreatePed(float x, float y, float z, float heading)
        {
            Model model = Utility.RANDOM.Next(2) == 1 ? PedHash.Lost01GMY : Utility.RANDOM.Next(2) == 1 ? PedHash.Lost03GMY : PedHash.Lost02GMY;
            await CreatePed(x, y, z, heading, model);
        }

        private static async Task CreateZombiePed(float x, float y, float z, float heading, Model selectedModel)
        {
            await BaseScript.Delay(10);
            if (DebugAreas)
            {
                AddTrigger($"npc_{Utility.RANDOM.Next(999999999)}", new Vector3(x, y, z), Color.FromArgb(255, 0, 0), 2f);
                return;
            }

            Vector3 position = new Vector3(x, y, z);
            Model model = selectedModel;
            await model.Request(10000);

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            API.RequestCollisionAtCoord(x, y, z);

            Ped spawnedPed = await World.CreatePed(model, position, heading);

            API.NetworkFadeInEntity(spawnedPed.Handle, false);

            spawnedPed.DropsWeaponsOnDeath = false;
            // mission maker
            ZombieCreator.InfectPed(spawnedPed, 300, Utility.RANDOM.NextBool(20));
            model.MarkAsNoLongerNeeded();

        }

        private static async Task CreatePed(float x, float y, float z, float heading, Model selectedModel)
        {
            await BaseScript.Delay(10);
            if (DebugAreas)
            {
                AddTrigger($"npc_{Utility.RANDOM.Next(999999999)}", new Vector3(x, y, z), Color.FromArgb(255, 0, 0), 2f);
                return;
            }

            Vector3 position = new Vector3(x, y, z);
            Model model = selectedModel;
            await model.Request(10000);

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            API.RequestCollisionAtCoord(x, y, z);

            Ped spawnedPed = await World.CreatePed(model, position, heading);
            // settings
            WeaponHash weaponHash = Utility.RANDOM.Next(2) == 1 ? WeaponHash.SawnOffShotgun : Utility.RANDOM.Next(2) == 1 ? WeaponHash.AssaultRifle : WeaponHash.MicroSMG;
            spawnedPed.Armor = 100;

            if (spawnedPed.Model.Hash == API.GetHashKey("u_m_y_juggernaut_01"))
            {
                spawnedPed.Health = 5000;
                spawnedPed.CanRagdoll = false;
                spawnedPed.CanSufferCriticalHits = false;
                spawnedPed.FiringPattern = FiringPattern.FullAuto;
                spawnedPed.IsMeleeProof = true;

                weaponHash = WeaponHash.Minigun;
            }

            spawnedPed.Weapons.Give(weaponHash, 999, true, true);
            spawnedPed.DropsWeaponsOnDeath = false;
            // mission maker
            MissionPed missionPed = MissionPedCreator.Ped(spawnedPed, Extensions.Alertness.FullyAlert, Extensions.Difficulty.BringItOn);
            Decorators.Set(missionPed.Handle, Decorators.PED_MISSION, true);
            model.MarkAsNoLongerNeeded();
        }

        private static List<Player> GetPlayersInArea()
        {
            return PluginManager.players.Select(p => p).Where(x => x.Character.Position.Distance(_location) <= 250).ToList();
        }
    }
}
