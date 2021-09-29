using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Racing.Client.Environment.Entities.Models;
using Curiosity.Racing.Client.Interface.Modules;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Curiosity.Racing.Client.Environment.Entities
{
    public class CuriosityPlayer
    {
        [JsonIgnore] protected Player Player => Game.Player;
        [JsonIgnore] public int LocalHandle => Game.Player.Handle;
    }
}
