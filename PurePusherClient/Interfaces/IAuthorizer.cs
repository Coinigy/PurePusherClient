namespace PurePusher.Interfaces
{
	public interface IAuthorizer
	{
		string Authorize(string channelName, string socketId);
	}
}
