using System;
using System.Threading.Tasks;
using CitizenFX.Core;

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

        public static async Task<Entity.User> GetUserAsync(string license, Player player)
        {
            return await Database.DatabaseUsers.GetUserWithCharacterAsync(license, player);
        }

        public static async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            return await Database.DatabaseUsers.GetUserLocationAsync(locationId);
        }

        public static async Task SavePlayerLocationAsync(string license, float x, float y, float z)
        {
            if (!Classes.SessionManager.PlayerList.ContainsKey(license)) return;

            Player player = Classes.SessionManager.PlayerList[license].Player;

            Entity.User user = await Database.DatabaseUsers.GetUserWithCharacterAsync(license, player);

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
