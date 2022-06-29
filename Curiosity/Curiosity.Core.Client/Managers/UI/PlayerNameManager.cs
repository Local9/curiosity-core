using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Enums;

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
                foreach (KeyValuePair<Player, int> gamerTag in gamerTags)
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

                    if (!HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17))
                    {
                        closeEnough = false;
                    }

                    bool isStaff = player.State.Get(StateBagKey.STAFF_MEMBER) == null ? false : player.State.Get(StateBagKey.STAFF_MEMBER);
                    bool isWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
                    bool isHidden = player.State.Get(StateBagKey.PLAYER_OFF_RADAR) ?? false;

                    if (isHidden || !player.Character.IsVisible)
                    {
                        if (gamerTags.ContainsKey(player))
                        {
                            RemoveAndHideNameTag(player, gamerTags[player]);
                        }
                        continue;
                    }

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

                        SetMpGamerTagVisibility(tagHandle, (int)GamerTagComponent.HealthArmour, IsPlayerTargettingEntity(PlayerId(), pedHandle));
                        SetMpGamerTagAlpha(tagHandle, (int)GamerTagComponent.HealthArmour, 255);

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

                        if (player.WantedLevel > 0 || isWanted)
                        {
                            SetMpGamerTagColour(tagHandle, (int)GamerTagComponent.GamerName, 7);
                            SetMpGamerTagVisibility(tagHandle, 7, true); // wantedStars
                            SetMpGamerTagWantedLevel(tagHandle, GetPlayerWantedLevel(player.Handle));
                        }
                        else
                        {
                            SetMpGamerTagColour(tagHandle, (int)GamerTagComponent.GamerName, 0);
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
    }
}
