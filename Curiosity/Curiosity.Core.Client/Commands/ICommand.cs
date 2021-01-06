using Curiosity.Core.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}