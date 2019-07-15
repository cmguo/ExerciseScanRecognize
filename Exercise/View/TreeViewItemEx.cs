using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Exercise.View
{
    internal class TreeViewItemEx : TreeViewItem
    {
        internal TreeView ParentTreeView
        {
            get
            {
                ItemsControl parent = ParentItemsControl;
                while (parent != null)
                {
                    TreeView tv = parent as TreeView;
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
        ///     Returns the immediate parent TreeViewItem. Null if the parent is a TreeView.
        /// </summary>
        internal TreeViewItemEx ParentTreeViewItem
        {
            get
            {
                return ParentItemsControl as TreeViewItemEx;
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
                        TreeViewItemEx parent = ParentTreeViewItem;
                        if (parent != null)
                            parent.SelectNext(this);
                        return;
                    }
                }
            }
            base.OnItemsChanged(e);
        }

        private void SelectNext(TreeViewItemEx child)
        {
            int index = ItemContainerGenerator.IndexFromContainer(child);
            if (++index < Items.Count)
            {
                TreeViewItem next = ItemContainerGenerator.ContainerFromIndex(index) as TreeViewItem;
                if (next != null)
                {
                    next.IsSelected = true;
                    return;
                }
                ParentTreeViewItem.SelectNext(this);
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
