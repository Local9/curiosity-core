using CitizenFX.Core;
using Curiosity.Police.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Police.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, List<string> arguments);
    }
}