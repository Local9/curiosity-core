using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobLockerRoomProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; }
        public Position Position { get; set; }

        public Dictionary<string, Tuple<string, Tuple<string, string>>> ClothingOptions { get; set; } =
            new Dictionary<string, Tuple<string, Tuple<string, string>>>();

        public string[] ClothingComponents { get; set; } = new string[0];
        public Action<string, string, Style> Callback { get; set; }

        public override async void Begin(Job job)
        {
            await Session.Loading();

            var character = Cache.Character;
            var marker = new Marker(Position)
            {
                Message = "Tryck ~INPUT_CONTEXT~ för att byta om",
                Scale = 3f,
                Color = Color.FromArgb(0, 0, 0, 0),
                Condition = self => character.Metadata.Employment == job.Attachment
            };

            var elements = new List<MenuItem>();

            foreach (var option in ClothingOptions)
            {
                elements.Add(new MenuItem(option.Key, option.Value.Item1));
            }

            marker.Callback += () =>
            {
                new Menu("Omklädningsrum")
                {
                    Items = elements,
                    Callback = async (menu, item, operation) =>
                    {
                        if (operation.Type != MenuOperationType.Select) return;

                        var player = Cache.Player;
                        var option = ClothingOptions.FirstOrDefault(self => self.Key == item.Seed);
                        var style = character.Style;
                        var name = style.Sex.Current == 0 ? option.Value.Item2.Item1 : option.Value.Item2.Item2;

                        if (name != "CHARACTER_STYLE")
                        {
                            style = style.Merge((Style)Activator.CreateInstance(Type.GetType(name) ?? throw new NullReferenceException($"[Job] [JobLockerRoomProfile] Could not find style class `{name}`.")),
                                ClothingComponents);
                        }

                        await player.Entity.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                            .Select("oddjobs@basejump@ig_15", "puton_parachute")
                        );

                        await style.Commit(player, false);

                        Callback?.Invoke(option.Key, name, style);
                    }
                }.Commit();
            };

            marker.Show();
        }
    }
}