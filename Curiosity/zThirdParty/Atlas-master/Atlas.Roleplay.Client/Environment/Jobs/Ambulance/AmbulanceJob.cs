using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Environment.Entities.Models;
using Atlas.Roleplay.Client.Environment.Jobs.Profiles;
using Atlas.Roleplay.Client.Events;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Client.Interface;
using Atlas.Roleplay.Library.Events;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment.Jobs.Ambulance
{
    public class AmbulanceJob : Job
    {
        public override Employment Attachment { get; set; } = Employment.Ambulance;
        public override string Label { get; set; } = "Sjukvården";

        public override BlipInfo[] Blips { get; set; } =
        {
            new BlipInfo
            {
                Name = "Sjukhus",
                Sprite = 61,
                Color = 5,
                Position = new Position(310.5256f, -591.5591f, 43.29183f, 73.92133f)
            },
        };

        public override Dictionary<int, string> Roles { get; set; } = new Dictionary<int, string>
        {
            [(int) EmploymentRoles.Ambulance.Chief] = "Sjukvårdschef",
            [(int) EmploymentRoles.Ambulance.Consultant] = "Överläkare",
            [(int) EmploymentRoles.Ambulance.Medic] = "Läkare",
            [(int) EmploymentRoles.Ambulance.Nurse] = "Sjuksköterska"
        };

        public override JobProfile[] Profiles { get; set; } =
        {
            new JobStorageProfile
            {
                Position = new Position(327.4694f, -581.9462f, 43.31739f, 118.863f),
            },
            new JobLockerRoomProfile
            {
                Position = new Position(269.3583f, -1364.149f, 24.53778f, 206.0394f),
                ClothingOptions = new Dictionary<string, Tuple<string, Tuple<string, string>>>
                {
                    ["civilian"] = new Tuple<string, Tuple<string, string>>("Egna kläder",
                        new Tuple<string, string>("CHARACTER_STYLE", "CHARACTER_STYLE")),
                },
                ClothingComponents = new[]
                {
                    "Shirt", "Torso", "Decals", "Body", "Pants", "Shoes", "BodyArmor", "Neck", "Head", "Glasses",
                    "EarAccessories"
                }
            },
            new JobGarageProfile
            {
                Calling = new Position(360.3838f, -585.23f, 28.81438f, 1.93344f),
                Spawn = new Position(361.4209f, -590.5991f, 28.66424f, 162.1186f),
                Parking = new Position(361.4209f, -590.5991f, 28.66424f, 162.1186f),
                Vehicles = new Dictionary<string, JobGarageVehicle>
                {
                    ["Ambulans"] = new JobGarageVehicle
                    {
                        Model = "ambulance"
                    },
                    ["Akutbil"] = new JobGarageVehicle
                    {
                        Model = "fbi"
                    }
                }
            },
            new JobBusinessProfile(),
            new JobPanelProfile
            {
                Position = new Position(262.1262f, -1360.026f, 24.53781f, 57.9517f)
            }
        };

        public Position[] Beds { get; } =
        {
            new Position(345.493f, -590.2751f, 43.31506f, 255.0764f),
            new Position(349.6313f, -591.2067f, 43.31506f, 240.6419f),
            new Position(353.0122f, -592.2543f, 43.31504f, 238.0026f),
            new Position(356.3295f, -593.9963f, 43.31504f, 245.5345f)
        };

        public Position Offset { get; } = new Position(-1.2f, 1.2f, 0f, 210f);
        public bool IsLayingInBed { get; set; }

        public override void Begin()
        {
            EventSystem.GetModule().Attach("ambulance:revive", new EventCallback(metadata =>
            {
                Cache.Character.Revive(Cache.Entity.Position);

                return null;
            }));

            {
                var enter = new Marker(new Position(334.2201f, -569.3099f, 43.31741f, 158.8153f))
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att gå till våning 2",
                    Scale = 2f,
                    Color = Color.Transparent
                };

                enter.Callback += async () =>
                {
                    API.DoScreenFadeOut(1000);

                    await BaseScript.Delay(3000);
                    await SafeTeleport.Teleport(Cache.Entity.Id,
                        new Position(275.5131f, -1361.161f, 24.53781f, 52.16409f));

                    API.DoScreenFadeIn(1000);
                };

                var exit = new Marker(new Position(275.5131f, -1361.161f, 24.53781f, 52.16409f))
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att gå till våning 1",
                    Scale = 2f,
                    Color = Color.Transparent
                };

                exit.Callback += async () =>
                {
                    API.DoScreenFadeOut(1000);

                    await BaseScript.Delay(3000);
                    await SafeTeleport.Teleport(Cache.Entity.Id,
                        new Position(334.2201f, -569.3099f, 43.31741f, 158.8153f));

                    API.DoScreenFadeIn(1000);
                };

                enter.Show();
                exit.Show();
            }
            foreach (var bed in Beds)
            {
                var marker = new Marker(bed)
                {
                    Message = "Tryck ~INPUT_CONTEXT~ för att lägga dig",
                    Scale = 1f,
                    Color = Color.Transparent,
                    Condition = self => !IsLayingInBed
                };

                marker.Callback += async () =>
                {
                    var ped = Cache.Entity;
                    var prop = GetBedProp(bed);
                    var position = API.GetEntityCoords(prop, false).ToPosition();
                    var endpoint = position.Add(Offset);

                    IsLayingInBed = true;

                    API.SetEntityCollision(prop, false, false);

                    await ped.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                        .Select("mp_bedmid", "f_getin_r_bighouse")
                        .AtPosition(endpoint)
                    );

                    API.SetEntityCollision(prop, true, true);

                    ped.Movable = false;
                    ped.Position = ped.Position.Subtract(new Position(0, 0f, 0.8f));
                    ped.AnimationQueue.AddToQueue(new AnimationBuilder()
                        .Select("mp_bedmid", "f_sleep_r_loop_bighouse")
                        .WithFlags(AnimationFlags.StayInEndFrame)
                    ).PlayQueue();

                    API.BeginTextCommandDisplayHelp("STRING");
                    API.AddTextComponentSubstringPlayerName("Tryck ~INPUT_CONTEXT~ för att resa dig");
                    API.EndTextCommandDisplayHelp(0, false, true, 5000);

                    AtlasPlugin.Instance.AttachTickHandler(OnBedTick);
                };

                marker.Show();
            }
        }

        private int GetBedProp(Position position)
        {
            return API.GetClosestObjectOfType(position.X, position.Y, position.Z, 3f, 2117668672, false, false,
                false);
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

        private async Task OnBedTick()
        {
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                AtlasPlugin.Instance.DetachTickHandler(OnBedTick);

                var ped = Cache.Entity;
                var bed = GetBedProp(ped.Position);

                API.SetEntityCollision(bed, false, true);
                API.ClearPedTasks(ped.Id);

                ped.Movable = true;

                await ped.AnimationQueue.PlayDirectInQueue(
                    new AnimationBuilder().Select("mp_bedmid", "f_getout_r_bighouse"));

                API.SetEntityCollision(bed, true, true);

                IsLayingInBed = false;
            }

            await Task.FromResult(0);
        }

        private void OpenInteractionMenu()
        {
            new Menu($"{Label} | Interaktionsmeny")
            {
                Items = new List<MenuItem>
                {
                    new MenuItem("revive_nearest", "Återuppliva")
                },
                Callback = (menu, item, operation) =>
                {
                    if (operation.Type != MenuOperationType.Select) return;

                    var player = GetClosestPlayer(1.5f, self => API.IsEntityDead(API.GetPlayerPed(self)));

                    if (player != -1)
                    {
                        var user = new AtlasUser
                        {
                            Handle = API.GetPlayerServerId(player)
                        };

                        user.Send("ambulance:revive");
                    }
                    else
                    {
                        Cache.Player.ShowNotification("Det finns ingen i närheten som du kan återuppliva!");
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