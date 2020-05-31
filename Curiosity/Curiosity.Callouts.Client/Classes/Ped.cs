using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Threading.Tasks;

namespace Curiosity.Callouts.Client.Classes
{
    [Serializable]
    internal class Ped
    {
        internal CitizenFX.Core.Ped Fx { get; set; }

        public Vector3 Position => Fx.Position;
        internal Tasks Task => Fx.Task;
        internal bool IsDead => Fx.IsDead;
        internal string Name => Fx.Model.ToString();
        internal bool IsInVehicle { get; set; }

        internal Ped(CitizenFX.Core.Ped fx)
        {
            Fx = fx;

            Fx.AlwaysKeepTask = true;

            API.SetBlockingOfNonTemporaryEvents(Fx.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Fx.Handle, true);
        }

        [Tick]
        internal async void Update()
        {
            await System.Threading.Tasks.Task.FromResult(100);
        }

        private async void ApplyHandcuffs()
        {
            if (Decorators.GetBoolean(Fx.Handle, Decorators.PED_ARREST))
            {
                if (Fx.IsCuffed)
                {
                    API.SetPedFleeAttributes(Fx.Handle, 0, false);

                    Game.PlayerPed.Task.TurnTo(Fx);
                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                    Fx.Task.PlayAnimation("mp_arresting", "idle", 8.0f, -1, (AnimationFlags)49);

                    float position = Fx.IsPlayingAnim("random@arrests@busted", "idle_a") ? 0.3f : 0.65f;
                    API.AttachEntityToEntity(Fx.Handle, Game.PlayerPed.Handle, 11816, 0.0f, position, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                    await BaseScript.Delay(2000);
                    Fx.Detach();
                    Fx.Task.ClearSecondary();
                    Game.PlayerPed.Task.ClearSecondary();

                    API.SetEnableHandcuffs(Fx.Handle, true);
                }
                else
                {
                    Game.PlayerPed.Task.TurnTo(Fx);
                    Game.PlayerPed.Task.PlayAnimation("mp_arresting", "a_uncuff", 8.0f, -1, (AnimationFlags)49);
                    API.AttachEntityToEntity(Fx.Handle, Game.PlayerPed.Handle, 11816, 0.0f, 0.65f, 0.0f, 0.0f, 0.0f, 0.0f, false, false, false, false, 2, true);
                    await BaseScript.Delay(2000);
                    Fx.Detach();
                    Fx.Task.ClearSecondary();
                    Game.PlayerPed.Task.ClearSecondary();
                }
            }
        }

        internal static async Task<Ped> Spawn(Model model, Vector3 position)
        {
            Vector3 sidewalkPosition = position.Sidewalk();
            CitizenFX.Core.Ped fxPed = await World.CreatePed(model, sidewalkPosition);
            Logger.Log(fxPed.ToString());
            var ped = new Ped(fxPed);
            return ped;
        }

        internal void PutInVehicle(Vehicle vehicle, VehicleSeat seat = VehicleSeat.Driver) =>
            Fx.SetIntoVehicle(vehicle.Fx, seat);

        internal Blip AttachBlip(BlipColor color, bool showRoute = false)
        {
            Blip blip = Fx.AttachBlip();
            blip.Color = color;
            blip.ShowRoute = showRoute;

            return blip;
        }

        internal void Dismiss()
        {
            foreach (Blip blip in Fx.AttachedBlips) blip.Delete();
            Fx.IsPersistent = false;
        }
    }
}
