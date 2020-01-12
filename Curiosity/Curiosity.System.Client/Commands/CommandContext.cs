using System.Drawing;

namespace Curiosity.System.Client.Commands
{
    public abstract class CommandContext
    {
        public abstract string[] Aliases { get; set; }
        public abstract string Title { get; set; }
        public abstract Color Color { get; set; }
        public abstract bool IsRestricted { get; set; }
    }
}