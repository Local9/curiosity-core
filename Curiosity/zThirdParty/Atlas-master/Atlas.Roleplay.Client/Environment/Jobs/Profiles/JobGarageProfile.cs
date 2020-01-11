using System;
using System.Collections.Generic;
using System.Drawing;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Jobs.Profiles
{
    public class JobGarageProfile : JobProfile
    {
        public override JobProfile[] Dependencies { get; set; }
        public Position Parking { get; set; }
        public Position Spawn { get; set; }
        public Position Calling { get; set; }
        public Position[] Cinematic { get; set; } = new Position[0];
        public Dictionary<string, JobGarageVehicle> Vehicles { get; set; } = new Dictionary<string, JobGarageVehicle>();

        public override async void Begin(Job job)
        {
            await Session.Loading();
            
            if (Calling != null && Spawn != null)
            {
                var character = Cache.Character;
                var marker = new Marker(Calling)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att köra ut ett tjänstefordon",
                    Color = Color.Transparent,
                    Scale = 3f,
                    Condition = self => character.Metadata.Employment == job.Attachment
                };

                marker.Callback += () =>
                {
                    var elements = new List<MenuItem>();
                    var index = 1;

                    foreach (var vehicle in Vehicles)
                    {
                        elements.Add(new MenuItem($"vehicle_{index}", vehicle.Key));

                        index++;
                    }

                    new Menu($"{job.Label} | Garage")
                    {
                        Items = elements,
                        Callback = (menu, item, operation) =>
                        {
                            if (operation.Type != MenuOperationType.Select) return;

                            menu.Hide();

                            SpawnVehicle(Vehicles[item.Label]);
                        }
                    }.Commit();
                };

                marker.Show();
            }

            if (Parking == null) return;
            {
                var character = Cache.Character;
                var marker = new Marker(Parking)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att parkera ditt fordon",
                    Color = Color.Transparent,
                    Scale = 3f,
                    Condition = self => Game.PlayerPed.IsInVehicle() && character.Metadata.Employment == job.Attachment
                };

                marker.Callback += () => Game.PlayerPed.CurrentVehicle?.Delete();
                marker.Show();
            }
        }

        public async void SpawnVehicle(JobGarageVehicle vehicle)
        {
            var spawn = vehicle.Position ?? Spawn;
            var entity = await World.CreateVehicle(new Model(API.GetHashKey(vehicle.Model)), spawn.AsVector(),
                spawn.Heading);
            
            entity.Wash();
            entity.Repair();
            entity.PlaceOnGround();
            
            API.TaskWarpPedIntoVehicle(Cache.Entity.Id, entity.Handle, -1);

            vehicle.Callback?.Invoke(entity);
        }
    }

    public class JobGarageVehicle
    {
        public string Model { get; set; }
        public Action<Vehicle> Callback { get; set; }
        public Position Position { get; set; }
    }
}