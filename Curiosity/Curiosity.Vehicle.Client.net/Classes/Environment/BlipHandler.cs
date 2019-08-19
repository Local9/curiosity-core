using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Vehicle.Client.net.Classes.Environment
{
    internal class BlipData
    {
        internal Blip Blip;
        public BlipCategory Category;
        private bool isEntityBlip;

        // We keep these here as well as we want to be able to allow filtering if blips without requiring
        // hacks like making sprite transparent (although we could, but it doesn't feel like a robust solution)

        // Only one of these three will be used for a blip depending on whether it is an entity or static blip
        private Vector3 Position { get; set; }
        private Entity Entity { get; set; }

        private BlipSprite Sprite { get; set; }
        private BlipColor Color { get; set; }
        private bool IsShortRange { get; set; }
        private string Name { get; set; }
        public int SpawnId { get; set; }

        public BlipData(int spawnId, string name, Vector3 position, BlipSprite sprite, BlipCategory category, BlipColor color = BlipColor.White, bool isShortRange = true)
        {
            this.Name = name;
            this.SpawnId = spawnId;
            this.isEntityBlip = false;
            this.Position = position;
            this.Sprite = sprite;
            this.Color = color;
            this.IsShortRange = isShortRange;
            this.Category = category;
            Create();
        }

        public BlipData(int spawnId, string name, Entity entity, BlipSprite sprite, BlipCategory category, BlipColor color = BlipColor.White, bool isShortRange = true)
        {
            this.Name = name;
            this.SpawnId = spawnId;
            this.isEntityBlip = true;
            this.Entity = entity;
            this.Sprite = sprite;
            this.Color = color;
            this.IsShortRange = isShortRange;
            this.Category = category;
            Create();
        }

        public void Create()
        {
            if (isEntityBlip)
            {
                Blip = new Blip(Function.Call<int>(Hash.ADD_BLIP_FOR_ENTITY, Entity));
            }
            else
            {
                Blip = World.CreateBlip(new Vector3(Position.X, Position.Y, Position.Z));
            }
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

    internal static class BlipHandler
    {
        internal static Dictionary<int, BlipData> All = new Dictionary<int, BlipData>(); // All registered blips
        internal static Dictionary<int, BlipData> Current = new Dictionary<int, BlipData>(); // Currently visible blips

        internal static void Init()
        {
            // Even if this is empty, it needs to stay to init the method early
        }

        internal static int Add(BlipData blip)
        {
            if (All.ContainsKey(blip.SpawnId)) return blip.SpawnId;

            All.Add(blip.SpawnId, blip);
            return blip.SpawnId;
        }

        internal static void Remove(int id)
        {
            try
            {
                All.Where(b => b.Key == id).First().Value.Blip.Delete();
                All.Remove(id);
            }
            catch (Exception ex)
            {
                Log.Warn($"Removing blip {id} threw {ex.GetType().ToString()}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bitFilter">Bitwise combination of BlipCategory flags to show</param>
        internal static void Filter(BlipCategory bitFilter)
        {
            Dictionary<int, BlipData> New = All
                .Where(b => ((b.Value.Category & bitFilter) != 0))
                .ToDictionary(b => b.Key, b => b.Value);
            // Delete blips that were filtered out
            Current
                .Where(b => New.Where(i => i.Value.Blip.Handle == b.Value.Blip.Handle).Count() == 0)
                .ToDictionary(i => i.Key, i => i.Value)
                .ToList()
                .ForEach(b => { b.Value.Blip.Delete(); });
            // Add blips that are now added
            New
                .Where(b => Current.Where(i => i.Value.Blip.Handle == b.Value.Blip.Handle).Count() == 0)
                .ToDictionary(i => i.Key, i => i.Value)
                .ToList()
                .ForEach(b => { b.Value.Create(); });
        }
    }
}
