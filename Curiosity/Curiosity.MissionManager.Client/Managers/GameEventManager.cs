using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.MissionManager.Client.Manager;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Managers
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Player victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);

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
            Logger.Info($"- [GameEventManager] Begin -----------------------");

            Instance.EventRegistry["gameEventTriggered"] += new Action<string, List<dynamic>>(OnGameEventTriggered);
        }

        private void OnGameEventTriggered(string name, List<dynamic> args)
        {
            try
            {

                if (name == "CEventNetworkPlayerEnteredVehicle")
                {
                    // Arg 0 - Player
                    // arg 1 - Vehicle Entity Handle
                    Entity player = Entity.FromHandle((int)args[0]);
                    Entity vehicle = Entity.FromHandle((int)args[1]);

                    HandleCEventNetworkPlayerEnteredVehicle(player, vehicle);
                }

                if (name == "CEventNetworkEntityDamage")
                {
                    Entity victim = Entity.FromHandle((int)args[0]);
                    Entity attacker = Entity.FromHandle((int)args[1]);
                    bool isDamageFatal = Convert.ToBoolean((int)args[3]);
                    uint weaponInfoHash = (uint)args[4];
                    bool isMeleeDamage = Convert.ToBoolean((int)args[9]);
                    int damageTypeFlag = (int)args[10];

                    HandleCEventNetworkEntityDamaged(
                        victim, attacker, (int)args[2], isDamageFatal, weaponInfoHash,
                        (int)args[5], (int)args[6], args[7], args[8], isMeleeDamage,
                        damageTypeFlag);

                }

            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        private void HandleCEventNetworkPlayerEnteredVehicle(Entity player, Entity ent)
        {
            if (player.Handle != Cache.PlayerPed.Handle) return;

            if (ent is Vehicle)
            {
                int vehicleNetworkId = ent.NetworkId;
                int playerVehicle = player.State.Get($"{StateBagKey.PLAYER_VEHICLE}") == null ? 0 : player.State.Get($"{StateBagKey.PLAYER_VEHICLE}");

                if (playerVehicle == 0) return;
                if (playerVehicle != vehicleNetworkId) return;

                PlayerManager.GetModule().SetVehicle(ent.Handle);
            }
        }

        /// <summary>
        /// Handle game event CEventNetworkEntityDamage,
        /// Useful for indicating entity damage/died/destroyed.
        /// </summary>
        /// <param name="victim">victim</param>
        /// <param name="attacker">attacker</param>
        /// <param name="arg2">Unknown</param>
        /// <param name="isDamageFatal">Is damage fatal to entity. or victim died/destroyed.</param>
        /// <param name="weaponInfoHash">Probably related to common.rpf/data/ai => Item type = "CWeaponInfo"</param>
        /// <param name="arg5">Unknown</param>
        /// <param name="arg6">Unknown</param>
        /// <param name="arg7">Unknown, might be int</param>
        /// <param name="arg8">Unknown, might be int</param>
        /// <param name="isMeleeDamage">Is melee damage</param>
        /// <param name="damageTypeFlag">0 for peds, 116 for the body of a vehicle, 93 for a tire, 120 for a side window, 121 for a rear window, 122 for a windscreen, etc</param>
        private static void HandleCEventNetworkEntityDamaged(
            Entity victim, Entity attacker, int arg2, bool isDamageFatal, uint weaponInfoHash,
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
                    OnDeath?.Invoke(playerVictim, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

            }
        }
    }

    public class GameEventTigger : Manager<GameEventTigger>
    {
        public override void Begin()
        {
            Logger.Info($"- [GameEventTigger] Begin ------------------------");

            GameEventManager.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;

            EventSystem.Attach("gameEvent:kill", new EventCallback(metadata =>
            {
                if (Cache.PlayerPed.IsAlive)
                    Cache.PlayerPed.Kill();

                return null;
            }));
        }

        // NOTE: MOVE TO CORE

        private static void GameEventManager_OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {

                if (!Mission.isOnMission) return;

                bool headshot = victim.LastDamagedBone() == Bone.SKEL_Head;

                //if (Decorators.GetBoolean(victim.Handle, Decorators.PED_MISSION))
                //{
                //    if (headshot)
                //    {
                //        if (PluginManager.IsBirthday)
                //        {
                //            Vector3 coord = Cache.PlayerPed.Position;
                //            Vector3 target = victim.Position;

                //            float distance = API.Vdist(coord.X, coord.Y, coord.Z, target.X, target.Y, target.Z);
                //            float volumeMultiplier = (0.5f / 100f);
                //            float distanceVolume = 0.5f - (distance * volumeMultiplier);

                //            if (distance <= 100f)
                //            {
                //                SoundManager.PlaySFX($"party", distanceVolume);

                //                ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset("scr_martin1");
                //                await particleEffectsAsset.Request(1000);
                //                particleEffectsAsset.CreateEffectOnEntity("scr_sol1_sniper_impact", victim.Bones[Bone.SKEL_Head], off: new Vector3(0, 0, .8f), startNow: true);
                //            }
                //        }
                //    }
                //}

                if (attacker != Game.Player) return;

                Mission.RegisteredPeds.ForEach(p =>
                {
                    if (p.Handle != victim.Handle) return;

                    if (p.IsMission && p.IsHostage)
                    {
                        Notify.Alert("A hostage has been killed.");
                        return;
                    }


                });
            }
            catch (Exception ex)
            {

            }
        }
    }
}
