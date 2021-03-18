using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.UI
{
    public class PlayerNameManager : Manager<PlayerNameManager>
    {
        Dictionary<int, PlayerNameTag> currentPlayerNameTags = new Dictionary<int, PlayerNameTag>();
        int debugColor = 0;

        public override void Begin()
        {

        }

        [TickHandler(SessionWait = true)]
        private async Task OnDebugColor()
        {
            if (Utils.ControlHelper.IsControlJustPressed(Control.Context))
            {
                debugColor++;
                
                if (debugColor >= 255)
                    debugColor = 0;

                await BaseScript.Delay(100);
            }
            Screen.ShowSubtitle($"Debug Color: {debugColor}");
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerNames()
        {
            foreach (Player player in Instance.PlayerList)
            {
                int playerHandle = player.Handle;
                int pedHandle = player.Character.Handle;

                if (NetworkIsPlayerActive(playerHandle))
                {
                    if (!currentPlayerNameTags.ContainsKey(playerHandle))
                    {
                        string playerName = $"{GetPlayerName(playerHandle)} [{GetPlayerServerId(playerHandle)}]";
                        if (currentPlayerNameTags.ContainsKey(playerHandle))
                        {
                            RemoveMpGamerTag(currentPlayerNameTags[playerHandle].TagHandle);
                        }

                        PlayerNameTag playerNameTag = new PlayerNameTag();

                        bool isStaff = player.State.Get($"{StateBagKey.STAFF_MEMBER}") == null ? false : player.State.Get($"{StateBagKey.STAFF_MEMBER}");
                        string staffTag = isStaff ? "STAFF" : string.Empty;

                        playerNameTag.TagHandle = CreateMpGamerTag(pedHandle, playerName, false, false, string.Empty, 0);

                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 1, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 2, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 3, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 4, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 5, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 6, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 7, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 8, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 9, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 10, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 11, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 12, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 13, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 14, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 15, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 16, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 17, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 18, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 19, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 20, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 21, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 22, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 23, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 24, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 25, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 26, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 27, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 28, true);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, 29, true);



                        SetMpGamerTagIcons(playerNameTag.TagHandle, false);
                        SetMpGamerTagColour(playerNameTag.TagHandle, 0, debugColor);
                        SetMpGamerTagBigText(playerNameTag.TagHandle, "BIG TEXT");
                        SetMpGamerTagChatting(playerNameTag.TagHandle, "typing...");
                        
                        playerNameTag.PedHandle = pedHandle;

                    }
                } else if (currentPlayerNameTags.ContainsKey(playerHandle))
                {
                    RemoveMpGamerTag(currentPlayerNameTags[playerHandle].TagHandle);
                    currentPlayerNameTags.Remove(playerHandle);
                }
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerBlips()
        {
            foreach (Player player in Instance.PlayerList)
            {
                if (Game.Player == player) continue;
                Blip blip;

                if (player.Character.AttachedBlip == null)
                {
                    blip = player.Character.AttachBlip();
                    blip.Sprite = BlipSprite.Standard;
                    blip.Scale = 0.85f;

                    SetBlipCategory(blip.Handle, 7);
                    SetBlipPriority(blip.Handle, 11);
                    ShowHeadingIndicatorOnBlip(blip.Handle, true);
                    SetBlipNameToPlayerName(blip.Handle, player.Handle);
                }
                else
                {
                    blip = player.Character.AttachedBlip;

                    if (player.Character.IsDead)
                    {
                        blip.Sprite = BlipSprite.Dead;
                        ShowHeadingIndicatorOnBlip(blip.Handle, false); ;
                    }

                    if (Game.IsPaused)
                    {
                        blip.Alpha = 255;
                    }
                    else
                    {
                        Vector3 charPos = player.Character.Position;
                        Vector3 playerPos = Cache.PlayerPed.Position;

                        double distance = (Math.Floor(Math.Abs(Math.Sqrt(
                            (charPos.X - playerPos.X) *
                            (charPos.X - playerPos.X) +
                            (charPos.Y - playerPos.Y) *
                            (charPos.Y - playerPos.Y))) / -1)) + 900;

                        if (distance < 0)
                            distance = 0;

                        if (distance > 255)
                            distance = 255;

                        if (player.Character.IsVisible)
                        {
                            blip.Alpha = (int)distance;
                        }
                        else
                        {
                            blip.Alpha = 0;
                        }
                    }
                }
            }
        }
    }

    public class PlayerNameTag
    {
        public int TagHandle;
        public int PedHandle;
    }
}
