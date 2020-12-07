using Curiosity.MissionManager.Client.Environment.Entities;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Commands
{
    public interface ICommand
    {
        void OnAsync(CuriosityPlayer player, CuriosityEntity entity, List<string> arguments);
    }
}