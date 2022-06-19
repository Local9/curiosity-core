using Curiosity.Core.Client.Environment.Entities;

namespace Curiosity.Core.Client.Commands
{
    public interface ICommand
    {
        void On(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}