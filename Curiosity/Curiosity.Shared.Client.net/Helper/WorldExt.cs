using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Shared.Client.net.Helpers
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

        public static CitizenFX.Core.Vehicle GetVehicleInFront(this Vehicle vehicle, float distance = 5f)
        {
            try
            {
                RaycastResult raycast = World.Raycast(vehicle.Position, vehicle.GetOffsetPosition(new Vector3(0f, distance, 0f)), (IntersectOptions)71, vehicle);
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

        public static CitizenFX.Core.Vehicle GetVehicleInFront(this Ped ped, float distance = 5f)
        {
            try
            {
                RaycastResult raycast = World.Raycast(ped.Position, ped.GetOffsetPosition(new Vector3(0f, distance, 0f)), (IntersectOptions)71, ped);
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

        public static CitizenFX.Core.Ped GetPedInFront(this Ped ped, float distance = 5f)
        {
            try
            {
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
    }
}
