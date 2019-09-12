using CitizenFX.Core;
using Curiosity.MobilePhone.Client.net.ClientData;
using Curiosity.MobilePhone.Client.net.Models;
using System.Threading.Tasks;


namespace Curiosity.MobilePhone.Client.net.Apps
{
    class QuickSave : App
	{
		public QuickSave(Phone phone) : base("Salvataggio Rapido", 43, phone)
		{

		}

		private static bool FirstTick = true;
		public override async Task Tick()
		{
			if (FirstTick)
			{
				FirstTick = false;
				await BaseScript.Delay(100);
				return;
			}
		}

		public override async void Initialize(Phone phone)
		{
			Phone = phone;
			BaseScript.TriggerServerEvent("lprp:salvaPlayer");
			BaseScript.TriggerEvent("lprp:phone_start", "Main");
			await BaseScript.Delay(5000);
			Func.ShowNotification("Salvataggio Completato", Func.NotificationColor.GreenDark);

		}

		public override void Kill()
		{
		}

	}
}
