using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class PointFinger : Manager<PointFinger>
    {
        public bool PointingFinger { get; set; }

        private void StopPointing()
        {
            var ped = Cache.Entity.Id;

            API.N_0xd01015c7316ae176(ped, "Stop");

            if (!API.IsPedInjured(ped))
            {
                API.ClearPedSecondaryTask(ped);
            }

            if (!API.IsPedInAnyVehicle(ped, true))
            {
                API.SetPedCurrentWeaponVisible(ped, true, true, true, true);
            }

            API.SetPedConfigFlag(ped, 36, false);
            API.ClearPedSecondaryTask(ped);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Session.CreatingCharacter) return;

            var ped = Cache.Entity.Id;

            if (Game.IsControlJustPressed(0, Control.SpecialAbilitySecondary))
            {
                if (!PointingFinger)
                {
                    API.RequestAnimDict("anim@mp_point");

                    while (!API.HasAnimDictLoaded("anim@mp_point"))
                    {
                        await BaseScript.Delay(10);
                    }

                    API.SetPedCurrentWeaponVisible(ped, false, true, true, true);
                    API.SetPedConfigFlag(ped, 36, true);
                    API.N_0x2d537ba194896636(ped, "task_mp_pointing", 0.5f, false, "anim@mp_point", 24);
                    API.RemoveAnimDict("anim@mp_point");
                }
                else
                {
                    StopPointing();
                }

                PointingFinger = !PointingFinger;
            }

            if (API.N_0x921ce12c489c4c41(ped) && !PointingFinger)
            {
                StopPointing();
            }

            if (API.N_0x921ce12c489c4c41(ped))
            {
                if (!API.IsPedOnFoot(ped))
                {
                    StopPointing();
                }
                else
                {
                    var pitch = API.GetGameplayCamRelativePitch();

                    if (pitch < -70f)
                    {
                        pitch = -70f;
                    }
                    else if (pitch > 42f)
                    {
                        pitch = 42f;
                    }

                    pitch = (pitch + 70f) / 112f;

                    var heading = API.GetGameplayCamRelativeHeading();
                    var cos = (float)Math.Cos(heading);
                    var sin = (float)Math.Sin(heading);

                    if (heading < -180f)
                    {
                        heading = -180f;
                    }
                    else if (heading > 180f)
                    {
                        heading = 180f;
                    }

                    heading = (heading + 180f) / 360f;

                    var position = API.GetOffsetFromEntityInWorldCoords(ped,
                        cos * -0.2f - cos * (0.4f * heading + 0.3f),
                        sin * -0.2f + sin * (0.4f * heading + 0.3f), 0.6f);
                    var raycast = API.Cast_3dRayPointToPoint(position.X, position.Y, position.Z - 0.2f, position.X,
                        position.Y,
                        position.Z + 0.2f, 0.4f, 95, ped, 7);
                    var hit = false;
                    var endpos = new Vector3();
                    var surfaceNormal = new Vector3();
                    var entityId = 0;

                    API.GetRaycastResult(raycast, ref hit, ref endpos, ref surfaceNormal, ref entityId);
                    API.N_0xd5bb4025ae449a4e(ped, "Pitch", pitch);
                    API.N_0xd5bb4025ae449a4e(ped, "Heading", heading * -1.0f + 1.0f);
                    API.N_0xb0a6cfd2c69c1088(ped, "isBlocked", hit);
                    API.N_0xb0a6cfd2c69c1088(ped, "isFirstPerson",
                        API.N_0xee778f8c7e1142e2(API.N_0x19cafa3c87f7c2ff()) == 4);
                }
            }

            await Task.FromResult(0);
        }
    }
}
