namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldChristmasManager : Manager<WorldChristmasManager>
    {
        bool isChristmasMonth = false;
        Prop externalChristmasTree = null;
        int externalChristmasTreePedScenario = 0;

        public override void Begin()
        {
            // On restart stop delete christmas tree if it exists
            Event("onClientResourceStop", new Action<string>(OnClientResourceStop));

            isChristmasMonth = DateTime.Now.Month == 12;

            if (isChristmasMonth)
            {
                SetUpChristmasTreeAsync();
                OverrideFireworksAsync();
                SetupSnowVfx();

                SetOverrideWeather("XMAS");
            }
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;

            Disable();
        }

        private void Disable()
        {
            if (externalChristmasTree != null)
                externalChristmasTree.Delete();

            RemoveScenarioBlockingArea(externalChristmasTreePedScenario, true);

            ResetParticleFxOverride("scr_firework_indep_burst_rwb");
            ResetParticleFxOverride("scr_firework_indep_spiral_burst_rwb");
            ResetParticleFxOverride("scr_firework_indep_repeat_burst_rwb");
            ResetParticleFxOverride("scr_firework_indep_ring_burst_rwb");
            RemoveNamedPtfxAsset("proj_xmas_firework");

            RemoveNamedPtfxAsset("core_snow");
            ReleaseNamedScriptAudioBank("SNOW_FOOTSTEPS");
            SetForcePedFootstepsTracks(false);
            SetForceVehicleTrails(false);
        }

        private async Task SetUpChristmasTreeAsync()
        {
            await BaseScript.Delay(3000);
            if (externalChristmasTree != null) return;
            Model tree = new Model("prop_xmas_ext");
            await tree.Request(1000);
            
            while (!tree.IsLoaded)
                await BaseScript.Delay(100);

            int treeProp = CreateObjectNoOffset((uint)tree.Hash, 237.4236f, -880.7832f, 29.4971f, false, false, false);

            externalChristmasTree = new Prop(treeProp);
            externalChristmasTree.IsPositionFrozen = true;
            externalChristmasTree.Rotation = new Vector3(0f, 0f, -87.66f);
            externalChristmasTree.LodDistance = 5000;

            tree.MarkAsNoLongerNeeded();
            Vector3 pos1 = new Vector3(29.4971f, -880.7832f, 237.4236f) + new Vector3(-6f, -6f, -6f);
            Vector3 pos2 = new Vector3(29.4971f, -880.7832f, 237.4236f) + new Vector3(20f, 6f, 6f);

            externalChristmasTreePedScenario = AddScenarioBlockingArea(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z, false, true, true, true);
        }

        private async Task OverrideFireworksAsync()
        {
            RequestNamedPtfxAsset("proj_xmas_firework");

            while (!HasNamedPtfxAssetLoaded("proj_xmas_firework"))
                await BaseScript.Delay(100);

            SetParticleFxOverride("scr_firework_indep_burst_rwb", "scr_firework_xmas_burst_rgw");
            SetParticleFxOverride("scr_firework_indep_spiral_burst_rwb", "scr_firework_xmas_spiral_burst_rgw");
            SetParticleFxOverride("scr_firework_indep_repeat_burst_rwb", "scr_firework_xmas_repeat_burst_rgw");
            SetParticleFxOverride("scr_firework_indep_ring_burst_rwb", "scr_firework_xmas_ring_burst_rgw");
        }

        private void SetupSnowVfx()
        {
            RequestNamedPtfxAsset("core_snow");
            RequestScriptAudioBank("SNOW_FOOTSTEPS", false);
            SetForcePedFootstepsTracks(true);
            SetForceVehicleTrails(true);
        }
    }
}
