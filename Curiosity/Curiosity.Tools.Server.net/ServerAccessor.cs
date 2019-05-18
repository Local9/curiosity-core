namespace Curiosity.Tools.Server.net
{
	public class ServerAccessor
	{
		protected Server Server { get; }

		public ServerAccessor( Server server ) {
			Server = server;
		}
	}
}
