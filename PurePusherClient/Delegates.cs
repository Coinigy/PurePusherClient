using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace PurePusher
{
    public delegate void SubscriptionEventHandler(object sender);
    public delegate void ErrorEventHandler(object sender, Exception error);
    public delegate void ConnectedEventHandler(object sender);
    public delegate void ConnectionStateChangedEventHandler(object sender, WebSocketState state);
    public delegate void MemberEventHandler(object sender);
    public delegate void MemberAddedEventHandler(object sender, KeyValuePair<string, dynamic> member);
}
