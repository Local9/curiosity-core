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

namespace Curiosity.Racing.Client.Managers
{
    public class TrackManager : Manager<TrackManager>
    {
        CheckpointIcon CP_FINISH = CheckpointIcon.CylinderCheckerboard;
        CheckpointIcon CP_ARROW_ONE = CheckpointIcon.CylinderSingleArrow;
        CheckpointIcon CP_ARROW_TWO = CheckpointIcon.CylinderDoubleArrow;
        CheckpointIcon CP_ARROW_THREE = CheckpointIcon.CylinderTripleArrow;

        List<Checkpoint> checkpoints = new();

        public override void Begin()
        {
            
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

        //[TickHandler]
        //private async Task OnTrackManagerTick()
        //{
        //    if (Game.IsControlJustPressed(0, Control.Context))
        //    {
        //        int hudRed = 0;
        //        int hudGreen = 0;
        //        int hudBlue = 0;
        //        int hudAlpha = 0;
        //        GetHudColour(1, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);

        //        Vector3 pos = Game.PlayerPed.Position;
        //        int cp = CreateCheckpoint((int)CP_ARROW_ONE, pos.X, pos.Y, pos.Z - 1f, pos.X, pos.Y, pos.Z, 7.5f, hudRed, hudGreen, hudBlue, hudAlpha, 0);
        //        Checkpoint checkpoint = new Checkpoint(cp);

        //        GetHudColour(13, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);
        //        SetCheckpointRgba(cp, hudRed, hudGreen, hudBlue, hudAlpha);

        //        GetHudColour(134, ref hudRed, ref hudGreen, ref hudBlue, ref hudAlpha);
        //        SetCheckpointRgba2(cp, hudRed, hudGreen, hudBlue, hudAlpha);

        //        SetCheckpointCylinderHeight(cp, 1.6f, 5f, 100f);

        //        checkpoints.Add(checkpoint);

        //        UpdateCheckpoints();

        //        PlaySoundFrontend(-1, "CHECKPOINT_NORMAL", "HUD_MINI_GAME_SOUNDSET", false);
        //    }
        //}

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
