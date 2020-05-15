using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class WorldExt
    {
        public static RaycastResult GetEntityInFrontOfPed(Ped ped, float maxDistance = 5.0f)
        {
            Vector3 offset = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, ped, 0.0, 5.0, 0.0);
            return Function.Call<RaycastResult>(Hash._START_SHAPE_TEST_RAY, ped.Position.X, ped.Position.Y, ped.Position.Z, offset.X, offset.Y, offset.Z, 10, ped, 0);
        }

        public static CitizenFX.Core.Entity GetEntityInFrontOfPlayer(float distance = 5f)
        {
            try
            {
                RaycastResult raycast = CitizenFX.Core.World.Raycast(CitizenFX.Core.Game.PlayerPed.Position, CitizenFX.Core.Game.PlayerPed.GetOffsetPosition(new Vector3(0f, distance, 0f)), IntersectOptions.Everything);
                if (raycast.DitHitEntity)
                    return (CitizenFX.Core.Entity)raycast.HitEntity;
            }
            catch (Exception ex)
            {
                Log.Info($"{ex.Message}");
            }
            return default(CitizenFX.Core.Entity);
        }

        public static CitizenFX.Core.Vehicle GetVehicleInFront(this Vehicle vehicle, float distance = 5f, float radius = 1f)
        {
            try
            {
                RaycastResult raycast = World.RaycastCapsule(vehicle.Position, vehicle.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, vehicle);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    return (CitizenFX.Core.Vehicle)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"GetVehicleInDirection -> {ex}");
            }
            return default(CitizenFX.Core.Vehicle);
        }

        public static CitizenFX.Core.Vehicle GetVehicleInFront(this Ped ped, float distance = 5f, float radius = 1f)
        {
            try
            {
                RaycastResult raycast = World.RaycastCapsule(ped.Position, ped.GetOffsetPosition(new Vector3(0f, distance, 0f)), radius, (IntersectOptions)10, ped);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsVehicle)
                {
                    return (CitizenFX.Core.Vehicle)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"GetVehicleInDirection -> {ex}");
            }
            return default(CitizenFX.Core.Vehicle);
        }

        public static CitizenFX.Core.Ped GetPedInFront(this Ped ped, float distance = 5f, float angle = 120f, Ped pedToCheck = null)
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
                        Log.Verbose($"Found ped in angled area");
                        return pedToCheck;
                    }
                }

                RaycastResult raycast = World.Raycast(ped.Position, ped.GetOffsetPosition(new Vector3(0f, distance, 0f)), IntersectOptions.Peds1, ped);
                if (raycast.DitHitEntity && raycast.HitEntity.Model.IsPed)
                {
                    return (CitizenFX.Core.Ped)raycast.HitEntity;
                }
            }
            catch (Exception ex)
            {
                Log.Info($"GetPedInDirection -> {ex}");
            }
            return default(CitizenFX.Core.Ped);
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
                Log.Info($"DrawView -> {ex}");
            }
        }
    }
}
