namespace Base.Events
{
    public interface IEventQueue
    {
        void Subscribe<Arg>(Event<Arg> @event);
        void Unsubscribe<Arg>(Event<Arg> @event);
        void Send<Arg>(Event<Arg> @event, Arg msg);
    }
}
