using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using System;

namespace Curiosity.MissionManager.Client.Utils
{
    public static class Extensions
    {
        public static RaycastResult GetEntityInFrontOfPed(Ped ped, float maxDistance = 5.0f)
        {
            Vector3 offset = ped.GetOffsetPosition(new Vector3(0f, maxDistance, 0f));
            return World.Raycast(ped.Position, offset, 10, IntersectOptions.Everything, ped);
        }

        public static Entity GetEntityInFrontOfPlayer(float distance = 5f)
        {
            try
            {
                RaycastResult raycast = World.Raycast(Game.PlayerPed.Position, Game.PlayerPed.GetOffsetPosition(new Vector3(0f, distance, 0f)), IntersectOptions.Everything);
                if (raycast.DitHitEntity)
                    return (Entity)raycast.HitEntity;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message}");
            }
            return default(Entity);
        }

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
                Logger.Error($"GetVehicleInDirection -> {ex}");
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
                Logger.Error($"GetVehicleInDirection -> {ex}");
            }
            return default(Vehicle);
        }

        public static Ped GetPedInFront(this Ped ped, float distance = 5f, float angle = 120f, Ped pedToCheck = null)
        {
            try
            {
                Vector3 playerPos = Game.PlayerPed.Position;
                Vector3 offset = new Vector3(0f, 3f, 0f);
                Vector3 worldOffset = Game.PlayerPed.GetOffsetPosition(offset);

                if (pedToCheck != null)
                {
                    if (API.IsEntityInAngledArea(pedToCheck.Handle, playerPos.X, playerPos.Y, playerPos.Z, worldOffset.X, worldOffset.Y, worldOffset.Z, angle, false, true, 0))
                    {
                        return pedToCheck;
                    }
                }

                RaycastResult raycast = World.Raycast(ped.Position, ped.GetOffsetPosition(new Vector3(0f, distance, 0f)), IntersectOptions.Peds1, ped);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsPed)
                {
                    return (Ped)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"GetPedInDirection -> {ex}");
            }
            return default(Ped);
        }

        public static void DrawEntityHit(this Vehicle vehicle, float distance = 10f, float radius = 1f)
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
                Logger.Error($"DrawView -> {ex}");
            }
        }
    }
}
