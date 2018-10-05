using PurePusher.Interfaces;
using PurePusher.Types;
using PureWebSockets;

namespace PurePusher
{
    public class PurePusherClientOptions : PureWebSocketOptions
    {
        public const string ClientName = "netstandard_purepusherclient";
        public const string VersionNumber = "1.0.0";
        public string Host = "ws.pusherapp.com";
        public ProtocolVersions ProtocolVersion = ProtocolVersions.Seven;
        public ISerializer Serializer { get; set; }
        public IAuthorizer Authorizer { get; set; }
        public bool Encrypted { get; set; }

        internal void SetSslOptionOn() => Encrypted = true;
    }
}
