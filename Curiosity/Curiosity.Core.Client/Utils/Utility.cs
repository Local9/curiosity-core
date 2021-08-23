using CitizenFX.Core;
using Curiosity.Core.Client.Exceptions;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Utils
{
    static class Utility
    {
        public static async Task<Model> LoadModel(string modelHash)
        {
            Model model = new Model(modelHash);

            if (!model.IsInCdImage) throw new CitizenFxException($"Model is not loaded, please open a support ticket.");

            DateTime initDate = DateTime.UtcNow.AddSeconds(5);

            await model.Request(1000);

            while (!model.IsLoaded)
            {
                await model.Request(1000);

                if (DateTime.UtcNow > initDate) break;
            }

            if (!model.IsLoaded) throw new CitizenFxException("Model did not load in time.");

            if (!model.IsValid) throw new CitizenFxException("Model is invalid, please open a support ticket.");

            return model;
        }
    }
}
