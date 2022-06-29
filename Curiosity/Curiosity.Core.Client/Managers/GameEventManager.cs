using CitizenFX.Core.UI;
using Curiosity.Core.Client.Environment.Data;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.Events;
using Curiosity.Core.Client.Managers.Milo;
using Curiosity.Core.Client.State;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using Newtonsoft.Json;
using System.Drawing;

namespace Curiosity.Core.Client.Managers.Events
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);
    public delegate void EnteredVehicle(Player player, Vehicle vehicle);
}

namespace Curiosity.Core.Client.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {
        public bool EnableDebug = false;

        public static event PlayerKillPlayerEvent OnPlayerKillPlayer;
        public static event PlayerKillPedEvent OnPlayerKillPed;
        public static event PedKillPlayerEvent OnPedKillPlayer;
        public static event PedKillPedEvent OnPedKillPed;
        public static event EntityKillEntityEvent OnEntityKillEntity;
        public static event DeadEvent OnDeath;
        public static event EnteredVehicle OnEnteredVehicle;

        PlayerOptionsManager playerOptions => PlayerOptionsManager.GetModule();

        public override void Begin()
        {
            Instance.EventRegistry["gameEventTriggered"] += new Action<string, List<dynamic>>(OnGameEventTriggered);
            Instance.EventRegistry["CEventPlayerDeath"] += new Action<dynamic, dynamic, List<dynamic>>(OnEventShockingGunshotFired);
            // Instance.EventRegistry["CEventDamage"] += new Action<dynamic, dynamic, List<dynamic>>(OnEventShockingGunshotFired);
            // Instance.EventRegistry["CEventShockingGunshotFired"] += new Action<dynamic, dynamic, List<dynamic>>(OnEventShockingGunshotFired);
        }

        private void OnEventShockingGunshotFired(dynamic entities, dynamic eventEntity, List<dynamic> data)
        {
            Logger.Debug($"{JsonConvert.SerializeObject(entities)} / {eventEntity} / {JsonConvert.SerializeObject(data)}");
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            if (EnableDebug)
                Logger.Debug($"game event {name} ({String.Join(", ", args.ToArray())})");

            try
            {
                if (name == "CEventNetworkPlayerEnteredVehicle")
                {
                    // Arg 0 - Player
                    // arg 1 - Vehicle Entity Handle
                    Player player = new Player((int)args[0]);

                    if (player.ServerId != Game.Player.ServerId) return;

                    int entityId = (int)args[1];
                    if (!API.IsEntityAVehicle(entityId)) return;

                    Vehicle vehicle = new Vehicle(entityId);

                    if (!vehicle.Model.IsTrain)
                        HandleCEventNetworkPlayerEnteredVehicle(player, vehicle);
                }


                if (name == "CEventNetworkEntityDamage")
                {
                    Entity victim = Entity.FromHandle((int)args[0]);
                    Entity attacker = Entity.FromHandle((int)args[1]);
                    bool isDamageFatal = Convert.ToBoolean((int)args[5]);
                    uint weaponInfoHash = (uint)args[6];
                    bool isMeleeDamage = Convert.ToBoolean((int)args[11]);
                    int damageTypeFlag = (int)args[12];

                    //Logger.Debug($"CEventNetworkEntityDamage;\n" +
                    //    $"victim: {victim}\n" +
                    //    $"attacker: {attacker}\n" +
                    //    $"isDamageFatal: {isDamageFatal}\n" +
                    //    $"weaponInfoHash: {weaponInfoHash}\n" +
                    //    $"isMeleeDamage: {isMeleeDamage}\n" +
                    //    $"damageTypeFlag: {damageTypeFlag}\n" +
                    //    $"");

                    HandleCEventNetworkEntityDamaged(
                        victim, attacker, (int)args[2], (int)args[3], (int)args[4], isDamageFatal, weaponInfoHash,
                        (int)args[7], (int)args[8], args[9], args[10], isMeleeDamage,
                        damageTypeFlag);

                }

            }
            catch (Exception ex)
            {
                Logger.Debug($"{ex}");
            }
        }


        Dictionary<int, VehicleState> previousVehicles = new Dictionary<int, VehicleState>();

        private void HandleCEventNetworkPlayerEnteredVehicle(Player player, Vehicle vehicle)
        {
            OnEnteredVehicle?.Invoke(player, vehicle);

            // MOVE THIS SHIT

            if (player.Character.Handle != Cache.PlayerPed.Handle) return;

            if (vehicle is Vehicle)
            {
                VehicleManager vehicleManager = VehicleManager.GetModule();

                VehicleState currentVehicle;

                if (vehicle.Driver == Cache.PlayerPed)
                {
                    if (previousVehicles.ContainsKey(vehicle.NetworkId))
                    {
                        currentVehicle = previousVehicles[vehicle.NetworkId];
                    }
                    else
                    {
                        currentVehicle = new VehicleState(vehicle);
                        previousVehicles.Add(vehicle.NetworkId, currentVehicle);

                        Dictionary<int, VehicleState> vehCopy = new Dictionary<int, VehicleState>(previousVehicles);
                        foreach (KeyValuePair<int, VehicleState> keyValuePair in vehCopy)
                        {
                            VehicleState vehicleState = keyValuePair.Value;
                            if (DateTime.Now.Subtract(vehicleState.Created).TotalMinutes > 5)
                            {
                                previousVehicles.Remove(keyValuePair.Key);
                            }
                        }
                    }

                    Logger.Debug($"No. Previous Vehicles: {previousVehicles.Count}");

                    vehicleManager.InitialiseVehicleFuel(currentVehicle);

                    if (vehicle.Model.Hash == (int)VehicleHash.Skylift)
                    {
                        vehicleManager.InitialiseSkylift(currentVehicle);
                    }
                }

                if (vehicle.AttachedBlip is not null)
                {
                    int blipHandle = API.GetBlipFromEntity(vehicle.Handle);
                    API.SetBlipAlpha(blipHandle, 0);

                    Vehicle veh = (Vehicle)vehicle.TowedVehicle;

                    if (veh is not null)
                    {
                        if (veh.AttachedBlip is not null)
                        {
                            int blipHandleAttached = API.GetBlipFromEntity(veh.Handle);
                            API.SetBlipAlpha(blipHandleAttached, 0);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handle game event CEventNetworkEntityDamage,
        /// Useful for indicating entity damage/died/destroyed.
        /// </summary>
        /// <param name="victim">victim</param>
        /// <param name="attacker">attacker</param>
        /// <param name="arg2">Unknown</param>
        /// <param name="arg3">Unknown</param>
        /// <param name="arg4">Unknown</param>
        /// <param name="isDamageFatal">Is damage fatal to entity. or victim died/destroyed.</param>
        /// <param name="weaponInfoHash">Probably related to common.rpf/data/ai => Item type = "CWeaponInfo"</param>
        /// <param name="arg5">Unknown</param>
        /// <param name="arg6">Unknown</param>
        /// <param name="arg7">Unknown, might be int</param>
        /// <param name="arg8">Unknown, might be int</param>
        /// <param name="isMeleeDamage">Is melee damage</param>
        /// <param name="damageTypeFlag">0 for peds, 116 for the body of a vehicle, 93 for a tire, 120 for a side window, 121 for a rear window, 122 for a windscreen, etc</param>
        private void HandleCEventNetworkEntityDamaged(
            Entity victim, Entity attacker, int arg2, int arg3, int arg4, bool isDamageFatal, uint weaponInfoHash,
            int arg5, int arg6, object arg7, object arg8, bool isMeleeDamage,
            int damageTypeFlag)
        {
            bool isAttackerPed = false;
            bool isAttackerPlayer = false;
            Ped pedAttacker = null;
            Player playerAttacker = null;
            // Ped
            if (attacker is Ped)
            {
                pedAttacker = (Ped)attacker;
                isAttackerPed = true;
                // Player
                if (pedAttacker.IsPlayer)
                {
                    playerAttacker = new Player(API.NetworkGetPlayerIndexFromPed(pedAttacker.Handle));
                    isAttackerPlayer = true;
                }
            }
            bool isVictimPed = false;
            bool isVictimPlayer = false;
            bool isVictimThisPlayer = false;
            Ped pedVictim = null;
            Player playerVictim = null;
            // Ped
            if (victim is Ped)
            {
                pedVictim = (Ped)victim;
                isVictimPed = true;
                // Player
                if (pedVictim.IsPlayer)
                {
                    playerVictim = new Player(API.NetworkGetPlayerIndexFromPed(pedVictim.Handle));
                    isVictimPlayer = true;
                    if (playerVictim == Game.Player)
                    {
                        isVictimThisPlayer = true;
                    }
                }
            }

            Logger.Debug($"Damage Event: weaponInfoHash : {weaponInfoHash} : StunGun: {GetHashKey("WEAPON_STUNGUN")}");

            if (isDamageFatal)
            {
                if (isAttackerPlayer && isVictimPlayer)
                {
                    OnPlayerKillPlayer?.Invoke(playerAttacker, playerVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isAttackerPlayer && isVictimPed)
                {
                    OnPlayerKillPed?.Invoke(playerAttacker, pedVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isAttackerPed && isVictimPlayer)
                {
                    OnPedKillPlayer?.Invoke(pedAttacker, playerVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else if (isVictimPed && isAttackerPed)
                {
                    OnPedKillPed?.Invoke(pedAttacker, pedVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }
                else
                {
                    OnEntityKillEntity?.Invoke(attacker, victim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

                if (isVictimThisPlayer)
                {
                    OnDeath?.Invoke(attacker, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

            }
            else if (!isDamageFatal)
            {
                if (isAttackerPlayer && isVictimPlayer && weaponInfoHash == GetHashKey("WEAPON_STUNGUN"))
                {
                    if (pedAttacker == Cache.PlayerPed && playerOptions.CurrentJob == ePlayerJobs.POLICE_OFFICER)
                    {
                        EventSystem.Send("police:officerTazedPlayer", playerAttacker.ServerId, playerVictim.ServerId);
                    }
                }
            }

        }
    }

    public class GameEventTigger : Manager<GameEventTigger> // THIS Could just be moved
    {
        DateTime DateOfDeath = DateTime.Now;
        Camera DeathCamera;
        float AngleY;
        float AngleZ;

        Vector3 CameraOffset = new Vector3();

        bool wasKilledByPlayer = false;

        public override void Begin()
        {
            GameEventManager.OnDeath += GameEventManager_OnDeath;
            GameEventManager.OnPlayerKillPlayer += GameEventManager_OnPlayerKillPlayer;

            EventSystem.Attach("character:respawnNow", new EventCallback(metadata =>
            {
                Respawn(Cache.Player);
                EventSystem.Send("world:routing:city");
                return null;
            }));

            EventSystem.Attach("character:respawn:hospital", new EventCallback(metadata =>
            {
                RespawnAtHospital(Cache.Player, metadata.Find<bool>(0));
                EventSystem.Send("world:routing:city");
                return null;
            }));
        }

        //[TickHandler]
        //private async Task SafeCoord()
        //{
        //    Vector3 entityPos = Game.PlayerPed.Position;
        //    Vector2 coords = new Vector2(entityPos.X, entityPos.Y);
        //    Vector3 spawnLocation = World.GetNextPositionOnSidewalk(coords);

        //    Vector3 sidewalk1 = Vector3.Zero;
        //    API.GetSafeCoordForPed(entityPos.X, entityPos.Y, entityPos.Z, true, ref sidewalk1, 0);

        //    Vector3 sidewalk2 = Vector3.Zero;
        //    API.GetSafeCoordForPed(entityPos.X, entityPos.Y, entityPos.Z, false, ref sidewalk2, 0);

        //    Screen.ShowSubtitle($"{spawnLocation} ~n~ {sidewalk1} : {sidewalk2}");
        //}

        private void GameEventManager_OnPlayerKillPlayer(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {
                if (victim == attacker) return;
                if (attacker != Game.Player) return;


                int hash = (int)weaponInfoHash;

                string causeOfDeath = "Unknown";
                if (DeathHash.CauseOfDeath.ContainsKey(hash))
                    causeOfDeath = DeathHash.CauseOfDeath[(int)weaponInfoHash];

                Cache.Player.Character.IsDead = true;
                wasKilledByPlayer = true;

                EventSystem.Send("gameEvent:playerKillPlayer", attacker.ServerId, victim.ServerId, causeOfDeath);
            }
            catch (Exception ex)
            {
                Logger.Error($"OnPlayerKillPlayer -> {ex.Message}");
            }
        }

        private void GameEventManager_OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            DateOfDeath = DateTime.Now;
            EventSystem.Send("character:death");

            Cache.Player.Character.IsDead = true;
            wasKilledByPlayer = false;

            CreateCamera();

            PluginManager.Instance.AttachTickHandler(OnRespawnControlTask);
        }

        async void RespawnAtHospital(CuriosityPlayer curiosityPlayer, bool hospitalSpawn)
        {
            await ScreenInterface.FadeOut();
            Game.PlayerPed.IsInvincible = true;

            float randX = Utility.RANDOM.Next(200, 300);
            float randY = Utility.RANDOM.Next(200, 300);

            if (!wasKilledByPlayer)
            {
                randX = Utility.RANDOM.Next(10, 50);
                randY = Utility.RANDOM.Next(10, 50);
            }

            Vector3 spawnPosition = curiosityPlayer.Entity.Position.AsVector() + new Vector3(randX, randY, 1f);

            if (hospitalSpawn)
            {
                if (Cache.Character.IsOnIsland)
                {
                    CayoPericoManager.GetModule().SetupLosSantos();

                    NotificationManager.GetModule().Info($"Chartering a flight to the nearest hospital.");

                    await BaseScript.Delay(3000);
                }

                Position spawnLocation = LocationManager.LocationManagerInstance.NearestHospital();

                if (spawnLocation.X == 0f)
                {
                    spawnLocation = new Position(297.8683f, -584.3318f, 43.25863f, Game.PlayerPed.Heading);
                }

                spawnPosition.X = spawnLocation.X;
                spawnPosition.Y = spawnLocation.Y;
                spawnPosition.Z = spawnLocation.Z;
            }

            Cache.Player.Character.IsDead = false;
            wasKilledByPlayer = false;

            float groundZ = spawnPosition.Z;
            if (API.GetGroundZFor_3dCoord_2(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, ref groundZ, false))
                spawnPosition = new Vector3(spawnPosition.X, spawnPosition.Y, groundZ);

            float waterHeight = spawnPosition.Z;

            if (API.TestVerticalProbeAgainstAllWater(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, 1, ref waterHeight))
            {
                spawnPosition.Z = waterHeight;
            }

            Vector3 safeSpawn = spawnPosition;
            if (!hospitalSpawn && GetSafeCoordForPed(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, true, ref safeSpawn, 1))
            {
                spawnPosition = safeSpawn;
            }

            curiosityPlayer.Character.Revive(new Position(spawnPosition.X, spawnPosition.Y, spawnPosition.Z, Game.PlayerPed.Heading));

            BaseScript.TriggerEvent("onPlayerResurrected", "hospital");
            RemoveCamera();

            if (hospitalSpawn)
                await BaseScript.Delay(1000);

            Game.PlayerPed.IsInvincible = false;
            await ScreenInterface.FadeIn(3000);
        }

        public async void Respawn(CuriosityPlayer curiosityPlayer)
        {
            await ScreenInterface.FadeOut();
            Game.PlayerPed.IsInvincible = true;
            PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);

            Vector3 spawnLocation = curiosityPlayer.Entity.Position.AsVector();
            BaseScript.TriggerEvent("onPlayerResurrected", "local");
            curiosityPlayer.Character.Revive(new Position(spawnLocation.X, spawnLocation.Y, spawnLocation.Z, Cache.PlayerPed.Heading));

            RemoveCamera();

            Cache.Player.Character.IsDead = false;
            wasKilledByPlayer = false;

            await BaseScript.Delay(1000);
            Game.PlayerPed.IsInvincible = false;
            await ScreenInterface.FadeIn(3000);
        }

        async Task OnRespawnControlTask() // Change to a camera
        {
            Cache.Player.DisableHud();
            TimeSpan timeSpan = (DateOfDeath.AddMinutes(5) - DateTime.Now);
            string timeSpanLeft = timeSpan.ToString(@"mm\:ss");

            API.DisableFirstPersonCamThisFrame();

            Vector3 camPosition = ProcessCameraPosition();

            // Screen.ShowSubtitle($"{camPosition}");

            DeathCamera.PointAt(Cache.PlayerPed, new Vector3(0f, 0f, 0.5f));
            DeathCamera.Position = camPosition;
            API.SetFocusArea(camPosition.X, camPosition.Y, camPosition.Z, 0f, 0f, 0f);

            if (!Game.PlayerPed.IsDead)
                Game.PlayerPed.Kill();

            ScreenInterface.DrawTextLegacy($"~w~You are unconscious. (~y~{timeSpanLeft}~w~)~n~(Press E to respawn now ~g~${Cache.Player.Character.RespawnCharge() * 2:N0}~w~)", 0.3f, new Vector2(0.5f, 0.75f), Color.FromArgb(175, 175, 175), true);
            ScreenInterface.DrawTextLegacy($"~w~(Press Q to respawn at a hospital ~g~${Cache.Player.Character.RespawnCharge():N0}~w~)", 0.3f, new Vector2(0.5f, 0.79f), Color.FromArgb(175, 175, 175), true);

            Screen.DisplayHelpTextThisFrame($"~w~You are unconscious, wait for revive. (~y~{timeSpanLeft}~w~)~n~Press ~INPUT_CONTEXT~ to respawn now for ~g~${Cache.Player.Character.RespawnCharge()}~w~");

            if (Game.IsControlPressed(0, Control.Context))
            {
                EventSystem.Send("character:respawn:charge", false);
                PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);
            }
            else if (Game.IsControlPressed(0, Control.Cover))
            {
                EventSystem.Send("character:respawn:charge", true);
                PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);
            }
            else if (timeSpan.TotalSeconds <= 0)
            {
                EventSystem.Send("character:respawn");
                PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);
            }
        }

        void CreateCamera()
        {
            API.ClearFocus();

            DeathCamera = World.CreateCamera(Cache.PlayerPed.Position, Vector3.Zero, GameplayCamera.FieldOfView);
            DeathCamera.IsActive = true;
            API.RenderScriptCams(true, true, 1000, true, false);
            API.SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
        }

        void RemoveCamera()
        {
            API.ClearFocus();
            API.RenderScriptCams(false, false, 0, true, false);

            if (DeathCamera != null)
                DeathCamera.Delete();

            DeathCamera = null;
            API.SetTransitionTimecycleModifier($"DEFAULT", 5.0f);
        }

        Vector3 ProcessCameraPosition()
        {
            float mouseX;
            float mouseY;

            if (API.IsInputDisabled(0))
            {
                mouseX = (API.GetDisabledControlNormal(1, 1) * 8f);
                mouseY = (API.GetDisabledControlNormal(1, 2) * 8f);
            }
            else
            {
                mouseX = (API.GetDisabledControlNormal(1, 1) * 2f);
                mouseY = (API.GetDisabledControlNormal(1, 2) * 2f);
            }

            AngleZ = AngleZ - mouseX;
            AngleY = AngleY + mouseY;

            if (AngleY > 89.0)
            {
                AngleY = 89.0f;
            }
            else if (AngleY < -89.0)
            {
                AngleY = -89.0f;
            }

            Vector3 pPos = Cache.PlayerPed.Position;

            float radius = 8.0f;

            Vector3 behindTheCamera = new Vector3();
            behindTheCamera.X = (pPos.X + ((Cos(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Cos(AngleZ))) / 2 * radius);
            behindTheCamera.Y = (pPos.Y + ((Sin(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Sin(AngleZ))) / 2 * radius);
            behindTheCamera.Z = (pPos.Z + (Sin(AngleY) * radius));

            Vector3 raycastPosition = pPos;
            raycastPosition.Z = raycastPosition.Z + 0.5f;

            RaycastResult raycastResult = World.Raycast(raycastPosition, behindTheCamera, IntersectOptions.Everything, Cache.PlayerPed);

            float maxRadius = radius;

            if (raycastResult.DitHit && Cache.PlayerPed.IsInRangeOf(raycastResult.HitPosition, maxRadius))
            {
                Vector3 hitPos = raycastResult.HitPosition;
                maxRadius = API.Vdist(pPos.X, pPos.Y, pPos.Z, hitPos.X, hitPos.Y, hitPos.Z);
            }

            CameraOffset.X = ((Cos(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Cos(AngleZ))) / 2 * maxRadius;
            CameraOffset.Y = ((Sin(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Sin(AngleZ))) / 2 * maxRadius;
            CameraOffset.Z = (Sin(AngleY) * maxRadius);

            Vector3 finalPosition = new Vector3();
            finalPosition.X = pPos.X + CameraOffset.X;
            finalPosition.Y = pPos.Y + CameraOffset.Y;
            finalPosition.Z = pPos.Z + CameraOffset.Z;

            // World.DrawMarker(MarkerType.VerticalCylinder, pPos, Vector3.Zero, Vector3.Zero, new Vector3(.03f, .03f, 5f), Color.FromArgb(255, 255, 0, 0));
            // World.DrawMarker(MarkerType.VerticalCylinder, pPos, Vector3.Zero, new Vector3(0f, 90f, 0f), new Vector3(.03f, .03f, 5f), Color.FromArgb(255, 0, 255, 0));
            // World.DrawMarker(MarkerType.VerticalCylinder, pPos, Vector3.Zero, new Vector3(-90f, 0f, 0f), new Vector3(.03f, .03f, 5f), Color.FromArgb(255, 0, 0, 255));

            return finalPosition;
        }
    }
}
