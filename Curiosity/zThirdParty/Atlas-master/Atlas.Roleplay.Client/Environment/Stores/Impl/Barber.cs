using System.Collections.Generic;
using System.Drawing;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Models;
using Atlas.Roleplay.Library.Utilities;

namespace Atlas.Roleplay.Client.Environment.Stores.Impl
{
    public class Barber : Store
    {
        public const int Price = 40;

        public static readonly string[] Components =
        {
            "Hair", "Eyebrows", "Beard", "ChestHair", "Blush", "Makeup", "Lipstick"
        };

        public Position[] Stores { get; } =
        {
            new Position(-814.308f, -183.823f, 36.568f),
            new Position(136.826f, -1708.373f, 28.291f),
            new Position(-1282.604f, -1116.757f, 5.990f),
            new Position(1931.513f, 3729.671f, 31.844f),
            new Position(1212.840f, -472.921f, 65.208f),
            new Position(-32.885f, -152.319f, 56.076f),
            new Position(-278.077f, 6228.463f, 30.695f)
        };

        public override void Load()
        {
            foreach (var store in Stores)
            {
                new BlipInfo
                {
                    Name = "Frisör",
                    Sprite = 71,
                    Color = 4,
                    Position = store
                }.Commit();

                var marker = new Marker(store)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att klippa dig",
                    Scale = 2f,
                    Color = Color.Transparent,
                    Condition = self => InterfaceManager.GetModule().MenuContext == null
                };

                marker.Show();
                marker.Callback += OpenBarberShop;
            }
        }

        public void OpenBarberShop()
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

                new Menu("Vill du köpa alla ändringar?")
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
                            menu.Hide();

                            if (item.Seed == "option_no")
                            {
                                await outfit.Commit(player);
                            }
                            else
                            {
                                if (character.Cash >= price)
                                {
                                    character.Cash -= price;

                                    player.ShowNotification(
                                        $"Du köpte ändringar för SEK {price}.");
                                }
                                else
                                {
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