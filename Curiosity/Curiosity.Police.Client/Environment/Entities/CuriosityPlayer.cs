using CitizenFX.Core;
using Curiosity.Police.Client.Environment.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] public Player Player => Game.Player;
        [JsonIgnore] public Ped Ped => Player.Character;
        [JsonIgnore] public int LocalHandle => Game.Player.Handle;

        public string GetHeadingDirection()
        {
            foreach (KeyValuePair<int, string> kvp in CompassDirections.Direction)
            {
                float vehDirection = Ped.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }
    }
}
