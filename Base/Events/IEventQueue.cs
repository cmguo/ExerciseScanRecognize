namespace Base.Events
{
    public interface IEventQueue
    {
        void Subscribe(IEvent @event);
        void Unsubscribe(IEvent @event);
        void Publish(IEvent @event, string msg);
    }
}
