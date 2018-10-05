using System;
using System.Collections.Generic;
using PurePusher.Interfaces;

namespace PurePusher
{
    public class EventEmitter
    {
        private readonly Dictionary<string, List<Action<dynamic>>> _eventListeners = new Dictionary<string, List<Action<dynamic>>>();
        private readonly List<Action<string, dynamic>> _generalListeners = new List<Action<string, dynamic>>();

        public void Bind(string eventName, Action<dynamic> listener)
        {
            if (_eventListeners.ContainsKey(eventName))
            {
                _eventListeners[eventName].Add(listener);
            }
            else
            {
                var listeners = new List<Action<dynamic>> {listener};
                _eventListeners.Add(eventName, listeners);
            }
        }

        public void BindAll(Action<string, dynamic> listener) => _generalListeners.Add(listener);

        internal void EmitEvent(ISerializer serializer, string eventName, string data)
        {
            var obj = serializer.Deserialize<dynamic>(data);

            // emit to general listeners regardless of event type
            foreach (var action in _generalListeners)
                action(eventName, obj);

            if (!_eventListeners.ContainsKey(eventName)) return;

            foreach (var action in _eventListeners[eventName])
                action(obj);
        }
    }
}
