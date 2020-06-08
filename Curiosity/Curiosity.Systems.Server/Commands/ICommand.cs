using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Systems.Server.Commands
{
    public interface ICommand
    {
        void On(CuriosityUser player, int entityHandle, List<string> arguments);
    }
}