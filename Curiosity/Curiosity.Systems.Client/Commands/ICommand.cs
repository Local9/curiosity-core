using System.Collections.Generic;
using Curiosity.Systems.Client.Environment.Entities;

namespace Curiosity.Systems.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}