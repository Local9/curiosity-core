using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MobilePhone.Client.net.ClientData
{
    public class Phone
    {
		dynamic data;
		public List<Phone_data> phone_data = new List<Phone_data>();
		public bool Visible { get; set; } = false;
		public bool SleepMode { get; set; } = false;
		public Scaleform Scaleform { get; set; } = new Scaleform("cellphone_ifruit");
		public int VisibleAnimProgress { get; set; }
		public bool InCall { get; set; } = false;
		public Phone()
		{
			phone_data.Add(new Phone_data());
		}
		public Phone(dynamic result)
		{
			data = JsonConvert.DeserializeObject(result.phone_data);
			for (int i = 0; i < data.Count; i++)
				phone_data.Add(new Phone_data(data[i]));
		}

		public Phone_data getCurrentCharPhone()
		{
			for (int i = 0; i < phone_data.Count; i++)
			{
				if ((Eventi.Data.char_current - 1) == phone_data[i].id - 1)
					return phone_data[i];
			}
			return null;
		}

		public async void Kill()
		{
			Scaleform.CallFunction("SHUTDOWN_MOVIE");
			await BaseScript.Delay(50);
			Scaleform.Dispose();
			DestroyMobilePhone();
			Visible = false;
			Game.PlayerPed.SetConfigFlag(242, true);
			Game.PlayerPed.SetConfigFlag(243, true);
			Game.PlayerPed.SetConfigFlag(244, false);
		}

		public async Task AnimationIn()
		{
			if (Game.PlayerPed.IsDead) { return; }
			Visible = true;
		}
	}

	public class Phone_data
	{
		public int id { get; set; } = 1;
		public int Theme { get; set; } = 1;
		public int Wallpaper { get; set; } = 4;
		public int Ringtone { get; set; } = 0;
		public int SleepMode { get; set; } = 0;
		public int Vibration { get; set; } = 1;
		public string Numero { get; private set; } = "0";

		public List<Contatto> contatti = new List<Contatto>()
		{
			new Contatto("Aggiungi Contatto", "CHAR_MULTIPLAYER", false, "", 1),
			new Contatto("Polizia", "CHAR_CALL911", false, "polizia", 2),
			new Contatto("Medico", "CHAR_CALL911", false, "medici", 3),
			new Contatto("Meccanico", "CHAR_LS_CUSTOMS", false, "meccanico", 4),
			new Contatto("Taxi", "CHAR_TAXI", false, "taxi", 5),
			new Contatto("Concessionario", "CHAR_CARSITE2", false, "concessionario", 6),
			new Contatto("Agente Immobiliare", "CHAR_PEGASUS_DELIVERY", false, "immobiliare", 7),
			new Contatto("Reporter", "CHAR_LIFEINVADER", false, "reporter", 8),
		};

		public List<Message> messaggi = new List<Message>();

		public Phone_data() { }

		public Phone_data(dynamic result)
		{
			Theme = (int)result["Theme"].Value;
			Wallpaper = (int)result["WallPaper"].Value;
			if (result["contatti"].HasValues)
				for (int i = 0; i < result["contatti"].Count; i++)
					contatti.Add(new Contatto(result["contatti"]["Name"].Value, result["contatti"]["Icon"].Value, result["contatti"]["TelephoneNumber"].Value, result["contatti"]["IsPlayer"].Value, result["contatti"]["PlayerIndex"].Value, result["contatti"]["Player"].Value));
			if (result["messaggi"].HasValues)
				for (int i = 0; i < result["messaggi"].Count; i++)
					messaggi.Add(new Message(result["messaggi"]["From"].Value, result["messaggi"]["Title"].Value, result["messaggi"]["Messaggio"].Value, (DateTime)result["messaggi"]["Data"].Value));
		}
	}
}
