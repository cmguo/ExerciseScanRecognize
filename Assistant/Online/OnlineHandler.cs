
using Base.Boot;
using Base.Events;
using System.ComponentModel.Composition;

namespace Assistant.Online
{
    [InheritedExport(typeof(IComponent)),
        ExportMetadata("MainProcessOnly", true)]
    public class OnlineHandler : IComponent
    {
        [ImportingConstructor]
        public OnlineHandler(EventBus bus)
        {
            bus.GetEvent<Event<OnlineStatus>>().Publish(new OnlineStatus());
        }
    }
}
