using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using PurePusher.Messages;
using PurePusher.Types;

namespace PurePusher
{
    public class PurePusherClient : EventEmitter
    {
        private readonly string _applicationKey;
        public readonly PurePusherClientOptions Options;
        private ErrorEventHandler _errorEvent;
        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();
        public WsConnection Connection;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PurePusherClient" /> class.
        /// </summary>
        /// <param name="applicationKey">The application key.</param>
        /// <param name="options">The options.</param>
        public PurePusherClient(string applicationKey, PurePusherClientOptions options = null)
        {
            _applicationKey = applicationKey;

            Options = options ?? new PurePusherClientOptions {Encrypted = false};

            if (Options.Serializer == null)
                Options.Serializer = new Utf8JsonSerializer();
        }

        public string SocketId => Connection?.SocketId;
        public event ConnectedEventHandler Connected;
        public event ConnectionStateChangedEventHandler ConnectionStateChanged;

        private bool AlreadySubscribed(string channelName)
        {
            return Channels.ContainsKey(channelName) && Channels[channelName].IsSubscribed;
        }

        public event ErrorEventHandler Error
        {
            add
            {
                _errorEvent += value;
                if (Connection != null)
                    Connection.Error += value;
            }
            remove
            {
                try
                {
                    if (_errorEvent != null)
                        // ReSharper disable once DelegateSubtraction
                        _errorEvent -= value;
                }
                catch
                {
                    // ignored
                }

                try
                {
                    if (Connection != null)
                        Connection.Error -= value;
                }
                catch
                {
                    // ignored
                }
            }
        }

        internal void MarkChannelsAsUnsubscribed()
        {
            foreach (var channel in Channels)
                channel.Value.Unsubscribe();
        }

        internal void SubscribeExistingChannels()
        {
            foreach (var channel in Channels)
                Subscribe(channel.Key);
        }

        public bool Connect()
        {
            // check current connection state
            if (Connection?.State == WebSocketState.Open)
                return false;

            var url = Options.Encrypted ? "wss://" : "ws://" + Options.Host + "/app/" + _applicationKey + "?protocol=" + (int) Options.ProtocolVersion + "&client=" + PurePusherClientOptions.ClientName + "&version=" + PurePusherClientOptions.VersionNumber;

            Connection = new WsConnection(this, url);
            Connection.Connected += _connection_Connected;
            Connection.ConnectionStateChanged += _connection_ConnectionStateChanged;

            if (_errorEvent != null)
                foreach (var del in _errorEvent.GetInvocationList())
                {
                    var handler = (ErrorEventHandler) del;
                    Connection.Error += handler;
                }

            return Connection.Connect();
        }

        public void Disconnect() => Connection?.Disconnect();

        public Channel Subscribe(string channelName)
        {
            if (AlreadySubscribed(channelName)) return Channels[channelName];

            // if private or presence channel, check that auth endpoint has been set
            var chanType = ChannelTypes.Public;

            if (channelName.StartsWith("private-", StringComparison.OrdinalIgnoreCase))
                chanType = ChannelTypes.Private;
            else if (channelName.StartsWith("presence-", StringComparison.OrdinalIgnoreCase))
                chanType = ChannelTypes.Presence;

            return SubscribeToChannel(chanType, channelName);
        }

        private Channel SubscribeToChannel(ChannelTypes type, string channelName)
        {
            if (!Channels.ContainsKey(channelName))
                CreateChannel(type, channelName);

            if (Connection.State != WebSocketState.Open) return Channels[channelName];

            if (type == ChannelTypes.Presence || type == ChannelTypes.Private)
            {
                var jsonAuth = Options.Authorizer.Authorize(channelName, Connection.SocketId);

                var message = Options.Serializer.Deserialize<SubscribeToChannelMessage>(jsonAuth);

                Connection.Send(Options.Serializer.Serialize(new {@event = Constants.CHANNEL_SUBSCRIBE, data = new {channel = channelName, message.auth, message.channel_data}}));
            }
            else
            {
                // no need for auth details just send subscribe event
                Connection.Send(Options.Serializer.Serialize(new {@event = Constants.CHANNEL_SUBSCRIBE, data = new {channel = channelName}}));
            }

            return Channels[channelName];
        }

        private void CreateChannel(ChannelTypes type, string channelName)
        {
            switch (type)
            {
                case ChannelTypes.Private:
                    AuthEndpointCheck();
                    Channels.Add(channelName, new PrivateChannel(channelName, this));
                    break;
                case ChannelTypes.Presence:
                    AuthEndpointCheck();
                    Channels.Add(channelName, new PresenceChannel(channelName, this));
                    break;
                case ChannelTypes.Public:
                default:
                    Channels.Add(channelName, new Channel(channelName, this));
                    break;
            }
        }

        private void AuthEndpointCheck()
        {
            if (Options.Authorizer != null) return;

            var pusherException = new Exception("You must set a ChannelAuthorizer to use private or presence channels");
            RaiseError(pusherException);
            throw pusherException;
        }

        internal void Trigger(string channelName, string eventName, object obj) => Connection?.Send(Options.Serializer.Serialize(new {@event = eventName, channel = channelName, data = obj}));

        internal void Unsubscribe(string channelName)
        {
            if (Connection == null) return;
            if (Connection.State == WebSocketState.Open)
                Connection.Send(Options.Serializer.Serialize(new {@event = Constants.CHANNEL_UNSUBSCRIBE, data = new {channel = channelName}}));
        }

        private void _connection_ConnectionStateChanged(object sender, WebSocketState state)
        {
            switch (state)
            {
                case WebSocketState.Closed:
                    MarkChannelsAsUnsubscribed();
                    break;
                case WebSocketState.Open:
                    SubscribeExistingChannels();
                    break;
            }

            ConnectionStateChanged?.Invoke(sender, state);
        }

        private void _connection_Connected(object sender) => Connected?.Invoke(sender);

        private void RaiseError(Exception error) => _errorEvent?.Invoke(this, error);
    }
}
