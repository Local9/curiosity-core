using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Server.net.Business
{
    public class BusinessUser : BaseScript
    {
        Database.DatabaseUsers databaseUser;

        static BusinessUser businessUser;

        public static BusinessUser GetInstance()
        {
            return businessUser;
        }

        public BusinessUser()
        {
            databaseUser = Database.DatabaseUsers.GetInstance();
            businessUser = this;
        }

        public async Task TestQueryAsync()
        {
            await databaseUser.TestQueryAsync();
        }

        public async Task<Entity.User> GetUserAsync(string steamId)
        {
            return await databaseUser.GetUserAsync(steamId);
        }

        public async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            return await databaseUser.GetUserLocationAsync(locationId);
        }

        public async Task SavePlayerLocationAsync(string steamId, float x, float y, float z)
        {
            Entity.User user = await databaseUser.GetUserAsync(steamId);

            if (user.LocationId == 1)
            {
                user.LocationId = await databaseUser.SaveLocationAsync(2, x, y, z);
                await Delay(0);
                await databaseUser.UpdateUserLocationId(user.UserId, user.LocationId);
            }
            else
            {
                await databaseUser.UpdatePlayerLocation(user.LocationId, x, y, z);
            }
        }
    }
}
