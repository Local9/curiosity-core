using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
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
            // DisableScenario("lsa_planes");
            // DisableScenario("sandy_planes");
            // DisableScenario("grapeseed_planes");
            DisableScenario("ng_planes");

            await BaseScript.Delay(10000);
        }

        private void EnableScenario(string scenario)
        {
            // Logger.Debug($"EnableScenario: {scenario}");

            if (IsScenarioGroupEnabled(scenario))
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

            // Zancudo
            EnableScenario("fort_zancudo_guards");
            // Air
            EnableScenario("blimp");
            // cinemas
            EnableScenario("cinema_downtown");
            EnableScenario("cinema_morningwood");
            EnableScenario("cinema_textile");
            // Banks
            EnableScenario("city_banks");
            EnableScenario("countryside_banks");
            EnableScenario("paleto_bank");
            // police
            EnableScenario("guards_at_prison");
            EnableScenario("paleto_cops");
            EnableScenario("police_at_court");
            EnableScenario("police_pound1");
            EnableScenario("police_pound2");
            EnableScenario("police_pound3");
            EnableScenario("police_pound4");
            EnableScenario("police_pound5");
            EnableScenario("prison_towers");
            EnableScenario("prison_transport");
            EnableScenario("sandy_cops");
            EnableScenario("mp_police");
            EnableScenario("mp_police2");
            // FIB
            EnableScenario("fib_group_1");
            EnableScenario("fib_group_2");
            // Extra groups
            EnableScenario("scrap_security");
            EnableScenario("vagos_hangout");
            EnableScenario("lost_hangout");
            EnableScenario("lost_bikers");
            EnableScenario("observatory_bikers");
            EnableScenario("chinese2_hillbillies");
            EnableScenario("chinese2_lunch");
            EnableScenario("movie_studio_security");
            // facilities - Only needed on load
            //EnableScenario("facility_cannon");
            //EnableScenario("facility_main_1");
            //EnableScenario("facility_main_2");
            //EnableScenario("facility_main_3");
            // Others
            EnableScenario("dealership");
            EnableScenario("kortz_security");
            EnableScenario("ammunation");
            EnableScenario("rampage1");
            EnableScenario("sew_machine");
            EnableScenario("solomon_gate");
            EnableScenario("attract_pap");
            EnableScenario("quarry");
            // Animals
            EnableScenario("armenian_cats");
        }
    }
}
