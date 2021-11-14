using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Managers.Events;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.Managers.Events
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);
    public delegate void EnteredVehicle(Player player, Vehicle vehicle);
}

namespace Curiosity.Police.Client.Managers
{
    public class GameEventManager : Manager<GameEventManager>
    {

        public static event PlayerKillPlayerEvent OnPlayerKillPlayer;
        public static event PlayerKillPedEvent OnPlayerKillPed;
        public static event PedKillPlayerEvent OnPedKillPlayer;
        public static event PedKillPedEvent OnPedKillPed;
        public static event EntityKillEntityEvent OnEntityKillEntity;
        public static event DeadEvent OnDeath;
        public static event EnteredVehicle OnEnteredVehicle;

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
                    Player player = new Player((int)args[0]);

                    if (player.ServerId != Game.Player.ServerId) return;

                    int entityId = (int)args[1];
                    if (!API.IsEntityAVehicle(entityId)) return;

                    Vehicle vehicle = new Vehicle(entityId);

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

        private void HandleCEventNetworkPlayerEnteredVehicle(Player player, Vehicle vehicle)
        {
            OnEnteredVehicle?.Invoke(player, vehicle);
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
}
