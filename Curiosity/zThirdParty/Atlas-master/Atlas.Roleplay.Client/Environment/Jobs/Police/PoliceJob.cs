using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Billing;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Environment.Entities.Modules.Impl;
using Atlas.Roleplay.Client.Environment.Jobs.Profiles;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Inventory;
using Atlas.Roleplay.Client.Inventory.Items;
using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Style = Atlas.Roleplay.Library.Models.Style;

namespace Atlas.Roleplay.Client.Environment.Jobs.Police
{
    public sealed class PoliceJob : Job
    {
        public override Employment Attachment { get; set; } = Employment.Police;
        public override string Label { get; set; } = "Polisen";

        public override BlipInfo[] Blips { get; set; } =
        {
            new BlipInfo
            {
                Name = "Polisen",
                Sprite = 60,
                Color = 42,
                Position = new Position(428.9843f, -981.8438f, 30.71029f, 273.0397f)
            }
        };

        public override Dictionary<int, string> Roles { get; set; } = new Dictionary<int, string>
        {
            [(int) EmploymentRoles.Police.Chief] = "Rikspolischef",
            [(int) EmploymentRoles.Police.Superintendent] = "Kommissarie",
            [(int) EmploymentRoles.Police.Inspector] = "Inspektör",
            [(int) EmploymentRoles.Police.Assistant] = "Assistent",
            [(int) EmploymentRoles.Police.Trainee] = "Aspirant",
        };

        public override JobProfile[] Profiles { get; set; } =
        {
            new JobStorageProfile
            {
                Position = new Position(452.26f, -980f, 30.69f, 263.3528f)
            },
            new JobArmoryProfile
            {
                Armory = new Position(452.26f, -980f, 30.69f, 263.3528f),
                MerchantModel = API.GetHashKey("s_m_y_cop_01"),
                MerchantPosition = new Position(454.11f, -980.26f, 30.7f, 90.35059f)
            },
            new JobLockerRoomProfile
            {
                Position = new Position(474.3886f, -991.4667f, 24.91476f, 199.8519f),
                ClothingOptions = new Dictionary<string, Tuple<string, Tuple<string, string>>>
                {
                    ["civilian"] = new Tuple<string, Tuple<string, string>>("Ägda kläder",
                        new Tuple<string, string>("CHARACTER_STYLE", "CHARACTER_STYLE")),
                    ["uniform"] = new Tuple<string, Tuple<string, string>>("Polisuniform",
                        new Tuple<string, string>("MalePoliceUniform", "FemalePoliceUniform")),
                    ["swat"] = new Tuple<string, Tuple<string, string>>("Insatsstyrka",
                        new Tuple<string, string>("MaleSwatUniform", "FemaleSwatUniform"))
                },
                ClothingComponents = new[]
                {
                    "Shirt", "Torso", "Decals", "Body", "Pants", "Shoes", "BodyArmor", "Neck", "Head", "Glasses",
                    "EarAccessories", "Mask", "Bag"
                },
                Callback = async (seed, outfit, style) =>
                {
                    if (seed == "swat") API.SetPedArmour(Cache.Entity.Id, 100);
                    else if (seed == "uniform")
                    {
                        var character = Cache.Character;
                        var decorations = new Style
                        {
                            Torso = style.Torso,
                            TorsoType =
                            {
                                Current = character.Metadata.EmploymentRole
                            }
                        };


                        await style.Merge(decorations, "Torso").Commit(Cache.Player, false);
                    }
                }
            },
            new JobGarageProfile
            {
                Parking = new Position(452.4755f, -996.3059f, 25.77393f, 359.5667f),
                Spawn = new Position(447.3467f, -997.0787f, 25.76291f, 179.2201f),
                Calling = new Position(442.093f, -1014.3f, 28.63699f, 1.912454f),
                Vehicles = new Dictionary<string, JobGarageVehicle>
                {
                    ["Insats"] = new JobGarageVehicle
                    {
                        Model = "dubsta",
                        Callback = vehicle => new DubstaModifications().Install(vehicle.Handle)
                    },
                    ["V90"] = new JobGarageVehicle
                    {
                        Model = "police3"
                    },
                    ["V90 CC"] = new JobGarageVehicle
                    {
                        Model = "police2"
                    },
                    ["XC70"] = new JobGarageVehicle
                    {
                        Model = "police"
                    },
                    ["Helikopter"] = new JobGarageVehicle
                    {
                        Model = "polmav",
                        Position = new Position(449.8032f, -981.157f, 43.69167f, 78.5856f)
                    }
                }
            },
            new JobPanelProfile
            {
                Position = new Position(447.9819f, -973.3724f, 30.68967f, 113.0314f)
            },
            new JobBusinessProfile()
        };

        public override void Begin()
        {
            GetProfile<JobArmoryProfile>().Callback = OpenGeneralMenu;

            API.SetRelationshipBetweenGroups(0, (uint) API.GetHashKey("PLAYER"), (uint) API.GetHashKey("COP"));
            API.SetRelationshipBetweenGroups(0, (uint) API.GetHashKey("COP"), (uint) API.GetHashKey("PLAYER"));
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Game.IsControlJustPressed(0, Control.SelectCharacterTrevor) &&
                Cache.Character.Metadata.Employment == Attachment)
            {
                OpenInteractionMenu();

                await BaseScript.Delay(3000);
            }

            await Task.FromResult(0);
        }

        private void OpenGeneralMenu()
        {
            new Menu($"{Label} | Förråd")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("armory_get", "Ta ut vapen"),
                    new MenuItem("armory_put", "Lägg in vapen")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;
                    if (item.Seed == "armory_get") OpenGetArmory();
                    else if (item.Seed == "armory_put") OpenPutArmory();
                }
            }.Commit();
        }

        private void OpenGetArmory()
        {
            var profile = GetProfile<JobArmoryProfile>();
            var player = Cache.Player;

            new Menu($"{Label} | Vapen")
            {
                Items = new List<MenuItem>()
                {
                    new MenuItem("WEAPON_STUNGUN", "Elpistol", "PISTOL", WeaponHash.StunGun, 1, typeof(StungunItem)),
                    new MenuItem("WEAPON_NIGHTSTICK", "Batong", "PISTOL", WeaponHash.Nightstick, 1,
                        typeof(NightstickItem)),
                    new MenuItem("WEAPON_FLAREGUN", "Signalpistol", "PISTOL", WeaponHash.FlareGun, 5,
                        typeof(FlaregunItem)),
                    new MenuItem("WEAPON_COMBATPISTOL", "Sig Sauer", "PISTOL", WeaponHash.CombatPistol, 250,
                        typeof(PistolItem)),
                    new MenuItem("WEAPON_SMG", "MP5", "RIFLE", WeaponHash.SMG, 250, typeof(Mp5Item)),
                },
                Callback = async (menu, item, operation) =>
                {
                    var entity = player.Entity;
                    var merchant = profile.Merchant;

                    if (operation.Type == MenuOperationType.Select)
                    {
                        await merchant.NetworkModule.Claim();
                        await merchant.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                            .Select("mp_cop_armoury", $"{item.Metadata[0].ToString().ToLower()}_on_counter_cop")
                            .SkipTask()
                            .On(async () =>
                            {
                                await BaseScript.Delay(100);

                                entity.Weapons.Select(WeaponHash.Unarmed);
                                merchant.Weapons.Give((WeaponHash) item.Metadata[1], (int) item.Metadata[2], true,
                                    true);
                            })
                        );

                        await BaseScript.Delay(100);

                        var builder = new AnimationBuilder()
                            .Select("mp_cop_armoury", $"{item.Metadata[0].ToString().ToLower()}_on_counter")
                            .On(async () =>
                            {
                                await BaseScript.Delay(4000);

                                merchant.Weapons.RemoveAll();
                                entity.Weapons.Give((WeaponHash) item.Metadata[1], (int) item.Metadata[2],
                                    true, true);
                            });

                        entity.AnimationQueue.AddToQueue(builder).PlayQueue();

                        await builder.TaskWhenPlayed();

                        entity.Weapons.Select(WeaponHash.Unarmed);

                        var weapon = (WeaponItem) Activator.CreateInstance((Type) item.Metadata[3]);

                        weapon.Metadata["Weapon.Ammo"] = (int) item.Metadata[2];

                        if (!ItemHelper.Give(InventoryManager.GetModule().GetContainer("equipment_inventory"), weapon))
                            player.ShowNotification($"Du har inte plats med en {item.Label}!");
                    }
                    else if (operation.Type == MenuOperationType.PostClose)
                    {
                        OpenGeneralMenu();
                    }
                }
            }.Commit();
        }

        private void OpenPutArmory()
        {
            var elements = new List<MenuItem>();
            var registry = InventoryManager.GetModule().Registry;

            registry.ForEach(self => self.RefreshItemClassifications());
            registry.Where(self => self.Seed == "equipment_inventory" || self.Seed == "pockets_inventory").ToList()
                .ForEach(self => elements.AddRange(self.Slots.Where(slot => slot != null && slot.IsWeapon())
                    .ToList().Select(weapon => new MenuItem($"weapon_{weapon.Seed}", $"{weapon.Label}", weapon))));

            new Menu($"{Label} | Dina vapen")
            {
                Items = elements,
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    ItemHelper.Remove((WeaponItem) item.Metadata[0]);

                    menu.Hide();

                    OpenGeneralMenu();
                }
            }.Commit();
        }

        private void OpenInteractionMenu()
        {
            new Menu($"{Label} | Interaktionsmeny")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("handcuff", "Sätt på handbojor"),
                    new MenuItem("remove_handcuffs", "Ta bort handbojor"),
                    new MenuItem("drag", "Dra närmsta person"),
                    new MenuItem("bill", "Skriv en böter")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    if (item.Seed == "handcuff")
                    {
                        var player = GetClosestPlayer(1.5f, self =>
                        {
                            var decors = new EntityDecorModule
                            {
                                Id = API.GetPlayerPed(self)
                            };

                            return !decors.GetBoolean("Player.IsHandcuffed");
                        });

                        if (player != -1)
                        {
                            var user = new AtlasUser
                            {
                                Handle = API.GetPlayerServerId(player)
                            };

                            user.Send("handcuff:toggle", true);
                        }
                        else
                        {
                            Cache.Player.ShowNotification("Det finns ingen i närheten som du kan handboja!");
                        }
                    }
                    else if (item.Seed == "remove_handcuffs")
                    {
                        var player = GetClosestPlayer(1.5f, self =>
                        {
                            var decors = new EntityDecorModule
                            {
                                Id = API.GetPlayerPed(self)
                            };

                            return decors.GetBoolean("Player.IsHandcuffed");
                        });

                        if (player != -1)
                        {
                            var user = new AtlasUser
                            {
                                Handle = API.GetPlayerServerId(player)
                            };

                            user.Send("handcuff:toggle", false);
                        }
                        else
                        {
                            Cache.Player.ShowNotification("Det finns ingen i närheten som har handbojor på sig!");
                        }
                    }
                    else if (item.Seed == "drag")
                    {
                        var player = GetClosestPlayer(2f, self =>
                        {
                            var decors = new EntityDecorModule
                            {
                                Id = API.GetPlayerPed(self)
                            };

                            return decors.GetBoolean("Player.IsHandcuffed");
                        });

                        if (player != -1)
                        {
                            var user = new AtlasUser
                            {
                                Handle = API.GetPlayerServerId(player)
                            };

                            user.Send("handcuff:drag:toggle", API.GetPlayerServerId(API.PlayerId()));
                        }
                        else
                        {
                            Cache.Player.ShowNotification("Det finns ingen i närheten som har handbojor på sig!");
                        }
                    }
                    else if (item.Seed == "bill")
                    {
                        var character = Cache.Character;

                        BillingManager.GetModule().CreateBill(new BillSender
                        {
                            Business = Label,
                            Individual = character.Fullname
                        }, new BillReceiver
                        {
                            Type = BillReceiverType.Individual,
                            Name = ""
                        });
                    }
                }
            }.Commit();
        }

        private int GetClosestPlayer(float radius, Predicate<int> filter)
        {
            var closest = -1;
            var distance = radius + 1f;

            for (var i = 0; i < AtlasPlugin.MaximumPlayers; i++)
            {
                if (i == API.PlayerId()) continue;

                var ped = API.GetPlayerPed(i);

                var dist = API.GetEntityCoords(ped, false).ToPosition().Distance(Cache.Entity.Position);

                if (!(distance > dist) || !(dist < radius) || !filter.Invoke(i)) continue;

                closest = i;
                distance = dist;
            }

            return closest;
        }
    }
}