using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class Scoreboard
    {
        static int maxClients = -1;
        static bool ScaleSetup = false;
        static int currentPage = 0;
        static Scaleform scale;
        static int maxPages = (int)Math.Ceiling((double)Client.players.Count() / 16.0);

        public struct PlayerRowConfig
        {
            public string crewName;
            public int jobPoints;
            public bool showJobPointsIcon;
        }

        static Dictionary<int, PlayerRowConfig> playerConfigs = new Dictionary<int, PlayerRowConfig>();

        static Dictionary<int, string> textureCache = new Dictionary<int, string>();

        static Client client = Client.GetInstance();

        /// <summary>
        /// Constructor
        /// </summary>
        public static void Init()
        {
            Client.TriggerServerEvent("curiosity:Server:Scoreboard:MaxPlayers");
            client.RegisterTickHandler(ShowScoreboard);
            client.RegisterTickHandler(DisplayController);
            client.RegisterTickHandler(BackupTimer);

            // Periodically update the player headshots so, you don't have to wait for them later
            client.RegisterTickHandler(UpdateHeadshots);

            client.RegisterEventHandler("curiosity:Client:Scoreboard:MaxPlayers", new Action<int>(SetMaxPlayers));
            client.RegisterEventHandler("curiosity:Client:Scoreboard:SetPlayerRow", new Action<int, string, int, bool>(SetPlayerConfig));
        }

        /// <summary>
        /// Set the config for the specified player.
        /// </summary>
        /// <param name="playerServerId"></param>
        /// <param name="crewname"></param>
        /// <param name="jobpoints"></param>
        /// <param name="showJPicon"></param>
        static async void SetPlayerConfig(int playerServerId, string crewname, int jobpoints, bool showJPicon)
        {
            var cfg = new PlayerRowConfig()
            {
                crewName = crewname ?? "",
                jobPoints = jobpoints,
                showJobPointsIcon = showJPicon
            };
            playerConfigs[playerServerId] = cfg;
            if (currentPage > -1)
                await LoadScale();
        }


        /// <summary>
        /// Used to close the page if the regular timer fails to close it for some odd reason.
        /// </summary>
        /// <returns></returns>
        static async Task BackupTimer()
        {
            var timer = GetGameTimer();
            var oldPage = currentPage;
            while (GetGameTimer() - timer < 8000 && currentPage > 0 && currentPage == oldPage)
            {
                await Client.Delay(0);
            }
            if (oldPage == currentPage)
            {
                currentPage = 0;
            }
        }

        /// <summary>
        /// Updates the max pages to disaplay based on the player count.
        /// </summary>
        static void UpdateMaxPages()
        {
            maxPages = (int)Math.Ceiling((double)Client.players.Count() / 16.0);
        }

        /// <summary>
        /// Manages the display and page setup of the playerlist.
        /// </summary>
        /// <returns></returns>
        static async Task DisplayController()
        {
            if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
            {
                UpdateMaxPages();
                if (ScaleSetup)
                {
                    currentPage++;
                    if (currentPage > maxPages)
                    {
                        currentPage = 0;
                    }
                    await LoadScale();
                    var timer = GetGameTimer();
                    bool nextPage = false;
                    while (GetGameTimer() - timer < 8000)
                    {
                        await Client.Delay(1);
                        if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
                        {
                            nextPage = true;
                            break;
                        }
                    }
                    if (nextPage)
                    {
                        UpdateMaxPages();
                        if (currentPage < maxPages)
                        {
                            currentPage++;
                            await LoadScale();
                        }
                        else
                        {
                            currentPage = 0;
                        }
                    }
                    else
                    {
                        currentPage = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the max players (triggered from server event)
        /// </summary>
        /// <param name="count"></param>
        static void SetMaxPlayers(int count)
        {
            maxClients = count;
        }

        /// <summary>
        /// Shows the scoreboard.
        /// </summary>
        /// <returns></returns>
        static async Task ShowScoreboard()
        {
            if (maxClients != -1)
            {
                if (!ScaleSetup)
                {
                    await LoadScale();
                    ScaleSetup = true;
                }
                if (currentPage > 0)
                {
                    float safezone = GetSafeZoneSize();
                    float change = (safezone - 0.89f) / 0.11f;
                    float x = 50f;
                    x -= change * 78f;
                    float y = 50f;
                    y -= change * 50f;

                    var width = Screen.Resolution.Width > 1920 ? 200f : 300f;
                    var height = Screen.Resolution.Width > 1920 ? 290f : 390f;
                    if (scale != null)
                    {
                        if (scale.IsLoaded)
                        {
                            scale.Render2DScreenSpace(new System.Drawing.PointF(x, y), new System.Drawing.PointF(width, height));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Loads the scaleform.
        /// </summary>
        /// <returns></returns>
        static async Task LoadScale()
        {
            if (scale != null)
            {
                for (var i = 0; i < maxClients * 2; i++)
                {
                    scale.CallFunction("SET_DATA_SLOT_EMPTY", i);
                }
                scale.Dispose();
            }
            scale = null;
            while (!HasScaleformMovieLoaded(RequestScaleformMovie("MP_MM_CARD_FREEMODE")))
            {
                await Client.Delay(0);
            }
            scale = new Scaleform("MP_MM_CARD_FREEMODE");
            var titleIcon = "2";
            var titleLeftText = "Life V Network";
            var titleRightText = $"Players {NetworkGetNumConnectedPlayers()}/{maxClients}";
            scale.CallFunction("SET_TITLE", titleLeftText, titleRightText, titleIcon);
            await UpdateScale();
            scale.CallFunction("DISPLAY_VIEW");
        }

        /// <summary>
        /// Struct used for the player info row options.
        /// </summary>
        struct PlayerRow
        {
            public int serverId;
            public string name;
            public string rightText;
            public int color;
            public string iconOverlayText;
            public string jobPointsText;
            public string crewLabelText;
            public enum DisplayType
            {
                NUMBER_ONLY = 0,
                ICON = 1,
                NONE = 2
            };
            public DisplayType jobPointsDisplayType;
            public enum RightIconType
            {
                NONE = 0,
                INACTIVE_HEADSET = 48,
                MUTED_HEADSET = 49,
                ACTIVE_HEADSET = 47,
                RANK_FREEMODE = 65,
                KICK = 64,
                LOBBY_DRIVER = 79,
                LOBBY_CODRIVER = 80,
                SPECTATOR = 66,
                BOUNTY = 115,
                DEAD = 116,
                DPAD_GANG_CEO = 121,
                DPAD_GANG_BIKER = 122,
                DPAD_DOWN_TARGET = 123
            };
            public int rightIcon;
            public string textureString;
            public char friendType;
        }

        /// <summary>
        /// Returns the ped headshot string used for the image of the ped for each row.
        /// </summary>
        /// <param name="ped"></param>
        /// <returns></returns>
        static async Task<string> GetHeadshotImage(int ped)
        {
            var headshotHandle = RegisterPedheadshot(ped);
            /*
             * For some reason, the below loop didn't work originally without the Valid check or the re-registering of the headshot
             */
            while (!IsPedheadshotReady(headshotHandle) || !IsPedheadshotValid(headshotHandle))
            {
                headshotHandle = RegisterPedheadshot(ped);
                await Client.Delay(0);
            }
            return GetPedheadshotTxdString(headshotHandle) ?? "";
        }

        /// <summary>
        /// Updates the scaleform settings.
        /// </summary>
        /// <returns></returns>
        static async Task UpdateScale()
        {
            List<PlayerRow> rows = new List<PlayerRow>();

            for (var x = 0; x < 150; x++) // cleaning up in case of a reload, this frees up all ped headshot handles :)
            {
                UnregisterPedheadshot(x);
            }

            var amount = 0;
            foreach (CitizenFX.Core.Player p in Client.players)
            {
                if (IsRowSupposedToShow(amount))
                {
                    PlayerRow row = new PlayerRow(); // Set as a blank PlayerRow obj

                    if (playerConfigs.ContainsKey(p.ServerId))
                    {
                        row = new PlayerRow()
                        {
                            color = 111,
                            crewLabelText = playerConfigs[p.ServerId].crewName,
                            friendType = ' ',
                            iconOverlayText = "",
                            jobPointsDisplayType = playerConfigs[p.ServerId].showJobPointsIcon ? PlayerRow.DisplayType.ICON :
                                (playerConfigs[p.ServerId].jobPoints >= 0 ? PlayerRow.DisplayType.NUMBER_ONLY : PlayerRow.DisplayType.NONE),
                            jobPointsText = playerConfigs[p.ServerId].jobPoints >= 0 ? playerConfigs[p.ServerId].jobPoints.ToString() : "",
                            name = $"[{p.ServerId}] {p.Name.Replace("<", "").Replace(">", "").Replace("^", "").Replace("~", "").Trim()}",
                            rightIcon = (int)PlayerRow.RightIconType.NONE,
                            rightText = $"",
                            serverId = p.ServerId,
                        };
                    }
                    else
                    {
                        row = new PlayerRow()
                        {
                            color = 111,
                            crewLabelText = "",
                            friendType = ' ',
                            iconOverlayText = "",
                            jobPointsDisplayType = PlayerRow.DisplayType.NUMBER_ONLY,
                            jobPointsText = "",
                            name = $"[{p.ServerId}] {p.Name.Replace("<", "").Replace(">", "").Replace("^", "").Replace("~", "").Trim()}",
                            rightIcon = (int)PlayerRow.RightIconType.NONE,
                            rightText = $"",
                            serverId = p.ServerId,
                        };
                    }

                    //Debug.WriteLine("Checking if {0} is in the Dic. Their SERVER ID {1}.", p.Name, p.ServerId);
                    if (textureCache.ContainsKey(p.ServerId))
                    {
                        row.textureString = textureCache[p.ServerId];
                    }
                    else
                    {
                        //Debug.WriteLine("Not in setting image to blank");
                        row.textureString = "";
                    }

                    rows.Add(row);
                }
                amount++;
            }
            rows.Sort((row1, row2) => row1.serverId.CompareTo(row2.serverId));
            for (var i = 0; i < maxClients * 2; i++)
            {
                scale.CallFunction("SET_DATA_SLOT_EMPTY", i);
            }
            var index = 0;
            foreach (PlayerRow row in rows)
            {
                if (row.crewLabelText != "")
                {
                    scale.CallFunction("SET_DATA_SLOT", index, row.rightText, row.name, row.color, row.rightIcon, row.iconOverlayText, row.jobPointsText,
                        $"..+{row.crewLabelText}", (int)row.jobPointsDisplayType, row.textureString, row.textureString, row.friendType);
                }
                else
                {
                    scale.CallFunction("SET_DATA_SLOT", index, row.rightText, row.name, row.color, row.rightIcon, row.iconOverlayText, row.jobPointsText,
                        "", (int)row.jobPointsDisplayType, row.textureString, row.textureString, row.friendType);
                }
                index++;
            }

            await Client.Delay(0);
        }

        /// <summary>
        /// Used to check if the row from the loop is supposed to be displayed based on the current page view.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        static bool IsRowSupposedToShow(int row)
        {
            if (currentPage > 0)
            {
                var max = currentPage * 16;
                var min = (currentPage * 16) - 16;
                if (row >= min && row < max)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        /// <summary>
        /// Update the "textureCache" Dictionary with headshots of the players online.
        /// </summary>
        /// <returns></returns>
        static async Task UpdateHeadshots()
        {
            PlayerList playersToCheck = Client.players;

            foreach (CitizenFX.Core.Player p in playersToCheck)
            {
                string headshot = await GetHeadshotImage(GetPlayerPed(p.Handle));

                textureCache[p.ServerId] = headshot;
            }

            //Maybe make configurable?
            await Client.Delay(1000);
        }
    }
}
