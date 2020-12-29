using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Interface.Client.Managers
{
    public class PlayerManager : Manager<PlayerManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("PlayerProfile", new AsyncEventCallback(async metadata =>
            {
                string role = "USER";

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("user:getProfile");

                switch (curiosityUser.Role)
                {
                    case Role.DONATOR_LIFE:
                        role = "LifeV Early Supporter";
                        break;
                    case Role.DONATOR_LEVEL_1:
                        role = "LifeV Supporter I";
                        break;
                    case Role.DONATOR_LEVEL_2:
                        role = "LifeV Supporter II";
                        break;
                    case Role.DONATOR_LEVEL_3:
                        role = "LifeV Supporter III";
                        break;
                    default:
                        role = $"{curiosityUser.Role}".ToLowerInvariant();
                        break;
                }

                string jsn = new JsonBuilder().Add("operation", "PROFILE")
                        .Add("name", curiosityUser.LatestName)
                        .Add("userId", curiosityUser.UserId)
                        .Add("role", role)
                        .Add("wallet", curiosityUser.Wallet)
                        .Build();

                API.SendNuiMessage(jsn);

                return jsn;
            }));
        }
    }
}
