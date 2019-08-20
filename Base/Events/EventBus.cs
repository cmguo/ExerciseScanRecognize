using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;

namespace Base.Events
{

    public class EventBus : EventAggregator
    {

        public static EventBus Instance = new EventBus();

        private IEventQueue externalQueue;
        private Dictionary<string, IEvent> topicEvents = new Dictionary<string, IEvent>();

        public void SetExternalQueue(IEventQueue queue)
        {
            externalQueue = queue;
        }

        public new EventType GetEvent<EventType>() where EventType : EventBase, new()
        {
            EventType @event = base.GetEvent<EventType>();
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

        internal void SubscribeExternal<Arg>(Event<Arg> @event)
        {
            externalQueue.Subscribe(@event);
        }

        internal void UnsubscribeExternal<Arg>(Event<Arg> @event)
        {
            externalQueue.Unsubscribe(@event);
        }

        internal void PublishExternal<Arg>(Event<Arg> @event, Arg payload)
        {
            externalQueue.Send<Arg>(@event, payload);
        }


    }
}
