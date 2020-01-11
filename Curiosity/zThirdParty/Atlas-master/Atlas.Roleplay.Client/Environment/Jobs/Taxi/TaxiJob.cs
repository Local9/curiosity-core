using System.Collections.Generic;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Billing;
using Atlas.Roleplay.Client.Environment.Jobs.Profiles;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;

namespace Atlas.Roleplay.Client.Environment.Jobs.Taxi
{
    public class TaxiJob : Job
    {
        public override Employment Attachment { get; set; } = Employment.Taxi;
        public override string Label { get; set; } = "Taxi";

        public override BlipInfo[] Blips { get; set; } =
        {
            new BlipInfo
            {
                Name = "Taxi",
                Sprite = 198,
                Color = 5,
                Position = new Position(900.7405f, -172.5881f, 74.07522f, 312.4802f)
            }
        };

        public override Dictionary<int, string> Roles { get; set; } = new Dictionary<int, string>
        {
            [(int) EmploymentRoles.Taxi.CEO] = "Vd",
            [(int) EmploymentRoles.Taxi.COO] = "Vice Vd",
            [(int) EmploymentRoles.Taxi.Employee] = "Anställd",
            [(int) EmploymentRoles.Taxi.Probationary] = "Provanställd"
        };

        public override JobProfile[] Profiles { get; set; } =
        {
            new JobStorageProfile
            {
                Position = new Position(886.3754f, -181.7469f, 73.59856f, 53.61389f)
            },
            new JobGarageProfile
            {
                Calling = new Position(899.6927f, -176.9187f, 73.86578f, 349.5645f),
                Spawn = new Position(898.2938f, -179.9169f, 73.78387f, 240.0718f),
                Parking = new Position(898.2938f, -179.9169f, 73.78387f, 240.0718f),
                Vehicles = new Dictionary<string, JobGarageVehicle>
                {
                    ["Taxi 1"] = new JobGarageVehicle
                    {
                        Model = "taxi"
                    }
                }
            },
            new JobPanelProfile
            {
                Position = new Position(900.7405f, -172.5881f, 74.07522f, 312.4802f)
            },
            new JobBusinessProfile()
        };

        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            if (Game.IsControlJustPressed(0, Control.SelectCharacterTrevor) && Cache.Character.Metadata.Employment == Attachment)
            {
                OpenJobMenu();

                await BaseScript.Delay(3000);
            }

            await Task.FromResult(0);
        }

        private void OpenJobMenu()
        {
            new Menu($"{Label} | Jobbmeny")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("send_bill", "Skicka faktura")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

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
            }.Commit();
        }
    }
}