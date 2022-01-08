using CitizenFX.Core;
using Curiosity.Core.Client.Environment.Data;
using Curiosity.Core.Client.Exceptions;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Utils
{
    static class Utilities
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

        public static void SetCorrectBlipSprite(int ped, int blip, bool isWanted, bool isFriendly)
        {
            SetBlipCrew(blip, isFriendly);
            bool visible = IsEntityVisible(ped);
            SetBlipAlpha(blip, visible ? 255 : 0);

            if (IsPedInAnyVehicle(ped, false) && !IsEntityDead(ped))
            {
                ShowHeadingIndicatorOnBlip(blip, false);

                int vehicle = GetVehiclePedIsIn(ped, false);
                int blipSprite = BlipInfo.GetBlipSpriteForVehicle(vehicle);
                if (GetBlipSprite(blip) != blipSprite)
                {
                    SetBlipSprite(blip, blipSprite);

                    if (isWanted)
                    {
                        SetBlipColour(blip, (int)BlipColor.Red);
                    }

                    if (!isWanted)
                    {
                        SetBlipColour(blip, (int)BlipColor.White);
                    }
                }
            }
            else
            {
                if (IsEntityDead(ped))
                {
                    ShowHeadingIndicatorOnBlip(blip, false);
                    SetBlipSprite(blip, (int)BlipSprite.Dead);
                }
                else if (isWanted)
                {
                    ShowHeadingIndicatorOnBlip(blip, true);
                    SetBlipSprite(blip, 58);
                    SetBlipColour(blip, (int)BlipColor.Red);
                }
                else
                {
                    ShowHeadingIndicatorOnBlip(blip, true);
                    SetBlipSprite(blip, 1);
                    SetBlipColour(blip, (int)BlipColor.White);
                }
            }
        }
    }
}
