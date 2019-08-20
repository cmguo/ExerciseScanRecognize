using Newtonsoft.Json;
using Prism.Events;
using System;

namespace Base.Events
{
    public class Event<Arg> : PubSubEvent<Arg>, IEvent
    {
        public string Topic { get; }
        public bool IsExternal { get; }

        public Event()
        {
            Type eventType = GetType();
            if (eventType.IsGenericType)
            {
                if (eventType.GetGenericTypeDefinition() == typeof(Event<int>).GetGenericTypeDefinition())
                    eventType = eventType.GetGenericArguments()[0];
            }
            TopicAttribute[] topicAttr = eventType.GetCustomAttributes(typeof(TopicAttribute), false) as TopicAttribute[];
            ExternalAttribute[] extAttr = eventType.GetCustomAttributes(typeof(ExternalAttribute), false) as ExternalAttribute[];
            if (topicAttr != null && topicAttr.Length > 0)
            {
                Topic = topicAttr[0].Name;
            }
            if (extAttr != null && extAttr.Length > 0)
            {
                IsExternal = true;
            }
        }

        public override void Publish(Arg payload)
        {
            if (IsExternal)
            {
                EventBus.Instance.PublishExternal(this, payload);
            }
            else
            {
                base.Publish(payload);
            }
        }

        public override SubscriptionToken Subscribe(Action<Arg> action, ThreadOption threadOption, bool keepSubscriberReferenceAlive, Predicate<Arg> filter)
        {
            if (IsExternal)
            {
                if (base.Subscriptions.Count == 0)
                {
                    EventBus.Instance.UnsubscribeExternal(this);
                }
            }
            return base.Subscribe(action, threadOption, keepSubscriberReferenceAlive, filter);
        }

        public override void Unsubscribe(Action<Arg> subscriber)
        {
            base.Unsubscribe(subscriber);
            if (base.Subscriptions.Count == 0)
            {
                EventBus.Instance.UnsubscribeExternal(this);
            }
        }

        public void Publish(string payload)
        {
            Arg argsObj = JsonConvert.DeserializeObject<Arg>(payload);
            Publish(argsObj);
        }

        public void Subscribe(Action<string, string> action)
        {
            base.Subscribe(arg => action(Topic, JsonConvert.SerializeObject(arg)));
        }

    }

}
