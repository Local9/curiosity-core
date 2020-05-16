using Atlas.Roleplay.Client.Environment.Entities;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment.Stores.Impl
{
    public class Masks : Store
    {
        public const int MaskPrice = 300;
        public static Position[] Stores { get; set; } = { new Position(-1337.668f, -1277.599f, 3.9f, 355.9042f) };

        public override void Load()
        {
            foreach (var store in Stores)
            {
                new BlipInfo
                {
                    Name = "Maskaffär",
                    Sprite = 362,
                    Color = 2,
                    Position = store
                }.Commit();

                var marker = new Marker(store)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att kolla på alla masker",
                    Scale = 2f,
                    Color = Color.Transparent,
                    Condition = self => InterfaceManager.GetModule().MenuContext == null
                };

                marker.Show();
                marker.Callback += () =>
                {
                    StyleManager.GetModule().OpenStyleChange(Cache.Character.Style, "General", 0, type =>
                    {
                        if (type != 0) return;

                        var player = Cache.Player;
                        var character = player.Character;
                        var mask = character.Metadata.EquippedMask;

                        if (mask != null && mask.Item1 == character.Style.Mask.Current &&
                            mask.Item2 == character.Style.MaskType.Current) return;

                        new Menu("Vill du köpa denna mask?")
                        {
                            Items = new List<MenuItem>()
                            {
                                new MenuItem("option_yes", $"Ja (SEK {MaskPrice})"),
                                new MenuItem("option_no", "Nej")
                            },
                            Callback = async (menu, item, operation) =>
                            {
                                if (operation.Type == MenuOperationType.Select)
                                {
                                    if (item.Seed == "option_no")
                                    {
                                        await ResetMask(player, character);
                                    }
                                    else
                                    {
                                        if (character.Cash >= MaskPrice)
                                        {
                                            character.Cash -= MaskPrice;
                                            character.Metadata.EquippedMask = new Tuple<int, int>(
                                                character.Style.Mask.Current,
                                                character.Style.MaskType.Current);

                                            player.ShowNotification($"Du köpte en mask för SEK {MaskPrice}.");
                                        }
                                        else
                                        {
                                            await ResetMask(player, character);

                                            player.ShowNotification(
                                                $"Du har inte råd med en mask. (SEK {MaskPrice})");
                                        }
                                    }

                                    menu.Hide();
                                }
                                else if (operation.Type == MenuOperationType.Close)
                                {
                                    operation.Cancel();
                                }
                            }
                        }.Commit();
                    }, "Mask");
                };
            }
        }

        public async Task ResetMask(AtlasPlayer player, AtlasCharacter character)
        {
            var equipped = character.Metadata.EquippedMask;

            character.Style.Mask.Current = equipped?.Item1 ?? 0;
            character.Style.MaskType.Current = equipped?.Item2 ?? 0;

            await character.Style.Commit(player);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Game.IsControlPressed(0, Control.Sprint) && Game.IsControlJustPressed(0, Control.InteractionMenu))
            {
                var player = Cache.Player;
                var character = player.Character;

                if (character.Style.Mask.Current == 0)
                {
                    await ResetMask(player, character);
                }
                else
                {
                    character.Style.Mask.Current = 0;
                    character.Style.MaskType.Current = 0;

                    await character.Style.Commit(player);
                }
            }

            await Task.FromResult(0);
        }
    }
}