using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Classes;
using Curiosity.Shared.Client.net;

using System.Drawing;

namespace Curiosity.GameWorld.Client.net.Classes.Environment.EasterEggs
{
    class DeveloperWall
    {
        static Client client = Client.GetInstance();

        static string DEV_TXD = "DEVELOPER_TXD";
        static string DEV_TXD_NAME = DEV_TXD;
        static string DEV_TXD_LOCATION = "client/img/dev.png";

        static string MUSIC_EVENT_START = "KILL_LIST_START_MUSIC";
        static string MUSIC_EVENT_STOP = "KILL_LIST_STOP_MUSIC";

        static string PARTICLE_DICT = "proj_indep_firework";
        static List<string> ParticleNames = new List<string>()
        {
            "scr_indep_firework_grd_burst",
            "scr_indep_launcher_sparkle_spawn",
            "scr_indep_firework_air_burst",
            "proj_indep_flare_trail"
        };

        static int ParticleId = 0;

        static Ped stripper1;
        static Ped stripper2;
        static Ped stripper3;
        static Ped stripper4;

        static Vector3 imagePosition = new Vector3(144.1214f, -560.5704f, 21.99095f);
        static Vector3 positionToStand = new Vector3(144.1214f, -560.5704f, 21.99095f);

        static int startingAlpha = 0;

        static bool InsideLocation = false;

        static public void Init()
        {
            long runtimeTxd = CreateRuntimeTxd(DEV_TXD);
            CreateRuntimeTextureFromImage(runtimeTxd, DEV_TXD_NAME, DEV_TXD_LOCATION);

            client.RegisterTickHandler(LocationChecker);
        }

        static async Task LocationChecker()
        {
            try
            {
                while (true)
                {
                    await Client.Delay(0);
                    if (NativeWrappers.GetDistanceBetween(positionToStand, Game.PlayerPed.Position) <= 10f)
                    {

                        if (stripper1 == null)
                        {
                            stripper1 = await CreateStripper(Game.PlayerPed.Position + new Vector3(2f, 0f, 0f));
                            stripper2 = await CreateStripper(Game.PlayerPed.Position + new Vector3(-2f, 0f, 0f));
                            stripper3 = await CreateStripper(Game.PlayerPed.Position + new Vector3(0f, 2f, 0f));
                            stripper4 = await CreateStripper(Game.PlayerPed.Position + new Vector3(0f, -2f, 0f));
                        }

                        CommonFunctions.DrawImage3D(DEV_TXD_NAME, imagePosition, 1.6f, 1.5f, 0f, Color.FromArgb(startingAlpha, 255, 255, 255));
                        startingAlpha = startingAlpha >= 230 ? 255 : startingAlpha + 10;

                        if (PrepareMusicEvent(MUSIC_EVENT_START) && PrepareMusicEvent(MUSIC_EVENT_STOP))
                        {
                            PlayExtras();
                        }
                    }
                    else
                    {
                        Cleanup();
                        await Client.Delay(5000);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Error($"DeveloperWall -> {ex.Message}");
                }
            }
        }

        private static async Task<Ped> CreateStripper(Vector3 vector3)
        {
            Model stripper = PedHash.Stripper01SFY;
            await stripper.Request(10000);

            Ped ped = await World.CreatePed(stripper, vector3 + new Vector3(0f, 0f, -.5f));

            API.NetworkFadeInEntity(ped.Handle, true);

            stripper.MarkAsNoLongerNeeded();

            ped.CanPlayGestures = true;
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);
            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;

            ped.Task.PlayAnimation("mini@strip_club@lap_dance@ld_girl_a_song_a_p1", "ld_girl_a_song_a_p1_f", 8f, -1, AnimationFlags.Loop);

            return ped;
        }

        static async void PlayExtras()
        {
            if (InsideLocation) return;
            InsideLocation = true;

            await ShowLoopParticle(PARTICLE_DICT, ParticleNames[0], imagePosition, 2.0f, 1000);

            TriggerMusicEvent(MUSIC_EVENT_START);
        }

        static async void Cleanup()
        {
            if (!InsideLocation) return;
            InsideLocation = false;

            startingAlpha = 0;

            if (stripper1 != null)
            {

                int ref1 = stripper1.Handle, ref2 = stripper2.Handle, ref3 = stripper3.Handle, ref4 = stripper4.Handle;

                API.NetworkFadeOutEntity(ref1, false, false);
                API.NetworkFadeOutEntity(ref2, false, false);
                API.NetworkFadeOutEntity(ref3, false, false);
                API.NetworkFadeOutEntity(ref4, false, false);

                await BaseScript.Delay(1000);

                API.DeleteEntity(ref ref1);
                API.DeleteEntity(ref ref2);
                API.DeleteEntity(ref ref3);
                API.DeleteEntity(ref ref4);

                stripper1 = null;
                stripper2 = null;
                stripper3 = null;
                stripper4 = null;
            }

            StopParticleFxLooped(ParticleId, false);

            CancelMusicEvent(MUSIC_EVENT_START);
            TriggerMusicEvent(MUSIC_EVENT_STOP);
        }

        static async Task<int> ShowNonLoopParticle(string dict, string particleName, Vector3 coords, float scale)
        {
            // Request the particle dictionary.
            RequestNamedPtfxAsset(dict);
            // Wait for the particle dictionary to load.

            while (!HasNamedPtfxAssetLoaded(dict))
            {
                await Client.Delay(0);
            }

            // Tell the game that we want to use a specific dictionary for the next particle native.
            UseParticleFxAssetNextCall(dict);
            // Create a new non- looped particle effect, we don't need to store the particle handle because it will
            // automatically get destroyed once the particle has finished it's animation (it's non - looped).
            // SetParticleFxNonLoopedColour(particleHandle, 0, 255, 0, false);

            return StartParticleFxNonLoopedAtCoord(particleName, coords.X, coords.Y, coords.Z, 0.0f, 0.0f, 0.0f, scale, false, false, false);
        }

        static async Task<int> ShowLoopParticle(string dict, string particleName, Vector3 coords, float scale, int time)
        {
            // Request the particle dictionary.
            RequestNamedPtfxAsset(dict);
           // Wait for the particle dictionary to load.

            while (!HasNamedPtfxAssetLoaded(dict))
            {
                await Client.Delay(0);
            }
            // Tell the game that we want to use a specific dictionary for the next particle native.
            UseParticleFxAssetNextCall(dict);
            // Create a new non- looped particle effect, we don't need to store the particle handle because it will
            // automatically get destroyed once the particle has finished it's animation (it's non - looped).

            int particleHandle = StartParticleFxLoopedAtCoord(particleName, coords.X, coords.Y, coords.Z, 0.0f, 0.0f, 0.0f, scale, false, false, false, false);

            SetParticleFxLoopedColour(particleHandle, 0, 255, 0, false);

            await Client.Delay(time);

            StopParticleFxLooped(particleHandle, false);

            return particleHandle;
        }
    }
}
