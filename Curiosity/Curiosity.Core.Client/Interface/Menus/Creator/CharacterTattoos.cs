using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface.Menus.Data;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class CharacterTattoos
    {
        UIMenu baseMenu;

        List<dynamic> headTattoosList = new List<dynamic>();
        List<dynamic> torsoTattoosList = new List<dynamic>();
        List<dynamic> leftArmTattoosList = new List<dynamic>();
        List<dynamic> rightArmTattoosList = new List<dynamic>();
        List<dynamic> leftLegTattoosList = new List<dynamic>();
        List<dynamic> rightLegTattoosList = new List<dynamic>();
        List<dynamic> badgeTattoosList = new List<dynamic>();

        UIMenuListItem lstHeadTatts;
        UIMenuListItem lstTorsoTatts;
        UIMenuListItem lstLeftArmTatts;
        UIMenuListItem lstRightArmTatts;
        UIMenuListItem lstLeftLegTatts;
        UIMenuListItem lstRightLegTatts;
        UIMenuListItem lstBadgeTatts;
        UIMenuItem itRemoveAllTattoos = new UIMenuItem("Clear All Tattoos");

        public UIMenu CreateMenu(UIMenu menu)
        {
            TattoosData.GenerateTattoosData();
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            return baseMenu = menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state.Equals(MenuState.Opened) || state.Equals(MenuState.ChangeForward))
            {
                SetupMenu();
            }
        }

        private void SetupMenu()
        {
            baseMenu.Clear();

            bool male = Game.PlayerPed.Gender == Gender.Male;

            if (male)
            {
                if (MaleTattoosCollection.HEAD.Count == 0)
                {
                    Logger.Error($"Tattoos Data file failed to load.");
                    return;
                }

                int counter = 1;
                foreach (var tattoo in MaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"Tattoo #{counter} (of {MaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in MaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"Badge #{counter} (of {MaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
            }
            else
            {
                if (FemaleTattoosCollection.HEAD.Count == 0)
                {
                    Logger.Error($"Tattoos Data file failed to load.");
                    return;
                }

                int counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.HEAD)
                {
                    headTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.HEAD.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.TORSO)
                {
                    torsoTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.TORSO.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_ARM)
                {
                    leftArmTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.LEFT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_ARM)
                {
                    rightArmTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.RIGHT_ARM.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.LEFT_LEG)
                {
                    leftLegTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.LEFT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.RIGHT_LEG)
                {
                    rightLegTattoosList.Add($"Tattoo #{counter} (of {FemaleTattoosCollection.RIGHT_LEG.Count})");
                    counter++;
                }
                counter = 1;
                foreach (var tattoo in FemaleTattoosCollection.BADGES)
                {
                    badgeTattoosList.Add($"Badge #{counter} (of {FemaleTattoosCollection.BADGES.Count})");
                    counter++;
                }
            }

            const string tatDesc = "Cycle through the list to preview tattoos. If you like one, press enter to select it, selecting it will add the tattoo if you don't already have it.";
            lstHeadTatts = new UIMenuListItem("Head Tattoos", headTattoosList, 0, tatDesc);
            lstTorsoTatts = new UIMenuListItem("Torso Tattoos", torsoTattoosList, 0, tatDesc);
            lstLeftArmTatts = new UIMenuListItem("Left Arm Tattoos", leftArmTattoosList, 0, tatDesc);
            lstRightArmTatts = new UIMenuListItem("Right Arm Tattoos", rightArmTattoosList, 0, tatDesc);
            lstLeftLegTatts = new UIMenuListItem("Left Leg Tattoos", leftLegTattoosList, 0, tatDesc);
            lstRightLegTatts = new UIMenuListItem("Right Leg Tattoos", rightLegTattoosList, 0, tatDesc);
            lstBadgeTatts = new UIMenuListItem("Badge Overlays", badgeTattoosList, 0, tatDesc);

            baseMenu.AddItem(lstHeadTatts);
            baseMenu.AddItem(lstTorsoTatts);
            baseMenu.AddItem(lstLeftArmTatts);
            baseMenu.AddItem(lstRightArmTatts);
            baseMenu.AddItem(lstLeftLegTatts);
            baseMenu.AddItem(lstRightLegTatts);
            baseMenu.AddItem(lstBadgeTatts);
            baseMenu.AddItem(itRemoveAllTattoos);

            baseMenu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            baseMenu.OnIndexChange += Menu_OnIndexChange;
            baseMenu.OnListChange += Menu_OnListChange;
            baseMenu.OnListSelect += Menu_OnListSelect;
            baseMenu.OnItemSelect += Menu_OnItemSelect;

            baseMenu.RefreshIndex();
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == itRemoveAllTattoos)
            {
                CuriosityCharacter currentCharacter = Cache.Character;
                Notify.Success("All tattoos have been removed.");
                currentCharacter.Tattoos.HeadTattoos.Clear();
                currentCharacter.Tattoos.TorsoTattoos.Clear();
                currentCharacter.Tattoos.LeftArmTattoos.Clear();
                currentCharacter.Tattoos.RightArmTattoos.Clear();
                currentCharacter.Tattoos.LeftLegTattoos.Clear();
                currentCharacter.Tattoos.RightLegTattoos.Clear();
                currentCharacter.Tattoos.BadgeTattoos.Clear();
                ClearPedDecorations(Game.PlayerPed.Handle);
            }
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            CreateListsIfNull(Cache.Character);
            ApplySavedTattoos(Cache.Character);

            int tattooIndex = newIndex;

            CuriosityCharacter currentCharacter = Cache.Character;

            if (listItem == lstHeadTatts) // head
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.HeadTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstTorsoTatts) // torso
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.TorsoTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstLeftArmTatts) // left arm
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.LeftArmTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstRightArmTatts) // right arm
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.RightArmTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstLeftLegTatts) // left leg
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.LeftLegTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstRightLegTatts) // right leg
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.RightLegTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
            else if (listItem == lstBadgeTatts) // badges
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (!currentCharacter.Tattoos.BadgeTattoos.Contains(tat))
                {
                    SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tat.Key), (uint)GetHashKey(tat.Value));
                }
            }
        }

        private void Menu_OnIndexChange(UIMenu sender, int newIndex)
        {
            CreateListsIfNull(Cache.Character);
            ApplySavedTattoos(Cache.Character);
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            CuriosityCharacter currentCharacter = Cache.Character;

            CreateListsIfNull(Cache.Character);

            int tattooIndex = newIndex;

            if (listItem == lstHeadTatts) // head
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.HEAD.ElementAt(tattooIndex) : FemaleTattoosCollection.HEAD.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.HeadTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.HeadTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.HeadTattoos.Add(tat);
                }
            }
            else if (listItem == lstTorsoTatts) // torso
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.TORSO.ElementAt(tattooIndex) : FemaleTattoosCollection.TORSO.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.TorsoTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.TorsoTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.TorsoTattoos.Add(tat);
                }
            }
            else if (listItem == lstLeftArmTatts) // left arm
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_ARM.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.LeftArmTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.LeftArmTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.LeftArmTattoos.Add(tat);
                }
            }
            else if (listItem == lstRightArmTatts) // right arm
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_ARM.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.RightArmTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.RightArmTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.RightArmTattoos.Add(tat);
                }
            }
            else if (listItem == lstLeftLegTatts) // left leg
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.LEFT_LEG.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.LeftLegTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.LeftLegTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.LeftLegTattoos.Add(tat);
                }
            }
            else if (listItem == lstRightLegTatts) // right leg
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex) : FemaleTattoosCollection.RIGHT_LEG.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.RightLegTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.RightLegTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Tattoo #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.RightLegTattoos.Add(tat);
                }
            }
            else if (listItem == lstBadgeTatts) // badges
            {
                var Tattoo = currentCharacter.IsMale ? MaleTattoosCollection.BADGES.ElementAt(tattooIndex) : FemaleTattoosCollection.BADGES.ElementAt(tattooIndex);
                KeyValuePair<string, string> tat = new KeyValuePair<string, string>(Tattoo.collectionName, Tattoo.name);
                if (currentCharacter.Tattoos.BadgeTattoos.Contains(tat))
                {
                    Subtitle.Custom($"Badge #{tattooIndex + 1} has been ~r~removed~s~.");
                    currentCharacter.Tattoos.BadgeTattoos.Remove(tat);
                }
                else
                {
                    Subtitle.Custom($"Badge #{tattooIndex + 1} has been ~g~added~s~.");
                    currentCharacter.Tattoos.BadgeTattoos.Add(tat);
                }
            }

            ApplySavedTattoos(Cache.Character);
        }

        void CreateListsIfNull(CuriosityCharacter currentCharacter)
        {
            if (currentCharacter.Tattoos.HeadTattoos == null)
            {
                currentCharacter.Tattoos.HeadTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.TorsoTattoos == null)
            {
                currentCharacter.Tattoos.TorsoTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.LeftArmTattoos == null)
            {
                currentCharacter.Tattoos.LeftArmTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.RightArmTattoos == null)
            {
                currentCharacter.Tattoos.RightArmTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.LeftLegTattoos == null)
            {
                currentCharacter.Tattoos.LeftLegTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.RightLegTattoos == null)
            {
                currentCharacter.Tattoos.RightLegTattoos = new List<KeyValuePair<string, string>>();
            }
            if (currentCharacter.Tattoos.BadgeTattoos == null)
            {
                currentCharacter.Tattoos.BadgeTattoos = new List<KeyValuePair<string, string>>();
            }
        }

        void ApplySavedTattoos(CuriosityCharacter currentCharacter)
        {
            // remove all decorations, and then manually re-add them all. what a retarded way of doing this R*....
            ClearPedDecorations(Game.PlayerPed.Handle);

            foreach (var tattoo in currentCharacter.Tattoos.HeadTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.TorsoTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.LeftArmTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.RightArmTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.LeftLegTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.RightLegTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in currentCharacter.Tattoos.BadgeTattoos)
            {
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }

            if (!string.IsNullOrEmpty(currentCharacter.Appearance.HairOverlay.Key) && !string.IsNullOrEmpty(currentCharacter.Appearance.HairOverlay.Value))
            {
                // reset hair value
                SetPedDecoration(Game.PlayerPed.Handle, (uint)GetHashKey(currentCharacter.Appearance.HairOverlay.Key), (uint)GetHashKey(currentCharacter.Appearance.HairOverlay.Value));
            }
        }
    }
}
