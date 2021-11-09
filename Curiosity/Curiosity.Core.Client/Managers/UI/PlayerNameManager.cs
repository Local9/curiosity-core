using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.UI
{
    public class PlayerNameManager : Manager<PlayerNameManager>
    {
        Dictionary<Player, int> gamerTags = new Dictionary<Player, int>();

        public int NameTagColor = 0;
        public bool ShowMyName = false;
        public bool ShowServerHandle = false;
        public bool ShowPlayerNames = true;

        public float playerNamesDistance = 100f;

        public override void Begin()
        {

        }

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

        void RemoveAndHideNameTag(Player player, int tagHandle)
        {
            SetMpGamerTagEnabled(tagHandle, false);
            RemoveMpGamerTag(tagHandle);
            gamerTags.Remove(player);
        }

        //[TickHandler(SessionWait = true)]
        //private async Task OnWorldPlayerBlips()
        //{
        //    foreach (Player player in Instance.PlayerList)
        //    {
        //        if (Game.Player == player) continue;
        //        Blip blip;

        //        if (player.Character.AttachedBlip == null)
        //        {
        //            blip = player.Character.AttachBlip();
        //            blip.Sprite = BlipSprite.Standard;
        //            blip.Scale = 0.85f;

        //            string key = $"playerBlip{player.ServerId}";
        //            AddTextEntry(key, player.Name);

        //            AddTextComponentSubstringBlipName(blip.Handle);
        //            BeginTextCommandSetBlipName(key);
        //            EndTextCommandSetBlipName(blip.Handle);

        //            SetBlipCategory(blip.Handle, 7);
        //            SetBlipPriority(blip.Handle, 11);
        //            ShowHeadingIndicatorOnBlip(blip.Handle, true);
        //            SetBlipNameToPlayerName(blip.Handle, player.Handle);

        //            Utilities.SetCorrectBlipSprite(player.Character.Handle, blip.Handle);
        //        }
        //        else
        //        {
        //            blip = player.Character.AttachedBlip;

        //            string key = $"playerBlip{player.ServerId}";
        //            AddTextEntry(key, player.Name);

        //            AddTextComponentSubstringBlipName(blip.Handle);
        //            BeginTextCommandSetBlipName(key);
        //            EndTextCommandSetBlipName(blip.Handle);

        //            SetBlipCategory(blip.Handle, 7);
        //            SetBlipPriority(blip.Handle, 11);
        //            ShowHeadingIndicatorOnBlip(blip.Handle, true);
        //            SetBlipNameToPlayerName(blip.Handle, player.Handle);

        //            if (player.Character.IsDead)
        //            {
        //                blip.Sprite = BlipSprite.Dead;
        //                ShowHeadingIndicatorOnBlip(blip.Handle, false);
        //            }
        //            else
        //            {
        //                Utilities.SetCorrectBlipSprite(player.Character.Handle, blip.Handle);
        //                ShowHeadingIndicatorOnBlip(blip.Handle, true);
        //            }

        //            if (Game.IsPaused)
        //            {
        //                blip.Alpha = 255;
        //            }
        //            else
        //            {
        //                Vector3 charPos = player.Character.Position;
        //                Vector3 playerPos = Cache.PlayerPed.Position;

        //                double distance = (Math.Floor(Math.Abs(Math.Sqrt(
        //                    (charPos.X - playerPos.X) *
        //                    (charPos.X - playerPos.X) +
        //                    (charPos.Y - playerPos.Y) *
        //                    (charPos.Y - playerPos.Y))) / -1)) + 900;

        //                if (distance < 0)
        //                    distance = 0;

        //                if (distance > 255)
        //                    distance = 255;

        //                if (player.Character.IsVisible)
        //                {
        //                    blip.Alpha = (int)distance;
        //                }
        //                else
        //                {
        //                    blip.Alpha = 0;
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
