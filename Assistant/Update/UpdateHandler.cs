using System;
using Base.Events;

namespace Assistant.Update
{
    public class UpdateHandler
    {

        private static Event<UpdateMessage> @event = EventBus.Instance.GetEvent<Event<UpdateMessage>>();
        private static Event<string> @event2 = EventBus.Instance.GetEvent<Event<string>>();

        public static void Init()
        {
            @event.Subscribe(OnUpdate);
            EventBus.Instance.PublishEvent("app/update", "{}");
        }

        private static void OnUpdate(UpdateMessage obj)
        {
            
        }
    }
}
