using System.Collections.Generic;
using Curiosity.System.Client.Environment.Entities;

namespace Curiosity.System.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}