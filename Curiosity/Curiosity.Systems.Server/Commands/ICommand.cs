using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Systems.Server.Commands
{
    public interface ICommand
    {
        void On(CuriosityUser curiosityUser, Player player, List<string> arguments);
    }
}