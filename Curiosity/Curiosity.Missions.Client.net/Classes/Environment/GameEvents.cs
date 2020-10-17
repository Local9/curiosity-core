using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Utils;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.Scripts;
using Curiosity.Missions.Client.Utils;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Missions.Client.Classes.Environment
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, CitizenFX.Core.Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(CitizenFX.Core.Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(CitizenFX.Core.Ped attacker, CitizenFX.Core.Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Player victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);

    class GameEventHandlers
    {
        static bool wasKilledByScript = false;

        static List<string> knockedOut = new List<string>()
        {
            "got knocked the f' out!",
            "got given a black eye",
            "got into a fist fight and lost",
            "got given a noggy",
            "got given a weggie",
            "got given a chinese burn",
            "got told their mom has an onlyfans",
        };

        public static void Init()
        {
            GameEvents.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;
            GameEvents.OnPedKillPlayer += GameEventManager_OnPedKillPlayer;
            GameEvents.OnPlayerKillPlayer += GameEventManager_OnPlayerKillPlayer;
            GameEvents.OnDeath += GameEventManager_OnDeath;
        }

        private static void GameEventManager_OnDeath(Player victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            if (victim.Character.GetKiller() == null && PluginManager.IsPlayerSpawned && !wasKilledByScript)
            {
                string message = "killed themselves";

                string serializedEvent = JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Client:Notification:Simple", $"{victim.Name} {message}"));
                BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);

                BaseScript.TriggerServerEvent("curiosity:Server:Log:Message", $"{victim.Name} {message}");

                wasKilledByScript = false;
            }
        }

        private static void GameEventManager_OnPlayerKillPlayer(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {
                if (victim == attacker) return;

                if (victim == Game.Player && victim.IsDead)
                {
                    PluginManager.TriggerEvent("curiosity:Client:Player:Revive");
                }

                if (attacker != Game.Player) return;

                wasKilledByScript = false;

                // RDM...
                int victimMaxHealth = victim.Character.MaxHealth;
                int victimCurrentHealth = victim.Character.Health;
                int damage = victimMaxHealth - victimCurrentHealth;

                if (damage < 0)
                {
                    damage = 50;
                }

                if (attacker.IsInvincible)
                {
                    wasKilledByScript = true;
                    attacker.Character.Kill();
                }

                if (victim.IsDead)
                {
                    wasKilledByScript = true;
                    attacker.Character.Kill();
                }
                else
                {
                    attacker.Character.ApplyDamage(damage);
                }

                string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Client:Notification:Simple", $"[{victim.ServerId}] {victim.Name} was killed by [{attacker.ServerId}] {attacker.Name}"));
                BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);

                BaseScript.TriggerServerEvent("curiosity:Server:Log:Message", $"[{victim.ServerId}] {victim.Name} was killed by [{attacker.ServerId}] {attacker.Name}");
            }
            catch (Exception ex)
            {

            }
        }

        private static void GameEventManager_OnPedKillPlayer(CitizenFX.Core.Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            string message = "died in mysterious ways";
            if (isMeleeDamage)
            {
                message = knockedOut[Utility.RANDOM.Next(knockedOut.Count)];
            }
            else if (damageTypeFlag == 0)
            {
                message = "was killed by a ped";
            }

            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Client:Notification:Simple", $"{victim.Name} {message}"));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);

            BaseScript.TriggerServerEvent("curiosity:Server:Log:Message", $"{victim.Name} {message}");
        }

        private static async void GameEventManager_OnPlayerKillPed(Player attacker, CitizenFX.Core.Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {
                bool headshot = victim.LastDamagedBone() == Bone.SKEL_Head;

                if (Decorators.GetBoolean(victim.Handle, Decorators.PED_MISSION))
                {
                    if (headshot)
                    {
                        if (PluginManager.IsBirthday)
                        {
                            Vector3 coord = Game.PlayerPed.Position;
                            Vector3 target = victim.Position;

                            float distance = API.Vdist(coord.X, coord.Y, coord.Z, target.X, target.Y, target.Z);
                            float volumeMultiplier = (0.5f / 100f);
                            float distanceVolume = 0.5f - (distance * volumeMultiplier);

                            if (distance <= 100f)
                            {
                                SoundManager.PlaySFX($"party", distanceVolume);

                                ParticleEffectsAsset particleEffectsAsset = new ParticleEffectsAsset("scr_martin1");
                                await particleEffectsAsset.Request(1000);
                                particleEffectsAsset.CreateEffectOnEntity("scr_sol1_sniper_impact", victim.Bones[Bone.SKEL_Head], off: new Vector3(0, 0, .8f), startNow: true);
                            }
                        }
                    }
                }

                if (attacker != Game.Player) return;

                if (Decorators.GetBoolean(victim.Handle, Decorators.PED_MISSION))
                {

                    SkillMessage skillMessage = new SkillMessage();
                    skillMessage.PlayerHandle = $"{attacker.ServerId}";
                    skillMessage.IsHeadshot = headshot;

                    if (Decorators.GetBoolean(victim.Handle, Decorators.PED_HOSTAGE))
                    {
                        skillMessage.MissionPed = true;
                        skillMessage.Increase = false;
                    }
                    else
                    {
                        skillMessage.MissionPed = true;
                        skillMessage.Increase = true;
                    }

                    string json = JsonConvert.SerializeObject(skillMessage);

                    BaseScript.TriggerServerEvent("curiosity:Server:Missions:KilledPed", Encode.StringToBase64(json));
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    class GameEvents
    {
        public static event PlayerKillPlayerEvent OnPlayerKillPlayer;
        public static event PlayerKillPedEvent OnPlayerKillPed;
        public static event PedKillPlayerEvent OnPedKillPlayer;
        public static event PedKillPedEvent OnPedKillPed;
        public static event EntityKillEntityEvent OnEntityKillEntity;
        public static event DeadEvent OnDeath;

        public static void Init()
        {
            PluginManager.Instance.RegisterEventHandler("gameEventTriggered", new Action<string, List<dynamic>>(OnGameEventTriggered));
        }

        private static void OnGameEventTriggered(string name, List<dynamic> args)
        {
            // Log.Debug($"game event {name} ({String.Join(", ", args.ToArray())})");

            try
            {

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
                Log.Error($"{ex}");
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
                CitizenFX.Core.Ped pedAttacker = null;
                Player playerAttacker = null;
                // Ped
                if (attacker is CitizenFX.Core.Ped)
                {
                    pedAttacker = (CitizenFX.Core.Ped)attacker;
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
                CitizenFX.Core.Ped pedVictim = null;
                Player playerVictim = null;
                // Ped
                if (victim is CitizenFX.Core.Ped)
                {
                    pedVictim = (CitizenFX.Core.Ped)victim;
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
}
