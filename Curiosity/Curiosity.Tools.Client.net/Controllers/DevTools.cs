﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Tools.Client.net.Helpers;
using Curiosity.Tools.Client.net.Menus;

namespace Curiosity.Tools.Client.net.Controllers
{
	public class DevTools : ClientAccessor
	{
		public bool KeyCodeTest { get; set; }

		private DateTime _lastCollection = DateTime.UtcNow;
		private readonly List<KeyCodeEvent> _keyEvents = new List<KeyCodeEvent>();

		private Player _currentSpectate = null;
        private Vector3 _originalPosition = Vector3.Zero;

		public DevTools( Client client ) : base( client ) {
			var menu = new Menu( "Mooshe's DevTools" );

			var playerMenu = new MenuItemSubMenu( client, menu, new PlayerMenu( client, menu ), "Player Settings" );
			menu.Add( playerMenu );

			var mpMenu = new MenuItemSubMenu( client, menu, new MpMenu( client, menu ), "Multiplayer Settings" );
			menu.Add( mpMenu );

			var timeMenu = new MenuItemSubMenu( client, menu, new TimeMenu( client, menu ), "Time Settings" );
			menu.Add( timeMenu );

			var vehMenu = new MenuItemSubMenu( client, menu, new VehicleMenu( client, menu ), "Vehicle Settings" );
			menu.Add( vehMenu );

			var worldMenu = new MenuItemSubMenu( client, menu, new WorldMenu( client, menu ), "World Settings" );
			menu.Add( worldMenu );

			var hudMenu = new MenuItemSubMenu( client, menu, new HudMenu( client, menu ), "HUD Settings" );
			menu.Add( hudMenu );

			var iplMenu = new MenuItemSubMenu( client, menu, new InteriorMenu( client, menu ), "Interior Settings" );
			menu.Add( iplMenu );

			var keyCode = new MenuItemCheckbox( client, menu, "Keycode Tester" ) {
				IsChecked = () => KeyCodeTest
			};
			keyCode.Activate += () => {
				KeyCodeTest = !KeyCodeTest;
				return Task.FromResult( 0 );
			};
			menu.Add( keyCode );

			Client.Menu.RegisterMenuHotkey( Control.ReplayStartStopRecording, menu ); // F1

			Client.RegisterTickHandler( OnKeyCodeTick );
			Client.RegisterTickHandler( OnKeyCodeRender );
            Client.RegisterTickHandler( OnIsSpectating );

            Client.RegisterEventHandler( "UI.ShowNotification", new Action<string>( OnNotification ) );
			Client.RegisterEventHandler("curiosity:Tools:Player:Bring", new Action<int, float, float, float>( OnPlayerBring ) );
		}

        private async Task OnIsSpectating()
        {
            while (true)
            {
                if (_currentSpectate != null)
                {
                    Game.PlayerPed.Position = new Vector3(_currentSpectate.Character.Position.X, _currentSpectate.Character.Position.Y, -10);
                }
                await Client.Delay(0);
            }
        }


        private void OnPlayerBring( int serverId, float x, float y, float z ) {
			try {
				var target = new PlayerList().FirstOrDefault( p => p.ServerId == serverId );
				if( target != null ) {
					Game.PlayerPed.PositionNoOffset = new Vector3( x, y, z );
					UiHelper.ShowNotification( $"You were brought by ~g~{target.Name}~s~." );
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		public async Task Spectate( Player player ) {
			API.DoScreenFadeOut( 200 );
			await BaseScript.Delay( 200 );

            int playerPedId = API.GetPlayerPed(player.Handle);

            if ( _currentSpectate != null && _currentSpectate == player ) {

                API.NetworkSetInSpectatorMode(false, playerPedId);

                // API.FreezeEntityPosition(Game.PlayerPed.Handle, false);
                API.SetEntityCollision(Game.PlayerPed.Handle, true, true);
                Game.Player.IsInvincible = false;
                Game.PlayerPed.IsVisible = true;

                Game.PlayerPed.Detach();
                Game.PlayerPed.Position = _originalPosition;
                
                _originalPosition = Vector3.Zero;
                _currentSpectate = null;

				await BaseScript.Delay( 50 );

                API.DoScreenFadeIn(200);

                return;
			}

            API.ClearPlayerWantedLevel(Game.Player.Handle);

            if (_originalPosition == Vector3.Zero)
                _originalPosition = Game.PlayerPed.Position;

            _currentSpectate = player;

            // API.FreezeEntityPosition(Game.PlayerPed.Handle, true);
            API.SetEntityCollision(Game.PlayerPed.Handle, false, true);
            Game.Player.IsInvincible = true;
            Game.PlayerPed.IsVisible = false;

            Vector3 entityCords = API.GetEntityCoords(playerPedId, true);

            API.RequestCollisionAtCoord(entityCords.X, entityCords.Y, entityCords.Z);
            API.NetworkSetInSpectatorMode(true, playerPedId);

            API.DoScreenFadeIn( 200 );
			await BaseScript.Delay( 50 );
		}

		private void OnNotification( string msg ) {
			UiHelper.ShowNotification( msg );
			API.PlaySoundFrontend( -1, "Event_Message_Purple", "GTAO_FM_Events_Soundset", true );
		}

		private async Task OnKeyCodeRender() {
			try {
				if( !KeyCodeTest ) {
					await BaseScript.Delay( 100 );
					return;
				}

				var offsetY = 0f;
				foreach( var key in new List<KeyCodeEvent>( _keyEvents ) ) {
					var secs = (float)(DateTime.UtcNow - key.Time).TotalSeconds;
					offsetY += 0.024f * MathUtil.Clamp( secs * 4f, 0f, 1f );

					var alpha = Math.Pow( Math.Sin( MathUtil.Clamp( secs / 5f, 0f, 1f ) ), 1f / 16f ) * 255f;

					var color = Color.FromArgb( (int)Math.Ceiling( alpha ), 255, 255, 255 );

					UiHelper.DrawText( $@"{{{string.Join( ", ", key.Controls )}}}", new Vector2( 0.55f, 1f ) - new Vector2( 0f, offsetY ), color, 0.3f );
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		private async Task OnKeyCodeTick() {
			try {
				if( !KeyCodeTest ) {
					await BaseScript.Delay( 100 );
					return;
				}

				var key = new KeyCodeEvent();
				var vals = Enum.GetValues( typeof( Control ) );
				var count = 0;
				foreach( Control ctrl in vals ) {
					if( Game.IsControlJustPressed( 2, ctrl ) )
						key.Controls.Add( $"{Enum.GetName( typeof( Control ), ctrl ) ?? "UNK_KEY"}[{count}]" );
					count++;
				}

				if( key.Controls.Any() ) {
					_keyEvents.Add( key );
					Log.Info( $"[KeyCode] {{ {string.Join( ", ", key.Controls )} }}" );
				}

				// Clean up list every 5 seconds
				if( (DateTime.UtcNow - _lastCollection).TotalSeconds > 1f ) {
					_keyEvents.RemoveAll( e => (DateTime.UtcNow - e.Time).TotalSeconds > 5f );
					_lastCollection = DateTime.UtcNow;
				}
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}

		private class KeyCodeEvent
		{
			public DateTime Time { get; set; } = DateTime.UtcNow;
			public List<string> Controls { get; set; } = new List<string>();
		}
	}
}
