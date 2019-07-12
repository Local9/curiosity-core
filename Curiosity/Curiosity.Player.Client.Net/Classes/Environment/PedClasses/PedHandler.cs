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

        public static void Init()
        {
            client.RegisterTickHandler(PedTick);
        }

        public static async void Create(Model model, Vector3 position, float heading, uint group)
        {
            await model.Request(10000);

            while (!model.IsLoaded)
                await BaseScript.Delay(0);

            Ped ped = await World.CreatePed(model, position, 180.0f);

            ped.Weapons.Give(WeaponHash.Machete, 1000, true, true);
            ped.RelationshipGroup = group;
            ped.Task.FightAgainstHatedTargets(10.0f);
            ped.Task.WanderAround();

            API.SetPedCombatMovement(ped.Handle, 2);
            API.SetPedCombatAttributes(ped.Handle, 5, true);
            API.SetPedCombatAbility(ped.Handle, 100);
            ped.Armor = random.Next(100);

            Blip blip = ped.AttachBlip();
            blip.Color = BlipColor.Red;
            blip.Sprite = BlipSprite.Crosshair2;

            peds.Add(ped);
        }

        static async Task PedTick()
        {
            for(var i = 0; i < peds.Count; i++)
            {
                if (peds[i].IsDead)
                {
                    peds[i].AttachedBlip.Delete();
                    peds.Remove(peds[i]);
                }
            }
            await Task.FromResult(0);
        }
    }
}
