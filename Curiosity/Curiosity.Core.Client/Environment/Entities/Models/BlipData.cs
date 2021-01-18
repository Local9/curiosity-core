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
                blip.Name = Name;
                blip.Sprite = (BlipSprite)Sprite;
                blip.Color = (BlipColor)Color;
                blip.IsShortRange = IsShortRange;
                blip.Priority = Priority;

                string key = Name.Trim().Replace(" ", "");
                API.AddTextEntry(key, Name);
                API.SetBlipCategory(blip.Handle, Category);

                Blips.Add(blip);
            }
        }
    }
}
