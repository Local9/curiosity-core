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

        public async Task<Entity.User> GetUserAsync(string license)
        {
            return await Database.DatabaseUsers.GetUserAsync(license);
        }

        public async Task<Vector3> GetUserLocationAsync(long locationId)
        {
            return await Database.DatabaseUsers.GetUserLocationAsync(locationId);
        }

        public async Task SavePlayerLocationAsync(string license, float x, float y, float z)
        {
            Entity.User user = await Database.DatabaseUsers.GetUserAsync(license);

            if (user.LocationId == 1)
            {
                long id = await Database.DatabaseUsers.SaveLocationAsync(2, x, y, z);

                user.LocationId = (int)id;

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
