using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Police.Client;
using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Managers;
using Curiosity.Police.Client.Utils;
using Curiosity.Systems.Library.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.Casino.Games
{
    public class CasinoInsideTrackManager : Manager<CasinoInsideTrackManager>
    {
        const string SCALEFORM_RACING_WALL = "HORSE_RACING_WALL"; // func_6039
        const string SCALEFORM_RACING_CONSOLE = "HORSE_RACING_CONSOLE"; // func_6031
        const string TEXTURE_DICTIONARY_INSIDE_TRACK = "Prop_Screen_VW_InsideTrack"; // func_5975
        const string RENDER_TARGET_CASINO_SCREEN_2 = "casinoscreen_02"; // func_5961
        int RENDER_TARGET_LINK_HASH = -214651601; // func_6038 (this is the hash prop of the wall) // https://forge.plebmasters.de/objects/vw_vwint01_betting_screen

        const int CASINO_INTERIOR = 275201;

        private CasinoDataObject casinoDataObject = new CasinoDataObject();

        private bool isFullscreen = false;
        private bool initSetup = false;
        private bool blockClosingOfFullScreen = false;

        public override void Begin()
        {

        }

        // [TickHandler]
        private async Task IsPlayerInCasino()
        {
            int interior = GetInteriorFromEntity(Game.PlayerPed.Handle);
            if (interior == CASINO_INTERIOR)
                Init();

            if (interior != CASINO_INTERIOR)
                Dispose();
        }

        public void Init()
        {
            Instance.AttachTickHandler(OnSetupRenderTargets);
            Instance.AttachTickHandler(OnWallAndConsoleScaleform);
            Instance.AttachTickHandler(OnUpdateMousePosition);
            Instance.AttachTickHandler(OnControls);
        }

        public void Dispose()
        {
            ReleaseNamedRendertarget(RENDER_TARGET_CASINO_SCREEN_2);
            Instance.DetachTickHandler(OnSetupRenderTargets);
            Instance.DetachTickHandler(OnWallAndConsoleScaleform);
            Instance.DetachTickHandler(OnUpdateMousePosition);
            Instance.DetachTickHandler(OnControls);
        }

        private async Task OnControls()
        {
            if (Game.IsControlJustPressed(2, Control.CursorAccept) && isFullscreen)
            {
                int mouseValue = await GetMouseClickEvent();

                if (casinoDataObject.ConsoleScreenDisplay == 0 && mouseValue == 15) // Show Rules Button
                    SetScreenToBeDisplayed(casinoDataObject, 9, isFullscreen);

                if (casinoDataObject.ConsoleScreenDisplay == 0 && mouseValue == 1) // Single Event Button (Horse Selection)
                    SetScreenToBeDisplayed(casinoDataObject, 1, isFullscreen);

                if ((casinoDataObject.ConsoleScreenDisplay == 9
                     || casinoDataObject.ConsoleScreenDisplay == 1
                     || casinoDataObject.ConsoleScreenDisplay == 3) && mouseValue == 12) // Cancel Button
                    ShowMainScreen();

                if (casinoDataObject.ConsoleScreenDisplay == 7 && mouseValue == 13)
                    ShowMainScreen();

                if (casinoDataObject.ConsoleScreenDisplay == 1)
                {
                    if (mouseValue >= 2 && mouseValue <= 7)
                    {
                        casinoDataObject.SelectedHorse = (mouseValue - 1);
                        ShowBettingScreen();
                    }
                }

                if (casinoDataObject.ConsoleScreenDisplay == 3) // betting screen
                {
                    if (mouseValue >= 2 && mouseValue <= 7)
                    {
                        casinoDataObject.SelectedHorse = (mouseValue - 1);
                        ShowBettingScreen();
                    }

                    if (mouseValue == 8) // increase bet
                    {
                        casinoDataObject.IncreaseBet();
                        SetBettingValues();
                    }

                    if (mouseValue == 9) // decrease bet
                    {
                        casinoDataObject.DecreaseBet();
                        SetBettingValues();
                    }

                    if (mouseValue == 10) // start the race
                    {
                        StartTheRace();
                    }

                }

                Logger.Info($"Mouse Click: {mouseValue} - Screen: {casinoDataObject.ConsoleScreenDisplay}");
            }
        }

        async void StartTheRace()
        {
            blockClosingOfFullScreen = true;

            bool checkRaceStatus = true;
            casinoDataObject.GenerateHorseOrder();

            casinoDataObject.currentWinner = casinoDataObject.HorsePositions[0];

            if (isFullscreen)
            {
                BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "START_RACE");
            }
            else
            {
                BeginScaleformMovieMethod(casinoDataObject.WallScaleformHandle, "START_RACE");
            }
            
            ScaleformMovieMethodAddParamFloat(15000.0f);
            ScaleformMovieMethodAddParamInt(4);

            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[0]);
            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[1]);
            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[2]);
            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[3]);
            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[4]);
            ScaleformMovieMethodAddParamInt(casinoDataObject.HorsePositions[5]);

            ScaleformMovieMethodAddParamFloat(0.0f);
            ScaleformMovieMethodAddParamBool(false);
            EndScaleformMovieMethod();

            SetScreenToBeDisplayed(casinoDataObject, 5, isFullscreen); // change to the race screen;

            int timer = 0;
            int maxTime = 300;

            // need to check for finish and if the photo finish needs to be displayed
            while (checkRaceStatus)
            {
                await BaseScript.Delay(0);
                bool isRaceFinished = await IsRaceComplete();

                if (timer >= maxTime)
                {
                    isRaceFinished = true;
                }

                if (timer < maxTime)
                    timer++;

                if (isRaceFinished)
                {
                    if (casinoDataObject.SelectedHorse == casinoDataObject.currentWinner)
                    {
                        casinoDataObject.knownBalance += casinoDataObject.currentGain;
                    }

                    if (casinoDataObject.SelectedHorse != casinoDataObject.currentWinner)
                    {
                        Logger.Info($"LOOSER!");
                    }

                    SetScreenToBeDisplayed(casinoDataObject, 7, isFullscreen); // results

                    casinoDataObject.HorsePositions.Clear();
                    casinoDataObject.SelectedHorse = -1;
                    casinoDataObject.currentWinner = -1;
                    checkRaceStatus = false;
                    blockClosingOfFullScreen = false;

                    SetupHorses(casinoDataObject);
                    casinoDataObject.ResetBet();
                }
            }
        }

        async Task<bool> IsRaceComplete()
        {
            //if (isFullscreen)
            //{
            //    BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "GET_RACE_IS_COMPLETE");
            //}
            //else
            //{
            //    BeginScaleformMovieMethod(casinoDataObject.WallScaleformHandle, "GET_RACE_IS_COMPLETE");
            //}
            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "GET_RACE_IS_COMPLETE");
            int raceReturnValue = EndScaleformMovieMethodReturnValue();

            while (!IsScaleformMovieMethodReturnValueReady(raceReturnValue))
            {
                await BaseScript.Delay(0);
            }

            return GetScaleformMovieMethodReturnValueBool(raceReturnValue);
        }

        void ShowBettingScreen()
        {
            SetBettingValues(); // set these values based on character data (bet value, character chips, gain from bet) 

            SetScreenToBeDisplayed(casinoDataObject, 3, isFullscreen);

            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SET_BETTING_ENABLED");
            ScaleformMovieMethodAddParamBool(true);
            EndScaleformMovieMethod();
        }

        void SetBettingValues()
        {
            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SET_BETTING_VALUES");
            ScaleformMovieMethodAddParamInt(casinoDataObject.SelectedHorse);
            // these must touch the server to check they are allowed
            ScaleformMovieMethodAddParamInt(casinoDataObject.currentBet);
            ScaleformMovieMethodAddParamInt(casinoDataObject.knownBalance); // Get this from player object
            ScaleformMovieMethodAddParamInt(casinoDataObject.currentGain);
            //
            EndScaleformMovieMethod();
        }

        string RandomHorseName(int randomNumber)
        {
            return (randomNumber < 10) ? $"ITH_NAME_00{randomNumber}" : $"ITH_NAME_0{randomNumber}";
        }

        void SetupHorses(CasinoDataObject casino)
        {
            for(int i = 1; i < 7; i++)
            {
                int randomNumber = Utility.RANDOM.Next(100);
                string name = RandomHorseName(randomNumber);
                if (isFullscreen)
                {
                    BeginScaleformMovieMethod(casino.ConsoleScaleformHandle, "SET_HORSE");
                }
                else
                {
                    BeginScaleformMovieMethod(casino.WallScaleformHandle, "SET_HORSE");
                }
                
                ScaleformMovieMethodAddParamInt(i);

                BeginTextCommandScaleformString(name);
                EndTextCommandScaleformString();

                ScaleformMovieMethodAddParamPlayerNameString($"Cool Horse {i}");

                Tuple<int, int, int, int> style = casino.HorseStyles[randomNumber];

                ScaleformMovieMethodAddParamInt(style.Item1);
                ScaleformMovieMethodAddParamInt(style.Item2);
                ScaleformMovieMethodAddParamInt(style.Item3);
                ScaleformMovieMethodAddParamInt(style.Item4);
                EndScaleformMovieMethod();
            }

            casinoDataObject.GenerateHorseOrder();
        }

        private async Task OnUpdateMousePosition()
        {
            if (isFullscreen)
            {
                float xMouse = GetDisabledControlNormal(2, (int)Control.CursorX);
                float yMouse = GetDisabledControlNormal(2, (int)Control.CursorY);

                BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SET_MOUSE_INPUT");
                ScaleformMovieMethodAddParamFloat(xMouse);
                ScaleformMovieMethodAddParamFloat(yMouse);
                EndScaleformMovieMethod();

                SetPauseMenuActive(false); // disable pause screen if inside the scaleform
            }
        }

        private async Task OnWallAndConsoleScaleform()
        {
            if (!blockClosingOfFullScreen)
            Screen.DisplayHelpTextThisFrame($"~INPUT_CONTEXT~ FullScreen");

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                if (!blockClosingOfFullScreen)
                {
                    isFullscreen = !isFullscreen;
                    await BaseScript.Delay(100);
                    MainEventInProgress(true);
                }
            }

            OnScaleFormProgress(null, null, casinoDataObject, isFullscreen);
        }

        private async Task OnSetupRenderTargets()
        {
            int attempts = 0;
            RequestStreamedTextureDict(TEXTURE_DICTIONARY_INSIDE_TRACK, false);
            while (!HasStreamedTextureDictLoaded(TEXTURE_DICTIONARY_INSIDE_TRACK))
            {
                await BaseScript.Delay(100);
                RequestStreamedTextureDict(TEXTURE_DICTIONARY_INSIDE_TRACK, false);
                if (attempts > 10) break;
                attempts++;
            }

            for (int screen = 0; screen < 16; screen++)
            {
                if (casinoDataObject.BettingComputerPosition[screen] == 0)
                {
                    string casinoScreen = GetCasinoScreen(screen);
                    uint casinoScreenHash = (uint)GetCasinoScreenHash(screen);

                    RequestStreamedTextureDict(TEXTURE_DICTIONARY_INSIDE_TRACK, false);
                    if (HasStreamedTextureDictLoaded(TEXTURE_DICTIONARY_INSIDE_TRACK))
                    {
                        if (!IsNamedRendertargetRegistered(casinoScreen))
                        {
                            RegisterNamedRendertarget(casinoScreen, false);
                        }
                    }
                    if (IsNamedRendertargetLinked(casinoScreenHash))
                    {
                        LinkNamedRendertarget(casinoScreenHash);
                    }
                    if (IsNamedRendertargetRegistered(casinoScreen) && IsNamedRendertargetLinked(casinoScreenHash))
                    {
                        if (casinoDataObject.BettingComputerRenderId[screen] == -1)
                        {
                            casinoDataObject.BettingComputerRenderId[screen] = GetNamedRendertargetRenderId(casinoScreen);
                        }
                    }
                    if (casinoDataObject.BettingComputerRenderId[screen] != -1)
                    {
                        casinoDataObject.BettingComputerPosition[screen] = 3;
                    }
                }
                else if (casinoDataObject.BettingComputerPosition[screen] == 3 && casinoDataObject.BettingComputerScreenDisplay[screen] != -1)
                {
                    SetTextRenderId(casinoDataObject.BettingComputerRenderId[screen]);
                    SetScriptGfxAlign(73, 73);
                    SetScriptGfxDrawOrder(4);
                    SetScriptGfxDrawBehindPausemenu(true);
                    DrawInteractiveSprite(TEXTURE_DICTIONARY_INSIDE_TRACK, GetScreenToShow(casinoDataObject.BettingComputerScreenDisplay[screen]), 0.25f, 0.5f, 0.5f, 1.0f, 0.0f, 255, 255, 255, 255);
                    ResetScriptGfxAlign();
                    SetTextRenderId(GetDefaultScriptRendertargetRenderId());
                }
            }
        }

        string GetCasinoScreen(int screen) // func_5953
        {
            switch (screen)
            {
                case 0:
                    return "casinoscreen_03";
                case 1:
                    return "casinoscreen_04";
                case 2:
                    return "casinoscreen_05";
                case 3:
                    return "casinoscreen_06";
                case 4:
                    return "casinoscreen_07";
                case 5:
                    return "casinoscreen_08";
                case 6:
                    return "casinoscreen_09";
                case 7:
                    return "casinoscreen_10";
                case 8:
                    return "casinoscreen_11";
                case 9:
                    return "casinoscreen_12";
                case 10:
                    return "casinoscreen_13";
                case 11:
                    return "casinoscreen_14";
                case 12:
                    return "casinoscreen_15";
                case 13:
                    return "casinoscreen_16";
                case 14:
                    return "casinoscreen_17";
                case 15:
                    return "casinoscreen_18";
            }
            if (screen < 10)
            {
                return $"casinoscreen_0{screen}";
            }
            return $"casinoscreen_{screen}";
        }

        int GetCasinoScreenHash(int screen) // func_5974
        {
            switch (screen)
            {
                case 0:
                    return 903186242;
                case 1:
                    return 1144202237;
                case 2:
                    return -519873125;
                case 3:
                    return -292161344;
                case 4:
                    return 2004912791;
                case 5:
                    return -1960136213;
                case 6:
                    return -1003510800;
                case 7:
                    return 425283146;
                case 8:
                    return -563586975;
                case 9:
                    return 1712925937;
                case 10:
                    return 796540856;
                case 11:
                    return 499686485;
                case 12:
                    return 1207038119;
                case 13:
                    return 977491274;
                case 14:
                    return -424104402;
                case 15:
                    return -731707005;
            }
            string modelHash = $"vw_vwint01_betting_sreen_{screen}";
            if (screen < 10)
                modelHash = $"vw_vwint01_betting_sreen_0{screen}";
            return GetHashKey(modelHash);
        }

        string GetScreenToShow(int screenToDisplay) // func_5973
        {
            switch (screenToDisplay)
            {
                case 0:
                    return "BETTING";
                case 1:
                    return "BETTING_GENERIC_ORANGE";
                case 2:
                    return "BETTING_GENERIC_ORANGE_unavailable";
                case 3:
                    return "BETTING_GENERIC_PURPLE";
                case 4:
                    return "BETTING_GENERIC_PURPLE_unavailable";
                case 5:
                    return "BETTING_SINGLE";
                case 6:
                    return "BETTING_MAIN";
            }
            return "NULL";
        }

        private async void OnScaleFormProgress(dynamic uParam0, dynamic uParam1, CasinoDataObject casinoDataObject, bool isFullscreen)
        {
            int progress = 4;

            if (!isFullscreen)
            {
                progress = casinoDataObject.WallScreenPosition;
            }
            else
            {
                progress = casinoDataObject.ConsoleScreenPosition;
            }

            switch (progress)
            {
                case 0:
                    if (!isFullscreen)
                    {
                        casinoDataObject.WallScaleformHandle = RequestScaleformMovie_2(SCALEFORM_RACING_WALL);
                        if (HasScaleformMovieLoaded(casinoDataObject.WallScaleformHandle))
                        {
                            SetupRenderTarget(casinoDataObject);
                            if (casinoDataObject.WallRenderHandle != -1)
                            {
                                SetScaleformFitRendertarget(casinoDataObject.WallScaleformHandle, true);
                            }
                            SetScreenPosition(casinoDataObject, 1, isFullscreen);
                            SetScreenToBeDisplayed(casinoDataObject, 5, isFullscreen);
                        }
                    }
                    else
                    {
                        casinoDataObject.ConsoleScaleformHandle = RequestScaleformMovie_2(SCALEFORM_RACING_CONSOLE);
                        if (HasScaleformMovieLoaded(casinoDataObject.WallScaleformHandle))
                        {

                        }
                        SetScreenPosition(casinoDataObject, 1, isFullscreen);
                        SetScreenToBeDisplayed(casinoDataObject, 0, isFullscreen);
                    }
                    await BaseScript.Delay(100);
                    MainEventInProgress(true);
                    SetCooldown(casinoDataObject, 60);
                    SetupHorses(casinoDataObject);
                    break;
                case 1:
                    // Need to clear players when a race is finished
                    //if (isFullscreen)
                    //{
                    //    CallScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "CLEAR_ALL_PLAYERS");
                    //}
                    //else
                    //{
                    //    CallScaleformMovieMethod(casinoDataObject.WallScaleformHandle, "CLEAR_ALL_PLAYERS");
                    //}

                    DrawScaleForms(casinoDataObject, isFullscreen);
                    ScaleformShowScreen(casinoDataObject, 0, isFullscreen);

                    break;
            }
        }

        void SetScreenPosition(CasinoDataObject casinoWall, int screenToShow, bool isFullscreen)
        {
            if (!isFullscreen)
            {
                casinoWall.WallScreenPosition = screenToShow;
            }
            else
            {
                casinoWall.ConsoleScreenPosition = screenToShow;
            }
        }

        void SetScreenToBeDisplayed(CasinoDataObject casinoWall, int screenToShow, bool isFullscreen)
        {
            if (!isFullscreen)
            {
                casinoWall.WallScreenDisplay = screenToShow;
            }
            else
            {
                casinoWall.ConsoleScreenDisplay = screenToShow;
            }
        }

        void SetupRenderTarget(CasinoDataObject casinoWall)
        {
            if (Common.IsNamedAndLinkedTargetRegistered(RENDER_TARGET_CASINO_SCREEN_2, (uint)RENDER_TARGET_LINK_HASH))
            {
                if (casinoWall.WallRenderHandle == -1)
                {
                    casinoWall.WallRenderHandle = Common.GetNamedRenderTargetRenderId(RENDER_TARGET_CASINO_SCREEN_2);
                }
            }
        }

        void DrawScaleForms(CasinoDataObject casinoWall, bool fullscreen)
        {
            SetScriptGfxDrawBehindPausemenu(true);
            if (fullscreen)
            {
                HideHudAndRadarThisFrame();
                HideScriptedHudComponentThisFrame(19);

                ThefeedHideThisFrame();
                SetScriptGfxDrawOrder(1);
                DrawScaleformMovieFullscreen(casinoWall.ConsoleScaleformHandle, 255, 255, 255, 255, 0);
            }
            else
            {
                SetTextRenderId(casinoWall.WallRenderHandle);
                SetScriptGfxDrawOrder(4);
                DrawScaleformMovie(casinoWall.WallScaleformHandle, 0.5f, 0.5f, 0.999f, 0.999f, 255, 255, 255, 255, 0);
            }
            SetTextRenderId(GetDefaultScriptRendertargetRenderId());
        }

        void SetCooldown(CasinoDataObject casinoData, int duration)
        {
            if (isFullscreen)
            {
                BeginScaleformMovieMethod(casinoData.ConsoleScaleformHandle, "SET_COUNTDOWN");
            }
            else
            {
                BeginScaleformMovieMethod(casinoData.WallScaleformHandle, "SET_COUNTDOWN");
            }
            
            ScaleformMovieMethodAddParamInt(duration);
            EndScaleformMovieMethod();
        }

        void ShowMainScreen()
        {
            SetScreenToBeDisplayed(casinoDataObject, 0, isFullscreen);
            MainEventInProgress(true);

            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "CLEAR_ALL");
            EndScaleformMovieMethod();
        }

        void MainEventInProgress(bool isInProgress = false)
        {
            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SET_MAIN_EVENT_IN_PROGRESS");
            ScaleformMovieMethodAddParamBool(isInProgress);
            EndScaleformMovieMethod();
        }

        void ShowNotAvaliableError()
        {
            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SHOW_ERROR");

            BeginTextCommandScaleformString("IT_ERROR_TITLE");
            EndTextCommandScaleformString();

            BeginTextCommandScaleformString("IT_ERROR_MSG");
            EndTextCommandScaleformString();

            EndScaleformMovieMethod();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cdo"></param>
        /// <param name="screenToDisplay"></param>
        /// <param name="isConsole"></param>
        void ScaleformShowScreen(CasinoDataObject cdo, int screenToDisplay, bool isConsole) // func_6021
        {
            int screenToShow = 0;
            if (isConsole)
            {
                // 0 - EVENT SELECT - Select to play with yourself or others
                // 1 - SINGLE EVENT - Select Horse
                // 2 - #MULTI EVENT - Select Horse
                // 3 - SINGLE EVENT - Place Bet
                // 4 - #MULTI EVENT - Place Bet
                // 5 - Race
                // 6 - Photo Finish
                // 7 - Results
                // 8 - EVENT SELECT - No Buttons?
                // 9 - Rules

                screenToShow = cdo.ConsoleScreenDisplay;
                BeginScaleformMovieMethod(cdo.ConsoleScaleformHandle, "SHOW_SCREEN");
            }
            else
            {
                // 0 - Jockies
                // 1 - Horse and Jockies (rotation)
                // 2 - Race
                // 3 - Race
                // 4 - Results
                // 5 - Wall Display

                screenToShow = cdo.WallScreenDisplay;
                BeginScaleformMovieMethod(cdo.WallScaleformHandle, "SHOW_SCREEN");
            }
            ScaleformMovieMethodAddParamInt(screenToShow);
            EndScaleformMovieMethod();
        }

        async Task<int> GetMouseClickEvent()
        {
            int returnValue = -1;

            CallScaleformMovieMethodWithNumber(casinoDataObject.ConsoleScaleformHandle, "SET_INPUT_EVENT", 237.0f, -1082130432, -1082130432, -1082130432, -1082130432);
            BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "GET_CURRENT_SELECTION");
            returnValue = EndScaleformMovieMethodReturnValue();

            while(!IsScaleformMovieMethodReturnValueReady(returnValue))
            {
                await BaseScript.Delay(0);
            }

            return GetScaleformMovieFunctionReturnInt(returnValue);
        }
    }

    internal class CasinoDataObject
    {
        // Player things
        public int SelectedHorse = -1;
        public int currentBet { get; private set; } = 100;
        public int currentGain { get; private set; } = 200;
        public int knownBalance = 5000;
        public int currentWinner = 0;

        public void IncreaseBet()
        {
            if (currentBet < knownBalance)
                currentBet = currentBet + 100;

            currentGain = currentBet * 2;
        }

        public void DecreaseBet()
        {
            if (currentBet > 100)
                currentBet = currentBet - 100;
            
            currentGain = currentBet * 2;
        }

        // Scaleform management
        public int WallRenderHandle = -1;// uParam0->f_280
        public int WallScaleformHandle = -1; // uParam0->f_298

        public int ConsoleScaleformHandle = -1;

        public int WallScreenPosition = 0; // uParam0->f_191
        public int WallScreenDisplay = 0; // uParam0->f_192

        public int ConsoleScreenPosition = 0; // uParam0->f_193
        public int ConsoleScreenDisplay = 0; // uParam0->f_194

        // DATA
        [JsonIgnore]
        public List<int> HorsePositions = new();

        bool IsPositionAvaliable(int position)
        {
            for (int i = 0; i < HorsePositions.Count; i++)
            {
                if (HorsePositions[i] == position)
                {
                    return false;
                }
            }
            return true;
        }

        public async void GenerateHorseOrder()
        {
            while(HorsePositions.Count < 6)
            {
                await BaseScript.Delay(0);
                for (int i = 0; i < 6; i++)
                {
                    int rnd = Utility.RANDOM.Next(1, 7);
                    if (IsPositionAvaliable(rnd))
                        HorsePositions.Add(rnd);
                }
            }
        }

        [JsonIgnore]
        public int[] BettingComputerRenderId = new int[16]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };

        [JsonIgnore]
        public int[] BettingComputerPosition = new int[16]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        [JsonIgnore]
        public int[] BettingComputerScreenDisplay = new int[16]
        {
            6, 6, 6, 6, 6, 5, 4, 3, 3, 1, 5, 4, 3, 2, 1, 0
        };

        [JsonIgnore]
        public List<Tuple<int, int, int, int>> HorseStyles = new List<Tuple<int, int, int, int>>()
        {
            new Tuple<int, int, int, int>(15553363,5474797,9858144,4671302),
            new Tuple<int, int, int, int>(16724530,3684408,14807026,16777215),
            new Tuple<int, int, int, int>(13560920,15582764,16770746,7500402),
            new Tuple<int, int, int, int>(16558591,5090807,10446437,7493977),
            new Tuple<int, int, int, int>(5090807,16558591,3815994,9393493),
            new Tuple<int, int, int, int>(16269415,16767010,10329501,16777215),
            new Tuple<int, int, int, int>(2263807,16777215,9086907,3815994),
            new Tuple<int, int, int, int>(4879871,16715535,3815994,16777215),
            new Tuple<int, int, int, int>(16777215,2263807,16769737,15197642),
            new Tuple<int, int, int, int>(16338779,16777215,11166563,6974058),
            new Tuple<int, int, int, int>(16777215,16559849,5716493,3815994),
            new Tuple<int, int, int, int>(16760644,3387257,16701597,16777215),
            new Tuple<int, int, int, int>(6538729,2249420,16777215,3815994),
            new Tuple<int, int, int, int>(15913534,15913534,16304787,15985375),
            new Tuple<int, int, int, int>(15655629,16240452,16760474,13664854),
            new Tuple<int, int, int, int>(16320263,16777215,14920312,16773316),
            new Tuple<int, int, int, int>(7176404,15138618,6308658,13664854),
            new Tuple<int, int, int, int>(4879871,8453903,11382189,15724527),
            new Tuple<int, int, int, int>(16777215,16777215,16754809,16777215),
            new Tuple<int, int, int, int>(16732497,16732497,3815994,16777215),
            new Tuple<int, int, int, int>(5739220,5739220,11382189,15724527),
            new Tuple<int, int, int, int>(16712909,6935639,8742735,3877137),
            new Tuple<int, int, int, int>(2136867,16777215,16761488,3877137),
            new Tuple<int, int, int, int>(3118422,10019244,14932209,6121086),
            new Tuple<int, int, int, int>(2136867,10241979,8081664,3815994),
            new Tuple<int, int, int, int>(16769271,13724403,9852728,14138263),
            new Tuple<int, int, int, int>(13724403,16769271,6444881,14138263),
            new Tuple<int, int, int, int>(10017279,4291288,16304787,15985375),
            new Tuple<int, int, int, int>(1071491,4315247,14935011,6121086),
            new Tuple<int, int, int, int>(3861944,16627705,14932209,6121086),
            new Tuple<int, int, int, int>(15583546,4671303,11836798,3090459),
            new Tuple<int, int, int, int>(15567418,4671303,9985296,3815994),
            new Tuple<int, int, int, int>(5701417,16711680,16771760,6970713),
            new Tuple<int, int, int, int>(16760303,5986951,12353664,15395562),
            new Tuple<int, int, int, int>(8907670,2709022,9475214,4278081),
            new Tuple<int, int, int, int>(5429688,6400829,16777215,16773316),
            new Tuple<int, int, int, int>(15138618,5272210,14920312,16773316),
            new Tuple<int, int, int, int>(10241979,12396337,14920312,15395562),
            new Tuple<int, int, int, int>(16777215,13481261,13667152,3815994),
            new Tuple<int, int, int, int>(5077874,16777215,15444592,7820105),
            new Tuple<int, int, int, int>(10408040,2960685,7424036,10129549),
            new Tuple<int, int, int, int>(7754308,16777215,12944259,3815994),
            new Tuple<int, int, int, int>(16736955,16106560,16771760,6970713),
            new Tuple<int, int, int, int>(16106560,16770224,16767659,15843765),
            new Tuple<int, int, int, int>(9573241,14703194,9789279,3815994),
            new Tuple<int, int, int, int>(44799,14703194,10968156,16777215),
            new Tuple<int, int, int, int>(7143224,16753956,10975076,4210752),
            new Tuple<int, int, int, int>(7895160,4013373,5855577,11645361),
            new Tuple<int, int, int, int>(16075595,6869196,13530742,7105644),
            new Tuple<int, int, int, int>(16090955,6272992,16777215,16777215),
            new Tuple<int, int, int, int>(13313356,13313356,5849409,11623516),
            new Tuple<int, int, int, int>(13911070,5583427,14935011,6121086),
            new Tuple<int, int, int, int>(8604661,10408040,12944259,3815994),
            new Tuple<int, int, int, int>(9716612,2960685,16767659,6708313),
            new Tuple<int, int, int, int>(7806040,16777215,16765601,14144436),
            new Tuple<int, int, int, int>(15632075,11221989,16777215,16770037),
            new Tuple<int, int, int, int>(1936722,14654697,16763851,3815994),
            new Tuple<int, int, int, int>(10377543,3815994,14807026,16777215),
            new Tuple<int, int, int, int>(16775067,11067903,16770746,7500402),
            new Tuple<int, int, int, int>(16741712,8669718,16777215,16777215),
            new Tuple<int, int, int, int>(16515280,6318459,3815994,9393493),
            new Tuple<int, int, int, int>(65526,16515280,10329501,16777215),
            new Tuple<int, int, int, int>(16711680,4783925,3815994,3815994),
            new Tuple<int, int, int, int>(65532,4783925,16766671,15197642),
            new Tuple<int, int, int, int>(16760303,16760303,3815994,14207663),
            new Tuple<int, int, int, int>(16770048,16770048,3815994,3815994),
            new Tuple<int, int, int, int>(16737792,16737792,11166563,6974058),
            new Tuple<int, int, int, int>(12773119,12773119,5716493,3815994),
            new Tuple<int, int, int, int>(16777215,16763043,16701597,16777215),
            new Tuple<int, int, int, int>(6587161,6587161,16777215,3815994),
            new Tuple<int, int, int, int>(6329328,16749602,3815994,3815994),
            new Tuple<int, int, int, int>(15793920,16519679,14920312,15395562),
            new Tuple<int, int, int, int>(15466636,10724259,16760474,13664854),
            new Tuple<int, int, int, int>(11563263,327629,6308658,13664854),
            new Tuple<int, int, int, int>(58867,16777215,16754809,8082236),
            new Tuple<int, int, int, int>(4909311,16777215,5849409,11623516),
            new Tuple<int, int, int, int>(3700643,7602233,9852728,14138263),
            new Tuple<int, int, int, int>(16777215,1017599,8742735,3877137),
            new Tuple<int, int, int, int>(16772022,16772022,16761488,3877137),
            new Tuple<int, int, int, int>(7849983,5067443,8081664,3815994),
            new Tuple<int, int, int, int>(15913534,7602233,6444881,14138263),
            new Tuple<int, int, int, int>(12320733,16775618,11836798,3090459),
            new Tuple<int, int, int, int>(15240846,16777215,9985296,3815994),
            new Tuple<int, int, int, int>(14967137,3702939,3815994,14207663),
            new Tuple<int, int, int, int>(6343571,3702939,12353664,15395562),
            new Tuple<int, int, int, int>(16761374,15018024,9475214,4278081),
            new Tuple<int, int, int, int>(16743936,3756172,16777215,16773316),
            new Tuple<int, int, int, int>(2899345,5393472,16777215,4210752),
            new Tuple<int, int, int, int>(11645361,16777215,16771542,10123632),
            new Tuple<int, int, int, int>(3421236,5958825,16771542,3815994),
            new Tuple<int, int, int, int>(15851871,5395026,15444592,7820105),
            new Tuple<int, int, int, int>(16777215,9463517,7424036,10129549),
            new Tuple<int, int, int, int>(16760556,16733184,16767659,15843765),
            new Tuple<int, int, int, int>(4781311,15771930,16765601,14144436),
            new Tuple<int, int, int, int>(16760556,10287103,16767659,6708313),
            new Tuple<int, int, int, int>(13083490,16777215,9789279,3815994),
            new Tuple<int, int, int, int>(13810226,9115524,5855577,11645361),
            new Tuple<int, int, int, int>(14176336,9115524,13530742,7105644),
            new Tuple<int, int, int, int>(16770310,16751169,16772294,16777215)
        };

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        internal void ResetBet()
        {
            currentBet = 100;
            currentGain = currentBet * 2;
        }
    }
}
