using System.Collections.Generic;
using Atlas.Roleplay.Client.Environment.Entities;

namespace Atlas.Roleplay.Client.Commands
{
    public interface ICommand
    {
        void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments);
    }
}