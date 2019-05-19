using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Tools.Client.net.Controllers;
using Curiosity.Tools.Client.net.Menus;
using Curiosity.Shared.Client.net.Models;
using Curiosity.Shared.Client.net.Enums;

// ReSharper disable ClassNeverInstantiated.Global

namespace Curiosity.Tools.Client.net
{
	public class Client : BaseScript
	{
		public DevTools Tools { get; }
		public MenuController Menu { get; }
		public NoclipController NoClip { get; }
		public EntityDebugger Debugger { get; }

		private bool _isInstantiated;

        public bool IsDeveloper = false;

		public Client() {
			Menu = new MenuController( this );
			Tools = new DevTools( this );
			NoClip = new NoclipController( this );
			Debugger = new EntityDebugger( this );

			RegisterEventHandler( "playerSpawned", new Action( OnSpawn ) );
            RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(SetPriviledge));

			if( Game.PlayerPed.Position != default( Vector3 ) )
				OnSpawn();
		}

        private void SetPriviledge(string json)
        {
            PlayerInformationModel playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);
            Privilege p = (Privilege)playerInfo.RoleId;
            IsDeveloper = p.HasFlag(Privilege.IsDeveloper);
        }

        private void OnSpawn() {
			if( !_isInstantiated )
				TriggerServerEvent("curiosity:Tools:Player:Ready");
			_isInstantiated = true;
		}

		public void RegisterEventHandler( string eventName, Delegate action ) {
			EventHandlers[eventName] += action;
		}

		public void RegisterTickHandler( Func<Task> tick ) {
			Tick += tick;
		}

		public void DeregisterTickHandler( Func<Task> tick ) {
			Tick -= tick;
		}
	}
}
