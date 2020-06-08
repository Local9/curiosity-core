using System;

namespace Curiosity.Systems.Server.Commands
{
    public class CommandInfo : Attribute
    {
        public string[] Aliases { get; set; }

        public CommandInfo(string[] aliases)
        {
            Aliases = aliases;
        }
    }
}