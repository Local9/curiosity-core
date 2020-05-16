using Atlas.Roleplay.Client.Billing;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Client.Managers;
using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Menus
{
    public class InteractionMenu : Manager<InteractionMenu>
    {
        public bool LookingAtId { get; set; }

        public override void Begin()
        {
            EventSystem.Attach("id:card:show", new EventCallback(metadata =>
            {
                var character = metadata.Find<AtlasCharacter>(0);
                var ssn = metadata.Find<string>(1);

                LookAtId(character, ssn);

                return null;
            }));
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Game.IsControlJustPressed(0, Control.MpTextChatTeam))
            {
                OpenInteractionMenu();

                await BaseScript.Delay(3000);
            }

            await Task.FromResult(0);
        }

        private void OpenInteractionMenu()
        {
            new Menu("Interaktionsmeny")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("give_idcard", "Visa ID"),
                    new MenuItem("look_idcard", "Kolla på ditt ID"),
                    new MenuItem("view_bills", "Dina fakturor"),
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;
                    if (item.Seed == "view_bills") ViewBills();
                    else if (item.Seed == "look_idcard")
                    {
                        var character = Cache.Character;

                        LookAtId(character,
                            new string(
                                (character.DateOfBirth + character.LastDigits).Replace("-", "").Skip(2).ToArray()));
                    }
                    else if (item.Seed == "give_idcard")
                    {
                        var player = GetClosestPlayer(3f, self => true);

                        if (player != -1)
                        {
                            var character = Cache.Character;
                            var ssn = new string((character.DateOfBirth + character.LastDigits).Replace("-", "").Skip(2)
                                .ToArray());
                            var user = new AtlasUser
                            {
                                Handle = API.GetPlayerServerId(player)
                            };

                            user.Send("id:card:show", character, ssn);
                        }
                        else
                        {
                            Cache.Player.ShowNotification(
                                "Det finns ingen i närheten som du kan ge ditt id till!");
                        }
                    }
                }
            }.Commit();
        }

        public async void ViewBills()
        {
            var bills = await EventSystem.Request<List<Bill>>("Billing.Fetch", Cache.Character.Seed);

            bills.ForEach(self => self.IsCreated = true);

            new Menu("Dina fakturor")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("received", "Mottagna fakturor"),
                    new MenuItem("sent", "Skickade fakturor")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.PostClose) OpenInteractionMenu();
                    else if (operation.Type == MenuOperationType.Select)
                    {
                        if (item.Seed == "received") ViewReceivedBills(bills);
                        else if (item.Seed == "sent") ViewSentBills(bills);
                    }
                }
            }.Commit();
        }

        public void ViewReceivedBills(IEnumerable<Bill> bills)
        {
            var character = Cache.Character;

            new Menu("Mottagna fakturor")
            {
                Items = bills.Where(self => self.IsActive && self.Receiver.Name == character.Fullname)
                    .Select(self => new MenuItem($"bill_{self.Seed}", $"Faktura #{self.BillNumber}", self)).ToList(),
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.PostClose) ViewBills();
                    else if (operation.Type == MenuOperationType.Select)
                    {
                        menu.Hide(false);

                        ((Bill)item.Metadata[0]).LookAt();
                    }
                }
            }.Commit();
        }

        public void ViewSentBills(IEnumerable<Bill> bills)
        {
            var character = Cache.Character;

            new Menu("Skickade fakturor")
            {
                Items = bills.Where(self => self.IsActive && self.Sender.Individual == character.Fullname)
                    .Select(self => new MenuItem($"bill_{self.Seed}", $"Faktura #{self.BillNumber}", self)).ToList(),
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type == MenuOperationType.PostClose) ViewBills();
                    else if (operation.Type == MenuOperationType.Select)
                    {
                        menu.Hide(false);

                        ((Bill)item.Metadata[0]).LookAt();
                    }
                }
            }.Commit();
        }

        private void LookAtId(AtlasCharacter character, string ssn)
        {
            API.SendNuiMessage(new JsonBuilder().Add("Operation", "SHOW_ID_CARD").Add("Character", character)
                .Add("SSN", ssn).Build());

            LookingAtId = true;
        }

        [TickHandler]
        private async Task OnSecondaryTick()
        {
            if (LookingAtId)
            {
                if (Game.IsControlJustPressed(0, Control.FrontendPauseAlternate))
                {
                    API.SendNuiMessage(new JsonBuilder().Add("Operation", "CLOSE_ID_CARD").Build());

                    LookingAtId = false;
                }

                await Task.FromResult(0);
            }
            else
            {
                await BaseScript.Delay(2500);
            }
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