using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Core.Client.Managers
{
    public class ConfigurationManager : Manager<ConfigurationManager>
    {
        ClientConfig _configCache;
        List<int> PropsToDeleteHash = new List<int>();

        public override void Begin()
        {

        }

        private ClientConfig GetConfig()
        {
            ClientConfig config = new();

            try
            {
                if (_configCache is not null)
                    return _configCache;

                _configCache = JsonConvert.DeserializeObject<ClientConfig>(Properties.Resources.config);
                return _configCache;
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public List<Job> Jobs()
        {
            return GetConfig().Jobs;
        }

        public List<string> VehiclesToSuppress()
        {
            return GetConfig().VehiclesToSuppress;
        }

        public List<string> PartyPeds()
        {
            return GetConfig().PartyPeds;
        }

        public List<Companion> SupporterCompanions()
        {
            if (GetConfig().Supporter.Companions is null)
                return new List<Companion>();

            return GetConfig().Supporter.Companions;
        }

        public List<SupporterModel> SupporterModels()
        {
            if (GetConfig().Supporter.Companions is null)
                return new List<SupporterModel>();

            return GetConfig().Supporter.SupporterModels;
        }

        public List<string> ActiveIpls()
        {
            if (GetConfig().Milos.ActiveIpls is null)
                return new List<string>();

            return GetConfig().Milos.ActiveIpls;
        }

        public List<string> CayoIslandIpls()
        {
            if (GetConfig().Milos.CayoLOD is null)
                return new List<string>();

            return GetConfig().Milos.CayoLOD;
        }

        public List<string> LosSantosIpls()
        {
            if (GetConfig().Milos.LosSantosLOD is null)
                return new List<string>();

            return GetConfig().Milos.LosSantosLOD;
        }

        public List<string> EletricVehicles()
        {
            if (GetConfig().EletricVehicles is null)
                return new List<string>();

            return GetConfig().EletricVehicles;
        }

        public List<int> PropsToDelete()
        {
            if (GetConfig().PropsToDelete is null)
                return new List<int>();

            if (PropsToDeleteHash.Count == 0)
            {
                foreach (string prop in GetConfig().PropsToDelete)
                {
                    int hash = API.GetHashKey(prop);
                    if (!PropsToDeleteHash.Contains(hash))
                    {
                        PropsToDeleteHash.Add(hash);
                    }
                }
            }

            return PropsToDeleteHash;
        }
    }
}
