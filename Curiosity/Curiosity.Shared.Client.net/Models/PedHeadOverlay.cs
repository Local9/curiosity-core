using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Shared.Client.net.Models
{
    public class PedHeadOverlay
    {
        public int ID;
        public string Name;
        public int maxOption;
        public int colorType = 0;
        public List<int> OptionValues { get { var list = Enumerable.Range(0, maxOption + 1).ToList(); list.Add(255); return list; } }
        public List<string> OptionNames { get { var list = Enumerable.Range(0, maxOption + 1).Select(i => i.ToString()).ToList(); list.Add("None"); return list; } }
    }
}
