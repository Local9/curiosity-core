using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] protected Player Player => Game.Player;
        [JsonIgnore] public CuriosityUser User { get; set; }
        [JsonIgnore] public CuriosityEntity Entity { get; set; }
        [JsonIgnore] public CuriosityCharacter Character { get; set; }
    }
}
