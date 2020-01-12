$(document).ready(() => {
    const Viewmodel = new Vue({
        el: "#inventory-overlay",
        data: {
            Overlay: Overlay,
            overlay: Overlay.NONE,
            inventories: [],
            secondary_inventory: {
                Name: "Omgivning",
                Seed: "proximity_inventory",
                Slots: [],
                Registered: false
            },
            hoveredItem: "-1",
            selectedItem: "-1",
            clickedItem: "-1",
            tooltip: {
                enabled: false,
                usable: false,
                position: [0, 0],
                title: "",
                description: "",
                additional_description: "",
                item: {}
            },
            hoveredOverUseButton: false,
            cash: 0,
            bank: 0
        },
        methods: {
            get_position(element) {
                let firefox = (navigator.userAgent.toLowerCase().indexOf('firefox') !== -1);
                let x = 0, y = 0;

                while (element) {
                    x += element.offsetLeft - element.scrollLeft + (!firefox ? element.clientLeft : 0);
                    y += element.offsetTop - element.scrollTop + (!firefox ? element.clientTop : 0);
                    element = element.offsetParent;
                }

                return [x + window.scrollX, y + window.scrollY];
            },
            update_hover(item) {
                this.hoveredItem = item;

                if (item === "-1") {
                    this.tooltip.enabled = false;
                } else if (this.get_item_from_unique(item).Seed !== "__none") {
                    let ticks = 0;

                    const copy = item;
                    const interval = setInterval(() => {
                        ticks++;

                        if (this.hoveredItem !== copy) {
                            clearInterval(interval);

                            return;
                        }

                        if (ticks >= 5) {
                            this.tooltip.enabled = true;

                            const result = this.get_item_from_unique(item);
                            const position = this.get_position(document.getElementById(item));

                            position[0] += 150;
                            position[1] -= 50;

                            this.tooltip.position = position;
                            this.tooltip.item = result;
                            this.tooltip.usable = result.Usable;
                            this.tooltip.title = result.Label;
                            this.tooltip.description = result.Description;

                            if (result.Name.startsWith("weapon::")) {
                                this.tooltip.additional_description = "Skott: " + result.Metadata["Weapon.Ammo"];
                            } else {
                                this.tooltip.additional_description = "";
                            }

                            clearInterval(interval)
                        }
                    }, 100);
                }
            },
            use_item() {
                this.hoveredOverUseButton = false;

                Atlas.send("INVENTORY_ITEM_USE", {0: JSON.stringify(this.get_item_from_unique(this.clickedItem))})
            },
            get_item_from_unique(unique) {
                const parts = unique.split("-");

                if (parts.length === 3) return this.inventories.filter(self => self.Seed === parts[1])[0].Slots[parts[2]];
            },
            is_selected(seed) {
                return this.secondary_inventory.Seed === seed;
            },
            select_inventory_tab(seed) {
                let skeleton = Viewmodel.inventories.filter(self => self.Seed === seed)[0];

                if (skeleton == null) return;

                Viewmodel.secondary_inventory = skeleton;
            },
            select_item() {
                this.selectedItem = this.hoveredItem;
            },
            drop_item() {
                if (this.hoveredItem === "-1" || this.selectedItem === "-1") return;

                let source = null;
                let target = null;

                for (let i = 0; i < this.inventories.length; i++) {
                    let inventory = this.inventories[i];

                    for (let slotIndex = 0; slotIndex < inventory.Slots.length; slotIndex++) {
                        let slot = inventory.Slots[slotIndex];

                        if (slot.Unique === this.selectedItem) {
                            source = [i, JSON.parse(JSON.stringify(slot)), slotIndex]
                        } else if (slot.Unique === this.hoveredItem) {
                            target = [i, JSON.parse(JSON.stringify(slot)), slotIndex]
                        }
                    }
                }

                if (source != null && target != null) {
                    const sourceUnique = source[1].Unique;
                    const sourceSlot = source[1].Slot;
                    const targetUnique = target[1].Unique;
                    const targetSlot = target[1].Slot;

                    const modifiedSource = target[1];

                    modifiedSource.Unique = sourceUnique;
                    modifiedSource.Slot = sourceSlot;

                    const modifiedTarget = source[1];

                    modifiedTarget.Unique = targetUnique;
                    modifiedTarget.Slot = targetSlot;

                    Vue.set(this.inventories[source[0]].Slots, source[2], modifiedSource);
                    Vue.set(this.inventories[target[0]].Slots, target[2], modifiedTarget);

                    Atlas.send("INVENTORY_ITEM_CHANGE", {
                        0: this.inventories[source[0]].Seed,
                        1: JSON.stringify(modifiedSource),
                        2: this.inventories[target[0]].Seed,
                        3: JSON.stringify(modifiedTarget)
                    })
                }
            }
        },
        mounted() {
            window.addEventListener("mouseup", () => {
                if (!this.hoveredOverUseButton) {
                    this.clickedItem = this.hoveredItem;
                }

                this.drop_item();
            });
            window.addEventListener("mousedown", (event) => {
                if (this.overlay === this.Overlay.INVENTORY) {
                    event.preventDefault();

                    this.select_item();
                }
            })
        }
    });

    $(async () => {
        let create_inventory = (skeleton) => {
            for (let i = 0; i < skeleton.SlotAmount; i++) {
                let slot = skeleton.Slots[i];

                if (slot == null) {
                    slot = {
                        Seed: "__none",
                        Name: "none",
                        Label: "",
                        Description: "",
                        Icon: "",
                        Usable: false
                    };
                }

                slot.Slot = i;
                slot.Unique = "slot-" + skeleton.Seed + "-" + i;

                skeleton.Slots[i] = slot;
            }

            skeleton.Registered = true;

            if (Viewmodel.inventories.filter(self => self.Seed === skeleton.Seed).length === 0)
                Viewmodel.inventories.push(skeleton);

            return skeleton;
        };

        let update_inventory = (skeleton) => {
            let inventory = Viewmodel.inventories.filter(self => self.Seed === skeleton.Seed)[0];

            if (inventory == null || !inventory.Registered) return;

            for (let i = 0; i < skeleton.SlotAmount; i++) {
                let slot = skeleton.Slots[i];

                if (slot == null) {
                    slot = {
                        Seed: "__none",
                        Name: "none",
                        Label: "",
                        Description: "",
                        Icon: "",
                        Usable: false
                    };
                }

                slot.Slot = i;
                slot.Unique = "slot-" + skeleton.Seed + "-" + i;

                Vue.set(inventory.Slots, i, slot);
            }
        };

        Viewmodel.select_inventory_tab("proximity_inventory");

        window.addEventListener("message", event => {
            switch (event.data["Operation"]) {
                case "OPEN_INTERFACE":
                    Viewmodel.cash = event.data["Cash"];
                    Viewmodel.bank = event.data["Bank"];
                    Viewmodel.secondary_inventory = Viewmodel.inventories.filter(self => self.Seed === "proximity_inventory")[0];

                    for (let i = 0; i < Viewmodel.inventories.length; i++) {
                        const inventory = Viewmodel.inventories[i];

                        inventory.IsVisible = event.data["Inventories"].filter(self => self.Seed === inventory.Seed)[0].IsVisible;
                    }

                    Overlay = Overlay.INVENTORY;

                    break;
                case "REGISTER_INVENTORY":
                    const created = create_inventory(event.data["Inventory"]);

                    if (!Viewmodel.secondary_inventory.Registered && Viewmodel.secondary_inventory.Seed === event.data["Inventory"]["Seed"]) {
                        Viewmodel.secondary_inventory = created;
                    }

                    break;
                case "UPDATE_INVENTORY":
                    update_inventory(event.data["Inventory"]);

                    break;
            }
        });

        window.addEventListener("keydown", event => {
            if (Overlay === Overlay.INVENTORY && (event.keyCode === 9 || event.keyCode === 27)) {
                event.preventDefault();

                Overlay = Overlay.NONE;

                Atlas.send("CLOSE_INVENTORY", {});
            }
        }, false);
    });
});