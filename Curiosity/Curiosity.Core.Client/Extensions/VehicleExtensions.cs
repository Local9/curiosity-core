using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Extensions
{
    public static class VehicleExtensions
    {
        private const string BLIP_PERSONAL_VEHICLE = "blipPersonalVehicle";
        private const string BLIP_PERSONAL_TRAILER = "blipPersonalTrailer";
        private const string BLIP_PERSONAL_PLANE = "blipPersonalPlane";
        private const string BLIP_PERSONAL_BOAT = "blipPersonalBoat";
        private const string BLIP_PERSONAL_HELICOPTER = "blipPersonalHelicopter";

        /// <summary>
        /// Gets the vehicle that is being hooked.
        /// </summary>
        /// <param name="vehicle">The cargobob, truck or towtruck.</param>
        /// <returns>The Vehicle that is being hooked, null if there is nothing.</returns>
        internal static Vehicle GetHookedVehicle(this Vehicle vehicle)
        {
            // If the vehicle is invalid, return
            if (vehicle == null || !vehicle.Exists())
            {
                return null;
            }

            // Start by trying to get the vehicle attached as a trailer
            int trailer = 0;
            if (API.GetVehicleTrailerVehicle(vehicle.Handle, ref trailer))
            {
                return Entity.FromHandle(trailer) as Vehicle;
            }

            // Try to get a hooked cargobob vehicle and return it if there is somehing
            Vehicle cargobobHook = Entity.FromHandle(API.GetVehicleAttachedToCargobob(vehicle.Handle)) as Vehicle;
            if (cargobobHook != null && cargobobHook.Exists())
            {
                return cargobobHook;
            }

            // Then, try to get it as a tow truck and return it if it does
            Vehicle towHooked = Entity.FromHandle(API.GetEntityAttachedToTowTruck(vehicle.Handle)) as Vehicle;
            if (towHooked != null && towHooked.Exists())
            {
                return towHooked;
            }

            // If we got here, just send nothing
            return null;
        }

        public static void DisableCollisionsThisFrame(this Entity one, Entity two, bool print = false)
        {
            // If one of the entities is null, return
            if (one == null || two == null)
            {
                return;
            }

            // Otherwise, just disable the collisions
            API.SetEntityNoCollisionEntity(one.Handle, two.Handle, true);
            API.SetEntityNoCollisionEntity(two.Handle, one.Handle, true);

            // If we need to print the handles of the entities, do it
            if (print)
            {
                Logger.Debug($"Disabled collisions between {one.Handle} and {two.Handle}");
            }
        }

        internal static Vehicle GetVehicleInFront(this Vehicle vehicle, float distance = 5f, float radius = 1.5f)
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

        internal static Vehicle GetVehicleInFront(this Ped ped, float distance = 5f, float radius = 1f)
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

        internal static void DrawHit(this Vehicle vehicle, float distance = 10f, float radius = 1f)
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

        internal async static Task FadeOut(this Vehicle veh, bool slow = true)
        {
            await Fade(veh, false, slow);
        }

        internal async static Task FadeIn(this Vehicle veh, bool slow = true)
        {
            await Fade(veh, true, slow);
        }

        internal async static Task Fade(this Vehicle veh, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
            {
                Function.Call((Hash)0x1F4ED342ACEFE62D, veh.Handle, fadeOutNormal, slow);
                // API.NetworkFadeInEntity(veh.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(veh.Handle, fadeOutNormal, slow);
            }

            while (API.NetworkIsEntityFading(veh.Handle))
            {
                await BaseScript.Delay(10);
            }
        }

        internal static async void ApplyVehicleModsDelayed(this Vehicle vehicle, VehicleInfo vehicleInfo, int delay)
        {
            try
            {
                if (vehicle != null && vehicle.Exists())
                {
                    vehicle.Mods.InstallModKit();
                    // set the extras

                    foreach (var extra in vehicleInfo.extras)
                    {
                        if (DoesExtraExist(vehicle.Handle, extra.Key))
                            vehicle.ToggleExtra(extra.Key, extra.Value);
                    }

                    SetVehicleWheelType(vehicle.Handle, vehicleInfo.wheelType);
                    SetVehicleMod(vehicle.Handle, 23, 0, vehicleInfo.customWheels);
                    if (vehicle.Model.IsBike)
                    {
                        SetVehicleMod(vehicle.Handle, 24, 0, vehicleInfo.customWheels);
                    }
                    ToggleVehicleMod(vehicle.Handle, 18, vehicleInfo.turbo);
                    SetVehicleTyreSmokeColor(vehicle.Handle, vehicleInfo.colors["tyresmokeR"], vehicleInfo.colors["tyresmokeG"], vehicleInfo.colors["tyresmokeB"]);
                    ToggleVehicleMod(vehicle.Handle, 20, vehicleInfo.tyreSmoke);
                    ToggleVehicleMod(vehicle.Handle, 22, vehicleInfo.xenonHeadlights);
                    SetVehicleLivery(vehicle.Handle, vehicleInfo.livery);

                    SetVehicleColours(vehicle.Handle, vehicleInfo.colors["primary"], vehicleInfo.colors["secondary"]);
                    SetVehicleInteriorColour(vehicle.Handle, vehicleInfo.colors["trim"]);
                    SetVehicleDashboardColour(vehicle.Handle, vehicleInfo.colors["dash"]);

                    SetVehicleExtraColours(vehicle.Handle, vehicleInfo.colors["pearlescent"], vehicleInfo.colors["wheels"]);

                    SetVehicleNumberPlateText(vehicle.Handle, vehicleInfo.plateText);
                    SetVehicleNumberPlateTextIndex(vehicle.Handle, vehicleInfo.plateStyle);

                    SetVehicleWindowTint(vehicle.Handle, vehicleInfo.windowTint);

                    vehicle.CanTiresBurst = !vehicleInfo.bulletProofTires;

                    SetVehicleEnveffScale(vehicle.Handle, vehicleInfo.enveffScale);

                    SetHeadlightsColorOnVehicle(vehicle, vehicleInfo.headlightColor);

                    vehicle.Mods.NeonLightsColor = System.Drawing.Color.FromArgb(red: vehicleInfo.colors["neonR"], green: vehicleInfo.colors["neonG"], blue: vehicleInfo.colors["neonB"]);
                    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vehicleInfo.neonLeft);
                    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vehicleInfo.neonRight);
                    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vehicleInfo.neonFront);
                    vehicle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vehicleInfo.neonBack);

                    void DoMods()
                    {
                        vehicleInfo.mods.ToList().ForEach(mod =>
                        {
                            if (vehicle != null && vehicle.Exists() && mod.Key != (int)VehicleModType.Tank)
                                SetVehicleMod(vehicle.Handle, mod.Key, mod.Value, vehicleInfo.customWheels);
                        });
                    }

                    DoMods();
                    // Performance mods require a delay after setting the modkit,
                    // so we just do it once first so all the visual mods load instantly,
                    // and after a small delay we do it again to make sure all performance
                    // mods have also loaded.
                    await BaseScript.Delay(delay);
                    DoMods();
                }
            }
            catch (Exception ex)
            {

            }
        }

        internal static int GetHeadlightsColorFromVehicle(this Vehicle vehicle)
        {
            if (vehicle != null && vehicle.Exists())
            {
                if (IsToggleModOn(vehicle.Handle, 22))
                {
                    int val = GetVehicleHeadlightsColour(vehicle.Handle);
                    if (val > -1 && val < 13)
                    {
                        return val;
                    }
                    return -1;
                }
            }
            return -1;
        }

        internal static void SetHeadlightsColorOnVehicle(this Vehicle veh, int newIndex)
        {

            if (veh != null && veh.Exists())
            {
                if (newIndex > -1 && newIndex < 13)
                {
                    SetVehicleHeadlightsColour(veh.Handle, newIndex);
                }
                else
                {
                    SetVehicleHeadlightsColour(veh.Handle, -1);
                }
            }
        }

        internal static Blip CreateBlip(this Vehicle vehicle, SpawnType spawnType = SpawnType.Unknown)
        {
            Blip blip = vehicle.AttachBlip();

            if (spawnType == SpawnType.Unknown)
            {
                int stateSpawnType = vehicle.State.Get(StateBagKey.VEH_SPAWN_TYPE) ?? 0;
                if (stateSpawnType == 0)
                {
                    spawnType = SpawnType.Vehicle;
                    goto MakeBlip;
                }
                spawnType = (SpawnType)stateSpawnType;
            }

        MakeBlip:
            bool setBlip = false;

            if (spawnType == SpawnType.Vehicle)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_VEHICLE);
                blip.Sprite = BlipSprite.PersonalVehicleCar;
                setBlip = true;
            }

            if (spawnType == SpawnType.Trailer)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_TRAILER);
                API.SetBlipSprite(blip.Handle, 479);
                setBlip = true;
            }

            if (spawnType == SpawnType.Boat)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_BOAT);
                blip.Sprite = BlipSprite.Boat;
                setBlip = true;
            }

            if (spawnType == SpawnType.Plane)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_PLANE);
                blip.Sprite = BlipSprite.Plane;
                setBlip = true;
            }

            if (spawnType == SpawnType.Helicopter)
            {
                API.BeginTextCommandSetBlipName(BLIP_PERSONAL_HELICOPTER);
                blip.Sprite = BlipSprite.Helicopter;
                setBlip = true;
            }

            API.EndTextCommandSetBlipName(blip.Handle);

            VehicleHash vehicleHash = (VehicleHash)vehicle.Model.Hash;

            if (!setBlip)
            {
                if (ScreenInterface.VehicleBlips.ContainsKey(vehicleHash))
                {
                    API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleBlips[vehicleHash]);
                }
                else
                {
                    if (ScreenInterface.VehicleClassBlips.ContainsKey(vehicle.ClassType))
                    {
                        API.SetBlipSprite(blip.Handle, ScreenInterface.VehicleClassBlips[vehicle.ClassType]);
                    }
                }
            }

            blip.Scale = 0.85f;
            blip.Color = BlipColor.White;
            blip.Priority = 10;
            return blip;
        }
    }
}
