using Curiosity.Callout.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Callout.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}