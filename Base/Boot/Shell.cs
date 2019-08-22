using Base.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;

namespace Base.Boot
{

    [InheritedExport]
    [PartNotDiscoverable]
    public class Shell : DependencyObject
    {

        public virtual void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
