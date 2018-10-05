namespace PurePusher.Messages
{
    public class EventResponseMessage
    {
        public string @event { get; set; }
        public string channel { get; set; }
        public object data { get; set; }
    }
}
