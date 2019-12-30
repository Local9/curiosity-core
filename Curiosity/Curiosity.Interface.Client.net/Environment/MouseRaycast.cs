using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Classes.Environment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.net.Environment
{
    class MouseRaycast
    {
        static Client client = Client.GetInstance();

        static DateTime lastRayCast = DateTime.Now;
        static Entity raycastEntity;

        static public void Init()
        {
            client.RegisterTickHandler(TaskAsync);
        }

        static async Task TaskAsync()
        {
            await BaseScript.Delay(0);

            while (Game.IsControlPressed(0, Control.Pickup))
            {
                await BaseScript.Delay(0);

                API.SetNuiFocus(false, true);

                Game.DisableControlThisFrame(0, Control.Aim);
                Game.DisableControlThisFrame(0, Control.Attack);
                Game.DisableControlThisFrame(0, Control.LookLeftRight);
                Game.DisableControlThisFrame(0, Control.LookUpDown);
                API.DisablePlayerFiring(Game.Player.Handle, true);

                if (raycastEntity != null)
                    World.DrawLine(Game.PlayerPed.Position, raycastEntity.Position, System.Drawing.Color.FromArgb(100, 255, 255, 255));

                if (DateTime.Now > lastRayCast)
                {
                    lastRayCast = DateTime.Now.AddMilliseconds(100);
                    RaycastResult raycastResult = ScreenToWorld.Screen2World((IntersectOptions)22, Game.PlayerPed);

                    if (raycastResult.DitHitEntity)
                    {
                        raycastEntity = raycastResult.HitEntity;
                    }
                }
            }
            API.SetNuiFocus(false, false);
            raycastEntity = null;
        }
    }
}
