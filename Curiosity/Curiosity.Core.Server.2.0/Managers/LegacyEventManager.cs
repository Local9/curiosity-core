using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class LegacyEventManager : Manager<LegacyEventManager>
    {
        const int VEHICLE_REPAIR_CHARGE = 100;

        public override void Begin()
        {
            EventSystem.GetModule().Attach("vehicle:tow", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                CommonErrors cannotTow = CommonErrors.UnknownError;

                foreach(KeyValuePair<int, CuriosityUser> kvp in PluginManager.ActiveUsers)
                {
                    CuriosityUser user = kvp.Value;

                    if (user.PersonalVehicle == networkId)
                        cannotTow = CommonErrors.VehicleIsOwned;
                }

                if (cannotTow == CommonErrors.VehicleIsOwned)
                    return CommonErrors.VehicleIsOwned;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[senderHandle];

                int rep = 0;

                if (rep > 1000)
                {
                    // move
                    //bool paymentMade = Instance.ExportDictionary["curiosity-core"].AdjustWallet($"{senderHandle}", 1000, false);

                    //if (paymentMade)
                    //{
                    //    BaseScript.TriggerClientEvent("curiosity:Player:Vehicle:Delete:NetworkId", networkId); // change
                    //    return CommonErrors.PurchaseSuccessful;
                    //}
                    //else
                    //{
                        return CommonErrors.PurchaseUnSuccessful;
                    //}
                }

                return CommonErrors.NotEnoughPoliceRep1000;
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

            EventSystem.GetModule().Attach("vehicle:delete", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;
                int networkId = metadata.Find<int>(0);

                BaseScript.TriggerClientEvent("curiosity:Player:Vehicle:Delete:NetworkId", networkId); // change

                return true;
            }));

            EventSystem.GetModule().Attach("vehicle:repair", new AsyncEventCallback(async metadata =>
            {
                int senderHandle = metadata.Sender;
                var player = PluginManager.PlayersList[metadata.Sender];

                string exportResponse = Instance.ExportDictionary["curiosity-core"].GetUser(player.Handle);

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");
                PluginManager.ActiveUsers.TryUpdate(senderHandle, curiosityUser, curiosityUser);

                if (curiosityUser.Character.Cash < VEHICLE_REPAIR_CHARGE)
                {
                    return false;
                }
                else
                {
                    // charge player
                    return false;
                }
            }));
        }
    }
}
