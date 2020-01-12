using System;

namespace Curiosity.System.Client.Commands
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