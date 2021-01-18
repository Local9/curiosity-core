using CitizenFX.Core;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Client.Managers
{
    public class BlipManager : Manager<BlipManager>
    {
        public static BlipManager ManagerInstance;
        public Dictionary<string, BlipData> AllBlips = new Dictionary<string, BlipData>(); // All registered blips
        public Dictionary<string, BlipData> CurrentBlips = new Dictionary<string, BlipData>(); // Currently visible blips

        public override void Begin()
        {
            ManagerInstance = this;
        }

        public Dictionary<string, List<Position>> Locations = new Dictionary<string, List<Position>>();

        public void RemoveAllBlips()
        {
            foreach (KeyValuePair<string, BlipData> kvp in AllBlips)
            {
                BlipData blipData = kvp.Value;
                blipData.Blips.ForEach(x =>
                {
                    if (x.Exists())
                        x.Delete();
                });
            }

            AllBlips.Clear();
        }

        public void RemoveAllBlips(string blipName)
        {
            BlipData blipData = AllBlips.Where(b => b.Key == blipName).Select(b => b.Value).FirstOrDefault();

            if (blipData != null)
            {
                blipData.Blips.ForEach(x =>
                {
                    if (x.Exists())
                        x.Delete();
                });
            }

            AllBlips.Remove(blipName);
        }

        public string AddBlip(BlipData blipData)
        {
            if (AllBlips.ContainsKey(blipData.Name)) return blipData.Name;
            blipData.Create();
            AllBlips.Add(blipData.Name, blipData);

            if (Locations.ContainsKey(blipData.Name))
            {
                Locations[blipData.Name] = blipData.Positions;
            }
            else
            {
                Locations.Add(blipData.Name, blipData.Positions);
            }

            return blipData.Name;
        }
    }
}
