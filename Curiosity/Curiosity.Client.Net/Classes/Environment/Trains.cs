using CitizenFX.Core;
using Curiosity.Shared.Client.net;
using System;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment
{
    class Trains
    {
        static Client client = Client.GetInstance();
        // vehicles
        static Model metroModel;
        static VehicleHash metro = VehicleHash.MetroTrain;

        static public void Init()
        {
            client.RegisterEventHandler("environment:client:train:activate", new Action(OnActivateTrains));
        }

        static async void OnActivateTrains()
        {
            metroModel = metro;
            await metroModel.Request(10000);

            while (!metroModel.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            int metroTrain = CreateMissionTrain(24, 40.2f, -1201.3f, 31.0f, true);
            int metroTrain2 = CreateMissionTrain(24, -618.0f, -1476.8f, 16.2f, true);
            CreatePedInsideVehicle(metroTrain, 26, (uint)PedHash.Lsmetro01SMM, -1, true, true);
            CreatePedInsideVehicle(metroTrain2, 26, (uint)PedHash.Lsmetro01SMM, -1, true, true);
            SetEntityAsMissionEntity(metroTrain, true, true);
            SetEntityAsMissionEntity(metroTrain2, true, true);
            // SPEED
            SetTrainCruiseSpeed(metroTrain, 20f);
            SetTrainCruiseSpeed(metroTrain2, 20f);
            SetTrainSpeed(metroTrain, 20f);
            SetTrainSpeed(metroTrain2, 20f);
            // LOD
            SetEntityLodDist(metroTrain, 500);
            SetEntityLodDist(metroTrain2, 500);

            Log.Verbose("Train service active");
        }
    }
}
