/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using ZWave.Devices;

namespace ZWaveControllerUI.Drawing
{
    public class SelectionService : FrameworkElement
    {
        public Panel Panel { get; set; }
        public NodeTag SelectedSourceId
        {
            get { return (NodeTag)GetValue(SelectedSourceIdProperty); }
            set { SetValue(SelectedSourceIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedSourceIdProperty =
            DependencyProperty.Register("SelectedSourceId", typeof(NodeTag), typeof(SelectionService), new UIPropertyMetadata(NodeTag.Empty));

        public NodeTag SelectedDestinationId
        {
            get { return (NodeTag)GetValue(SelectedDestinationIdProperty); }
            set { SetValue(SelectedDestinationIdProperty, value); }
        }

        public static readonly DependencyProperty SelectedDestinationIdProperty =
            DependencyProperty.Register("SelectedDestinationId", typeof(NodeTag), typeof(SelectionService), new UIPropertyMetadata(NodeTag.Empty));

        private ISelectableExt SelectedSourceItem;
        private ISelectableExt SelectedDestinationItem;

        private List<ISelectableExt> currentSelection;
        internal List<ISelectableExt> CurrentSelection
        {
            get
            {
                if (currentSelection == null)
                    currentSelection = new List<ISelectableExt>();

                return currentSelection;
            }
        }

        internal void SelectItem(ISelectableExt item)
        {
            this.ClearSelectionInner();
            this.AddToSelection(item);
            if (CurrentSelection.Count > 0)
            {
                foreach (var i in CurrentSelection)
                {
                    if (i.Id != SelectedSourceId)
                    {
                        if (SelectedDestinationItem != null)
                        {
                            SelectedDestinationItem.IsSelectedAsDestination = false;
                            SelectedDestinationItem.IsSelectedAsSource = false;
                            SelectedDestinationItem = null;
                        }
                        if (SelectedSourceItem != null)
                        {
                            SelectedDestinationItem = SelectedSourceItem;
                            SelectedDestinationItem.IsSelectedAsDestination = true;
                            SelectedDestinationItem.IsSelectedAsSource = false;
                            SelectedDestinationId = SelectedSourceId;
                        }
                        SelectedSourceItem = i;
                        SelectedSourceItem.IsSelectedAsSource = true;
                        SelectedSourceItem.IsSelectedAsDestination = false;
                        SelectedSourceId = i.Id;
                    }

                }
            }
        }

        internal void AddToSelection(ISelectableExt item)
        {
            item.IsSelected = true;
            CurrentSelection.Add(item);
        }

        internal void RemoveFromSelection(ISelectableExt item)
        {
            item.IsSelected = false;
            CurrentSelection.Remove(item);
        }

        internal void ClearSelection()
        {
            ClearSelectionInner();
            //SelectedSourceId = 0;
            //SelectedDestinationId = 0;
            //if (SelectedSourceItem != null)
            //{
            //    SelectedSourceItem.IsSelectedAsDestination = false;
            //    SelectedSourceItem.IsSelectedAsSource = false;
            //    SelectedSourceItem = null;
            //}
            //if (SelectedDestinationItem != null)
            //{
            //    SelectedDestinationItem.IsSelectedAsDestination = false;
            //    SelectedDestinationItem.IsSelectedAsSource = false;
            //    SelectedDestinationItem = null;
            //}
        }

        private void ClearSelectionInner()
        {
            CurrentSelection.ForEach(item => item.IsSelected = false);
            CurrentSelection.Clear();
        }

        internal void SelectAll()
        {
            ClearSelection();
            if (Panel != null)
            {
                foreach (FrameworkElement itemF in Panel.Children)
                {
                    FrameworkElement item = null;
                    if (itemF is ContentPresenter)
                    {
                        item = (FrameworkElement)VisualTreeHelper.GetChild(itemF, 0);
                    }
                    else if (itemF is Control)
                    {
                        item = (FrameworkElement)itemF;
                    }

                    if (item != null && item is ISelectableExt)
                    {
                        ISelectableExt di = item as ISelectableExt;
                        di.IsSelected = true;
                        AddToSelection(di);
                    }
                }
            }
        }

        //internal void UpdateSelected()
        //{
        //    ClearSelection();

        //    foreach (FrameworkElement itemF in Panel.Children)
        //    {
        //        FrameworkElement item = null;
        //        if (itemF is ContentPresenter)
        //        {
        //            item = (FrameworkElement)VisualTreeHelper.GetChild(itemF, 0);
        //        }
        //        else if (itemF is Control)
        //        {
        //            item = (FrameworkElement)itemF;
        //        }

        //        if (item != null && item is ISelectableExt)
        //        {
        //            ISelectableExt di = item as ISelectableExt;
        //            if (di.IsSelected)
        //                AddToSelection(di);
        //        }
        //    }
        //}
    }
}
