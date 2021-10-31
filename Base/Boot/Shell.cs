using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Base.Boot
{

    [InheritedExport]
    [PartNotDiscoverable]
    public class Shell : DependencyObject
    {
        [ImportMany(typeof(IComponent))]
        private IEnumerable<Lazy<IComponent, IComponentMetadata>> assistants = null;

        public virtual void Initialize()
        {
            if (Application.Current != null)
            {
                assistants.Any(a => a.Value == null);
            }
            else
            {
                assistants.Where(a => a.Metadata.MainProcessOnly == false).Any(a => a.Value == null);
            }
        }
    }
}
