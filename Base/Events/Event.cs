using Prism.Events;

namespace Base.Events
{
    public class Event<Arg> : PubSubEvent<Arg>, IEvent
    {
    }

}
