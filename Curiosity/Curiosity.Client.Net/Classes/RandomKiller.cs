using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes
{
    internal class RandomKiller
    {
        static Client Instance = Client.GetInstance();
        private static int gameTime;
        private static Ped killer;

        internal static void Init()
        {
            gameTime = API.GetGameTimer();
            Instance.RegisterTickHandler(OnRandomPed);
            Instance.RegisterTickHandler(OnRandomPedCleanUp);
        }

        private async static Task OnRandomPedCleanUp()
        {
            if (killer == null) return;

            if (killer.IsDead)
            {
                await Client.Delay(10000);

                API.NetworkFadeOutEntity(killer.Handle, false, false);
                await Client.Delay(3000);
                killer?.Delete();
            }
        }

        private async static Task OnRandomPed()
        {
            while ((API.GetGameTimer() - gameTime) < 60000)
            {
                await Client.Delay(1000);
            }

            gameTime = API.GetGameTimer();

            if (Game.PlayerPed.IsInVehicle())
            {
                if (Game.PlayerPed.CurrentVehicle.Speed > 4f)
                    return;
            }

            if (Utility.RANDOM.Bool(0.05f))
            {
                Model model = PedHash.ChemWork01GMM;
                await model.Request(10000);

                while (!model.IsLoaded)
                {
                    await Client.Delay(100);
                }

                Vector3 offset = new Vector3(-5f, -5f, 0f);
                Vector3 pos = Game.PlayerPed.GetOffsetPosition(offset);
                float groundZ = pos.Z;

                if (API.GetGroundZFor_3dCoord(pos.X, pos.Y, pos.Z, ref groundZ, false))
                    pos.Z = groundZ;


                int pedId = API.CreatePed((int)PedTypes.PED_TYPE_CRIMINAL, (uint)model.Hash, pos.X, pos.Y, pos.Z, 0f, false, false);

                API.NetworkFadeOutEntity(pedId, false, false);

                model.MarkAsNoLongerNeeded();

                killer = new Ped(pedId);
                if (Game.PlayerPed.IsInVehicle())
                {
                    killer.Weapons.Give(WeaponHash.Pistol, 1, true, true);
                }
                else
                {
                    killer.Weapons.Give(WeaponHash.Knife, 1, true, true);
                }
                killer.Armor = 200;
                killer.Health = 200;
                killer.DropsWeaponsOnDeath = false;

                killer.Task.ShootAt(Game.PlayerPed);
            }
        }
    }
}
