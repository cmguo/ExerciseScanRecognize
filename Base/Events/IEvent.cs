using System;

namespace Base.Events
{
    public interface IEvent
    {
        string Topic { get; }
        bool IsExternal { get; }

        void Publish(string payload);
        void Subscribe(Action<string, string> action);
        void Unsubscribe(Action<string, string> action);
    }
}
