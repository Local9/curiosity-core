﻿using System.Linq;

namespace Curiosity.Core.Client.Utils
{
    public class Peds : BaseScript
    {
        private static readonly int[] BoneIDs = new int[]
        {
            0x0, // SKEL_ROOT
            0x2E28, // SKEL_Pelvis
            0xE0FD, // SKEL_Spine_Root
            0x5C01, // SKEL_Spine0
            0x60F0, // SKEL_Spine1
            0x60F1, // SKEL_Spine2
            0x60F2, // SKEL_Spine3
            0x9995, // SKEL_Neck_1
            0x796E, // SKEL_Head
            0xE39F, // SKEL_L_Thigh
            0xF9BB, // SKEL_L_Calf
            0x3779, // SKEL_L_Foot
            0xB1C5, // SKEL_L_UpperArm
            0xEEEB, // SKEL_L_Forearm
            0x49D9, // SKEL_L_Hand
            0xCA72, // SKEL_R_Thigh
            0x9000, // SKEL_R_Calf
            0xCC4D, // SKEL_R_Foot
            0x9D4D, // SKEL_R_UpperArm
            0x6E5C, // SKEL_R_Forearm
            0xDEAD // SKEL_R_Hand
        };

        private static readonly string[] BoneNames = new string[]
        {
            "SKEL_ROOT",
            "SKEL_Pelvis",
            "SKEL_Spine_Root",
            "SKEL_Spine0",
            "SKEL_Spine1",
            "SKEL_Spine2",
            "SKEL_Spine3",
            "SKEL_Neck_1",
            "SKEL_Head",
            "SKEL_L_Thigh",
            "SKEL_L_Calf",
            "SKEL_L_Foot",
            "SKEL_L_UpperArm",
            "SKEL_L_Forearm",
            "SKEL_L_Hand",
            "SKEL_R_Thigh",
            "SKEL_R_Calf",
            "SKEL_R_Foot",
            "SKEL_R_UpperArm",
            "SKEL_R_Forearm",
            "SKEL_R_Hand"
        };

        [Flags]
        public enum Filter
        {
            None = 0,
            Animals = 1,
            Players = 2,
            NonPlayers = 4,
            LocalPlayer = 8,
            Dead = 16,
            VehiclePassengers = 32,
            CurrentVehiclePassengers = 64
        }

        public const Filter DefaultFilters = Filter.LocalPlayer | Filter.Dead;
        private const string BurnDecor = "POCCE_BURN";
        private static readonly List<int> _knownPeds = new List<int>();

        public Peds()
        {
            API.DecorRegister(BurnDecor, 2);
        }

        public static List<int> Get(Filter exclude = DefaultFilters, float rangeSquared = 1600f)
        {
            var peds = new List<int>();
            int ped = 0;
            int handle = API.FindFirstPed(ref ped);
            var player = API.GetPlayerPed(-1);
            var coords = API.GetEntityCoords(player, false);
            var vehicle = API.GetVehiclePedIsIn(player, false);

            if (!API.IsPedInAnyVehicle(player, false))
                exclude &= ~Filter.CurrentVehiclePassengers;

            if (handle == -1)
                return peds;

            bool HasFilter(Filter filter)
            {
                return (exclude & filter) == filter;
            }

            do
            {
                var pos = API.GetEntityCoords(ped, true);

                if (HasFilter(Filter.Animals) && !API.IsPedHuman(ped))
                    continue;

                if (HasFilter(Filter.LocalPlayer) && ped == player)
                    continue;

                if (API.IsPedAPlayer(ped))
                {
                    if (HasFilter(Filter.Players))
                        continue;
                }
                else
                {
                    if (HasFilter(Filter.NonPlayers))
                        continue;
                }

                if (HasFilter(Filter.Dead) && API.IsPedDeadOrDying(ped, true))
                    continue;

                if (HasFilter(Filter.VehiclePassengers) && API.IsPedInAnyVehicle(ped, true))
                    continue;

                if (HasFilter(Filter.CurrentVehiclePassengers) && API.GetVehiclePedIsIn(ped, false) == vehicle)
                    continue;

                if (rangeSquared > 0f && coords.DistanceToSquared(pos) > rangeSquared)
                    continue;

                peds.Add(ped);

            } while (API.FindNextPed(handle, ref ped));

            API.EndFindPed(handle);
            return peds;
        }

        public static string GetClosestPedBoneToOffset(int ped, Vector3 offset)
        {
            int closestBone = 0;
            float minDist = float.MaxValue;

            var boneCoords = BoneIDs.Select(bone => API.GetPedBoneCoords(ped, bone, 0f, 0f, 0f)).Select(bone => API.GetOffsetFromEntityGivenWorldCoords(ped, bone.X, bone.Y, bone.Z)).ToArray();
            for (int i = 0; i < boneCoords.Length; i++)
            {
                var dist = offset.DistanceToSquared(boneCoords[i]);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestBone = i;
                }
            }

            return BoneNames[closestBone];
        }

        public static Vector3 GetSafeCoords(Vector3 coords)
        {
            var pos = new Vector3();
            if (!API.GetSafeCoordForPed(coords.X, coords.Y, coords.Z, true, ref pos, 16))
            {
                pos.X = coords.X;
                pos.Y = coords.Y;
                pos.Z = coords.Z + 1f;
            }

            float groundZ = pos.Z;
            Vector3 normal = Vector3.Zero;

            if (API.GetGroundZAndNormalFor_3dCoord(pos.X, pos.Y, pos.Z, ref groundZ, ref normal))
            {
                pos.Z = groundZ;
            }

            return pos;
        }

        public static async Task<int> Spawn(uint modelKey, Vector3 coords, bool trySafeCoords = true, int pedType = 26)
        {
            Model model = (int)modelKey;
            var pos = trySafeCoords ? GetSafeCoords(coords) : coords;
            var ped = await World.CreatePed(model, coords, API.GetRandomFloatInRange(0f, 360f));
            model.MarkAsNoLongerNeeded();
            return ped.Handle;
        }

        public static Task<int> Spawn(uint[] modelList, int pedType = 26)
        {
            if (modelList.Length == 0)
                return Task.FromResult(-1);

            var model = modelList[API.GetRandomIntInRange(0, modelList.Length)];
            var coords = API.GetEntityCoords(API.GetPlayerPed(-1), true);
            return Spawn(model, coords, true, pedType);
        }

        public static async Task<int> SpawnInRange(uint[] modelList, Vector3 center, float minRange, float maxRange, int pedType = 26)
        {
            if (modelList.Length == 0)
                return -1;

            var model = modelList[API.GetRandomIntInRange(0, modelList.Length)];
            var coords = Common.GetRandomSpawnCoordsInRange(center, minRange, maxRange, out float heading);
            var ped = await Spawn(model, coords, false, pedType);
            return ped;
        }

        public static async Task Arm(int ped, string[] weaponList)
        {
            try
            {
                API.TaskSetBlockingOfNonTemporaryEvents(ped, true);
                API.SetPedKeepTask(ped, true);
                await Delay(10);

                API.SetPedCombatAbility(ped, 100);
                API.SetPedCombatMovement(ped, 2);
                API.SetPedCombatRange(ped, 2);
                API.SetPedHearingRange(ped, float.MaxValue);
                API.SetPedCombatAttributes(ped, 2, true);
                API.SetPedCombatAttributes(ped, 5, true);
                API.SetPedCombatAttributes(ped, 17, true);
                API.SetPedCombatAttributes(ped, 46, true);
                API.SetPedCombatAttributes(ped, 1424, true);
                API.SetPedFleeAttributes(ped, 0, false);
                API.SetEntityHealth(ped, 200);

                if (weaponList != null && weaponList.Length > 0)
                {
                    API.SetPedArmour(ped, 200);
                    var weapon = weaponList[API.GetRandomIntInRange(0, weaponList.Length)];
                    int weaponHash = API.GetHashKey(weapon);
                    Weapons.Give(ped, (uint)weaponHash);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Arm Ped");
            }
        }

        public static void Burn(int ped)
        {
            API.DecorSetBool(ped, BurnDecor, true);
        }

        private static Task Update()
        {
            foreach (var ped in _knownPeds.ToArray())
            {
                if (!API.DoesEntityExist(ped))
                    _knownPeds.Remove(ped);
            }

            var playerID = API.PlayerId();
            var peds = Get(Filter.Players | Filter.Dead).Except(_knownPeds);

            foreach (var ped in peds)
            {
                if (IsMultiplayerPed(ped) &&
                    API.NetworkGetEntityIsNetworked(ped) &&
                    API.NetworkGetEntityOwner(ped) != playerID)
                {
                    var owner = API.NetworkGetEntityOwner(ped);
                    TriggerServerEvent("PocceMod:RequestMPSkin", API.PedToNet(ped), API.GetPlayerServerId(owner));
                }

                if (API.DecorGetBool(ped, BurnDecor) && !API.IsEntityOnFire(ped))
                    API.StartEntityFire(ped);
            }

            _knownPeds.AddRange(peds);

            return Delay(1000);
        }

        public static bool IsMultiplayerPed(int ped)
        {
            var model = (uint)API.GetEntityModel(ped);
            return (model == (uint)API.GetHashKey("mp_m_freemode_01") || model == (uint)API.GetHashKey("mp_f_freemode_01"));
        }
    }
}
