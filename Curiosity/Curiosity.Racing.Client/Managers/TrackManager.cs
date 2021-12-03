using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System.Drawing;
using CitizenFX.Core.UI;
using Curiosity.Racing.Client.Diagnostics;
using Curiosity.Racing.Client.Utils;

namespace Curiosity.Racing.Client.Managers
{
    public class TrackManager : Manager<TrackManager>
    {
        CheckpointIcon CP_FINISH = CheckpointIcon.CylinderCheckerboard;
        CheckpointIcon CP_ARROW_ONE = CheckpointIcon.CylinderSingleArrow;
        CheckpointIcon CP_ARROW_TWO = CheckpointIcon.CylinderDoubleArrow;
        CheckpointIcon CP_ARROW_THREE = CheckpointIcon.CylinderTripleArrow;

        List<Checkpoint> checkpoints = new();

        WorldArea worldArea = new WorldArea();
        WorldArea worldArea2 = new WorldArea();
        WorldArea worldArea3 = new WorldArea();

        public override void Begin()
        {
            // worldArea.Pos1 = new Vector3(-2346.975f, 3259.757f, 31.81074f);
            worldArea.Pos1 = new Vector3(-2157.381f, 3247.418f, 31.80657f);
            worldArea.Width = 1.05f;

            worldArea2.Pos1 = new Vector3(-2162.328f, 3241.776f, 31.81028f);
            worldArea2.Pos2 = new Vector3(-2137.868f, 3227.3f, 42.81028f);
            worldArea2.Width = 1f;

            //worldArea3.Pos1 = new Vector3(-2162.328f, 3241.776f, 31.81028f);
            //worldArea3.Pos2 = new Vector3(-2137.868f, 3227.3f, 42.81028f);
            //worldArea3.MarkerScale = new Vector3(2f, 2f, 0.5f);
            //worldArea3.Width = 40f;
        }

        public void Dispose()
        {
            checkpoints.ForEach(checkpoint =>
            {
                if (checkpoint.Exists())
                    checkpoint.Delete();
            });
            checkpoints.Clear();
        }

        // [TickHandler]
        private async Task OnTrackManagerTick()
        {
            worldArea.Draw();
            worldArea2.Draw();
            // worldArea3.Draw();

            if (worldArea.IsInArea)
                Screen.ShowNotification("World One");
            if (worldArea2.IsInArea)
                Screen.ShowNotification("World Two");

            //if (Game.IsControlJustPressed(0, Control.Context))
            //{
            //    Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_portoflsheist");
            //    particleEffectsAssetNetworked.CreateEffectAtCoord("scr_bio_flare", Game.PlayerPed.Position, startNow: true);
            //    particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_bio_flare", Game.PlayerPed.Position);
            //}
        }

        private static void Adversary()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_sr_adversary");
            particleEffectsAssetNetworked.CreateEffectAtCoord("scr_sr_lg_take_zone", Game.PlayerPed.Position, startNow: true);
            particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_sr_lg_take_zone", Game.PlayerPed.Position);
        }

        private static void PowerPlay()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_powerplay");
            // particleEffectsAssetNetworked.CreateEffectAtCoord("scr_powerplay_beast_vanish", Game.PlayerPed.Position, startNow: true);
            particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_powerplay_beast_appear", Game.PlayerPed.Position);
        }

        private static void OribitalBlast()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_xm_orbital");
            // particleEffectsAssetNetworked.CreateEffectAtCoord("scr_xm_orbital_blast", Game.PlayerPed.Position, startNow: true);
            particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_xm_orbital_blast", Game.PlayerPed.Position);
        }

        private static async Task Fireworks2()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"proj_indep_firework_v2");

            for (int i = 0; i < 10; i++)
            {
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_indep_burst_rwb", Game.PlayerPed.Position, off: new Vector3(-5, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_indep_spiral_burst_rwb", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_xmas_firework_sparkle_spawn", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_indep_ring_burst_rwb", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_xmas_firework_burst_fizzle", Game.PlayerPed.Position, off: new Vector3(0, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_indep_repeat_burst_rwb", Game.PlayerPed.Position, off: new Vector3(+2, 0, +10));
                await BaseScript.Delay(2000);
            }
        }

        private static async Task XmasFireworks()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"proj_xmas_firework");

            for (int i = 0; i < 10; i++)
            {
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_xmas_ring_burst_rgw", Game.PlayerPed.Position, off: new Vector3(-5, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_xmas_ring_burst_rgw", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_xmas_burst_rgw", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_xmas_repeat_burst_rgw", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_firework_xmas_spiral_burst_rgw", Game.PlayerPed.Position, off: new Vector3(0, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_xmas_firework_sparkle_spawn", Game.PlayerPed.Position, off: new Vector3(+2, 0, +10));
                await BaseScript.Delay(2000);
            }
        }

        private static async Task Fireworks()
        {
            Utils.ParticleEffectsAssetNetworked particleEffectsAssetNetworked = new Utils.ParticleEffectsAssetNetworked($"scr_indep_fireworks");

            for (int i = 0; i < 10; i++)
            {
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_starburst", Game.PlayerPed.Position, off: new Vector3(-5, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_shotburst", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_sparkle_spawn", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_burst_spawn", Game.PlayerPed.Position, off: new Vector3(-2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_trailburst", Game.PlayerPed.Position, off: new Vector3(0, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_trail_spawn", Game.PlayerPed.Position, off: new Vector3(+2, 0, +10));
                particleEffectsAssetNetworked.StartNonLoopedAtCoordNetworked("scr_indep_firework_fountain", Game.PlayerPed.Position, off: new Vector3(+5, 0, +10));
                await BaseScript.Delay(2000);
            }
        }

        private void CreateCheckpoint()
        {
            int hudRed = 0;
            int hudGreen = 0;
            int hudBlue = 0;
            int hudAlpha = 0;
            GetHudColour(1, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);

            Vector3 pos = Game.PlayerPed.Position;
            int cp = API.CreateCheckpoint((int)CP_ARROW_ONE, pos.X, pos.Y, pos.Z - 1f, pos.X, pos.Y, pos.Z, 7.5f, hudRed, hudGreen, hudBlue, hudAlpha, 0);
            Checkpoint checkpoint = new Checkpoint(cp);

            GetHudColour(13, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);
            SetCheckpointRgba(cp, hudRed, hudGreen, hudBlue, hudAlpha);

            GetHudColour(134, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);
            SetCheckpointRgba2(cp, hudRed, hudGreen, hudBlue, hudAlpha);

            SetCheckpointCylinderHeight(cp, 1.6f, 5f, 100f);

            checkpoints.Add(checkpoint);

            UpdateCheckpoints();

            PlaySoundFrontend(-1, "CHECKPOINT_NORMAL", "HUD_MINI_GAME_SOUNDSET", false);
        }

        public void UpdateCheckpoints()
        {
            try
            {
                for (int i = 0; i < checkpoints.Count; i++)
                {
                    if (i == checkpoints.Count)
                        continue;

                    checkpoints[i].TargetPosition = checkpoints[i + 1].Position;
                }
            }
            catch(Exception ex)
            {
                // IGNORE
            }
        }
    }
}
