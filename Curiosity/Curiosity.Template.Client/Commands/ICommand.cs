using Curiosity.Template.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.Template.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}