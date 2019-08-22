using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Base.Boot
{

    [InheritedExport]
    public interface IAssistant
    {
    }

    public interface IAssistantMetadata
    {
        [DefaultValue(false)]
        bool MainProcessOnly { get; }
    }

}
