using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class ScenarioManager : Manager<ScenarioManager>
    {
        public override void Begin()
        {
            OnSetupWorld();
            LoadScenarios();
        }

        private void InternalRequestIpl(string iplName)
        {
            if (!IsIplActive(iplName))
                RequestIpl(iplName);
        }

        private void InternalRemoveIpl(string iplName)
        {
            if (IsIplActive(iplName))
                RemoveIpl(iplName);
        }

        private async Task OnSetupWorld()
        {
            await Session.Loading();

            InternalRemoveIpl("hei_bi_hw1_13_door");
            InternalRequestIpl("bkr_bi_hw1_13_int");
            int bikerInterior = GetInteriorAtCoords(984.1553f, -95.36626f, 75.9326f);
            SetInteriorActive(bikerInterior, true);
            DisableInterior(bikerInterior, false);
            // Night Clubs
            EnableScenario("club_cypress_flats_warehouse");
            InternalRequestIpl("ba_barriers_case4");

            EnableScenario("club_del_perro_building");
            InternalRequestIpl("ba_barriers_case5");

            EnableScenario("club_vinewood_dt");
            InternalRequestIpl("ba_barriers_case8");

            EnableScenario("club_elysian_island_warehouse");
            InternalRequestIpl("ba_barriers_case7");

            EnableScenario("club_la_mesa_warehouse");
            InternalRequestIpl("ba_barriers_case0");

            EnableScenario("club_lsia_warehouse");
            InternalRequestIpl("ba_barriers_case6");

            EnableScenario("club_mission_row_building");
            InternalRequestIpl("ba_barriers_case1");

            EnableScenario("club_strawberry_warehouse");
            InternalRequestIpl("ba_barriers_case2");

            EnableScenario("club_vespucci_canals_building");
            InternalRequestIpl("ba_barriers_case9");

            EnableScenario("club_west_vinewood_building");
            InternalRequestIpl("ba_barriers_case3");

            InternalRequestIpl("ba_mpbattleipl");
            InternalRequestIpl("ba_mpbattleipl_long_0");
            InternalRequestIpl("ba_mpbattleipl_strm_0");
        }

        [TickHandler(SessionWait = true)]
        private async Task OnScenarioTick()
        {
            DisableScenario("army_guard");
            DisableScenario("army_heli");
            DisableScenario("WORLD_VEHICLE_MILITARY_PLANES_SMALL");
            DisableScenario("WORLD_VEHICLE_MILITARY_PLANES_BIG");
            DisableScenario("lsa_planes");
            DisableScenario("sandy_planes");
            DisableScenario("grapeseed_planes");
            DisableScenario("ng_planes");

            await BaseScript.Delay(10000);
        }

        private void EnableScenario(string scenario)
        {
            // Logger.Debug($"EnableScenario: {scenario}");

            if (!IsScenarioGroupEnabled(scenario))
                SetScenarioGroupEnabled(scenario, true);
        }

        private void DisableScenario(string scenario)
        {
            // Logger.Debug($"DisableScenario: {scenario}");

            if (IsScenarioGroupEnabled(scenario))
                SetScenarioGroupEnabled(scenario, false);
        }

        private async void LoadScenarios()
        {
            await Session.Loading();

            try
            {
                List<ScenarioItem> scenarios = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ScenarioItem>>(Properties.Resources.scenarios);
                foreach(ScenarioItem scenario in scenarios)
                {
                    if (scenario.Enabled)
                    {
                        EnableScenario(scenario.Scenario);
                    }
                    else
                    {
                        DisableScenario(scenario.Scenario);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error converting scenario file");
                Logger.Debug($"STACK TRACE");
                Logger.Debug($"{ex}");
            }

        }
    }
}
