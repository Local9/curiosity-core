using Curiosity.Systems.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Systems.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}