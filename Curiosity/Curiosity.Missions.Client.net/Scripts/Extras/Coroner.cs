using CitizenFX.Core;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.Wrappers;

namespace Curiosity.Missions.Client.net.Scripts.Extras
{
    class Coroner
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            RegisterCommand("ems", new Action(RequestService), false);
        }

        // STATE
        static public bool IsServiceActive = false;
        
        // ENTITIES
        static Ped PedToRecover;

        static public void RequestService()
        {
            try
            {
                Helpers.Animations.AnimationRadio();

                if (IsServiceActive)
                {
                    Wrappers.Helpers.ShowNotification("Coroner", "Service Unavailable", string.Empty);
                    return;
                }

                int spawnDistance = Client.Random.Next(100, 200);
                RaycastResult raycastResult = World.RaycastCapsule(Game.PlayerPed.Position, Game.PlayerPed.Position, 2.0f, IntersectOptions.Peds1, Game.Player.Character);
                if (raycastResult.DitHitEntity)
                {
                    if (raycastResult.HitEntity.Model.IsPed)
                    {
                        PedToRecover = raycastResult.HitEntity as Ped;

                        if (PedToRecover.IsAlive)
                        {
                            Wrappers.Helpers.ShowNotification("Coroner", "Really...", "We don't pick up the living.");
                            Reset();
                            return;
                        }

                        if (!PedToRecover.Exists())
                        {
                            Wrappers.Helpers.ShowNotification("Coroner", "Ped not found.", string.Empty);
                            Reset();
                            return;
                        }

                        Client.TriggerEvent("curiosity:interaction:coroner", PedToRecover.NetworkId); // PED UPDATE

                        CleanUpPed();
                    }
                }
            }
            catch (Exception ex)
            {
                IsServiceActive = false;
            }
        }

        static async void CleanUpPed()
        {
            NetworkFadeOutEntity(PedToRecover.Handle, true, false);
            await Client.Delay(1000);

            if (PedToRecover.IsPlayer)
            {
                Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Update", $"Sorry, this one is lost.");
            }
            else
            {
                PedToRecover.Delete();
            }
            
            Reset(true);
        }

        static public async void Reset(bool validCleanup = false)
        {
            if (validCleanup)
            {
                Wrappers.Helpers.ShowNotification("Dispatch", "Scene has been cleared", $"");
            }

            client.RegisterTickHandler(OnCooldownTask);
        }

        static async Task OnCooldownTask()
        {
            await Task.FromResult(0);
            int countdown = 60000;

            long gameTime = GetGameTimer();

            while ((GetGameTimer() - gameTime) < countdown)
            {
                await Client.Delay(500);
            }

            IsServiceActive = false;
            client.DeregisterTickHandler(OnCooldownTask);
            Wrappers.Helpers.ShowNotification("Dispatch", "Coroner Available", $"");
        }
    }
}
