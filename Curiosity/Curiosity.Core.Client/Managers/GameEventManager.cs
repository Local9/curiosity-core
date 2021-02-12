using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Events
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);
}

namespace Curiosity.Core.Client.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {

        public static event PlayerKillPlayerEvent OnPlayerKillPlayer;
        public static event PlayerKillPedEvent OnPlayerKillPed;
        public static event PedKillPlayerEvent OnPedKillPlayer;
        public static event PedKillPedEvent OnPedKillPed;
        public static event EntityKillEntityEvent OnEntityKillEntity;
        public static event DeadEvent OnDeath;

        public override void Begin()
        {
            Instance.EventRegistry["gameEventTriggered"] += new Action<string, List<dynamic>>(OnGameEventTriggered);
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            Logger.Debug($"game event {name} ({String.Join(", ", args.ToArray())})");

            try
            {
                if (name == "CEventNetworkPlayerEnteredVehicle")
                {
                    // Arg 0 - 128
                    // arg 1 - Vehicle Entity Handle
                    int arg0 = (int)args[1];
                    Entity ent = Entity.FromHandle((int)args[1]);

                    HandleCEventNetworkPlayerEnteredVehicle(arg0, ent);
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

        private void HandleCEventNetworkPlayerEnteredVehicle(int arg0, Entity ent)
        {
            if (ent is Vehicle)
            {
                VehicleManager.GetModule().InitialiseVehicleFuel((Vehicle)ent);
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
            if (isDamageFatal)
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

        }
    }

    public class GameEventTigger : Manager<GameEventTigger> // THIS Could just be moved
    {
        DateTime DateOfDeath = DateTime.Now;
        Camera DeathCamera;
        float AngleY;
        float AngleZ;

        Vector3 CameraOffset = new Vector3();

        public override void Begin()
        {
            GameEventManager.OnDeath += GameEventManager_OnDeath;

            EventSystem.Attach("character:respawnNow", new EventCallback(metadata =>
            {
                Respawn(Cache.Player);

                return null;
            }));

            EventSystem.Attach("character:respawn:hospital", new EventCallback(metadata =>
            {
                RespawnAtHospital(Cache.Player);

                return null;
            }));
        }
            
        private void GameEventManager_OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            DateOfDeath = DateTime.Now;
            EventSystem.Send("character:death");

            CreateCamera();

            PluginManager.Instance.AttachTickHandler(OnRespawnControlTask);
        }

        async void RespawnAtHospital(CuriosityPlayer curiosityPlayer)
        {
            await ScreenInterface.FadeOut();

            Vector3 spawnLocation = LocationManager.LocationManagerInstance.NearestHospital();
            curiosityPlayer.Character.Revive(new Position(spawnLocation.X, spawnLocation.Y, spawnLocation.Z, Game.PlayerPed.Heading));

            RemoveCamera();

            await BaseScript.Delay(1000);

            await ScreenInterface.FadeIn(3000);
        }

        async void Respawn(CuriosityPlayer curiosityPlayer)
        {
            await ScreenInterface.FadeOut();
            PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);

            Vector3 spawnLocation = curiosityPlayer.Entity.Position.AsVector();
            curiosityPlayer.Character.Revive(new Position(spawnLocation.X, spawnLocation.Y, spawnLocation.Z, Game.PlayerPed.Heading));

            RemoveCamera();

            await BaseScript.Delay(1000);

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

            DeathCamera.PointAt(Game.PlayerPed, new Vector3(0f, 0f, 0.5f));
            DeathCamera.Position = camPosition;
            API.SetFocusArea(camPosition.X, camPosition.Y, camPosition.Z, 0f, 0f, 0f);

            ScreenInterface.DrawText($"~w~You are unconscious. (~y~{timeSpanLeft}~w~)~n~(Press E to re-emerge at the hospital ~g~${Cache.Player.Character.RespawnCharge()}~w~)",
                0.3f, new Vector2(0.5f, 0.75f), Color.FromArgb(175, 175, 175), true);

            if (Game.IsControlPressed(0, Control.Context))
            {
                EventSystem.Send("character:respawn:charge");
                PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);
            }
            else if (timeSpan.TotalSeconds <= 0)
            {
                EventSystem.Send("character:respawn");
            }
        }

        void CreateCamera()
        {
            API.ClearFocus();

            DeathCamera = World.CreateCamera(Game.PlayerPed.Position, Vector3.Zero, GameplayCamera.FieldOfView);
            DeathCamera.IsActive = true;
            API.RenderScriptCams(true, true, 1000, true, false);
        }

        void RemoveCamera()
        {
            API.ClearFocus();
            API.RenderScriptCams(false, false, 0, true, false);
            
            if (DeathCamera != null)
                DeathCamera.Delete();

            DeathCamera = null;
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

            Vector3 pPos = Game.PlayerPed.Position;

            float radius = 8.0f;

            Vector3 behindTheCamera = new Vector3();
            behindTheCamera.X = (pPos.X + ((Cos(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Cos(AngleZ))) / 2 * radius);
            behindTheCamera.Y = (pPos.Y + ((Sin(AngleZ) * Cos(AngleY)) + (Cos(AngleY) * Sin(AngleZ))) / 2 * radius);
            behindTheCamera.Z = (pPos.Z + (Sin(AngleY) * radius));

            Vector3 raycastPosition = pPos;
            raycastPosition.Z = raycastPosition.Z + 0.5f;

            RaycastResult raycastResult = World.Raycast(raycastPosition, behindTheCamera, IntersectOptions.Everything, Game.PlayerPed);

            float maxRadius = radius;

            if (raycastResult.DitHit && Game.PlayerPed.IsInRangeOf(raycastResult.HitPosition, maxRadius))
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
