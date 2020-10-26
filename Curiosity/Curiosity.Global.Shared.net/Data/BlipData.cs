using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Global.Shared.Data
{
    public class BlipData
    {
        public Blip Blip;
        public BlipCategory Category;
        public bool isEntityBlip;

        // We keep these here as well as we want to be able to allow filtering if blips without requiring
        // hacks like making sprite transparent (although we could, but it doesn't feel like a robust solution)

        // Only one of these three will be used for a blip depending on whether it is an entity or static blip
        private Vector3 Position { get; set; }
        private CitizenFX.Core.Entity Entity { get; set; }

        private BlipSprite Sprite { get; set; }
        private BlipColor Color { get; set; }
        private bool IsShortRange { get; set; }
        private string Name { get; set; }
        public string BlipName { get; set; }

        public BlipData(string blipName, string name, Vector3 position, BlipSprite sprite, BlipCategory category, BlipColor color = BlipColor.White, bool isShortRange = true)
        {
            this.Name = name;
            this.BlipName = blipName;
            this.isEntityBlip = false;
            this.Position = position;
            this.Sprite = sprite;
            this.Color = color;
            this.IsShortRange = isShortRange;
            this.Category = category;
            Create();
        }

        public void Create()
        {
            Blip = World.CreateBlip(new Vector3(Position.X, Position.Y, Position.Z));
            Blip.Name = Name;
            Blip.Sprite = Sprite;
            Blip.Color = Color;
            Blip.IsShortRange = IsShortRange;

            string blipKey = Name.Replace(" ", "");
            API.AddTextEntry(blipKey, Name);
            API.AddTextComponentSubstringBlipName(Blip.Handle);
            API.BeginTextCommandSetBlipName(blipKey);
            API.EndTextCommandSetBlipName(Blip.Handle);
        }
    }

    public static class BlipHandler
    {
        public static Dictionary<string, BlipData> AllBlips = new Dictionary<string, BlipData>(); // All registered blips
        public static Dictionary<string, BlipData> CurrentBlips = new Dictionary<string, BlipData>(); // Currently visible blips

        public static void Init()
        {
            AllBlips.Clear();
            CurrentBlips.Clear();
            // Even if this is empty, it needs to stay to init the method early
        }

        public static string AddBlip(BlipData blip)
        {
            if (AllBlips.ContainsKey(blip.BlipName)) return blip.BlipName;

            AllBlips.Add(blip.BlipName, blip);
            return blip.BlipName;
        }

        public static void RemoveBlip(string id)
        {
            try
            {
                AllBlips.Where(b => b.Key == id).First().Value.Blip.Delete();
                AllBlips.Remove(id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Removing blip {id} threw {ex.GetType().ToString()}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitFilter">Bitwise combination of BlipCategory flags to show</param>
        public static void FilterBlips(BlipCategory bitFilter)
        {
            Dictionary<string, BlipData> New = AllBlips
                .Where(b => ((b.Value.Category & bitFilter) != 0))
                .ToDictionary(b => b.Key, b => b.Value);
            // Delete blips that were filtered out
            CurrentBlips
                .Where(b => New.Where(i => i.Value.Blip.Handle == b.Value.Blip.Handle).Count() == 0)
                .ToDictionary(i => i.Key, i => i.Value)
                .ToList()
                .ForEach(b => { b.Value.Blip.Delete(); });
            // Add blips that are now added
            New
                .Where(b => CurrentBlips.Where(i => i.Value.Blip.Handle == b.Value.Blip.Handle).Count() == 0)
                .ToDictionary(i => i.Key, i => i.Value)
                .ToList()
                .ForEach(b => { b.Value.Create(); });
        }
    }
}
