using CitizenFX.Core;
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
        int nameTagColor = 0;
        int debugColor = 0;

        public override void Begin()
        {

        }

        //[TickHandler(SessionWait = true)]
        //private async Task OnDebugColor()
        //{
        //    if (Utils.ControlHelper.IsControlJustPressed(Control.Context))
        //    {
        //        debugColor++;

        //        if (debugColor >= 255)
        //            debugColor = 0;

        //        await BaseScript.Delay(100);
        //    }
        //    Screen.ShowSubtitle($"Debug Color: {debugColor}");
        //}

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerNames()
        {
            foreach (Player player in Instance.PlayerList)
            {
                if (player == Game.Player) continue;

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

                        if (isStaff)
                            nameTagColor = 64;

                        playerNameTag.TagHandle = CreateMpGamerTag(pedHandle, playerName, false, isStaff, string.Empty, 0);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.GamerName, true);

                        SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, NetworkIsPlayerTalking(playerHandle));
                        SetMpGamerTagColour(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, 208);
                        SetMpGamerTagAlpha(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, 255);

                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.CrewTag, true);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.HealthArmour, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.BigText, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpUsingMenu, false);
                        bool isPassive = player.Character.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpPassiveMode, isPassive);
                        SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.WantedStars, isStaff);

                        bool isDriving = false;

                        if (IsPedSittingInAnyVehicle(pedHandle))
                        {
                            isDriving = player.Character.CurrentVehicle.Driver.Handle == pedHandle;
                        }

                        SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpDriver, isDriving);

                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpCoDriver, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTagged, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.GamerNameNearby, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.Arrow, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpPackages, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.InvIfPedFollowing, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.RankText, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTyping, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpBagLarge, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTagArrow, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpGangCeo, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpGangBiker, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.BikerArrow, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRolePresident, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleVicePresident, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleRoadCaptain, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleSargeant, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleEnforcer, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleProspect, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTransmitter, false);
                        //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpBomb, false);

                        SetMpGamerTagIcons(playerNameTag.TagHandle, false);
                        SetMpGamerTagColour(playerNameTag.TagHandle, (int)GamerTagComponent.WantedStars, nameTagColor);
                        //SetMpGamerTagBigText(playerNameTag.TagHandle, "BIG TEXT");
                        //SetMpGamerTagChatting(playerNameTag.TagHandle, "typing...");

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
                        ShowHeadingIndicatorOnBlip(blip.Handle, false);
                    }
                    else
                    {
                        blip.Sprite = BlipSprite.Standard;
                        ShowHeadingIndicatorOnBlip(blip.Handle, true);
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
