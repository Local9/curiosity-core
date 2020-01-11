using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Diagnostics;
using Atlas.Roleplay.Client.Environment;
using Atlas.Roleplay.Client.Inventory.Impl;
using Atlas.Roleplay.Client.Inventory.Items;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Atlas.Roleplay.Client.Inventory
{
    public class InventoryManager : Manager<InventoryManager>
    {
        public bool OpenedInventory { get; set; }
        public List<InventoryContainer> Registry { get; set; } = new List<InventoryContainer>();
        public event Action<InventoryStateChange> StateChanged;
        public long SwitchDelay;

        public override void Begin()
        {
            StateChanged += state =>
            {
                Logger.Debug(state == InventoryStateChange.Show
                    ? "[Inventory] Opening inventory..."
                    : "[Inventory] Closed inventory.");
            };
            
            Atlas.AttachNuiHandler("CLOSE_INVENTORY", new EventCallback(metadata =>
            {
                OpenedInventory = false;

                if (StateChanged != null)
                    foreach (var invocation in StateChanged?.GetInvocationList())
                    {
                        ((Action<InventoryStateChange>) invocation).Invoke(InventoryStateChange.Hide);
                    }

                API.SetNuiFocus(false, false);

                return null;
            }));

            Atlas.AttachNuiHandler("INVENTORY_ITEM_CHANGE", new EventCallback(metadata =>
            {
                var source = GetContainer(metadata.Find<string>(0));
                var sourceItem = JsonConvert.DeserializeObject<InventoryItem>(metadata.Find<string>(1));

                source.CallUpdate(source.Slots[sourceItem.Slot], InventoryUpdateType.Remove);
                source.Slots[sourceItem.Slot] = sourceItem;
                source.CallUpdate(sourceItem, InventoryUpdateType.Add);

                var target = GetContainer(metadata.Find<string>(2));
                var targetItem = JsonConvert.DeserializeObject<InventoryItem>(metadata.Find<string>(3));

                target.CallUpdate(target.Slots[targetItem.Slot], InventoryUpdateType.Remove);
                target.Slots[targetItem.Slot] = targetItem;
                target.CallUpdate(targetItem, InventoryUpdateType.Add);

                UpdateInventory(source);
                UpdateInventory(target);

                return null;
            }));
        }

        public InventoryContainer GetContainer(string seed)
        {
            return Registry.FirstOrDefault(self => self.Seed == seed);
        }

        public void UpdateInventory(InventoryContainer container)
        {
            var inventories = Cache.Character.Metadata.Inventories;

            if (inventories.All(self => self.Seed != container.Seed)) return;

            inventories.RemoveAll(self => self.Seed == container.Seed);
            inventories.Add(container);
        }

        public void RegisterContainer(InventoryContainer inventory)
        {
            if (inventory.Slots.Length < inventory.SlotAmount)
            {
                var modified = inventory.Slots.ToList();

                for (var i = inventory.Slots.Length; i < inventory.SlotAmount; i++)
                {
                    modified.Add(new EmptyItem
                    {
                        Slot = i
                    });
                }

                inventory.Slots = modified.ToArray();
            }

            Registry.Add(inventory);

            API.SendNuiMessage(new JsonBuilder().Add("Operation", "REGISTER_INVENTORY").Add("Inventory", inventory)
                .Build());
        }

        public Dictionary<int, Control> EquipmentSlots { get; set; } = new Dictionary<int, Control>()
        {
            [0] = Control.SelectWeaponUnarmed,
            [1] = Control.SelectWeaponMelee,
            [2] = Control.SelectWeaponShotgun,
            [3] = Control.SelectWeaponHeavy,
            [4] = Control.SelectWeaponSpecial,
        };

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            var container = GetContainer("equipment_inventory");

            if (container != null && SwitchDelay + 1000 < Date.Timestamp)
            {
                foreach (var entry in EquipmentSlots)
                {
                    if (!Game.IsControlJustPressed(0, entry.Value)) continue;

                    var equipment = (EquipmentInventory) container;
                    var slot = equipment.Slots[entry.Key];

                    if (slot == null || !slot.IsWeapon()) continue;

                    var weapon = (WeaponItem) slot;
                    var selected = equipment.GetSelected();

                    if (selected != null && selected.Seed != weapon.Seed) selected.Unequip();

                    if (weapon.IsEquipped) weapon.Unequip();
                    else weapon.Equip();

                    SwitchDelay = Date.Timestamp;
                }
            }

            if (!OpenedInventory)
            {
                if (Game.IsControlJustPressed(0, Control.ReplaySnapmaticPhoto) && !HandcuffManager.GetModule().IsHandcuffed)
                {
                    OpenedInventory = true;

                    if (StateChanged != null)
                        foreach (var invocation in StateChanged?.GetInvocationList())
                        {
                            ((Action<InventoryStateChange>) invocation).Invoke(InventoryStateChange.Show);
                        }

                    API.SendNuiMessage(new JsonBuilder().Add("Operation", "OPEN_INTERFACE").Add("Inventories", Registry).Add("Cash", Cache.Character.Cash).Add("Bank", Cache.Character.BankAccount.Balance).Build());               
                    API.SetNuiFocus(true, true);
                }
            }

            await Task.FromResult(0);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWeaponTick()
        {
            var container = GetContainer("equipment_inventory");

            if (container != null)
            {
                var equipment = (EquipmentInventory) container;
                var selected = equipment.GetSelected();

                selected?.UpdateAmmunition();

                await BaseScript.Delay(300);
            }
        }
    }

    public enum InventoryStateChange
    {
        Show,
        Hide
    }
}