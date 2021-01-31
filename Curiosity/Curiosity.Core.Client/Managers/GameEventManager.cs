using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Managers.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Threading.Tasks;

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
                Logger.Debug($"{ex}");
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
        private void HandleCEventNetworkEntityDamaged(
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
                    OnDeath?.Invoke(attacker, isMeleeDamage, weaponInfoHash, damageTypeFlag);
                }

            }

        }
    }

    public class GameEventTigger : Manager<GameEventTigger>
    {
        private const string DEAD_ANIM_DICT = "dead";
        DateTime DateOfDeath = DateTime.Now;
        List<string> DeathAnim = new List<string>() { "dead_a", "dead_b", "dead_c", "dead_d", "dead_e", "dead_f", "dead_g", "dead_h" };
        string CurrentDeathAnim;

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
            
        private async void GameEventManager_OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            EventSystem.Send("character:death");

            var player = Cache.Player;

            int costOfRespawn = player.Character.RespawnCharge();

            Game.PlayerPed.Health = Game.PlayerPed.MaxHealth;

            API.RequestAnimDict(DEAD_ANIM_DICT);

            while (!API.HasAnimDictLoaded(DEAD_ANIM_DICT))
            {
                await BaseScript.Delay(0);
            }
            CurrentDeathAnim = DeathAnim[Utility.RANDOM.Next(DeathAnim.Count)];

            Game.PlayerPed.Task.PlayAnimation(DEAD_ANIM_DICT, CurrentDeathAnim, 8f, -1, AnimationFlags.StayInEndFrame);
            Game.PlayerPed.IsPositionFrozen = true;

            PluginManager.Instance.AttachTickHandler(OnRespawnControlTask);
        }

        async void RespawnAtHospital(CuriosityPlayer curiosityPlayer)
        {
            await ScreenInterface.FadeOut();

            Vector3 spawnLocation = LocationManager.LocationManagerInstance.NearestHospital();
            curiosityPlayer.Character.Revive(new Position(spawnLocation.X, spawnLocation.Y, spawnLocation.Z, Game.PlayerPed.Heading));

            await BaseScript.Delay(1000);

            await ScreenInterface.FadeIn(3000);
        }

        async void Respawn(CuriosityPlayer curiosityPlayer)
        {
            await ScreenInterface.FadeOut();
            PluginManager.Instance.DetachTickHandler(OnRespawnControlTask);

            Vector3 spawnLocation = curiosityPlayer.Entity.Position.AsVector();
            curiosityPlayer.Character.Revive(new Position(spawnLocation.X, spawnLocation.Y, spawnLocation.Z, Game.PlayerPed.Heading));

            await BaseScript.Delay(1000);

            await ScreenInterface.FadeIn(3000);
        }

        async Task OnRespawnControlTask()
        {
            Cache.Player.DisableHud();
            TimeSpan timeSpan = (DateOfDeath.AddMinutes(5) - DateTime.Now);
            string timeSpanLeft = timeSpan.ToString(@"mm\:ss");

            if (!API.IsEntityPlayingAnim(Game.PlayerPed.Handle, DEAD_ANIM_DICT, CurrentDeathAnim, 3))
            {
                API.RequestAnimDict(DEAD_ANIM_DICT);

                while (!API.HasAnimDictLoaded(DEAD_ANIM_DICT))
                {
                    await BaseScript.Delay(0);
                }

                Game.PlayerPed.Task.PlayAnimation(DEAD_ANIM_DICT, CurrentDeathAnim, 8f, -1, AnimationFlags.StayInEndFrame);
            }

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
    }
}
