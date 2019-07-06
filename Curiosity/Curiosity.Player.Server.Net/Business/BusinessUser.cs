using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Server.net.Business
{
    class BusinessUser
    {
        static BusinessUser businessUser;

        public static BusinessUser GetInstance()
        {
            return businessUser;
        }

        public BusinessUser()
        {
            businessUser = this;
        }

        public static async Task TestQueryAsync()
        {
            await Database.DatabaseUsers.TestQueryAsync();
        }

        public static async Task<GlobalEntity.User> GetUserAsync(string license, Player player)
        {
            return await Database.DatabaseUsers.GetUserWithCharacterAsync(license, player);
        }

        public static async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            return await Database.DatabaseUsers.GetUserLocationAsync(locationId);
        }

        public static async Task SavePlayerLocationAsync(string playerHandle, float x, float y, float z)
        {
            if (!Classes.SessionManager.PlayerList.ContainsKey(playerHandle)) return;

            Classes.Session session = Classes.SessionManager.PlayerList[playerHandle];

            GlobalEntity.User user = await Database.DatabaseUsers.GetUserWithCharacterAsync(session.License, session.Player);

            int starterLocation = Server.startingLocationId;

            if (user.LocationId == starterLocation)
            {
                long id = await Database.DatabaseUsers.SaveLocationAsync(2, x, y, z);

                user.LocationId = (int)id;

                await BaseScript.Delay(0);
                await Database.DatabaseUsers.UpdateUserLocationId(user.CharacterId, user.LocationId);
            }
            else
            {
                await Database.DatabaseUsers.UpdatePlayerLocation(user.LocationId, x, y, z);
            }
        }
    }
}
