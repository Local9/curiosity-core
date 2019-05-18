using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Tools.Server.net.Helpers;

namespace Curiosity.Tools.Server.net
{
	public class VersionCheck : ServerAccessor
	{
		private const int Version = 101;

		private bool _needsUpdate;

		public VersionCheck( Server server ) : base( server ) {
			Task.Factory.StartNew( async () => {
				if( bool.TryParse( API.GetConvar( "DevTools.CheckVersion", "true" ), out var doCheck ) && !doCheck ) {
					Log.Verbose( "Suppressing version check for DevTools. This feature is not recommended." );
					return;
				}

				if( !int.TryParse( await Server.Http.DownloadString( "https://raw.githubusercontent.com/MoosheTV/DevTools/master/version" ),
						out var version ) ) {
					Log.Error( "\r\n***\r\n[DevTools] Failed to check for updates.\r\n***\r\n" );
					return;
				}

				if( Version != version ) {
					Log.Warn( "\r\n***\r\n[DevTools] A new update is available for DevTools! Please inform 127.0.0.1 \r\n\r\n***\r\n" );
					_needsUpdate = true;
				}
				else {
					Log.Verbose( $"[DevTools] You have the latest version of DevTools ({version})" );
				}
			} );
			Server.RegisterEventHandler("curiosity:Tools:Player:Ready", new Action<Player>( OnClientReady ) );
		}

		private void OnClientReady( [FromSource] Player source ) {
			try {
				if( !_needsUpdate ) return;

				source.TriggerEvent( "UI.ShowNotification", "~y~WARNING~s~:~n~Your version of DevTools is ~r~Outdated~s~. Please inform 127.0.0.1");
			}
			catch( Exception ex ) {
				Log.Error( ex );
			}
		}
	}
}
