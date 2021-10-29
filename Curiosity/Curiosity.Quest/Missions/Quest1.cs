using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using System.Drawing;
using System.Threading.Tasks;
using Curiosity.MissionManager.Client.Diagnostics;
using System.Collections.Generic;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.MissionManager.Client.Interface;
using System;
using Curiosity.Systems.Library.Utils;

namespace Curiosity.Quest.Missions
{
    [MissionInfo("Halloween Quest", "quest1", -543.9988f, -157.8393f, 38.54123f, MissionType.Halloween, true, "None")]
    public class Quest1 : Mission
    {
        const int ZOMBIE_AMOUNT = 5;
        private const int NUMBER_OF_ZOMBIES_TO_KILL = 10;
        const float ZOMBIE_ATTR_CHANCE = 0.6f;
        const int ZOMBIE_MAX_HEALTH = 500;
        const int ZOMBIE_MAX_ARMOR = 500;
        const string ZOMBIE_DECOR = "_ZOMBIE_HALLOWEEN";

        const float SPAWN_MIN_DISTANCE = 10f;
        const float SPAWN_DESPAWN_DISTANCE = 50f;
        const double SPAWN_EVENT_CHANCE = 0.005;
        const int SPAWN_TICK_RATE = 100;
        const string SPAWN_DESPAWN_DECOR = "_MARKED_FOR_DESPAWN";
        const float SPAWN_HOST_DECIDE_DISTANCE = 300f;

        List<Ped> zombies;
        public static RelationshipGroup ZombieGroup { get; private set; }

        Vector3 _start = new Vector3(-543.9988f, -157.8393f, 38.54123f);
        Vector3 _scale = new Vector3(1f, 1f, 1f);

        List<Ped> peds = new List<Ped>();

        const string BASE_FOLDER = "assets/images/";
        
        NUIMarker questMarker1; // -1646.346f, -1118.321f, 13.0319f | q1shrimp
        NUIMarker questMarker2; // -1030.282f, -2751.631f, 21.1143f | q1bird
        NUIMarker questMarker3; // -1617.125f, -3013.02f, -75.20506f | q1trophy
        NUIMarker questMarker4; // 569.2401f, -3126.711f, 18.76861f | q1mayweather
        NUIMarker questMarker5; // 4893.735f, -4904.243f, 3.486644f | q1perico
        NUIMarker questMarker6; // 777.3207f, 1175.666f, 345.9563f | q1thetip
        NUIMarker questMarker7; // 1640.993f, 2523.347f, 45.56486f | q1pumpIron
        NUIMarker questMarker8; // -1724.81f, -223.4717f, 56.34682f | q1flowers

        string textureDictionary = "MISSION_HALLOWEEN_DICT";
        long textureDict;
        string q1shrimp = "q1shrimp";
        string q1bird = "q1bird";
        string q1trophy = "q1trophy";
        string q1mayweather = "q1mayweather";
        string q1perico = "q1perico";
        string q1thetip = "q1thetip";
        string q1pumpIron = "q1pumpIron";
        string q1flowers = "q1flowers";

        Vector3 posQ1Shrimp = new Vector3(-1646.346f, -1118.321f, 13.0319f);
        Vector3 posQ1Bird = new Vector3(-1030.282f, -2751.631f, 21.1143f);
        Vector3 posQ1Trophy = new Vector3(-1617.125f, -3013.02f, -75.20506f);
        Vector3 posQ1MayWeather = new Vector3(569.2401f, -3126.711f, 18.76861f);
        Vector3 posQ1Perico = new Vector3(4893.735f, -4904.243f, 3.486644f);
        Vector3 posQ1TheTip = new Vector3(777.3207f, 1175.666f, 345.9563f);
        Vector3 posQ1PumpIron = new Vector3(1640.993f, 2523.347f, 45.56486f);
        Vector3 posQ1Flowers = new Vector3(-1724.81f, -223.4717f, 56.34682f);

        Color markerColor = Color.FromArgb(0, 0, 0, 0);

        Dictionary<string, NUIMarker> markerPositions = new Dictionary<string, NUIMarker>();

        MissionPhase missionPhase = MissionPhase.MARKER_ONE_SETUP;
        int _soundId = -1;

        NUIMarker activeMarker;
        string currentClueText = string.Empty;
        string currentClueTexture = string.Empty;
        int NumberKilled = 0;

        public override async void Start()
        {
            zombies = new List<Ped>();
            ZombieGroup = World.AddRelationshipGroup("zombies");

            textureDict = CreateRuntimeTxd(q1shrimp);
            CreateRuntimeTextureFromImage(textureDict, q1shrimp, $"assets/images/{q1shrimp}.png");
            textureDict = CreateRuntimeTxd(q1bird);
            CreateRuntimeTextureFromImage(textureDict, q1bird, $"assets/images/{q1bird}.png");
            textureDict = CreateRuntimeTxd(q1trophy);
            CreateRuntimeTextureFromImage(textureDict, q1trophy, $"assets/images/{q1trophy}.png");
            textureDict = CreateRuntimeTxd(q1mayweather);
            CreateRuntimeTextureFromImage(textureDict, q1mayweather, $"assets/images/{q1mayweather}.png");
            textureDict = CreateRuntimeTxd(q1perico);
            CreateRuntimeTextureFromImage(textureDict, q1perico, $"assets/images/{q1perico}.png");
            textureDict = CreateRuntimeTxd(q1thetip);
            CreateRuntimeTextureFromImage(textureDict, q1thetip, $"assets/images/{q1thetip}.png");
            textureDict = CreateRuntimeTxd(q1pumpIron);
            CreateRuntimeTextureFromImage(textureDict, q1pumpIron, $"assets/images/{q1pumpIron}.png");
            textureDict = CreateRuntimeTxd(q1flowers);
            CreateRuntimeTextureFromImage(textureDict, q1flowers, $"assets/images/{q1flowers}.png");

            questMarker1 = new NUIMarker(MarkerType.VerticalCylinder, posQ1Shrimp, 5f, markerColor, true);
            questMarker2 = new NUIMarker(MarkerType.VerticalCylinder, posQ1Bird, 5f, markerColor, true);
            questMarker3 = new NUIMarker(MarkerType.VerticalCylinder, posQ1Trophy, 5f, markerColor, true);
            questMarker4 = new NUIMarker(MarkerType.VerticalCylinder, posQ1MayWeather, 5f, markerColor, true);
            questMarker5 = new NUIMarker(MarkerType.VerticalCylinder, posQ1Perico, 5f, markerColor, true);
            questMarker6 = new NUIMarker(MarkerType.VerticalCylinder, posQ1TheTip, 5f, markerColor, true);
            questMarker7 = new NUIMarker(MarkerType.VerticalCylinder, posQ1PumpIron, 5f, markerColor, true);
            questMarker8 = new NUIMarker(MarkerType.VerticalCylinder, posQ1Flowers, 5f, markerColor, true);

            markerPositions.Add("pos1", questMarker1);
            markerPositions.Add("pos2", questMarker2);
            markerPositions.Add("pos3", questMarker3);
            markerPositions.Add("pos4", questMarker4);
            markerPositions.Add("pos5", questMarker5);
            markerPositions.Add("pos6", questMarker6);
            markerPositions.Add("pos7", questMarker7);
            markerPositions.Add("pos8", questMarker8);

            missionPhase = MissionPhase.MARKER_EIGHT_SETUP;

            await BaseScript.Delay(1000);

            Notify.Info($"Press ~b~ALT+1~s~ to show the clue again.");

            MissionInitiated();
        }

        void MissionInitiated()
        {
            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }
        private async Task OnZombieTick()
        {
            await BaseScript.Delay(SPAWN_TICK_RATE);

            if (zombies.Count < ZOMBIE_AMOUNT)
                SpawnRandomZombie();
            else if (zombies.Count > 0)
                foreach (Ped zombie in zombies.ToArray())
                    if (!Helpers.IsPosShitSpawn(Mission.PlayerList, zombie.Position, SPAWN_DESPAWN_DISTANCE)
                        || zombie.IsDead)
                    {
                        if (zombie.IsDead) NumberKilled++;

                        Decorators.Set(zombie.Handle, SPAWN_DESPAWN_DECOR, true);
                        zombie.MarkAsNoLongerNeeded();

                        zombies.Remove(zombie);
                    }

            HandleZombies();
        }

        private void OnCleanUp()
        {
            Ped[] worldPeds = World.GetAllPeds();
            foreach (Ped ped in worldPeds)
            {
                if (!ped.Exists()) continue;

                if (Decorators.GetBoolean(ped.Handle, SPAWN_DESPAWN_DECOR))
                {
                    if (ped.IsDead) NumberKilled++;

                    ped.MarkAsNoLongerNeeded();
                }
            }
        }

        private void HandleZombies()
        {
            Ped[] worldPeds = World.GetAllPeds();
            foreach (Ped ped in worldPeds)
            {
                if (!ped.Exists()) continue;

                if (Decorators.GetBoolean(ped.Handle, ZOMBIE_DECOR))
                {
                    ped.Voice = "ALIENS";
                    ped.IsPainAudioEnabled = false;
                    ped.RelationshipGroup.SetRelationshipBetweenGroups(Game.PlayerPed.RelationshipGroup, Relationship.Hate, true);

                    RequestAnimSet("move_m@drunk@verydrunk");
                    SetPedMovementClipset(ped.Handle, "move_m@drunk@verydrunk", 1f);
                }
            }
        }

        private void ZombieAttrChances(Ped zombie)
        {
            if (AttrChance())
                SetPedRagdollOnCollision(zombie.Handle, true);
            if (AttrChance())
                SetPedHelmet(zombie.Handle, true);
            if (AttrChance())
                SetPedRagdollBlockingFlags(zombie.Handle, 1);
        }

        private bool AttrChance()
        {
            return Utility.RANDOM.Bool(ZOMBIE_ATTR_CHANCE);
        }

        private async void SpawnRandomZombie()
        {
            Vector3 spawnPos = Helpers.GetRandomSpawnPosFromPlayer(Game.Player, SPAWN_MIN_DISTANCE, SPAWN_DESPAWN_DISTANCE);

            

            if (!Helpers.IsPosShitSpawn(Mission.PlayerList, spawnPos, SPAWN_MIN_DISTANCE))
            {
                Ped zombie = await World.CreatePed(PedHash.Zombie01, spawnPos);
                int zombieHandle = zombie.Handle;
                SetPedCombatRange(zombieHandle, 2);
                SetPedHearingRange(zombieHandle, float.MaxValue);
                SetPedCombatAttributes(zombieHandle, 46, true);
                SetPedCombatAttributes(zombieHandle, 5, true);
                SetPedCombatAttributes(zombieHandle, 1, false);
                SetPedCombatAttributes(zombieHandle, 0, false);
                SetPedCombatAbility(zombieHandle, 0);
                // SetAiMeleeWeaponDamageModifier(float.MaxValue);
                SetPedRagdollBlockingFlags(zombieHandle, 4);
                SetPedCanPlayAmbientAnims(zombieHandle, false);

                int randHealth = Utility.RANDOM.Next(1, ZOMBIE_MAX_HEALTH);
                zombie.MaxHealth = randHealth;
                zombie.Health = randHealth;
                zombie.Armor = Utility.RANDOM.Next(ZOMBIE_MAX_ARMOR);
                zombie.RelationshipGroup = ZombieGroup;
                ZombieAttrChances(zombie);
                zombie.CanSufferCriticalHits = false;

                zombie.Task.WanderAround();
                Decorators.Set(zombieHandle, ZOMBIE_DECOR, true);

                zombies.Add(zombie);
            }
        }

        async Task OnMissionTick()
        {
            ShowClue();

            switch (missionPhase)
            {
                case MissionPhase.MARKER_ONE_SETUP:
                    ChangeActiveMarker(string.Empty, "pos1");
                    currentClueText = "No shrimps are allowed here.";
                    currentClueTexture = q1shrimp;
                    missionPhase = MissionPhase.MARKER_ONE;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_ONE:
                    if ((await CanMoveToNextPhase("pos1")))
                    {
                        missionPhase = MissionPhase.MARKER_TWO_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_TWO_SETUP:
                    ChangeActiveMarker("pos1", "pos2");
                    currentClueText = "Who saw her when you started, her birds will take you any where.";
                    currentClueTexture = q1bird;
                    missionPhase = MissionPhase.MARKER_TWO;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_TWO:
                    if ((await CanMoveToNextPhase("pos2")))
                    {
                        missionPhase = MissionPhase.MARKER_THREE_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_THREE_SETUP:
                    ChangeActiveMarker("pos2", "pos3");
                    currentClueText = "In the club, you always have an award on display.";
                    currentClueTexture = q1trophy;
                    missionPhase = MissionPhase.MARKER_THREE;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_THREE:
                    if ((await CanMoveToNextPhase("pos3")))
                    {
                        missionPhase = MissionPhase.MARKER_FOUR_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_FOUR_SETUP:
                    ChangeActiveMarker("pos3", "pos4");
                    currentClueText = "These guys have a sub, but some where the gate was made to open.";
                    currentClueTexture = q1mayweather;
                    missionPhase = MissionPhase.MARKER_FOUR;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_FOUR:
                    if ((await CanMoveToNextPhase("pos4")))
                    {
                        missionPhase = MissionPhase.MARKER_FIVE_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_FIVE_SETUP:
                    ChangeActiveMarker("pos4", "pos5");
                    currentClueText = "This holiday space is dark to visit on Halloween, but the party is still going hard.";
                    currentClueTexture = q1perico;
                    missionPhase = MissionPhase.MARKER_FIVE;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_FIVE:
                    if ((await CanMoveToNextPhase("pos5")))
                    {
                        missionPhase = MissionPhase.MARKER_SIX_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_SIX_SETUP:
                    ChangeActiveMarker("pos5", "pos6");
                    currentClueText = "You'll only want the tip.";
                    currentClueTexture = q1thetip;
                    missionPhase = MissionPhase.MARKER_SIX;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_SIX:
                    if ((await CanMoveToNextPhase("pos6")))
                    {
                        Game.PlayerPed.Weapons.Give(WeaponHash.Parachute, 1, true, true);
                        missionPhase = MissionPhase.MARKER_SEVEN_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_SEVEN_SETUP:
                    ChangeActiveMarker("pos6", "pos7");
                    currentClueText = "You'll find this, where the convicts pump iron.";
                    currentClueTexture = q1pumpIron;
                    missionPhase = MissionPhase.MARKER_SEVEN;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_SEVEN:
                    if ((await CanMoveToNextPhase("pos7")))
                    {
                        missionPhase = MissionPhase.MARKER_EIGHT_SETUP;
                    }
                    break;
                case MissionPhase.MARKER_EIGHT_SETUP:
                    ChangeActiveMarker("pos7", "pos8");
                    currentClueText = "Mans best friend always visits this place.";
                    currentClueTexture = q1flowers;
                    missionPhase = MissionPhase.MARKER_EIGHT;
                    ShowClueNow();
                    break;
                case MissionPhase.MARKER_EIGHT:
                    if ((await CanMoveToNextPhase("pos8")))
                    {
                        missionPhase = MissionPhase.SPAWN_ZOMBIES_SETUP;
                    }
                    break;
                case MissionPhase.SPAWN_ZOMBIES_SETUP:
                    missionPhase = MissionPhase.SPAWN_ZOMBIES;
                    Notify.Warning($"Some strange sounds are coming from nearby!", "top-right");
                    ForceLightningFlash();
                    MissionManager.Instance.RegisterTickHandler(OnZombieTick);
                    break;
                case MissionPhase.SPAWN_ZOMBIES: // Monitor kills
                    if (NumberKilled >= NUMBER_OF_ZOMBIES_TO_KILL)
                    {
                        Pass();
                        MissionManager.Instance.DeregisterTickHandler(OnZombieTick);

                        if (zombies.Count > 0)
                        {
                            foreach(Ped ped in zombies.ToArray())
                            {
                                ped.Kill();
                                ped.MarkAsNoLongerNeeded();

                                zombies.Remove(ped);
                            }
                            zombies.Clear();
                        }
                    }
                    break;
            }
        }

        async void ShowClue(bool force = false)
        {
            if (ControlHelper.IsControlPressed(Control.SelectWeaponUnarmed, modifier: ControlModifier.Alt) || force)
            {
                BlockWeaponWheelThisFrame();

                if (!HasStreamedTextureDictLoaded(currentClueTexture))
                    RequestStreamedTextureDict(currentClueTexture, false);

                float posY = 0.2f;

                if (currentClueText.Length >= 39)
                    posY = 0.225f;

                if (currentClueText.Length >= 65)
                    posY = 0.25f;

                DrawSprite(currentClueTexture, currentClueTexture, 0.17f, posY, 0.3f, 0.275f, 0f, 255, 255, 255, 255);

                Screen.DisplayHelpTextThisFrame(currentClueText);
            }
        }

        async void ShowClueNow()
        {
            DateTime timeToHide = DateTime.UtcNow.AddSeconds(5);
            while (DateTime.UtcNow < timeToHide)
            {
                ShowClue(true);
                await BaseScript.Delay(0);
            }
        }

        async void ChangeActiveMarker(string previous, string next)
        {
            if (!string.IsNullOrEmpty(previous))
                Mission.RemoveMarker(previous);

            await BaseScript.Delay(1000);

            activeMarker = markerPositions[next];
            Mission.AddMarker(next, activeMarker);

            Logger.Debug($"Set new active position: {next}");

            await BaseScript.Delay(1000);
        }

        void ShowClueMessage()
        {
            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to find the next clue.");
        }

        async Task<bool> CanMoveToNextPhase(string marker)
        {
            if (Mission.IsMarkerActive(marker))
            {
                ShowClueMessage();
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    int mySoundId = -1;
                    PlaySoundFrontend(mySoundId, "Bus_Schedule_Pickup", "DLC_PRISON_BREAK_HEIST_SOUNDS", true);
                    await BaseScript.Delay(500);
                    StopSound(_soundId);
                    ReleaseSoundId(_soundId);
                    _soundId = -1;
                    return true;
                }
                return false;
            }
            return false;
        }


    }

    enum MissionPhase
    {
        START,
        MARKER_ONE_SETUP,
        MARKER_ONE,
        MARKER_TWO_SETUP,
        MARKER_TWO,
        MARKER_THREE_SETUP,
        MARKER_THREE,
        MARKER_FOUR_SETUP,
        MARKER_FOUR,
        MARKER_FIVE_SETUP,
        MARKER_FIVE,
        MARKER_SIX_SETUP,
        MARKER_SIX,
        MARKER_SEVEN_SETUP,
        MARKER_SEVEN,
        MARKER_EIGHT_SETUP,
        MARKER_EIGHT,
        SPAWN_ZOMBIES_SETUP,
        SPAWN_ZOMBIES,
        END
    }
}
