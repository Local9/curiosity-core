using System;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Server.Net.Business
{
    public class BusinessUser : BaseScript
    {
        Database.DatabaseUser databaseUser;

        static BusinessUser businessUser;

        public static BusinessUser GetInstance()
        {
            return businessUser;
        }

        public BusinessUser()
        {
            databaseUser = Database.DatabaseUser.GetInstance();
            businessUser = this;
        }

        public async Task TestQueryAsync()
        {
            await databaseUser.TestQueryAsync();
        }

        public async Task<Entity.User> GetUserIdAsync(string steamId)
        {
            return await databaseUser.GetUserIdAsync(steamId);
        }

        public async Task<Vector3> GetUserPositionAsync(long locationId)
        {
            return await databaseUser.GetUserPositionAsync(locationId);
        }
    }
}
