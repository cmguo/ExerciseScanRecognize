using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Exercise.View
{
    internal class TreeViewItemEx : TreeViewItem
    {
        internal TreeViewEx ParentTreeView
        {
            get
            {
                ItemsControl parent = ParentItemsControl;
                while (parent != null)
                {
                    TreeViewEx tv = parent as TreeViewEx;
                    if (tv != null)
                    {
                        return tv;
                    }

                    parent = ItemsControl.ItemsControlFromItemContainer(parent);
                }

                return null;
            }
        }

        /// <summary>
        ///     Returns the immediate parent ItemsControl.
        /// </summary>
        internal ItemsControl ParentItemsControl
        {
            get
            {
                return ItemsControl.ItemsControlFromItemContainer(this);
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems.Contains(ParentTreeView.SelectedItem))
                {
                    if (e.OldStartingIndex < Items.Count)
                    {
                        TreeViewItem next = ItemContainerGenerator.ContainerFromIndex(e.OldStartingIndex) as TreeViewItem;
                        if (next != null)
                        {
                            next.IsSelected = true;
                            return;
                        }
                    }
                    if (Items.Count != 0)
                    {
                        SelectNext(ParentItemsControl, this);
                        return;
                    }
                }
            }
            base.OnItemsChanged(e);
        }

        private static void SelectNext(ItemsControl parent, TreeViewItemEx child)
        {
            int index = parent.ItemContainerGenerator.IndexFromContainer(child);
            if (++index < parent.Items.Count)
            {
                TreeViewItem next = parent.ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                if (next != null)
                {
                    next.IsSelected = true;
                    return;
                }
                child = (parent as TreeViewItemEx);
                SelectNext(child.ParentItemsControl, child);
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            if (Items.Count > 0)
            {
                TreeViewItem next = ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                Dispatcher.InvokeAsync(SelectNext);
                return;
            }
            base.OnSelected(e);
        }

        private void SelectNext()
        {
            if (0 < Items.Count)
            {
                TreeViewItemEx next = ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItemEx;
                if (next != null)
                {
                    next.SelectNext();
                    return;
                }
            }
            else
            {
                IsSelected = true;
            }
        }

    }
}
