using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class ConfigurationManager : Manager<ConfigurationManager>
    {
        ClientConfig _configCache;

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
    }
}
