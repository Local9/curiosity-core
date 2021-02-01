using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using System;

namespace Curiosity.Core.Client.Extensions
{
    public static class VehicleExtensions
    {

        public static Vehicle GetVehicleInFront(this Vehicle vehicle, float distance = 5f, float radius = 1.5f)
        {
            try
            {
                RaycastResult raycast = World.RaycastCapsule(vehicle.Position, vehicle.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, vehicle);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    return (Vehicle)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"GetVehicleInDirection -> {ex}");
            }
            return default(Vehicle);
        }

        public static Vehicle GetVehicleInFront(this Ped ped, float distance = 5f, float radius = 1f)
        {
            try
            {
                RaycastResult raycast = World.RaycastCapsule(ped.Position, ped.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, ped);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    return (Vehicle)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug($"GetVehicleInDirection -> {ex}");
            }
            return default(Vehicle);
        }

        public static void DrawHit(this Vehicle vehicle, float distance = 10f, float radius = 1f)
        {
            try
            {
                int r = 255;
                int g = 0;

                if (Game.IsControlPressed(0, Control.Pickup))
                {
                    r = 0;
                    g = 255;
                }

                RaycastResult raycast = World.RaycastCapsule(vehicle.Position, vehicle.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, vehicle);

                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                    API.DrawLine(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, raycast.HitPosition.X, raycast.HitPosition.Y, raycast.HitPosition.Z, r, g, 0, 255);
            }
            catch (Exception ex)
            {
                Logger.Debug($"DrawView -> {ex}");
            }
        }

    }
}
