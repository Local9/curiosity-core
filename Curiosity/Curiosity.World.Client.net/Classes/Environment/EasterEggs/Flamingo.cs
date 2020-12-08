using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.GameWorld.Client.net.Classes.Environment.EasterEggs
{
    class Flamingo
    {
        static Client client = Client.GetInstance();

        const ObjectHash phFlamingo = ObjectHash.prop_flamingo;

        static List<Tuple<Vector3, float>> lstFlamingoPositions = new List<Tuple<Vector3, float>>()
        {
            new Tuple<Vector3, float>(new Vector3(455.5271f, 5566.663f, 780.1837f), 0f),
            new Tuple<Vector3, float>(new Vector3(-901.8747f, 6051.04f, 43.53199f), 30.82964f),
            new Tuple<Vector3, float>(new Vector3(-1865.116f, -1237.561f, 7.615778f), 99.98402f),
            new Tuple<Vector3, float>(new Vector3(-1274.903f, -1921.969f, 1.730884f), 142.5712f),
            new Tuple<Vector3, float>(new Vector3(2616.285f, 1694.555f, 34.86676f), 47.29236f),
            new Tuple<Vector3, float>(new Vector3(2510.506f, 3787.817f, 50.85282f), 101.1173f),
            new Tuple<Vector3, float>(new Vector3(711.2071f, 1198.193f, 348.5265f), 164.9149f),
            new Tuple<Vector3, float>(new Vector3(-170.7412f, -1003.239f, 299.5232f), 116.2947f),
        };

        static Prop propFlamingo;
        static Model mdFlamingo;

        static DateTime lastCheck = DateTime.Now;

        public static void Init()
        {
            client.RegisterTickHandler(OnFlamingoPropTick);
        }

        static async Task OnFlamingoPropTick()
        {
            if (DateTime.Now.Subtract(lastCheck).TotalSeconds < 10)
            {
                await Client.Delay(1000);

                return;
            }

            lastCheck = DateTime.Now;

            Tuple<Vector3, float> pos = lstFlamingoPositions.Where(x=> x.Item1.Distance(Game.PlayerPed.Position, true) < 20f).FirstOrDefault();

            if (pos == null || pos.Item1.IsZero)
            {
                DespawnProp();
                return;
            }

            if (propFlamingo != null)
            {
                return;
            }

            int rnd = new Random().Next(100);

            if (rnd < 90)
            {
                return;
            }

            mdFlamingo = (int)phFlamingo;
            await mdFlamingo.Request(10000);

            if (!mdFlamingo.IsLoaded)
            {
                await Client.Delay(1000);
                return;
            }

            propFlamingo = await World.CreateProp(mdFlamingo, pos.Item1, false, false);
            propFlamingo.Heading = pos.Item2 - 180.0f;
            propFlamingo.IsPositionFrozen = true;

            mdFlamingo.MarkAsNoLongerNeeded();

            API.NetworkFadeInEntity(propFlamingo.Handle, false);
        }

        static void DespawnProp()
        {
            if (propFlamingo != null)
            {
                if (propFlamingo.Exists())
                {
                    propFlamingo.MarkAsNoLongerNeeded();
                    propFlamingo.Delete();

                    propFlamingo = null;
                }
            }
        }
    }
}
