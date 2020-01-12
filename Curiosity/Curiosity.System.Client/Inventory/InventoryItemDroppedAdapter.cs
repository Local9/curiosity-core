using System;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.System.Client.Environment;
using Curiosity.System.Client.Extensions;
using Curiosity.System.Client.Inventory.Impl;
using Curiosity.System.Library.Inventory;
using Curiosity.System.Library.Models;
using CitizenFX.Core;

namespace Curiosity.System.Client.Inventory
{
    public class InventoryItemDroppedAdapter
    {
        public InventoryItem Item { get; set; }
        public Position Position { get; set; }
        public bool IsActivated { get; set; }
        public bool IsAccessible { get; set; }

        public InventoryItemDroppedAdapter(InventoryItem item, Position position, bool activate = true)
        {
            Item = item;
            Position = position;

            if (activate) Activate();
        }

        public void Activate()
        {
            CuriosityPlugin.Instance.AttachTickHandler(OnTick);
            CuriosityPlugin.Instance.AttachTickHandler(OnSecondaryTick);

            IsActivated = true;
        }

        public void Destroy()
        {
            CuriosityPlugin.Instance.DetachTickHandler(OnTick);
            CuriosityPlugin.Instance.DetachTickHandler(OnSecondaryTick);

            IsActivated = false;
        }

        private async Task OnTick()
        {
            var position = Cache.Player.Entity.Position;
            var distance = position.Distance(Position, true);

            if (distance < 5.0)
            {
                WorldText.Draw($"{(Item.IsWeapon() ? "Vapen" : "Föremål")}: {Item.Label}", 1f, Position);

                IsAccessible = true;

                await Task.FromResult(0);
            }
            else
            {
                IsAccessible = false;

                await BaseScript.Delay(Convert.ToInt32(distance * 2));
            }
        }

        private async Task OnSecondaryTick()
        {
            if (((ProximityInventory) InventoryManager.GetModule().GetContainer("proximity_inventory"))
                .DroppedWorldItems.FirstOrDefault(self => self.Item.Seed == Item.Seed) == null)
            {
                Destroy();
            }
            
            await BaseScript.Delay(500);
        }
    }
}