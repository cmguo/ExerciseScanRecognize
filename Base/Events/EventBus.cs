using Newtonsoft.Json;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Base.Events
{

    public class EventBus : EventAggregator
    {

        public static EventBus Instance = new EventBus();

        private Dictionary<string, EventBase> topicEvents = new Dictionary<string, EventBase>();

        public new EventType GetEvent<EventType>() where EventType : EventBase, new()
        {
            EventType @event = base.GetEvent<EventType>();
            Type eventType = @event.GetType();
            if (eventType.IsGenericType)
            {
                if (eventType.GetGenericTypeDefinition() == typeof(Event<int>).GetGenericTypeDefinition())
                    eventType = eventType.GetGenericArguments()[0];
            }
            TopicAttribute[] topic = eventType.GetCustomAttributes(typeof(TopicAttribute), false) as TopicAttribute[];
            if (topic != null)
            {
                lock (topicEvents)
                {
                    foreach (TopicAttribute t in topic)
                    {
                        topicEvents[t.Name] = @event;
                    }
                }
            }
            return @event;
        }


        public bool PublishEvent(string topic, string args)
        {
            EventBase @event = null;
            lock (topicEvents)
            {
                if (!topicEvents.TryGetValue(topic, out @event))
                    return false;
            }
            Type eventType = @event.GetType();
            if (!eventType.IsGenericType)
                eventType = eventType.BaseType;
            if (!eventType.IsGenericType
                || eventType.GetGenericTypeDefinition() != typeof(Event<int>).GetGenericTypeDefinition())
            {
                return false;
            }
            Type argsType = eventType.GetGenericArguments()[0];
            object argsObj = JsonConvert.DeserializeObject(args, argsType);
            @event.GetType().GetMethod("Publish").Invoke(@event, new object[] { argsObj });
            return true;
        }

    }
}
