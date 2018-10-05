using System.Collections.Generic;
using Utf8Json;

namespace PurePusher
{
    public class PresenceChannel : PrivateChannel
    {
        public Dictionary<string, dynamic> Members = new Dictionary<string, dynamic>();

        public PresenceChannel(string channelName, PurePusherClient purePusherClient) : base(channelName, purePusherClient)
        {
        }

        public event MemberAddedEventHandler MemberAdded;
        public event MemberEventHandler MemberRemoved;

        internal override void SubscriptionSucceeded(string data)
        {
            Members = ParseMembersList(data);
            base.SubscriptionSucceeded(data);
        }

        internal void AddMember(string data)
        {
            var member = ParseMember(data);

            if (!Members.ContainsKey(member.Key))
                Members.Add(member.Key, member.Value);
            else
                Members[member.Key] = member.Value;

            MemberAdded?.Invoke(this, member);
        }

        internal void RemoveMember(string data)
        {
            var member = ParseMember(data);

            if (Members.ContainsKey(member.Key))
            {
                Members.Remove(member.Key);

                MemberRemoved?.Invoke(this);
            }
        }

        private static Dictionary<string, dynamic> ParseMembersList(string data)
        {
            var members = new Dictionary<string, dynamic>();

            var dataAsObj = JsonSerializer.Deserialize<dynamic>(data);

            for (var i = 0; i < (int) dataAsObj.presence.count; i++)
            {
                var id = (string) dataAsObj.presence.ids[i];
                var val = dataAsObj.presence.hash[id];
                members.Add(id, val);
            }

            return members;
        }

        private static KeyValuePair<string, dynamic> ParseMember(string data)
        {
            var dataAsObj = JsonSerializer.Deserialize<dynamic>(data);

            var id = (string) dataAsObj.user_id;
            var val = dataAsObj.user_info;

            return new KeyValuePair<string, dynamic>(id, val);
        }
    }
}
