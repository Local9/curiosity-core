using System;
using System.Collections.Generic;
using System.Linq;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Interface.Impl;
using Atlas.Roleplay.Library.Models;

namespace Atlas.Roleplay.Client.Managers
{
    public class StyleManager : Manager<StyleManager>
    {
        public List<CachedStyle> Cached { get; set; } = new List<CachedStyle>();
        public Metadata Context { get; set; } = new Metadata();

        public void Cache(Style style)
        {
            var properties = typeof(Style).GetProperties();
            var group = new List<CachedStyleComponent>();

            foreach (var property in properties)
            {
                var first = group.FirstOrDefault();
                var name = property.Name.ToLower();

                if (first != null && !name.StartsWith(first.Seed))
                {
                    Cached.Add(new CachedStyle
                    {
                        Seed = first.Seed,
                        Label = first.Component.Label,
                        Group = group.Select(self => (CachedStyleComponent) self.Clone()).ToList()
                    });

                    group.Clear();
                }

                group.Add(new CachedStyleComponent
                {
                    Seed = name,
                    Component = style.GetByName(name)
                });
            }

            Cached.Add(new CachedStyle
            {
                Seed = group.FirstOrDefault()?.Seed,
                Label = group.FirstOrDefault()?.Component.Label,
                Group = group.Select(self => (CachedStyleComponent) self.Clone()).ToList()
            });

            Logger.Info($"[Style] Successfully cached {properties.Length} style component(s)");
        }

        public void OpenStyleChange(Style style, string category, int index, Action<int> callback,
            params string[] whitelist)
        {
            var menu = new Menu("Utseende")
            {
                ItemIndex = index
            };
            var root = category.Length < 1 || category == "General" || category == "All";

            if (root)
            {
                foreach (var cache in Cached)
                {
                    if (!whitelist.Select(self => self.ToLower()).Contains(cache.Seed) &&
                        !whitelist.Select(self => self.ToLower()).Contains("all")) continue;

                    menu.Items.Add(new MenuItem($"style_component_{cache.Seed}", cache.Label));
                }

                if (whitelist.Contains("CHAR_CREATE")) menu.Items.Add(new MenuItem("style_confirmation", "[Bekräfta]"));

                // ReSharper disable once ImplicitlyCapturedClosure
                menu.Callback = (self, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.Select)
                    {
                        if (item.Seed != "style_confirmation")
                        {
                            Context.Datapack.Clear();
                            Context.Write(0, menu.ItemIndex);

                            OpenStyleChange(style, item.Seed.Replace("style_component_", ""), 0, callback, whitelist);
                        }
                        else
                        {
                            menu.Hide();
                        }
                    }
                    else if (operation.Type == MenuOperationType.Close && whitelist.Contains("CHAR_CREATE"))
                    {
                        operation.Cancel();
                    }
                    else if (operation.Type == MenuOperationType.PostClose)
                    {
                        Context.Datapack.Clear();

                        callback(0);
                    }
                    else if (operation.Type == MenuOperationType.Update)
                    {
                        callback(1);
                    }
                };
            }
            else
            {
                foreach (var cache in Cached)
                {
                    if (cache.Seed != category) continue;

                    foreach (var entry in cache.Group)
                    {
                        var component = style.GetByName(entry.Seed.Replace("style_component_", ""));

                        if (component != null)
                            menu.Items.Add(new MenuItem($"style_component_{entry.Seed}", component.Label)
                            {
                                Profile = new MenuProfileSlider
                                {
                                    Minimum = component.Minimum,
                                    Current = component.Current,
                                    Maximum = component.Maximum
                                }
                            });
                    }
                }

                menu.Callback = async (self, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.SliderUpdate)
                    {
                        style.GetByName(item.Seed.Replace("style_component_", "")).Current =
                            ((MenuProfileSlider) item.Profile).Current;

                        await style.Commit(Atlas.Local);

                        OpenStyleChange(style, category, menu.ItemIndex, callback, whitelist);
                    }
                    else if (operation.Type == MenuOperationType.Close)
                    {
                        operation.Cancel();

                        OpenStyleChange(style, "General", Context.Find<int>(0), callback, whitelist);
                    }
                };
            }

            menu.Commit();
        }
    }

    public class CachedStyle
    {
        public string Seed { get; set; }
        public string Label { get; set; }
        public List<CachedStyleComponent> Group { get; set; } = new List<CachedStyleComponent>();
    }

    public class CachedStyleComponent : ICloneable
    {
        public string Seed { get; set; }
        public StyleComponent Component { get; set; }

        public object Clone()
        {
            return new CachedStyleComponent
            {
                Seed = Seed,
                Component = Component
            };
        }
    }
}