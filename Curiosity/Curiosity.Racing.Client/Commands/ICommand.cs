using Curiosity.Racing.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Racing.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}