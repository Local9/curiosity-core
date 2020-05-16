using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Interface.Impl;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Library.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Atlas.Roleplay.Client.Environment.Stores.Impl
{
    public class Clothing : Store
    {
        public const int Price = 30;

        public static readonly string[] Components =
        {
            "Shirt", "Torso", "Decals", "Body", "Pants", "Shoes", "BodyArmor", "Neck", "Head", "Glasses",
            "EarAccessories", "Bag"
        };

        public Position[] Stores { get; } =
        {
            new Position(72.254f, -1399.102f, 28.376f),
            new Position(-703.776f, -152.258f, 36.415f),
            new Position(-167.863f, -298.969f, 38.733f),
            new Position(428.694f, -800.106f, 28.491f),
            new Position(-829.413f, -1073.710f, 10.328f),
            new Position(-1447.797f, -242.461f, 48.820f),
            new Position(11.632f, 6514.224f, 30.877f),
            new Position(123.646f, -219.440f, 53.557f),
            new Position(1696.291f, 4829.312f, 41.063f),
            new Position(618.093f, 2759.629f, 41.088f),
            new Position(1190.550f, 2713.441f, 37.222f),
            new Position(-1193.429f, -772.262f, 16.324f),
            new Position(-3172.496f, 1048.133f, 19.863f),
            new Position(-1108.441f, 2708.923f, 18.107f)
        };

        public override void Load()
        {
            foreach (var store in Stores)
            {
                new BlipInfo
                {
                    Name = "Klädbutik",
                    Sprite = 73,
                    Color = 42,
                    Position = store
                }.Commit();

                var marker = new Marker(store)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att kolla på kläder",
                    Scale = 2f,
                    Color = Color.Transparent,
                    Condition = self => InterfaceManager.GetModule().MenuContext == null
                };

                marker.Show();
                marker.Callback += OpenClothingStore;
            }
        }

        public void OpenClothingStore()
        {
            new Menu("Klädbutik")
            {
                Items = new List<MenuItem>()
                {
                    new MenuItem("new_outfit", "Ny utstyrelse"),
                    new MenuItem("select_outfit", "Välj en sparad utstyrelse")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    if (item.Seed == "new_outfit") OpenOutfitCreation();
                    else if (item.Seed == "select_outfit") OpenSavedOutfits();
                }
            }.Commit();
        }

        public void OpenSavedOutfits()
        {
            var player = Cache.Player;
            var character = player.Character;
            var outfits = new List<MenuItem>();

            foreach (var outfit in character.Metadata.SavedOutfits)
            {
                outfits.Add(new MenuItem($"outfit_{outfit.Key}", outfit.Key));
            }

            new Menu("Dina utsyrelser")
            {
                Items = outfits,
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.Select)
                    {
                        OpenOutfitMenu(item.Seed.Replace("outfit_", ""));
                    }
                    else if (operation.Type == MenuOperationType.PostClose)
                    {
                        OpenClothingStore();
                    }
                }
            }.Commit();
        }

        public void OpenOutfitMenu(string outfitName)
        {
            var player = Cache.Player;
            var character = player.Character;
            var outfit = character.Metadata.SavedOutfits.FirstOrDefault(self =>
                string.Equals(self.Key, outfitName, StringComparison.CurrentCultureIgnoreCase));

            new Menu(outfit.Key)
            {
                Items = new List<MenuItem>()
                {
                    new MenuItem("select", "Sätt på utstyrsel"),
                    new MenuItem("remove", "Ta bort utstyrsel")
                },
                Callback = async (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.Select)
                    {
                        if (item.Seed == "select")
                        {
                            await player.Entity.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                                .Select("oddjobs@basejump@ig_15", "puton_parachute")
                            );

                            var merged = player.Character.Style.Merge(outfit.Value, Components);

                            await merged.Commit(player);
                        }
                        else if (item.Seed == "remove")
                        {
                            character.Metadata.SavedOutfits.Remove(outfit.Key);

                            OpenSavedOutfits();
                        }
                    }
                    else if (operation.Type == MenuOperationType.PostClose)
                    {
                        OpenSavedOutfits();
                    }
                }
            }.Commit();
        }

        public void OpenOutfitCreation()
        {
            var original = Cache.Character.Style;
            var outfit = Clipboard.Process(original);

            StyleManager.GetModule().OpenStyleChange(original, "General", 0, type =>
            {
                if (type != 0) return;

                var player = Cache.Player;
                var character = player.Character;
                var differences = character.Style.Differ(outfit);
                var price = Price * differences;

                new Menu("Vill du köpa denna utstyrsel?")
                {
                    Items = new List<MenuItem>()
                    {
                        new MenuItem("option_yes", $"Ja (SEK {price})"),
                        new MenuItem("option_no", "Nej")
                    },
                    Callback = async (menu, item, operation) =>
                    {
                        if (operation.Type == MenuOperationType.Select)
                        {
                            if (item.Seed == "option_no")
                            {
                                menu.Hide();

                                await outfit.Commit(player);
                            }
                            else
                            {
                                if (character.Cash >= price)
                                {
                                    new Menu("Välj ett namn för utstyrseln")
                                    {
                                        Profile = new MenuProfileDialog(
                                            $"Utstyrelse #{(character.Metadata.SavedOutfits.Count + 1).ToString()}"),
                                        Callback = (_menu, _item, _operation) =>
                                        {
                                            if (_operation.Type != MenuOperationType.Select) return;

                                            var name = ((MenuProfileDialog)_menu.Profile).Value;

                                            if (!character.Metadata.SavedOutfits.ContainsKey(name))
                                            {
                                                character.Cash -= price;
                                                character.Metadata.SavedOutfits.Add(name,
                                                    Clipboard.Process(character.Style));

                                                player.ShowNotification(
                                                    $"Du köpte en utstyrsel för SEK {price} ({name}).");
                                            }
                                            else
                                            {
                                                player.ShowNotification($"Du har redan en utstyrsel som heter: {name}");
                                            }

                                            _menu.Hide();
                                        }
                                    }.Commit();
                                }
                                else
                                {
                                    menu.Hide();

                                    await outfit.Commit(player);

                                    player.ShowNotification(
                                        $"Du har inte råd med denna utstyrsel (SEK {price}).");
                                }
                            }
                        }
                        else if (operation.Type == MenuOperationType.Close)
                        {
                            operation.Cancel();
                        }
                    }
                }.Commit();
            }, Components);
        }
    }
}