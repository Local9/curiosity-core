namespace Curiosity.Tools.Client.net
{
	public class ClientAccessor
	{
		protected Client Client { get; }

		public ClientAccessor( Client client ) {
			Client = client;
		}
	}
}
