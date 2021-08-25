using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Environment.Entities.Models
{
    public class BlipData : LocationBlip
    {
        public List<Blip> Blips = new List<Blip>();

        public void Create()
        {
            foreach(Position position in Positions)
            {
                Blip blip = World.CreateBlip(new Vector3(position.X, position.Y, position.Z));

                // Logger.Debug(Name);

                blip.Name = Name;
                blip.Sprite = (BlipSprite)Sprite;
                blip.Color = (BlipColor)Color;
                blip.IsShortRange = IsShortRange;
                blip.Priority = Priority;
                API.SetBlipShrink(blip.Handle, true);

                string key = Name.Trim().Replace(" ", "");
                API.AddTextEntry(key, Name);
                API.SetBlipCategory(blip.Handle, Category);

                API.AddTextComponentSubstringBlipName(blip.Handle);
                API.BeginTextCommandSetBlipName(key);
                API.EndTextCommandSetBlipName(blip.Handle);

                Blips.Add(blip);
            }
        }
    }
}
