using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Exercise.View
{
    class TreeViewEx : TreeView
    {

        public void Select(object item)
        {
            if (SelectedItem == item)
                return;
            var tvi = FindItem(this, item);
            if (tvi != null)
            {
                tvi.IsSelected = true;
            }
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            base.OnSelectedItemChanged(e);
        }

        private static TreeViewItem FindItem(ItemsControl container, object item)
        {
            var c = container.ItemContainerGenerator.ContainerFromItem(item);
            if (c != null)
            {
                return c as TreeViewItem;
            }
            foreach (var i in container.Items)
            {
                var cc = container.ItemContainerGenerator.ContainerFromItem(i);
                c = FindItem(cc as ItemsControl, item);
                if (c != null)
                {
                    return c as TreeViewItem;
                }
            }
            return null;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Contains(SelectedItem) && e.OldStartingIndex < this.Items.Count)
                {
                    TreeViewItem next = ItemContainerGenerator.ContainerFromIndex(e.OldStartingIndex) as TreeViewItem;
                    if (next != null)
                    {
                        next.IsSelected = true;
                        return;
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (Items.Count == e.NewItems.Count)
                {
                    this.SelectNext();
                }
            }
            base.OnItemsChanged(e);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

    }
}
