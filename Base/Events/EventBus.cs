using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Base.Events
{

    [Export]
    public class EventBus
    {

        internal static EventBus Instance;

        [Import]
        private IEventAggregator eventAggregator = null;

        [Import]
        private IEventQueue externalQueue = null;

        private Dictionary<string, IEvent> topicEvents = new Dictionary<string, IEvent>();

        public EventBus()
        {
            Instance = this;
        }

        public EventType GetEvent<EventType>() where EventType : EventBase, new()
        {
            EventType @event = eventAggregator.GetEvent<EventType>();
            if (@event is IEvent e)
            {
                if (e.Topic != null)
                {
                    topicEvents.Add(e.Topic, e);
                }
            }
            return @event;
        }


        public bool PublishEvent(string topic, string args)
        {
            IEvent @event = null;
            lock (topicEvents)
            {
                if (!topicEvents.TryGetValue(topic, out @event))
                    return false;
            }
            @event.Publish(args);
            return true;
        }

        public void SubscribeEvent(string topic, Action<string, string> action)
        {
            IEvent @event = null;
            lock (topicEvents)
            {
                if (!topicEvents.TryGetValue(topic, out @event))
                    throw new InvalidOperationException("No such Event");
            }
            @event.Subscribe(action);
        }

        public void UnsubscribeEvent(string topic, Action<string, string> action)
        {
            IEvent @event = null;
            lock (topicEvents)
            {
                if (!topicEvents.TryGetValue(topic, out @event))
                    throw new InvalidOperationException("No such Event");
            }
            @event.Unsubscribe(action);
        }

        internal void SubscribeExternal(IEvent @event)
        {
            externalQueue.Subscribe(@event);
        }

        internal void UnsubscribeExternal(IEvent @event)
        {
            externalQueue.Unsubscribe(@event);
        }

        internal void PublishExternal(IEvent @event, string payload)
        {
            externalQueue.Publish(@event, payload);
        }


    }
}
