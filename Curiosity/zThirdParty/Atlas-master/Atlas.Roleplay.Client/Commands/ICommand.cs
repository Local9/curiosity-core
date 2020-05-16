using Atlas.Roleplay.Client.Environment.Entities;
using System.Collections.Generic;

namespace Atlas.Roleplay.Client.Commands
{
    public interface ICommand
    {
        void On(AtlasPlayer player, AtlasEntity entity, List<string> arguments);
    }
}