using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Diagnostics;
using Curiosity.System.Client.Environment;
using Curiosity.System.Client.Environment.Entities;
using Curiosity.System.Client.Extensions;
using Curiosity.System.Client.Inventory;
using Curiosity.System.Client.Inventory.Impl;
using Curiosity.System.Library.Events;
using Curiosity.System.Library.Inventory;
using Curiosity.System.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.System.Client.Managers
{
    public class CharacterManager : Manager<CharacterManager>
    {
        public List<CuriosityCharacter> AvailableCharacters { get; set; } = new List<CuriosityCharacter>();

        public override void Begin()
        {
            Atlas.AttachNuiHandler("CREATE_CHAR", new AsyncEventCallback(async metadata =>
            {
                var firstname = metadata.Find<string>(0);
                var surname = metadata.Find<string>(1);
                var dateofbirth = metadata.Find<string>(2);
                var character =
                    await EventSystem.Request<CuriosityCharacter>("characters:create", firstname, surname, dateofbirth);

                AvailableCharacters.Add(character);

                API.SetNuiFocus(true, true);
                API.SendNuiMessage(new JsonBuilder().Add("Operation", "LOAD_CHARACTERS")
                    .Add("Characters", AvailableCharacters).Build());

                return null;
            }));

            Atlas.AttachNuiHandler("DELETE_CHAR", new EventCallback(metadata =>
            {
                var characterId = metadata.Find<int>(0);

                EventSystem.Send("characters:delete", characterId);

                AvailableCharacters.RemoveAll(self => self.CharacterId == characterId);

                return null;
            }));

            Atlas.AttachNuiHandler("SELECT_CHAR", new AsyncEventCallback(async metadata =>
            {
                var characterId = metadata.Find<int>(0);
                var character = AvailableCharacters.FirstOrDefault(self => self.CharacterId == characterId);

                if (character != null)
                {
                    Atlas.Local.Character = character;

                    API.SetNuiFocus(false, false);
                    API.SendNuiMessage(new JsonBuilder().Add("Operation", "CLOSE_CHARACTERS").Build());

                    await Load(Atlas.Local);
                }
                else
                {
                    Logger.Info("[Characters] Cannot select a character that isn't yours.");
                }

                return null;
            }));
        }

        public async Task Synchronize()
        {
            API.DoScreenFadeIn(0);

            Logger.Debug("[Characters] Fetching characters from the database...");

            API.SendLoadingScreenMessage(new JsonBuilder().Add("eventName", "UPDATE_STATUS").Add("status", "Laddar in karaktärer").Build());

            AvailableCharacters = await EventSystem.Request<List<CuriosityCharacter>>("characters:find", null);

            Logger.Debug($"[Characters] Loaded {AvailableCharacters.Count} character(s) from the database.");

            API.SetNuiFocus(true, true);
            API.SendNuiMessage(new JsonBuilder().Add("Operation", "LOAD_CHARACTERS")
                .Add("Characters", AvailableCharacters).Build());
            API.ShutdownLoadingScreen();
        }

        public async Task Load(CuriosityPlayer player)
        {
            API.DoScreenFadeOut(0);

            Atlas.DiscordRichPresence.Status = player.Name;
            Atlas.DiscordRichPresence.Commit();

            var transition = new LoadTransition();

            if (!player.Character.MarkedAsRegistered)
            {
                API.StopPlayerSwitch();
            }

            var character = player.Character;
            var position = !character.MarkedAsRegistered
                ? CharacterExtensions.RegistrationPosition
                : character.Metadata.LastPosition ?? CharacterExtensions.DefaultPosition;

            if (position == character.Metadata.LastPosition) position.Y += 1f;

            character.Revive(position);

            try
            {
                var ped = Cache.Entity.Id;
                var health = character.Health;

                if (health > API.GetEntityMaxHealth(ped))
                {
                    health = API.GetEntityMaxHealth(ped);
                }

                API.SetEntityHealth(ped, health);
                API.SetPedArmour(ped, character.Shield);
            }
            catch (Exception)
            {
                // Dunno
            }

            var inventories = InventoryManager.GetModule();
            var required = new InventoryContainer[]
            {
                new EquipmentInventory(new InventoryContainerBase
                {
                    Seed = "equipment_inventory",
                    Name = "Equipment",
                    SlotAmount = 5
                }),
                new PocketsInventory(new InventoryContainerBase
                {
                    Seed = "pockets_inventory",
                    Name = "Pockets",
                    SlotAmount = 20
                }),
                new ProximityInventory(new InventoryContainerBase
                {
                    Seed = "proximity_inventory",
                    Name = "Enviroment",
                    SlotAmount = 20
                })
            };

            foreach (var entry in required)
            {
                if (character.Metadata.Inventories.All(self => self.Seed != entry.Seed))
                {
                    inventories.RegisterContainer(entry);
                    character.Metadata.Inventories.Add(entry);

                    entry.CallRegistration();
                }
                else
                {
                    var created = entry;

                    switch (entry.Seed.ToUpper())
                    {
                        case "EQUIPMENT_INVENTORY":
                            created = new EquipmentInventory(
                                character.Metadata.Inventories.FirstOrDefault(
                                    self => self.Seed == "equipment_inventory"));

                            break;
                        case "POCKETS_INVENTORY":
                            created = new PocketsInventory(
                                character.Metadata.Inventories.FirstOrDefault(
                                    self => self.Seed == "pockets_inventory"));

                            break;
                        case "PROXIMITY_INVENTORY":
                            created = new ProximityInventory(
                                character.Metadata.Inventories.FirstOrDefault(
                                    self => self.Seed == "proximity_inventory"));
                            created.Slots = new InventoryItem[created.SlotAmount];

                            break;
                        default:
                            Logger.Info($"[Inventory] Could not find default required inventory {entry.Seed}");

                            break;
                    }

                    inventories.RegisterContainer(created);
                    created.CallRegistration();
                }
            }

            inventories.Registry.ForEach(self => self.RefreshItemClassifications());

            await player.Character.Load();

            Session.Join(player.Character.MarkedAsRegistered ? 1 : 100 + API.GetPlayerServerId(API.PlayerId()));

            await SafeTeleport.Teleport(player.Entity.Id, position);

            if (player.Character.MarkedAsRegistered)
            {
                await transition.Wait();

                API.DoScreenFadeIn(5000);

                await transition.Down(player);
            }
            else
            {
                await player.Character.PostLoad();
            }

            if (API.IsScreenFadedOut() && !API.IsScreenFadingOut())
            {
                API.DoScreenFadeIn(5000);
            }

            player.EnableHud();
        }

        public class LoadTransition
        {
            public CuriosityPlugin Curiosity { get; set; }

            public LoadTransition()
            {
                Curiosity = CuriosityPlugin.Instance;
            }

            public async Task Up(CuriosityPlayer player)
            {
                var timestamp = API.GetGameTimer();

                Curiosity.AttachTickHandler(OnTick);

                player.Sound.Disable();

                API.SwitchOutPlayer(player.Entity.Id, 0, 1);

                while (API.GetPlayerSwitchState() != 5 && timestamp + 15000 > API.GetGameTimer())
                {
                    await BaseScript.Delay(0);
                }

                player.Sound.Enable();

                Curiosity.DetachTickHandler(OnTick);
            }

            public async Task Wait()
            {
                while (API.GetPlayerSwitchState() < 3)
                {
                    await BaseScript.Delay(0);
                }
            }

            public async Task Down(CuriosityPlayer player)
            {
                while (API.GetPlayerSwitchState() != 5)
                {
                    await BaseScript.Delay(100);
                }

                Curiosity.AttachTickHandler(OnTick);

                var timestamp = API.GetGameTimer();

                while (timestamp + 3000 > API.GetGameTimer())
                {
                    await BaseScript.Delay(10);
                }

                API.SwitchInPlayer(player.Entity.Id);

                while (API.GetPlayerSwitchState() != 12)
                {
                    await BaseScript.Delay(10);
                }

                Curiosity.DetachTickHandler(OnTick);

                API.ClearDrawOrigin();
            }

            public async Task DownWait()
            {
                if (Cache.Character.MarkedAsRegistered)
                {
                    while (API.GetPlayerSwitchState() < 10)
                    {
                        await BaseScript.Delay(10);
                    }
                }
                else
                {
                    while (Cache.Character.MarkedAsRegistered && !API.IsScreenFadedOut())
                    {
                        await BaseScript.Delay(10);
                    }
                }
            }

            private async Task OnTick()
            {
                API.SetCloudHatOpacity(0.05f);
                API.HideHudAndRadarThisFrame();

                await Task.FromResult(0);
            }
        }
    }
}