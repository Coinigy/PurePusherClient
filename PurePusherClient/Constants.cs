namespace PurePusher
{
	internal class Constants
	{
		// ReSharper disable InconsistentNaming
		public const string ERROR = "pusher:error";
		public const string CONNECTION_ESTABLISHED = "pusher:connection_established";
		public const string CHANNEL_SUBSCRIBE = "pusher:subscribe";
		public const string CHANNEL_UNSUBSCRIBE = "pusher:unsubscribe";
		public const string CHANNEL_SUBSCRIPTION_SUCCEEDED = "pusher_internal:subscription_succeeded";
		public const string CHANNEL_SUBSCRIPTION_ERROR = "pusher_internal:subscription_error";
		public const string CHANNEL_MEMBER_ADDED = "pusher_internal:member_added";
		public const string PING = "pusher:ping"; // not really needed since our websocket supports ping but we will keep anyway
		public const string CHANNEL_MEMBER_REMOVED = "pusher_internal:member_removed";
		// ReSharper restore InconsistentNaming
	}
}
