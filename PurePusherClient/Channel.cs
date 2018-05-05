namespace PurePusher
{
	public class Channel : EventEmitter
	{
		private readonly PurePusherClient _purePusherClient;
		public string Name;

		public Channel(string channelName, PurePusherClient purePusherClient)
		{
			_purePusherClient = purePusherClient;
			Name = channelName;
		}

		public bool IsSubscribed { get; private set; }

		public event SubscriptionEventHandler Subscribed;

		internal virtual void SubscriptionSucceeded(string data)
		{
			if (IsSubscribed)
				return;

			IsSubscribed = true;

			Subscribed?.Invoke(this);
		}

		public void Trigger(string eventName, object obj) => _purePusherClient?.Trigger(Name, eventName, obj);

		public void Unsubscribe()
		{
			IsSubscribed = false;
			_purePusherClient.Unsubscribe(Name);
		}
	}
}
