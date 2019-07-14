using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment.PedClasses
{
    class PedHandler
    {
        static Client client = Client.GetInstance();
        static List<Ped> peds = new List<Ped>();
        static Random random = new Random();
        static int pedGroup = 1;

        public static void Init()
        {
            client.RegisterTickHandler(PedTick);
        }

        public static async void Create(Model model, Vector3 position, float heading, uint group, bool wander = false, bool chasePlayer = false, string name = "")
        {
            await model.Request(10000);

            while (!model.IsLoaded)
                await BaseScript.Delay(0);

            if (!API.DoesGroupExist(pedGroup))
            {
                API.CreateGroup(pedGroup);
                API.SetGroupFormation(pedGroup, 2);
            }

            Ped ped = await World.CreatePed(model, position, 180.0f);

            ped.Weapons.Give(WeaponHash.Machete, 1000, true, true);
            API.SetPedRelationshipGroupHash(ped.Handle, group);
            ped.Task.FightAgainstHatedTargets(10.0f);

            API.SetPedCombatMovement(ped.Handle, 2);
            API.SetPedCombatAttributes(ped.Handle, 5, true);
            API.SetPedCombatAbility(ped.Handle, 100);
            ped.Armor = random.Next(100);
            if (wander)
                ped.Task.WanderAround();

            if (chasePlayer)
            {
                if (string.IsNullOrEmpty(name))
                    name = Game.Player.Name;

                CitizenFX.Core.Player player = Client.players[name];

                ped.Weapons.Give(WeaponHash.Pistol, 1000, true, true);
                ped.Task.EnterAnyVehicle(VehicleSeat.Driver);
                ped.Task.VehicleChase(player.Character);
                ped.Task.ChaseWithGroundVehicle(player.Character);
            }

            Blip blip = ped.AttachBlip();
            blip.Color = BlipColor.Red;
            blip.Sprite = BlipSprite.Crosshair2;
            blip.IsShortRange = true;

            peds.Add(ped);

            if (peds.Count == 0)
            {
                API.SetPedAsGroupLeader(ped.Handle, pedGroup);
            }
            else
            {
                API.SetPedAsGroupMember(ped.Handle, pedGroup);
            }
        }

        static async Task PedTick()
        {
            List<Ped> pedsToRun = peds;
            for (var i = 0; i < pedsToRun.Count; i++)
            {
                try
                {
                    if (pedsToRun[i].IsDead)
                    {
                        pedsToRun[i].AttachedBlip.Delete();
                        peds.Remove(peds[i]);
                    }
                }
                catch (Exception ex)
                {
                    peds.Remove(peds[i]);
                }
            }
            await Task.FromResult(0);
        }
    }
}
