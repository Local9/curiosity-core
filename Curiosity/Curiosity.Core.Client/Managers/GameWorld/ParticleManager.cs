using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class ParticleManager : Manager<ParticleManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("world:particle", new AsyncEventCallback(async metadata =>
            {
                Particle particle = JsonConvert.DeserializeObject<Particle>(metadata.Find<string>(0));

                World.RemoveAllParticleEffectsInRange(Game.PlayerPed.Position, 10f);
                Game.PlayerPed.RemoveAllParticleEffects();

                ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAssetNetworked(particle.Asset);
                particleEffectsAsset.Request();
                while (!particleEffectsAsset.IsLoaded) await BaseScript.Delay(0);

                Vector3 pos = particle.Position.AsVector();

                if (!particleEffectsAsset.StartNonLoopedAtCoord(particle.Name, pos))
                    particleEffectsAsset.CreateEffectAtCoord(particle.Name, pos);

                if (particle.Name == "scr_xm_orbital_blast")
                {
                    foreach (Player player in PluginManager.Instance.PlayerList)
                    {
                        if (player.Character.Exists())
                        {
                            if (player.Character.IsInRangeOf(pos, 15f))
                            {
                                AddOwnedExplosion(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, 59, 1.0f, true, false, 1f);
                            }
                        }
                    }

                    Vehicle[] vehicles = World.GetAllVehicles();
                    int vehicleCount = 0;

                    while (vehicleCount < vehicles.Length)
                    {
                        Vehicle veh = vehicles[vehicleCount];

                        if (veh.Exists())
                        {
                            if (veh.IsInRangeOf(pos, 15f))
                            {
                                AddOwnedExplosion(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, 59, 1.0f, true, false, 1f);
                                veh.ExplodeNetworked();
                            }
                        }

                        vehicleCount++;
                    }

                    AddOwnedExplosion(Game.PlayerPed.Handle, pos.X, pos.Y, pos.Z, 59, 1.0f, true, false, 1f);
                    PlaySoundFromCoord(-1, "DLC_XM_Explosions_Orbital_Cannon", pos.X, pos.Y, pos.Z, string.Empty, true, 0, false);

                    if (Game.PlayerPed.IsInRangeOf(pos, 50f))
                    {
                        SetPadShake(0, 500, 256);
                        GameplayCamera.Shake(CameraShake.LargeExplosion, 1.5f);
                    }
                }

                particleEffectsAsset.MarkAsNoLongerNeeded();

                return null;
            }));
        }
    }
}
