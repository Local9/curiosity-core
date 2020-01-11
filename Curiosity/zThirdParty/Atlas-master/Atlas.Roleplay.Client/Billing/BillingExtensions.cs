using Atlas.Roleplay.Library.Billing;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Billing
{
    public static class BillingExtensions
    {
        public static void LookAt(this Bill bill)
        {
            API.SendNuiMessage(new JsonBuilder().Add("Operation", "SHOW_BILL").Add("Bill", bill).Build());
            API.SetNuiFocus(true, true);
        }
    }
}