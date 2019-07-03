using Base.Misc;
using Base.Mvvm;
using Base.Mvvm.Converter;
using Exercise.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Exercise.View
{
    /// <summary>
    /// HomePage.xaml 的交互逻辑
    /// </summary>
    public partial class ResolvePage : Page
    {

        public ResolvePage()
        {
            InitializeComponent();
            ResolveViewModel vm = DataContext as ResolveViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ResolveViewModel vm = sender as ResolveViewModel;
            if (e.PropertyName == "Selection")
            {
                var tvi = FindItem(treeView, vm.Selection);
                if (tvi != null)
                {
                    tvi.IsSelected = true;
                }
            }
        }

        private TreeViewItem FindItem(ItemsControl container, object item)
        {
            var c = container.ItemContainerGenerator.ContainerFromItem(item);
            if (c != null)
                return c as TreeViewItem;
            foreach (var i in container.ItemContainerGenerator.Items)
            {
                var cc = container.ItemContainerGenerator.ContainerFromItem(i);
                c = FindItem(cc as ItemsControl, item);
                if (c != null)
                    return c as TreeViewItem;
            }
            return null;
        }

        internal void Resolve()
        {
            ResolveViewModel vm = DataContext as ResolveViewModel;
            vm.ResolveCommand.Execute(this);
        }
    }
}
