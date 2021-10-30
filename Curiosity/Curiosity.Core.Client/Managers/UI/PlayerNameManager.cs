using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
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
        Dictionary<Player, int> gamerTags = new Dictionary<Player, int>();

        public int NameTagColor = 0;
        public bool ShowMyName = false;
        public bool ShowServerHandle = false;
        public bool ShowPlayerNames = true;

        public float playerNamesDistance = 100f;

        public override void Begin()
        {

        }

        //[TickHandler(SessionWait = true)]
        //private async Task OnDebugColor()
        //{
        //    if (Utils.ControlHelper.IsControlJustPressed(Control.PhoneUp))
        //    {
        //        NameTagColor++;

        //        if (NameTagColor >= 255)
        //            NameTagColor = 0;

        //        await BaseScript.Delay(100);
        //    }

        //    if (Utils.ControlHelper.IsControlJustPressed(Control.PhoneDown))
        //    {
        //        NameTagColor--;

        //        if (NameTagColor < 0)
        //            NameTagColor = 255;

        //        await BaseScript.Delay(100);
        //    }

        //    Screen.ShowSubtitle($"Debug Color: {NameTagColor}");
        //}

        [TickHandler(SessionWait = true)]
        private Task OnPlayerOverheadNamesTask()
        {
            if (!ShowPlayerNames)
            {
                foreach(KeyValuePair<Player, int> gamerTag in gamerTags)
                {
                    RemoveMpGamerTag(gamerTag.Value);
                }
                gamerTags.Clear();
            }
            else
            {
                foreach (Player player in Instance.PlayerList)
                {
                    if (player == Game.Player && !ShowMyName)
                    {
                        if (gamerTags.ContainsKey(player))
                        {
                            RemoveAndHideNameTag(player, gamerTags[player]);
                        }
                        continue;
                    }

                    var dist = player.Character.Position.Distance(Game.PlayerPed.Position);
                    bool closeEnough = dist < playerNamesDistance;

                    bool isStaff = player.State.Get(StateBagKey.STAFF_MEMBER) == null ? false : player.State.Get(StateBagKey.STAFF_MEMBER);

                    if (gamerTags.ContainsKey(player))
                    {
                        if (!closeEnough)
                        {
                            RemoveMpGamerTag(gamerTags[player]);
                            gamerTags.Remove(player);
                        }
                        else
                        {
                            gamerTags[player] = CreateMpGamerTag(player.Character.Handle, $"{player.Name} [{player.ServerId}]", false, isStaff, string.Empty, 0);
                        }
                    }
                    else if (closeEnough)
                    {
                        gamerTags.Add(player, CreateMpGamerTag(player.Character.Handle, $"{player.Name} [{player.ServerId}]", false, isStaff, string.Empty, 0));
                    }

                    if (closeEnough && gamerTags.ContainsKey(player))
                    {
                        int tagHandle = gamerTags[player];
                        int pedHandle = player.Character.Handle;

                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.HealthArmour, true);
                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.AudioIcon, NetworkIsPlayerTalking(player.Handle));
                        SetMpGamerTagColour(tagHandle, (int)GamerTagComponent.AudioIcon, 208);
                        SetMpGamerTagAlpha(tagHandle, (int)GamerTagComponent.AudioIcon, 255);

                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.CrewTag, isStaff);

                        bool isPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.MpPassiveMode, isPassive);

                        bool isDriving = false;

                        if (IsPedSittingInAnyVehicle(player.Character.Handle))
                        {
                            isDriving = player.Character.CurrentVehicle.Driver.Handle == pedHandle;
                        }

                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.MpDriver, isDriving);

                        if (player.WantedLevel > 0)
                        {
                            SetMpGamerTagVisibility(tagHandle, 7, true); // wantedStars
                            SetMpGamerTagWantedLevel(tagHandle, GetPlayerWantedLevel(player.Handle));
                        }
                        else
                        {
                            SetMpGamerTagVisibility(tagHandle, 7, false); // wantedStars hide
                        }
                    }
                }
            }

            return BaseScript.Delay(500);
        }

        //private Task OnWorldPlayerNames()
        //{
        //    foreach (Player player in Instance.PlayerList)
        //    {
        //        int playerHandle = player.Handle;
        //        int pedHandle = player.Character.Handle;

        //        if (player == Game.Player && !ShowMyName)
        //        {
        //            if (currentPlayerNameTags.ContainsKey(playerHandle))
        //            {
        //                // RemoveAndHideNameTag(currentPlayerNameTags[playerHandle], playerHandle);
        //            }
        //            continue;
        //        }

        //        if (NetworkIsPlayerActive(playerHandle) && ShowPlayerNames)
        //        {
        //            if (!currentPlayerNameTags.ContainsKey(playerHandle))
        //            {
        //                string playerName = $"{GetPlayerName(playerHandle)}";
        //                if (ShowServerHandle)
        //                {
        //                    playerName += $" [{GetPlayerServerId(playerHandle)}]";
        //                }

        //                if (currentPlayerNameTags.ContainsKey(playerHandle))
        //                {
        //                    RemoveMpGamerTag(currentPlayerNameTags[playerHandle].TagHandle);
        //                }

        //                PlayerNameTag playerNameTag = new PlayerNameTag();

        //                bool isStaff = player.State.Get($"{StateBagKey.STAFF_MEMBER}") == null ? false : player.State.Get($"{StateBagKey.STAFF_MEMBER}");

        //                if (isStaff)
        //                    StaffStarColor = 64;

        //                playerNameTag.TagHandle = CreateMpGamerTag(pedHandle, playerName, false, isStaff, string.Empty, 0);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.GamerName, true);

        //                SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, NetworkIsPlayerTalking(playerHandle));
        //                SetMpGamerTagColour(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, 208);
        //                SetMpGamerTagAlpha(playerNameTag.TagHandle, (int)GamerTagComponent.AudioIcon, 255);

        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.CrewTag, true);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.HealthArmour, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.BigText, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpUsingMenu, false);
        //                bool isPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
        //                SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpPassiveMode, isPassive);
        //                SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.WantedStars, isStaff);

        //                bool isDriving = false;

        //                if (IsPedSittingInAnyVehicle(pedHandle))
        //                {
        //                    isDriving = player.Character.CurrentVehicle.Driver.Handle == pedHandle;
        //                }

        //                SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpDriver, isDriving);

        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpCoDriver, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTagged, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.GamerNameNearby, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.Arrow, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpPackages, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.InvIfPedFollowing, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.RankText, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTyping, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpBagLarge, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTagArrow, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpGangCeo, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpGangBiker, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.BikerArrow, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRolePresident, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleVicePresident, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleRoadCaptain, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleSargeant, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleEnforcer, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.McRoleProspect, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpTransmitter, false);
        //                //SetMpGamerTagVisibility(playerNameTag.TagHandle, (int)GamerTagComponent.MpBomb, false);

        //                SetMpGamerTagIcons(playerNameTag.TagHandle, false);
        //                SetMpGamerTagColour(playerNameTag.TagHandle, (int)GamerTagComponent.WantedStars, StaffStarColor);
        //                //SetMpGamerTagBigText(playerNameTag.TagHandle, "BIG TEXT");
        //                //SetMpGamerTagChatting(playerNameTag.TagHandle, "typing...");

        //                NameTagColor = 0;
        //                if (GetPlayerWantedLevel(playerHandle) > 0)
        //                    NameTagColor = 6;

        //                SetMpGamerTagColour(playerNameTag.TagHandle, (int)GamerTagComponent.GamerName, NameTagColor);

        //                playerNameTag.PedHandle = pedHandle;

        //            }
        //            else if (!ShowPlayerNames)
        //            {
        //                // RemoveAndHideNameTag(currentPlayerNameTags[playerHandle], playerHandle);
        //            }
        //        }
        //        else if (currentPlayerNameTags.ContainsKey(playerHandle))
        //        {
        //            // RemoveAndHideNameTag(currentPlayerNameTags[playerHandle], playerHandle);
        //        }
        //    }
        //    return BaseScript.Delay(500);
        //}

        void RemoveAndHideNameTag(Player player, int tagHandle)
        {
            SetMpGamerTagEnabled(tagHandle, false);
            RemoveMpGamerTag(tagHandle);
            gamerTags.Remove(player);
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

                    string key = $"playerBlip{player.ServerId}";
                    AddTextEntry(key, player.Name);

                    AddTextComponentSubstringBlipName(blip.Handle);
                    BeginTextCommandSetBlipName(key);
                    EndTextCommandSetBlipName(blip.Handle);

                    blip.Name = player.Name;
                }
                else
                {
                    blip = player.Character.AttachedBlip;

                    SetBlipCategory(blip.Handle, 7);
                    SetBlipPriority(blip.Handle, 11);
                    ShowHeadingIndicatorOnBlip(blip.Handle, true);
                    SetBlipNameToPlayerName(blip.Handle, player.Handle);

                    blip.Name = player.Name;

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
