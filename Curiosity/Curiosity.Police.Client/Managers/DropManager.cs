using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using Curiosity.Police.Client.Extensions;

namespace Curiosity.Police.Client.Managers
{
    public class DropManager : Manager<DropManager>
    {
        public override void Begin()
        {
            
        }

        //[TickHandler]
        //private async Task OnControl()
        //{
        //    if (Game.IsControlJustPressed(0, Control.Jump))
        //    {
        //        Vector3 spawnPosition = Game.PlayerPed.Position + Game.PlayerPed.UpVector * 80f + Game.PlayerPed.ForwardVector * -35f;
        //        float heading = (Game.PlayerPed.Position - spawnPosition).ToHeading();

        //        Vehicle plane = await CreatePlane("cuban800", spawnPosition, heading: heading, pilotModel: "s_m_m_pilot_02");

        //        if (plane is null)
        //        {
        //            Debug.WriteLine("Plane failed to create");
        //            return;
        //        }

        //        Ped pilot = plane.Driver;

        //        Vector3 flyTo = LerpByDistance(Game.PlayerPed.Position, spawnPosition, 5000f);

        //        plane.Speed = 50f;
        //        plane.IsEngineRunning = true;

        //        SetVehicleLandingGear(plane.Handle, 3);

        //        Blip planeBlip = plane.AttachBlip();
        //        planeBlip.Sprite = BlipSprite.Plane;
        //        planeBlip.Name = "Resupply Plane";
        //        planeBlip.Color = BlipColor.FranklinGreen;

        //        await BaseScript.Delay(500);

        //        TaskPlaneMission(pilot.Handle, plane.Handle, 0, 0, flyTo.X, flyTo.Y, flyTo.Z, 4, 100f, 0f, 90f, 2000f, 50f);
        //    }
        //}

        //Vector3 LerpByDistance(Vector3 a, Vector3 b, float x)
        //{
        //    Vector3 p = x * Vector3.Normalize(b - a) + a;
        //    return p;
        //}

        //async Task<Vehicle> CreatePlane(string vehicleModel, Vector3 position, float heading = 0f, string pilotModel = "")
        //{
        //    Model model = await RequestModel(vehicleModel);
        //    if (model is null) return null;
        //    if (!(model?.IsVehicle ?? false)) return null; // isn't a vehicle

        //    Vehicle vehicle = await World.CreateVehicle(model, position);

        //    if (!string.IsNullOrEmpty(pilotModel))
        //    {
        //        Model pilot = await RequestModel(pilotModel);
        //        vehicle.CreatePedOnSeat(VehicleSeat.Driver, pilot);
        //        pilot.MarkAsNoLongerNeeded();
        //    }
            
        //    if (heading > 0f)
        //        vehicle.Heading = heading;

        //    model.MarkAsNoLongerNeeded();

        //    return vehicle;
        //}

        //async Task<Model> RequestModel(string modelStr)
        //{
        //    int modelHash = GetHashKey(modelStr);
        //    Model model = new Model(modelHash);

        //    if (!model.IsLoaded) await model.Request(10000);
        //    if (!(model?.IsLoaded ?? false)) return null; // failed to load

        //    return model;
        //}
    }
}
