using CitizenFX.Core;
using Curiosity.MobilePhone.Client.net.ClientData;
using Curiosity.MobilePhone.Client.net.Models;
using System.Threading.Tasks;


namespace Curiosity.MobilePhone.Client.net.Apps
{
    public class Messages : App
	{
		private int SelectedItem { get; set; } = 0;
		private static bool FirstTick = true;

		public Messages(Phone phone) : base("Messaggi", 2, phone)   // 8
		{

		}

		public override async Task Tick()
		{
			if (FirstTick)
			{
				FirstTick = false;
				await BaseScript.Delay(100);
				return;
			}
			var appName = "Messaggi";
			Phone.Scaleform.CallFunction("SET_HEADER", appName);

		}

		public override void Initialize(Phone phone)
		{
			Phone = phone;
			FirstTick = true;
			SelectedItem = 0;
		}

		public override void Kill()
		{
		}

	}
}
