using Curiosity.System.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.System.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}