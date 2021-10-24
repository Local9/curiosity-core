using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class DispatchManager : Manager<DispatchManager>
    {
        private PedType[] copTypes = new PedType[3] { PedType.PED_TYPE_COP, PedType.PED_TYPE_SWAT, PedType.PED_TYPE_ARMY };

        private DispatchType[] dispatchTypes = new DispatchType[11] {
            DispatchType.DT_PoliceAutomobile,
            DispatchType.DT_PoliceHelicopter,
            DispatchType.DT_SwatAutomobile,
            DispatchType.DT_PoliceRiders,
            DispatchType.DT_PoliceVehicleRequest,
            DispatchType.DT_PoliceRoadBlock,
            DispatchType.DT_PoliceAutomobileWaitPulledOver,
            DispatchType.DT_PoliceAutomobileWaitCruising,
            DispatchType.DT_SwatHelicopter,
            DispatchType.DT_PoliceBoat,
            DispatchType.DT_ArmyVehicle
        };

        private Dictionary<string, TrackedPed> _trackedPeds = new Dictionary<string, TrackedPed>();
        private Dictionary<string, Colour> _colours = new Dictionary<string, Colour>();

        private int[] maxCops = new int[5] { 10, 20, 30, 40, 50 };
        private int[] addCopsAmount = new int[5] { 5, 10, 15, 20, 25 };

        private int currentCops = -1;
        // Ticks
        private int pedTick = -1;
        private int copTick = -1;
        private int copTickStore = -1;
        private int coolTick = 0;
        private int fillTick = -1;
        private int countdownTick = -1;
        private int countdown = -1;

        private float curFill = 0.0f;

        private bool dispatch = false;
        private bool active = false;
        private bool decreased = false;

        // CONFIGURATION VALUES
        private int copsMinimum = 0; // Minimum cop number (Use negative values for longer grace periods)
        private int DecreaseLevel = 0; // Wanted level stars to decrease when there are no cops remaining - This overrides WantedClear (0 = Disable)

        public bool ResetCopsWhenCleared // Reset cop number to base when wanted level is cleared/decreased
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:ResetCopsWhenCleared");

                if (val is null)
                    ResetCopsWhenCleared = false;

                return GetResourceKvpInt("curiosity:dispatch:ResetCopsWhenCleared") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:ResetCopsWhenCleared", val);
            }
        }

        public bool RemoveCopsWhenFarAway // Decrease cop number for evaded cops (Far away)
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAway");

                if (val is null)
                    RemoveCopsWhenFarAway = true;

                return GetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAway") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAway", val);
            }
        }

        public bool RemoveCopsWhenFarAwayChase // CopsRemoveEvaded only decreases cop number during chases
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAwayChase");

                if (val is null)
                    RemoveCopsWhenFarAwayChase = true;

                return GetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAwayChase") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:RemoveCopsWhenFarAwayChase", val);
            }
        }

        public bool ClearWantedLevelWhenClear// Clear wanted level when there are no cops remaining
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:ClearWantedLevelWhenClear");

                if (val is null)
                    ClearWantedLevelWhenClear = true;

                return GetResourceKvpInt("curiosity:dispatch:ClearWantedLevelWhenClear") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:ClearWantedLevelWhenClear", val);
            }
        }

        // BACK UP
        public bool BackupEnabled // Enables backup
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:BackupEnabled");

                if (val is null)
                    BackupEnabled = true;

                return GetResourceKvpInt("curiosity:dispatch:BackupEnabled") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:BackupEnabled", val);
            }
        }

        public bool BackupPauseWhenSearching // Pause backup progress when cops are searching for the player (If any are still left)
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:BackupPauseWhenSearching");

                if (val is null)
                    BackupPauseWhenSearching = true;

                return GetResourceKvpInt("curiosity:dispatch:BackupPauseWhenSearching") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:BackupPauseWhenSearching", val);
            }
        }

        public int BackupDuration // Backup duration in milliseconds
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:BackupDuration");

                if (val is null)
                    BackupDuration = 60000;

                return GetResourceKvpInt("curiosity:dispatch:BackupDuration");
            }
            set
            {
                SetResourceKvpInt("curiosity:dispatch:BackupDuration", value);
            }
        }

        public int BackupCooldown // Backup cooldown in milliseconds
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:BackupCooldown");

                if (val is null)
                    BackupCooldown = 10000;

                return GetResourceKvpInt("curiosity:dispatch:BackupCooldown");
            }
            set
            {
                SetResourceKvpInt("curiosity:dispatch:BackupCooldown", value);
            }
        }

        public float BackupFraction // Maximum cop number fraction before backup can be activated (2500 = 25%, 5000 = 50%)
        {
            get
            {
                float? val = GetResourceKvpFloat("curiosity:dispatch:BackupFraction");

                if (val is null)
                    BackupFraction = .25f;

                return GetResourceKvpFloat("curiosity:dispatch:BackupFraction");
            }
            set
            {
                SetResourceKvpFloat("curiosity:dispatch:BackupFraction", value);
            }
        }

        // DISPLAY
        public bool DisplayDispatchUI // Enables the status display
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:DisplayDispatchUI");

                if (val is null)
                    DisplayDispatchUI = true;

                return GetResourceKvpInt("curiosity:dispatch:DisplayDispatchUI") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:DisplayDispatchUI", val);
            }
        }

        private float _displayPosX = 0.0f; // X screen coordinate (From right side)
        private float _displayPosY = 0.0335f; // Y screen coordinate (From top side)
        private float _displayWidth = 0.0725f; // Width of status
        private float _displayHeight = 0.01f; // Height of status

        public bool DisplayNumberOfRemainingPolice // Displays cop number next to status
        {
            get
            {
                int? val = GetResourceKvpInt("curiosity:dispatch:DisplayNumberOfRemainingPolice");

                if (val is null)
                    DisplayNumberOfRemainingPolice = false;

                return GetResourceKvpInt("curiosity:dispatch:DisplayNumberOfRemainingPolice") == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                SetResourceKvpInt("curiosity:dispatch:DisplayNumberOfRemainingPolice", val);
            }
        }

        // SOUND
        private bool soundEnabled = true; // Enables sounds

        public override void Begin()
        {
            string init = GetResourceKvpString("curiosity:dispatch:setup");

            if (init != "SETUP")
            {
                ResetCopsWhenCleared = false;
                RemoveCopsWhenFarAway = true;
                RemoveCopsWhenFarAwayChase = true;
                ClearWantedLevelWhenClear = true;

                BackupEnabled = true;
                BackupPauseWhenSearching = true;
                BackupDuration = 60000;
                BackupCooldown = 10000;

                BackupFraction = .25f;

                DisplayDispatchUI = true;
                DisplayNumberOfRemainingPolice = false;

                SetResourceKvp("curiosity:dispatch:setup", "SETUP");
            }
            
            

            _colours.Add("Bg", new Colour(0, 0, 0));
            _colours.Add("BgNoCops", new Colour(50, 0, 0));
            _colours.Add("BarCops", new Colour(240));
            _colours.Add("BarCopsFilling", new Colour(131));
            _colours.Add("BarCopsBack", new Colour(64));
            _colours.Add("BarCopsBackNoCops", new Colour(90, 0, 0));
            _colours.Add("BarBackup", new Colour(216, 170, 43));
            _colours.Add("BarBackupClose", new Colour(216, 125, 62));
            _colours.Add("BarBackupFrozen", new Colour(216, 175, 186));
            _colours.Add("BarBackupBack", new Colour(127, 100, 25));
            _colours.Add("BarBackupBackClose", new Colour(127, 53, 38));
            _colours.Add("BarBackupBackFrozen", new Colour(127, 103, 114));

            currentCops = maxCops[4];
            ToggleDispatch(true);

            Logger.Info($"Dispatch Manager INIT - Original: Finite Cops by Silver Finish");
        }

        public void Init()
        {
            Instance.AttachTickHandler(OnDispatchManagerTick);
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnDispatchManagerTick);
        }

        private bool IsPedType(Ped ped, PedType[] types)
        {
            int pedType = GetPedType(ped.Handle);

            for (int i = 0; i < types.Length; i++)
            {
                if ((int)types[i] == pedType)
                    return true;
            }

            return false;
        }

        private void ToggleDispatch(bool toggle)
        {
            dispatch = toggle;
            for (int i = 0; i < dispatchTypes.Length; i++)
            {
                EnableDispatchService((int)dispatchTypes[i], toggle);
            }
        }

        private TrackedPed GetTrackedPed(string key)
        {
            TrackedPed trackedPed;
            return _trackedPeds.TryGetValue(key, out trackedPed) ? trackedPed : (TrackedPed)null;
        }

        private int GetMaxCops(int wanted) => wanted > 0 ? maxCops[wanted - 1] : maxCops[4];

        private void DrawRect(float x, float y, float width, float height, Colour col, int alpha = 255)
        {
            if (width <= 0.0 || height <= 0.0)
                return;

            API.DrawRect(x, y, width, height, col.r, col.g, col.b, alpha);
        }

        private async Task OnDispatchManagerTick()
        {
            if (PlayerOptionsManager.GetModule().IsPassive)
            {
                Instance.DetachTickHandler(OnDispatchManagerTick);
            }

            int playerHandle = Game.Player.Handle;

            int tickInitTime = GetGameTimer();
            int wantedLevel = GetPlayerWantedLevel(playerHandle);

            bool areStarsGreyedOut = ArePlayerStarsGreyedOut(playerHandle);
            bool displayUi = !IsPauseMenuActive();

            if (wantedLevel == 0 && !active) return; // ney point in running

            // If wanted, and not active, then flag active and get the minimum number of current cops for the wanted level
            if (wantedLevel > 0 && !active)
            {
                active = true;
                currentCops = Math.Min(GetMaxCops(wantedLevel), currentCops);
            }

            // If the players wanted level is removed, then deactivate
            if (wantedLevel <= 0 && active)
            {
                active = false;
                fillTick = -1;
                countdownTick = -1;
                countdown = -1;
                curFill = 0.0f;
                coolTick = 0;
                if (ResetCopsWhenCleared)
                {
                    currentCops = maxCops[4];
                    ToggleDispatch(true);
                    copTick = -1;
                }
            }

            if (((!DisplayDispatchUI ? 0 : (wantedLevel > 0 ? 1 : 0)) & (displayUi ? 1 : 0)) != 0)
            {
                float num3 = Math.Min((float)currentCops / (float)GetMaxCops(wantedLevel), 1f);
                float val2 = num3 - curFill;

                if (fillTick <= tickInitTime)
                {
                    fillTick = tickInitTime + 25;
                    curFill = Math.Min(curFill + ((double)val2 > 0.0 ? System.Math.Max(val2 / 60f, System.Math.Min(0.0075f, val2)) : System.Math.Min(val2 / 60f, System.Math.Max(-0.0075f, val2))), 1f);
                }

                float num4 = (float)((1.0 - (double)GetSafeZoneSize()) / 2.0);
                float displayWidth = _displayWidth;
                float width1 = displayWidth - 1f / 500f;
                float displayHeight = _displayHeight;
                float height = displayHeight - 0.0035f;
                float num5 = 1f - num4 - _displayPosX;
                float y = num4 + _displayPosY;
                float width2 = curFill * width1;
                float num6 = Math.Max(curFill, 0.0f) * width1;
                int num7 = Math.Min(addCopsAmount[wantedLevel - 1], GetMaxCops(wantedLevel) - currentCops);
                float num8 = width1 / (float)GetMaxCops(wantedLevel);
                float width3 = 0.001041f;
                bool flag3 = currentCops <= 0;
                float width4 = num3 * width1;
                float num9 = (double)width2 < 0.0 ? width2 : 0.0f;
                float width5 = (float)((1.0 - 0.0 / (double)BackupDuration) * ((double)Math.Min(width1 - width2, num8 * (float)num7) + (double)num9));
                float width6 = (float)((1.0 - (double)((copTick == -1 ? tickInitTime : copTick) - tickInitTime) / (double)BackupDuration) * ((double)Math.Min(width1 - width2, num8 * (float)num7) + (double)num9));
                bool flag4 = ((!BackupPauseWhenSearching ? 0 : (copTick != -1 ? 1 : 0)) & (areStarsGreyedOut ? 1 : 0)) != 0 && (_trackedPeds.Count > 0 || dispatch);

                if (DisplayNumberOfRemainingPolice)
                {
                    float width7 = ScreenInterface.TextWidth(currentCops.ToString(), 0.5f, 2);
                    DrawRect((float)((double)num5 - (double)displayWidth - 3.0 / 1000.0 - (double)width7 / 2.0), y, width7, 0.025f, flag3 ? _colours["BgNoCops"] : _colours["Bg"], 128);
                    ScreenInterface.DrawText(currentCops.ToString(), (float)((double)num5 - (double)displayWidth - 3.0 / 1000.0), y - 0.0172f, 0.5f, font: 2);
                }

                DrawRect(num5 - displayWidth / 2f, y, displayWidth, displayHeight, flag3 ? _colours["BgNoCops"] : _colours["Bg"]);
                DrawRect(num5 - displayWidth / 2f, y, width1, height, flag3 ? _colours["BarCopsBackNoCops"] : _colours["BarCopsBack"]);

                if ((double)num3 > (double)curFill)
                    DrawRect((float)((double)num5 - (double)width4 / 2.0 - 1.0 / 1000.0), y, width4, height, _colours["BarCopsFilling"]);

                DrawRect((float)((double)num5 - (double)width2 / 2.0 - 1.0 / 1000.0), y, width2, height, _colours["BarCops"]);

                if (copTick != -1)
                {
                    DrawRect((float)((double)num5 - (double)num6 - 1.0 / 1000.0 - (double)width5 / 2.0), y, width5, height, flag4 ? _colours["BarBackupBackFrozen"] : (countdown != -1 ? _colours["BarBackupBackClose"] : _colours["BarBackupBack"]));
                    DrawRect((float)((double)num5 - (double)num6 - 1.0 / 1000.0 - (double)width6 / 2.0), y, width6, height, flag4 ? _colours["BarBackupFrozen"] : (countdown != -1 ? _colours["BarBackupClose"] : _colours["BarBackup"]));
                    if ((double)width5 > 0.0)
                        DrawRect((float)((double)num5 - (double)num6 - 0.00150000001303852) - width5, y, width3, height, flag3 ? _colours["BgNoCops"] : _colours["Bg"]);
                    if ((double)width6 > 0.0)
                        DrawRect((float)((double)num5 - (double)num6 - 0.00150000001303852) - width6, y, width3, height, flag3 ? _colours["BgNoCops"] : _colours["Bg"]);
                }

                if ((double)num3 > (double)curFill)
                    DrawRect((float)((double)num5 - (double)width4 - 0.00150000001303852), y, width3, height, flag3 ? _colours["BgNoCops"] : _colours["Bg"]);

                if ((double)width2 > 0.0 && (double)width2 < 1.0)
                    DrawRect((float)((double)num5 - (double)num6 - 0.00150000001303852), y, width3, height, flag3 ? _colours["BgNoCops"] : _colours["Bg"]);
            }

            if (ClearWantedLevelWhenClear && currentCops <= 0 && _trackedPeds.Count <= 0 && wantedLevel > 0 && (DecreaseLevel == 0 || !decreased))
            {
                if ((uint)DecreaseLevel > 0U)
                {
                    decreased = true;
                    SetPlayerWantedLevel(playerHandle, (wantedLevel - DecreaseLevel), false);
                }
                else
                    ClearPlayerWantedLevel(playerHandle);
            }

            if (currentCops > 0 && decreased)
                decreased = false;

            if (pedTick <= tickInitTime)
            {
                pedTick = tickInitTime + 250;
                Ped[] allPeds = World.GetAllPeds();
                Vector3 position1 = Game.PlayerPed.Position;

                for (int index = 0; index < allPeds.Length; ++index)
                {
                    int handle = ((Entity)allPeds[index]).Handle;
                    if (GetTrackedPed(handle.ToString()) == null && !((Entity)allPeds[index]).IsDead && IsPedType(allPeds[index], copTypes))
                    {
                        Dictionary<string, TrackedPed> trackedPeds = _trackedPeds;
                        handle = ((Entity)allPeds[index]).Handle;
                        string key = handle.ToString();
                        TrackedPed trackedPed = new TrackedPed(allPeds[index]);
                        trackedPeds[key] = trackedPed;
                    }
                }

                bool flag5;

                do
                {
                    flag5 = true;
                    foreach (KeyValuePair<string, TrackedPed> trackedPed1 in _trackedPeds)
                    {
                        TrackedPed trackedPed2 = trackedPed1.Value;
                        Ped ped = trackedPed2.ped;
                        int num10;
                        if (!trackedPed2.near)
                        {
                            Vector3 position2 = ((Entity)ped).Position;
                            num10 = position2.Distance(position1) < 100.0 ? 1 : 0;
                        }
                        else
                            num10 = 0;
                        if (num10 != 0)
                            trackedPed2.near = true;
                        int num11;
                        if (((Entity)ped).Exists())
                        {
                            Vector3 position3 = ((Entity)ped).Position;
                            num11 = position3.Distance(position1) > 750.0 ? 1 : 0;
                        }
                        else
                            num11 = 1;
                        bool flag6 = num11 != 0;
                        bool flag7 = !flag6 && ((Entity)ped).IsDead;
                        if (flag6 | flag7)
                        {
                            if (active && (flag7 || RemoveCopsWhenFarAway && (!RemoveCopsWhenFarAwayChase || !areStarsGreyedOut) && flag6 && trackedPed2.near))
                            {
                                currentCops = Math.Max(currentCops - 1, copsMinimum);
                                if (dispatch && currentCops <= 0)
                                    ToggleDispatch(false);
                            }
                            flag5 = false;
                            _trackedPeds.Remove(trackedPed1.Key);
                            break;
                        }
                    }
                }
                while (!flag5);
            }

            if (!BackupEnabled && (coolTick != -1 || copTick != -1))
            {
                coolTick = -1;
                copTick = -1;
            }

            if (countdown == -1 && BackupDuration >= 4000 && copTick != -1 && copTick - tickInitTime <= 4000)
            {
                countdown = 4;
                countdownTick = -1;
            }

            if (countdown > -1 && countdownTick <= tickInitTime)
            {
                countdownTick = tickInitTime + 1000;
                --countdown;

                if (countdown == -1)
                    countdownTick = -1;
                else if (displayUi && active && soundEnabled && copTickStore == -1 && currentCops + addCopsAmount[wantedLevel - 1] > 0)
                    PlaySoundFrontend(-1, "HORDE_COOL_DOWN_TIMER", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }

            if (coolTick != -1 && coolTick <= tickInitTime && currentCops < GetMaxCops(wantedLevel) && (double)currentCops / (double)GetMaxCops(wantedLevel) <= (double)BackupFraction)
            {
                copTick = tickInitTime + BackupDuration;

                if (displayUi && active && soundEnabled && wantedLevel > 0 && currentCops + addCopsAmount[wantedLevel - 1] > 0)
                    PlaySoundFrontend(-1, "Start_Squelch", "CB_RADIO_SFX", false);

                coolTick = -1;
            }

            if (((!BackupPauseWhenSearching ? 0 : (copTick != -1 ? 1 : 0)) & (areStarsGreyedOut ? 1 : 0)) != 0 && (_trackedPeds.Count > 0 || dispatch))
            {
                if (copTickStore == -1)
                    copTickStore = copTick - tickInitTime;
                copTick = tickInitTime + copTickStore;
            }

            int num12;
            if (copTickStore != -1)
                num12 = !ArePlayerStarsGreyedOut(playerHandle) ? 1 : 0;
            else
                num12 = 0;

            if (num12 != 0)
                copTickStore = -1;

            if (copTick == -1 || copTick > tickInitTime)
                return;

            coolTick = tickInitTime + BackupCooldown;
            copTick = -1;

            if (currentCops < GetMaxCops(wantedLevel))
            {
                countdown = -1;
                currentCops = Math.Min(currentCops + addCopsAmount[wantedLevel - 1], GetMaxCops(wantedLevel));

                if (soundEnabled && currentCops + addCopsAmount[wantedLevel - 1] > 0)
                    PlaySoundFrontend(-1, "Out_Of_Bounds_Timer", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", false);

                if (!dispatch && currentCops > 0)
                    ToggleDispatch(true);
            }

        }
    }
}
