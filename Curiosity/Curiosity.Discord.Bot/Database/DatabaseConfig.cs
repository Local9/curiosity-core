using Curiosity.LifeV.Bot.Entities;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Database
{
    static class DatabaseConfig
    {
        public async static Task<MySqlConnection> GetDatabaseConnection()
        {
            var json = "";
            using (var fs = File.OpenRead(@"config\appsettings.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            DiscordConfiguration discordConfiguration = JsonConvert.DeserializeObject<DiscordConfiguration>(json);

            MySqlConnection connection = new MySqlConnection(discordConfiguration.ConnectionStrings["DefaultConnection"]);

            try
            {
                return connection;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
