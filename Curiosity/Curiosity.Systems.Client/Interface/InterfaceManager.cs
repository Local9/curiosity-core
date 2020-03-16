using Curiosity.Systems.Client.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Interface
{
    public class InterfaceManager : Manager<InterfaceManager>
    {
        public bool IsMenuOpen { get; set; } = false;

        public override void Begin()
        {

        }
    }
}
