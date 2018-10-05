using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using PurePusher.Messages;
using PureWebSockets;

namespace PurePusher
{
	public class WsConnection
	{
		private readonly PurePusherClient _purePusherClient;
		private readonly string _url;
		private PureWebSocket _websocket;

		public event ConnectedEventHandler Connected;
		public event ConnectionStateChangedEventHandler ConnectionStateChanged;
		public event ErrorEventHandler Error;

		public string SocketId { get; private set; }
		public WebSocketState State => _websocket.State;

		public WsConnection(PurePusherClient purePusherClient, string url)
		{
			_url = url;
			_purePusherClient = purePusherClient;
		}

		internal bool Connect()
		{
			_websocket = new PureWebSocket(_url, _purePusherClient.Options);
			_websocket.OnOpened += _websocket_OnOpened;
			_websocket.OnError += _websocket_OnError;
			_websocket.OnClosed += _websocket_OnClosed;
			_websocket.OnFatality += _websocket_OnFatality;
			_websocket.OnMessage += _websocket_OnMessage;

			return _websocket.Connect();
		}

		private void _websocket_OnFatality(string reason)
		{
			if (_websocket?.State != null) ConnectionStateChanged?.Invoke(this, _websocket.State);
			Error?.Invoke(this, new Exception(reason));
		}

		private void _websocket_OnMessage(string message)
		{
			var dMessage = _purePusherClient.Options.Serializer.Deserialize<EventResponseMessage>(message);
			_purePusherClient.EmitEvent(_purePusherClient.Options.Serializer, dMessage.@event, dMessage.data.ToString());

			switch (dMessage.@event)
			{
				case Constants.ERROR:
					ParseError((ErrorDataMessage)dMessage.data);
					break;
				case Constants.PING:
					Send(_purePusherClient.Options.Serializer.Serialize(new { @event = "pusher:pong", channel = "", data = "" }));
					break;
				case Constants.CONNECTION_ESTABLISHED:
					ParseConnectionEstablished(dMessage.data.ToString());
					break;
				case Constants.CHANNEL_SUBSCRIPTION_SUCCEEDED:
					if (_purePusherClient.Channels.ContainsKey(dMessage.channel))
					{
						var channel = _purePusherClient.Channels[dMessage.channel];
						channel.SubscriptionSucceeded(dMessage.data.ToString());
					}
					break;
				case Constants.CHANNEL_SUBSCRIPTION_ERROR:
					RaiseError(new Exception("Error received on channel subscriptions: "));
					break;
				case Constants.CHANNEL_MEMBER_ADDED:
					// assume channel event
					if (_purePusherClient.Channels.ContainsKey(dMessage.channel))
					{
						var channel = _purePusherClient.Channels[dMessage.channel];

						if (channel is PresenceChannel presenceChannel)
						{
							presenceChannel.AddMember(dMessage.data.ToString());
						}
					}
					break;
				case Constants.CHANNEL_MEMBER_REMOVED:
					// assume channel event
					if (_purePusherClient.Channels.ContainsKey(dMessage.channel))
					{
						var channel = _purePusherClient.Channels[dMessage.channel];

						if (channel is PresenceChannel presenceChannel)
						{
							presenceChannel.RemoveMember(dMessage.data.ToString());
						}
					}
					break;
				default:
					// unhandled message type, Assume channel event
					if (_purePusherClient.Channels.ContainsKey(dMessage.channel))
						_purePusherClient.Channels[dMessage.channel].EmitEvent(_purePusherClient.Options.Serializer, dMessage.@event, dMessage.data.ToString());
					break;
			}
		}

		private void _websocket_OnClosed(WebSocketCloseStatus reason) => ConnectionStateChanged?.Invoke(this, WebSocketState.Closed);

		private void _websocket_OnError(Exception ex) => Error?.Invoke(this, ex);

		private void _websocket_OnOpened() => ConnectionStateChanged?.Invoke(this, WebSocketState.Open);

		internal void Disconnect() => _websocket?.Disconnect();

		internal bool Send(string message) => _websocket.Send(message);

		internal bool Send(byte[] message) => _websocket.Send(message);

		private void RaiseError(Exception error) => Error?.Invoke(this, error);

		private void ParseConnectionEstablished(string data)
		{
			SocketId = _purePusherClient.Options.Serializer.Deserialize<dynamic>(data)["socket_id"];

			Connected?.Invoke(this);
		}

		private void ParseError(ErrorDataMessage data)
		{
			// the server requires SSL
			if (data.code == 4000)
			{
				RaiseError(new Exception(data.code + "|" + data.message));
				Disconnect();
				_purePusherClient.Options.SetSslOptionOn();
				Connect();
			}

			// the server does not like something and wants us to go away
			if (data.code >= 4001 && data.code < 4100)
			{
				RaiseError(new Exception(data.code + "|" + data.message));
				_websocket.Disconnect();
				return;
			}

			// the server wants us to try again in a bit (possibly due to being over capacity)
			if (data.code >= 4100 && data.code < 4200)
			{
				Disconnect();
				RaiseError(new Exception(data.code + "|" + data.message));
				Task.Delay(1500).Wait(); // block for a while
				Connect();
				return;
			}

			// the server wants us to reconnect
			if (data.code >= 4200 && data.code < 4300)
			{
				Disconnect();
				RaiseError(new Exception(data.code + "|" + data.message));
				Task.Delay(500).Wait(); // slight block
				Connect();
				return;
			}

			RaiseError(new Exception(data.code + "|" + data.message));
		}
	}
}
