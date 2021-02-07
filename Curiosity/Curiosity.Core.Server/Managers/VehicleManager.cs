using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("vehicle:log:player", new EventCallback(metadata =>
            {
                int netId = metadata.Find<int>(0);
                PluginManager.ActiveUsers[metadata.Sender].PersonalVehicle = netId;
                Logger.Debug($"vehicle:log:player -> {metadata.Sender} - Vehicle: {netId}");
                return false;
            }));

            //EventSystem.GetModule().Attach("vehicle:spawn", new EventCallback(metadata =>
            //{
            //    if (arguments.Count <= 0) return;
            //    var model = API.GetHashKey(arguments.ElementAt(0));
            //    float x = arguments.ElementAt(1).ToFloat();
            //    float y = arguments.ElementAt(2).ToFloat();
            //    float z = arguments.ElementAt(3).ToFloat();

            //    Vector3 pos = new Vector3(x, y, z);
            //    int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);
            //    return null
            //}));

            EventSystem.GetModule().Attach("vehicle:refuel:charge", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle)) return false;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                float vehicleFuel = metadata.Find<float>(0);
                float cost = (100.0f - vehicleFuel) * 1.35f;

                bool canPay = (curiosityUser.Character.Cash - cost) >= 0;

                if (!canPay) return false;

                await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, (int)cost * -1);

                return true;
            }));

            EventSystem.GetModule().Attach("vehicle:owner", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                foreach (KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    CuriosityUser user = kvp.Value;

                    if (user.PersonalVehicle == networkId)
                        return user.LatestName;
                }

                return null;
            }));

            EventSystem.GetModule().Attach("vehicle:tow", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                CommonErrors cannotTow = CommonErrors.UnknownError;

                foreach (KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    CuriosityUser user = kvp.Value;

                    if (user.PersonalVehicle == networkId)
                        cannotTow = CommonErrors.VehicleIsOwned;
                }

                if (cannotTow == CommonErrors.VehicleIsOwned)
                    return CommonErrors.VehicleIsOwned;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                int rep = await Database.Store.StatDatabase.Get(curiosityUser.Character.CharacterId, Stat.POLICE_REPUATATION);

                if (rep > 1000)
                {
                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Get(curiosityUser.Character.CharacterId);

                    if (curiosityUser.Character.Cash < 1000)
                        return CommonErrors.PurchaseUnSuccessful;

                    curiosityUser.Character.Cash = await Database.Store.BankDatabase.Adjust(curiosityUser.Character.CharacterId, -1000);

                    EntityManager.EntityInstance.NetworkDeleteEntity(networkId);
                    return CommonErrors.PurchaseSuccessful;
                }

                return CommonErrors.NotEnoughPoliceRep1000;
            }));

            EventSystem.GetModule().Attach("vehicle:spawn", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;

                if (!PluginManager.ActiveUsers.ContainsKey(senderHandle)) return null;

                Player player = PluginManager.PlayersList[senderHandle];

                var model = API.GetHashKey(metadata.Find<string>(0));

                Vector3 pos = player.Character.Position;
                int vehicleId = API.CreateVehicle((uint)model, pos.X, pos.Y, pos.Z, player.Character.Heading, true, true);

                if (vehicleId == 0)
                {
                    Logger.Debug($"Possible OneSync is Disabled");
                    return null;
                }

                DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);

                while (!API.DoesEntityExist(vehicleId))
                {
                    await BaseScript.Delay(0);

                    if (maxWaitTime < DateTime.UtcNow) break;
                }

                if (!API.DoesEntityExist(vehicleId))
                {
                    Logger.Debug($"Failed to create vehicle in timely manor.");
                    return null;
                }

                return API.NetworkGetNetworkIdFromEntity(vehicleId);
            }));
        }
    }
}
