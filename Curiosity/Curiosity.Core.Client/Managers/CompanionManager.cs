using Curiosity.Core.Client.Extensions;
using System.Linq;

namespace Curiosity.Core.Client.Managers
{
    public class CompanionManager : Manager<CompanionManager>
    {
        const string COMPANION_FLAG = "curiosity:companion:flag";
        const string COMPANION_PLAYER_FLAG = "curiosity:companion:player";
        const WeaponHash PARACHUTE = WeaponHash.Parachute;
        string[] weaponList = new string[5] { "WEAPON_PISTOL", "WEAPON_ASSAULTRIFLE", "WEAPON_PUMPSHOTGUN", "WEAPON_BAT", "WEAPON_STUNGUN" };
        string[] scenarioList = new string[19]
        {
            "WORLD_HUMAN_AA_COFFEE",
            "WORLD_HUMAN_AA_SMOKE",
            "WORLD_HUMAN_BINOCULARS",
            "WORLD_HUMAN_CHEERING",
            "WORLD_HUMAN_DRINKING",
            "WORLD_HUMAN_HANG_OUT_STREET",
            "WORLD_HUMAN_JOG_STANDING",
            "WORLD_HUMAN_MUSCLE_FLEX",
            "WORLD_HUMAN_PARTYING",
            "WORLD_HUMAN_PICNIC",
            "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS",
            "WORLD_HUMAN_PROSTITUTE_LOW_CLASS",
            "WORLD_HUMAN_PUSH_UPS",
            "WORLD_HUMAN_SIT_UPS",
            "WORLD_HUMAN_SMOKING_POT",
            "WORLD_HUMAN_STAND_IMPATIENT",
            "WORLD_HUMAN_STAND_MOBILE",
            "WORLD_HUMAN_TOURIST_MAP",
            "WORLD_HUMAN_YOGA"
        };

        NotificationManager notification => NotificationManager.GetModule();

        Dictionary<int, int> ActiveCompanions = new Dictionary<int, int>();
        Dictionary<int, bool> CompanionMoving = new Dictionary<int, bool>();

        public override void Begin()
        {
            DecorRegister(COMPANION_FLAG, 2);
            DecorRegister(COMPANION_PLAYER_FLAG, 3);
        }

        bool IsCompanion(int ped)
        {
            return DecorGetBool(ped, COMPANION_FLAG) && DecorGetInt(ped, COMPANION_PLAYER_FLAG) == Cache.Player.Handle;
        }

        public List<int> Get(IEnumerable<int> peds)
        {
            var companions = new List<int>();

            foreach (var ped in peds)
            {
                if (IsCompanion(ped))
                {
                    if (IsPedDeadOrDying(ped, true))
                    {
                        var blip = GetBlipFromEntity(ped);
                        RemoveBlip(ref blip);
                        continue;
                    }

                    companions.Add(ped);
                }
            }

            return companions;
        }

        public void Add(int ped)
        {
            DecorSetBool(ped, COMPANION_FLAG, true);
            DecorSetInt(ped, COMPANION_PLAYER_FLAG, Cache.Player.Handle);
            SetPedRelationshipGroupHash(ped, (uint)GetPedRelationshipGroupHash(PlayerPedId()));
            TaskSetBlockingOfNonTemporaryEvents(ped, true);
            SetPedKeepTask(ped, true);
            SetPedDropsWeaponsWhenDead(ped, false);

            var blip = AddBlipForEntity(ped);
            SetBlipAsFriendly(blip, true);

            int netId = NetworkGetNetworkIdFromEntity(ped);
            ActiveCompanions.Add(ped, netId);
            CompanionMoving.Add(ped, false);
        }

        public async Task<int> SpawnHuman(string modelHash)
        {
            try
            {
                if (ActiveCompanions.Count == 2 && !Cache.Player.User.IsSeniorDeveloper)
                {
                    notification.Error("Maximum number of companions allowed.");
                    return -1;
                }

                Model pedModel = new Model(modelHash);
                await pedModel.Request(1000);

                int ped = -1;
                Ped playerPed = Game.PlayerPed;
                var coords = playerPed.Position + new Vector3(0f, 2f, 0f);

                if (playerPed.IsInVehicle())
                {
                    var vehicle = playerPed.CurrentVehicle.Handle;
                    if (Vehicles.GetFreeSeat(vehicle, out int seat))
                    {
                        ped = CreatePedInsideVehicle(vehicle, 26, (uint)pedModel.Hash, seat, true, false);
                    }
                    else if (GetEntitySpeed(vehicle) > 0.1f)
                    {
                        notification.Error("Player is in a moving vehicle and there are no free seats");
                        pedModel.MarkAsNoLongerNeeded();
                        return -1;
                    }
                    else
                    {
                        Ped pedSpawn = await World.CreatePed(pedModel, coords);
                        ped = pedSpawn.Handle;
                        pedModel.MarkAsNoLongerNeeded();
                    }
                }
                else
                {
                    Ped pedSpawn = await World.CreatePed(pedModel, coords);
                    pedModel.MarkAsNoLongerNeeded();
                    ped = pedSpawn.Handle;
                }

                //if (!IsPedHuman(ped))
                //{
                //    notification.Error("Ped is not human.");
                //    DeleteEntity(ref ped);
                //    return -1;
                //}

                Add(ped);
                await Peds.Arm(ped, weaponList);
                return ped;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"SpawnHuman");
                return -1;
            }
        }

        public async Task<int> SpawnNonHuman(uint model)
        {
            if (!IsModelAPed(model))
            {
                notification.Error("model is invalid and not a ped model");
                return -1;
            }

            int player = GetPlayerPed(-1);
            var coords = GetOffsetFromEntityInWorldCoords(player, 0, 2f, 0);

            if (IsPedInAnyHeli(player))
            {
                notification.Error("Don't spawn that poor pet on a heli");
                return -1;
            }
            else if (IsPedInAnyVehicle(player, false))
            {
                var vehicle = GetVehiclePedIsIn(player, false);
                if (GetVehicleDashboardSpeed(vehicle) > 0.1f)
                {
                    notification.Error("Player is in a moving vehicle");
                    return -1;
                }
            }

            var ped = await Peds.Spawn(model, coords, true, 28);
            Add(ped);
            await Peds.Arm(ped, null);
            return ped;
        }

        private bool CheckAndHandlePlayerCombat(IEnumerable<int> peds, IEnumerable<int> companions)
        {
            try
            {
                var player = GetPlayerPed(-1);
                int target = 0;
                if (GetEntityPlayerIsFreeAimingAt(PlayerId(), ref target) || GetPlayerTargetEntity(PlayerId(), ref target))
                {
                    foreach (var companion in companions)
                    {
                        TaskCombatPed(companion, target, 0, 16);
                    }

                    return true;
                }

                foreach (var ped in peds)
                {
                    if (IsPedInCombat(ped, player))
                    {
                        if (IsCompanion(ped))
                        {
                            ClearPedTasks(ped);
                            continue;
                        }

                        foreach (var companion in companions)
                        {
                            TaskCombatPed(companion, ped, 0, 16);
                        }

                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"CheckAndHandlePlayerCombat");
                return false;
            }
        }

        private bool CheckAndHandleFreefall(int companion, Vector3 coords)
        {
            try
            {
                var paraState = GetPedParachuteState(companion);
                if (paraState == 1 || paraState == 2)
                {
                    SetParachuteTaskTarget(companion, coords.X, coords.Y, coords.Z);
                    return true;
                }

                if ((IsPedFalling(companion) || IsPedInParachuteFreeFall(companion)) &&
                    !IsPedInAnyVehicle(companion, false))
                {
                    SetPedSeeingRange(companion, 1f);
                    SetPedHearingRange(companion, 1f);
                    SetPedKeepTask(companion, true);
                    GiveWeaponToPed(companion, (uint)PARACHUTE, 1, false, true);
                    TaskParachuteToTarget(companion, coords.X, coords.Y, coords.Z);
                    return true;
                }

                SetPedSeeingRange(companion, 100f);
                SetPedHearingRange(companion, 100f);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"CheckAndHandleFreefall");
                return false;
            }
        }

        private void FollowPlayerToVehicle(int player, IEnumerable<int> companions)
        {
            try
            {
                var vehicle = GetVehiclePedIsIn(player, false);
                var seats = Vehicles.GetFreeSeats(vehicle);

                foreach (var companion in companions)
                {
                    if (seats.Count == 0)
                        break;

                    var seat = seats.Dequeue();

                    if (!IsPedHuman(companion))
                    {
                        TaskWarpPedIntoVehicle(companion, vehicle, seat);
                        SetPedConfigFlag(companion, 292, true);
                        continue;
                    }

                    if (IsPedInAnyVehicle(companion, true))
                    {
                        var otherVehicle = GetVehiclePedIsUsing(companion);
                        if (otherVehicle != vehicle)
                            TaskLeaveVehicle(companion, otherVehicle, 0);

                        continue;
                    }

                    TaskEnterVehicle(companion, vehicle, -1, seat, 2f, 1, 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"FollowPlayerToVehicle");
            }
        }

        private void FollowPlayer(int player, IEnumerable<int> companions)
        {
            try
            {
                var coords = GetEntityCoords(player, true);
                foreach (var companion in companions)
                {
                    if (CheckAndHandleFreefall(companion, coords))
                        continue;

                    var pos = GetEntityCoords(companion, true);
                    bool isHuman = IsPedHuman(companion);
                    if (IsPedInAnyVehicle(companion, true) && isHuman)
                    {
                        var vehicle = GetVehiclePedIsIn(companion, false);
                        if (GetEntitySpeed(vehicle) < 0.1f)
                            TaskLeaveVehicle(companion, vehicle, 0);
                        else
                            TaskLeaveVehicle(companion, vehicle, 4096);
                    }
                    else if (IsPedInAnyVehicle(companion, true) && !isHuman)
                    {
                        var vehicle = GetVehiclePedIsIn(companion, false);
                        if (GetEntitySpeed(vehicle) < 0.1f)
                        {
                            TaskLeaveVehicle(companion, vehicle, 16);
                            SetPedConfigFlag(companion, 292, false);
                        }
                    }
                    else if (pos.DistanceToSquared(coords) > 25f)
                    {
                        if (IsPedActiveInScenario(companion))
                        {
                            ClearPedTasks(companion);
                            ClearPedTasksImmediately(companion);
                        }

                        if (CompanionMoving[companion]) continue;

                        TaskGoToEntity(companion, player, -1, 5f, 2f, 0, 0);

                        CompanionMoving[companion] = true;
                    }
                    else if (IsPedHuman(companion))
                    {
                        if (IsPedOnFoot(companion) && !IsPedUsingAnyScenario(companion))
                        {
                            var scenario = (scenarioList.Length > 0) ? scenarioList[GetRandomIntInRange(0, scenarioList.Length)] : "WORLD_HUMAN_STAND_MOBILE";
                            TaskStartScenarioInPlace(companion, scenario, 0, true);
                            CompanionMoving[companion] = false;
                        }
                        CompanionMoving[companion] = false;
                    }
                    else
                    {
                        CompanionMoving[companion] = false;
                        TaskLookAtEntity(companion, player, -1, 2048, 3);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"FollowPlayer");
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnUpdateCompanions()
        {
            try
            {
                var player = GetPlayerPed(-1);
                var peds = Peds.Get(Peds.Filter.LocalPlayer);
                var companions = Get(peds);

                if (companions.Count == 0 || CheckAndHandlePlayerCombat(peds, companions))
                    await BaseScript.Delay(2000);

                if (IsPedInAnyVehicle(player, false))
                    FollowPlayerToVehicle(player, companions);
                else
                    FollowPlayer(player, companions);

                await BaseScript.Delay(2000);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"OnUpdateCompanions");
                await BaseScript.Delay(2000);
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnCompanionCheck()
        {
            try
            {
                foreach (KeyValuePair<int, int> kvp in ActiveCompanions.ToArray())
                {
                    int pedId = kvp.Key;
                    int netId = kvp.Value;

                    if (!DoesEntityExist(pedId))
                    {
                        goto REMOVE;
                    }

                    Vector3 position = GetEntityCoords(pedId, false);

                    if (Game.PlayerPed.Position.Distance(position) > 100f)
                    {
                        goto REMOVE;
                    }

                    continue;

                REMOVE:
                    ActiveCompanions.Remove(pedId);
                    CompanionMoving.Remove(pedId);
                    EventSystem.Send("delete:entity", netId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"OnCompanionCheck");
            }

            await BaseScript.Delay(2000);
        }

        public void RemoveCompanions()
        {
            try
            {
                foreach (KeyValuePair<int, int> kvp in ActiveCompanions.ToArray())
                {
                    int pedId = kvp.Key;
                    int netId = kvp.Value;

                    if (DoesEntityExist(pedId))
                    {
                        goto REMOVE;
                    }

                REMOVE:
                    ActiveCompanions.Remove(pedId);
                    CompanionMoving.Remove(pedId);
                    EventSystem.Send("delete:entity", netId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"RemoveCompanions");
            }
        }
    }
}
