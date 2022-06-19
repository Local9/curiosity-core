using Curiosity.Core.Client.Environment.Entities.Models;
using NativeUI;
using System.Linq;

namespace Curiosity.Core.Client.Interface.Menus.Creator
{
    class Parent
    {
        public string Name;
        public int ParentId;
        public bool IsFather;

        public Parent(string name, int id, bool isFather)
        {
            Name = name;
            ParentId = id;
            IsFather = isFather;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    class CharacterHeritage
    {
        private UIMenuHeritageWindow HeritageWindow;
        private UIMenuListItem Mothers;
        private UIMenuListItem Fathers;
        private UIMenuSliderHeritageItem Resemblance;
        private UIMenuSliderHeritageItem SkinTone;

        private int MotherId = 0;
        private int FatherId = 0;

        private int MenuMotherIndex = 0;
        private int MenuFatherIndex = 0;

        private float ResembalanceBlend = .5f;
        private float SkinToneBlend = .5f;

        List<dynamic> Parents = new List<dynamic>() {
            new Parent("Benjamin", 0, true),
            new Parent("Daniel", 1, true),
            new Parent("Joshua", 2, true),
            new Parent("Noah", 3, true),
            new Parent("Andrew", 4, true),
            new Parent("Joan", 5, true),
            new Parent("Alex", 6, true),
            new Parent("Isaac", 7, true),
            new Parent("Evan", 8, true),
            new Parent("Ethan", 9, true),
            new Parent("Vincent", 10, true),
            new Parent("Angel", 11, true),
            new Parent("Diego", 12, true),
            new Parent("Adrian", 13, true),
            new Parent("Gabriel", 14, true),
            new Parent("Michael", 15, true),
            new Parent("Santiago", 16, true),
            new Parent("Kevin", 17, true),
            new Parent("Louis", 18, true),
            new Parent("Samuel", 19, true),
            new Parent("Anthony", 20, true),
            new Parent("Claude", 42, true),
            new Parent("Niko", 43, true),
            new Parent("John", 44, true),
            new Parent("Hannah", 21, false),
            new Parent("Audrey", 22, false),
            new Parent("Jasmine", 23, false),
            new Parent("Giselle", 24, false),
            new Parent("Amelia", 25, false),
            new Parent("Isabella", 26, false),
            new Parent("Zoe", 27, false),
            new Parent("Ava", 28, false),
            new Parent("Camilla", 29, false),
            new Parent("Violet", 30, false),
            new Parent("Sophia", 31, false),
            new Parent("Eveline", 32, false),
            new Parent("Nicole", 33, false),
            new Parent("Ashley", 34, false),
            new Parent("Grace", 35, false),
            new Parent("Brianna", 36, false),
            new Parent("Natalie", 37, false),
            new Parent("Olivia", 38, false),
            new Parent("Elizabeth", 39, false),
            new Parent("Charlotte", 40, false),
            new Parent("Emma", 41, false),
            new Parent("Misty", 45, false),
        };

        List<dynamic> MothersList => Parents.Where(x => !x.IsFather).ToList();
        List<dynamic> FathersList => Parents.Where(x => x.IsFather).ToList();

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.OnListChange += Menu_OnListChange;
            menu.OnSliderChange += Menu_OnSliderChange;
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;

            if (Cache.Character is not null)
            {
                if (Cache.Character.Heritage is not null)
                {
                    for (int i = 0; i < FathersList.Count; i++)
                    {
                        Parent parent = FathersList[i];
                        if (parent.ParentId == Cache.Character.Heritage.FatherId)
                        {
                            MenuFatherIndex = i;
                            break;
                        }
                    }

                    for (int i = 0; i < MothersList.Count; i++)
                    {
                        Parent parent = MothersList[i];
                        if (parent.ParentId == Cache.Character.Heritage.MotherId)
                        {
                            MenuMotherIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    MenuMotherIndex = PluginManager.Rand.Next(MothersList.Count);
                    MenuFatherIndex = PluginManager.Rand.Next(FathersList.Count);
                }
            }
            else
            {
                MenuMotherIndex = PluginManager.Rand.Next(MothersList.Count);
                MenuFatherIndex = PluginManager.Rand.Next(FathersList.Count);
            }

            HeritageWindow = new UIMenuHeritageWindow(MenuMotherIndex, MenuFatherIndex);
            menu.AddWindow(HeritageWindow);

            Mothers = new UIMenuListItem("Mother", MothersList, MenuMotherIndex);
            Fathers = new UIMenuListItem("Father", FathersList, MenuFatherIndex);

            Resemblance = new UIMenuSliderHeritageItem("Resemblance", "", true);
            Resemblance.Value = PluginManager.Rand.Next(100);
            SkinTone = new UIMenuSliderHeritageItem("Skin Tone", "", true);
            SkinTone.Value = PluginManager.Rand.Next(100);

            ResembalanceBlend = (100 - Resemblance.Value) / 100f;
            SkinToneBlend = (100 - SkinTone.Value) / 100f;

            UpdatePedBlendData(true);

            menu.AddItem(Mothers);
            menu.AddItem(Fathers);
            menu.AddItem(Resemblance);
            menu.AddItem(SkinTone);

            return menu;
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.ChangeBackward)
                OnMenuClose(oldMenu);

            if (state == MenuState.ChangeForward)
                OnMenuOpen(newMenu);
        }

        private async void OnMenuClose(UIMenu menu)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[2], CreatorMenus.CameraViews[1], 500)
            );
            PluginManager.Instance.DetachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();
        }

        private async void OnMenuOpen(UIMenu menu)
        {
            Cache.Player.CameraQueue.Reset();
            await Cache.Player.CameraQueue.View(new CameraBuilder()
                .SkipTask()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CreatorMenus.CameraViews[1], CreatorMenus.CameraViews[2], 500)
            );
            PluginManager.Instance.AttachTickHandler(OnPlayerControls);

            menu.InstructionalButtons.Clear();

            menu.InstructionalButtons.Add(CreatorMenus.btnRandom);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateLeft);
            menu.InstructionalButtons.Add(CreatorMenus.btnRotateRight);
        }

        private async Task OnPlayerControls()
        {
            CreatorMenus._MenuPool.ProcessMouse();

            if (Game.IsControlPressed(0, Control.Pickup))
            {
                Cache.PlayerPed.Heading += 10f;
            }

            if (Game.IsControlPressed(0, Control.Cover))
            {
                Cache.PlayerPed.Heading -= 10f;
            }

            if (Game.IsControlJustPressed(0, Control.Jump))
            {
                Randomise();
            }
        }

        private void Randomise()
        {
            MenuMotherIndex = PluginManager.Rand.Next(MothersList.Count);
            MenuFatherIndex = PluginManager.Rand.Next(FathersList.Count);

            MotherId = MenuMotherIndex;
            FatherId = MenuFatherIndex;

            Mothers.Index = MenuMotherIndex;
            Fathers.Index = MenuFatherIndex;

            Resemblance.Value = PluginManager.Rand.Next(100);
            SkinTone.Value = PluginManager.Rand.Next(100);

            ResembalanceBlend = (100 - Resemblance.Value) / 100f;
            SkinToneBlend = (100 - SkinTone.Value) / 100f;

            HeritageWindow.Index(MenuMotherIndex, MenuFatherIndex);
            UpdatePedBlendData();
        }

        private void Menu_OnSliderChange(UIMenu sender, UIMenuSliderItem listItem, int newIndex)
        {
            if (listItem == Resemblance)
                ResembalanceBlend = (100 - newIndex) / 100f;

            if (listItem == SkinTone)
                SkinToneBlend = (100 - newIndex) / 100f;

            UpdatePedBlendData();
        }

        private void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            Parent parent = (Parent)listItem.Items[newIndex];

            if (listItem == Mothers)
            {
                MotherId = parent.ParentId;
                MenuMotherIndex = newIndex;
            }

            if (listItem == Fathers)
            {
                FatherId = parent.ParentId;
                MenuFatherIndex = newIndex;
            }

            HeritageWindow.Index(MenuMotherIndex, MenuFatherIndex);
            UpdatePedBlendData();
        }

        public void UpdatePedBlendData(bool checkExisting = false)
        {
            // Logger.Debug($"[UpdatePedBlendData] fa/ma {FatherApperance}/{MotherApperance} | fs/ms {FatherSkin}/{MotherSkin} | ab/sb {ApperanceBlend}/{SkinBlend}");
            if (checkExisting)
            {
                FatherId = Cache.Character.Heritage.FatherId;
                MotherId = Cache.Character.Heritage.MotherId;
                ResembalanceBlend = Cache.Character.Heritage.BlendApperance;
                SkinToneBlend = Cache.Character.Heritage.BlendSkin;
            }

            Cache.Character.Heritage.UpdateBlendData(FatherId, MotherId, ResembalanceBlend, SkinToneBlend);
            API.SetPedHeadBlendData(Cache.Entity.Id, MotherId, FatherId, 0, MotherId, FatherId, 0, ResembalanceBlend, SkinToneBlend, 0f, false);
        }
    }
}
