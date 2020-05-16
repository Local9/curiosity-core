using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Atlas.Roleplay.Client.Inventory
{
    public class InventoryContainer : InventoryContainerBase
    {
        public event Action<InventoryItem, InventoryUpdateType> Update;
        public event Action Registration;
        [JsonIgnore] public Predicate<InventoryManager> Visibility { get; set; } = manager => true;
        public bool IsVisible => Visibility.Invoke(InventoryManager.GetModule());

        public InventoryContainer(InventoryContainerBase inventoryBase)
        {
            Seed = inventoryBase.Seed;
            Name = inventoryBase.Name;
            SlotAmount = inventoryBase.SlotAmount;
            Slots = inventoryBase.Slots ?? new InventoryItem[SlotAmount];
        }

        public void CallUpdate(InventoryItem item, InventoryUpdateType type)
        {
            if (Update?.GetInvocationList() == null) return;

            foreach (var invocation in Update?.GetInvocationList())
            {
                ((Action<InventoryItem, InventoryUpdateType>)invocation).Invoke(item, type);
            }
        }

        public void CallRegistration()
        {
            if (Registration?.GetInvocationList() == null) return;

            foreach (var invocation in Registration?.GetInvocationList())
            {
                ((Action)invocation).Invoke();
            }
        }

        public void Commit()
        {
            API.SendNuiMessage(new JsonBuilder().Add("Operation", "UPDATE_INVENTORY").Add("Inventory", this).Build());

            InventoryManager.GetModule().UpdateInventory(this);
        }

        public void RefreshItemClassifications()
        {
            var slots = new List<InventoryItem>();

            foreach (var item in Slots)
            {
                if (item != null && item.IsWeapon())
                {
                    slots.Add(JsonConvert.DeserializeObject<WeaponItem>(JsonConvert.SerializeObject(item)));
                }
                else
                {
                    slots.Add(item);
                }
            }

            Slots = slots.ToArray();
        }
    }

    public enum InventoryUpdateType
    {
        Remove,
        Add
    }
}