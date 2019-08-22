
using Base.Boot;
using Base.Events;
using System.ComponentModel.Composition;

namespace Assistant.Online
{
    [InheritedExport(typeof(IAssistant)),
        ExportMetadata("MainProcessOnly", true)]
    public class OnlineHandler : IAssistant
    {
        [ImportingConstructor]
        public OnlineHandler(EventBus bus)
        {
            bus.GetEvent<Event<OnlineStatus>>().Publish(new OnlineStatus());
        }
    }
}
