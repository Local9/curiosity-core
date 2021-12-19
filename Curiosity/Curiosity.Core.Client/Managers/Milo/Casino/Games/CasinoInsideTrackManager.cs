using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitizenFX.Core;
using CitizenFX.Core.UI;

namespace Curiosity.Core.Client.Managers.Milo.Casino.Games
{
    public class CasinoInsideTrackManager : Manager<CasinoInsideTrackManager>
    {
        const string SCALEFORM_RACING_WALL = "HORSE_RACING_WALL"; // func_6039
        const string SCALEFORM_RACING_CONSOLE = "HORSE_RACING_CONSOLE"; // func_6031
        const string TEXTURE_DICTIONARY_INSIDE_TRACK = "Prop_Screen_VW_InsideTrack"; // func_5975
        const string RENDER_TARGET_CASINO_SCREEN_2 = "casinoscreen_02"; // func_5961
        int RENDER_TARGET_LINK_HASH = -214651601; // func_6038 (this is the hash prop of the wall) // https://forge.plebmasters.de/objects/vw_vwint01_betting_screen

        private int position = 4;
        private CasinoDataObject casinoDataObject = new CasinoDataObject();

        public override void Begin()
        {
            
        }

        public void Init()
        {
            Instance.AttachTickHandler(OnSetupRenderTargets);
            Instance.AttachTickHandler(OnWallAndConsoleScaleform);
        }

        public void Dispose()
        {
            ReleaseNamedRendertarget(RENDER_TARGET_CASINO_SCREEN_2);
            Instance.DetachTickHandler(OnSetupRenderTargets);
            Instance.DetachTickHandler(OnWallAndConsoleScaleform);
        }

        private async Task OnWallAndConsoleScaleform()
        {
            OnScaleFormProgress(null, null, casinoDataObject, false);
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
            return null;
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
            return - 1;
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

        private void OnScaleFormProgress(dynamic uParam0, dynamic uParam1, CasinoDataObject casinoWall, bool isFullscreen)
        {
            int progress = 4;

            if (!isFullscreen)
            {
                progress = casinoWall.WallProgress;
            }
            else
            {
                progress = casinoWall.ConsoleProgress;
            }

            switch (progress)
            {
                case 0:
                    if (!isFullscreen)
                    {
                        casinoWall.WallScaleformHandle = RequestScaleformMovie_2(SCALEFORM_RACING_WALL);
                        if (HasScaleformMovieLoaded(casinoWall.WallScaleformHandle))
                        {
                            SetupRenderTarget(casinoWall);
                            if (casinoWall.WallRenderHandle != -1)
                            {
                                SetScaleformFitRendertarget(casinoWall.WallScaleformHandle, true);
                            }
                            SetScreenProgress(casinoWall, 1, isFullscreen);
                        }
                    }
                    else
                    {
                        casinoWall.ConsoleScaleformHandle = RequestScaleformMovie_2(SCALEFORM_RACING_CONSOLE);
                        if (HasScaleformMovieLoaded(casinoWall.WallScaleformHandle))
                        {

                        }
                        SetScreenProgress(casinoWall, 1, isFullscreen);
                    }
                    break;
                case 1:
                    if (isFullscreen)
                    {
                        CallScaleformMovieMethod(casinoWall.ConsoleScaleformHandle, "CLEAR_ALL_PLAYERS");
                    }
                    else
                    {
                        CallScaleformMovieMethod(casinoWall.WallScaleformHandle, "CLEAR_ALL_PLAYERS");
                    }
                    DrawScaleForms(casinoWall, isFullscreen);
                    SetScaleformShowScreen(casinoWall, 0, isFullscreen);
                    break;
            }
        }

        void SetScreenProgress(CasinoDataObject casinoWall, int progress, bool isConsole)
        {
            if (!isConsole)
            {
                casinoWall.WallProgress = progress;
            }
            else
            {
                casinoWall.ConsoleProgress = progress;
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

        void SetScaleformShowScreen(CasinoDataObject casinoWall, int progress, bool isConsole)
        {
            if (isConsole)
            {
                casinoWall.ConsoleScreen = progress;
                BeginScaleformMovieMethod(casinoDataObject.ConsoleScaleformHandle, "SHOW_SCREEN");
            }
            else
            {
                casinoWall.WallScreen = progress;
                BeginScaleformMovieMethod(casinoDataObject.WallScaleformHandle, "SHOW_SCREEN");
            }
            ScaleformMovieMethodAddParamInt(progress);
            EndScaleformMovieMethod();
        }
    }

    internal class CasinoDataObject
    {
        public int WallRenderHandle = -1;// uParam0->f_280
        public int WallScaleformHandle = -1; // uParam0->f_298

        public int ConsoleScaleformHandle = -1;

        public int WallProgress = 0; // uParam0->f_191
        public int WallScreen = 0; // uParam0->f_192

        public int ConsoleProgress = 0; // uParam0->f_193
        public int ConsoleScreen = 0; // uParam0->f_194

        public int[] BettingComputerRenderId = new int[16]
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };

        public int[] BettingComputerPosition = new int[16]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        public int[] BettingComputerScreenDisplay = new int[16]
        {
            6, 6, 6, 6, 6, 5, 4, 3, 3, 1, 5, 4, 3, 2, 1, 0
        };



        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
