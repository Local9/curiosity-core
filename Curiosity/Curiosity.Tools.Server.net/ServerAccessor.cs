using CitizenFX.Core;

namespace Curiosity.Tools.Server.net
{
	public class ServerAccessor : BaseScript
	{
		protected Server Server { get; }

		public ServerAccessor( Server server ) {
			Server = server;
		}
	}
}
