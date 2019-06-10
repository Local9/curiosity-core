using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Player
{
    class Sit
    {
        static float MaxDistance = 1.5f;

        public static void Init()
        {
            Helpers.Dictionary.PlayerInteractables.SetupInteractables();
            Helpers.Dictionary.PlayerInteractables.SetupSitableItems();


        }
    }
}
