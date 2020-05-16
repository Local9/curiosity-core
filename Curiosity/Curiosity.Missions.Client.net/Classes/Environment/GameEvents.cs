using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Scripts;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Classes.Environment
{
    public delegate void PlayerKillPlayerEvent(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PlayerKillPedEvent(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPlayerEvent(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void PedKillPedEvent(Ped attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void EntityKillEntityEvent(Entity attacker, Entity victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag);
    public delegate void DeadEvent(Player victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag);

    class GameEventHandlers
    {
        static int numberOfPedsKilled = 0;

        public static void Init()
        {
            GameEvents.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;
            GameEvents.OnPedKillPlayer += GameEventManager_OnPedKillPlayer;
            GameEvents.OnPlayerKillPlayer += GameEventManager_OnPlayerKillPlayer;
            GameEvents.OnDeath += GameEventManager_OnDeath;

            Client.GetInstance().RegisterTickHandler(OnPedDeathCooldown);
        }

        private async static Task OnPedDeathCooldown()
        {
            long gameTimer = API.GetGameTimer();
            while (numberOfPedsKilled > 0)
            {
                if ((API.GetGameTimer() - gameTimer) > 15000)
                {
                    gameTimer = API.GetGameTimer();
                    numberOfPedsKilled--;
                }

                if (numberOfPedsKilled < 0)
                {
                    numberOfPedsKilled = 0;
                }

                await Client.Delay(100);
            }
        }

        private static void GameEventManager_OnDeath(Player victim, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            if (victim.Character.GetKiller() == null)
            {
                string serializedEvent = JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Client:Notification:Simple", $"{victim.Name} killed themselves"));
                BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
            }
        }

        private static void GameEventManager_OnPlayerKillPlayer(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {
                if (victim == Game.Player && victim.IsDead)
                {
                    Client.TriggerEvent("curiosity:Client:Player:Revive");
                }

                if (attacker != Game.Player) return;

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
                    attacker.Character.Kill();
                }

                if (victim.IsDead)
                {
                    attacker.Character.Kill();
                }
                else
                {
                    attacker.Character.ApplyDamage(damage);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void GameEventManager_OnPedKillPlayer(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Client:Notification:Simple", $"{victim.Name} Died"));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }

        private static async void GameEventManager_OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            try
            {
                bool headshot = victim.LastDamagedBone() == Bone.SKEL_Head;

                if (Decorators.GetBoolean(victim.Handle, Client.DECOR_PED_MISSION))
                {
                    if (headshot)
                    {
                        if (Client.IsBirthday)
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

                if (Decorators.GetBoolean(victim.Handle, Client.DECOR_PED_MISSION))
                {

                    SkillMessage skillMessage = new SkillMessage();
                    skillMessage.PlayerHandle = $"{attacker.ServerId}";
                    skillMessage.IsHeadshot = headshot;

                    if (Decorators.GetBoolean(victim.Handle, Client.DECOR_PED_HOSTAGE))
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
                else
                {
                    numberOfPedsKilled++;

                    if (numberOfPedsKilled == 3)
                        Screen.ShowNotification($"~r~Warning:~w~ You will be punished if you killing random peds.");

                    if (numberOfPedsKilled >= 5)
                    {
                        if (numberOfPedsKilled == 5)
                            Screen.ShowNotification($"~r~Warning:~w~ Damage to peds is now reflected");

                        int damage = 10 * numberOfPedsKilled;

                        if (attacker.Character.IsInvincible)
                        {
                            attacker.Character.Kill();
                        }

                        if (attacker.Character.Health == 0)
                        {
                            attacker.Character.Kill();
                        }

                        attacker.Character.ApplyDamage(damage);
                    }
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
            Client.GetInstance().RegisterEventHandler("gameEventTriggered", new Action<string, List<dynamic>>(OnGameEventTriggered));
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
}
