using Curiosity.Interface.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Interface.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}