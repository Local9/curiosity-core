using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Server.net.Business
{
    public class BusinessUser : BaseScript
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

        public async Task TestQueryAsync()
        {
            await Database.DatabaseUsers.TestQueryAsync();
        }

        public async Task<Entity.User> GetUserAsync(string steamId)
        {
            return await Database.DatabaseUsers.GetUserAsync(steamId);
        }

        public async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            return await Database.DatabaseUsers.GetUserLocationAsync(locationId);
        }

        public async Task SavePlayerLocationAsync(string steamId, float x, float y, float z)
        {
            Entity.User user = await Database.DatabaseUsers.GetUserAsync(steamId);

            if (user.LocationId == 1)
            {
                user.LocationId = await Database.DatabaseUsers.SaveLocationAsync(2, x, y, z);
                await Delay(0);
                await Database.DatabaseUsers.UpdateUserLocationId(user.UserId, user.LocationId);
            }
            else
            {
                await Database.DatabaseUsers.UpdatePlayerLocation(user.LocationId, x, y, z);
            }
        }
    }
}
