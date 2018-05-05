using PurePusher.Interfaces;
using PurePusher.Types;
using PureWebSockets;

namespace PurePusher
{
    public class PurePusherClientOptions : PureWebSocketOptions
    {
	    public ISerializer Serializer { get; set; }
	    public IAuthorizer Authorizer { get; set; }
		public const string ClientName = "netstandard_purepusherclient";
	    public bool Encrypted { get; set; }
		public string Host = "ws.pusherapp.com";
	    public ProtocolVersions ProtocolVersion = ProtocolVersions.Seven;
	    public const string VersionNumber = "1.0.0";

		internal void SetSslOptionOn() => Encrypted = true;
	}
}
