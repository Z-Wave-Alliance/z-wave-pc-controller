/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ZWaveControllerUI.Drawing;
using System.Windows;
using Utils.UI.Wrappers;
using System.Windows.Media;

namespace ZWaveControllerUI.Bind
{
    public class GridItemWrapper : ContentControl, ISelectable
    {
        public static readonly DependencyProperty RowIndexProperty = DependencyProperty.
           Register("RowIndex", typeof(int), typeof(GridItemWrapper), new FrameworkPropertyMetadata(0));

        public int RowIndex
        {
            get { return (int)GetValue(RowIndexProperty); }
            set { SetValue(RowIndexProperty, value); }
        }

        #region IsSelected Property
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.
            Register("IsSelected", typeof(bool), typeof(GridItemWrapper), new FrameworkPropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        #endregion

        static GridItemWrapper()
        {
            FrameworkElement.DefaultStyleKeyProperty.
                OverrideMetadata(typeof(GridItemWrapper), new FrameworkPropertyMetadata(typeof(GridItemWrapper)));
        }

        public GridItemWrapper()
        {
            this.Loaded += new RoutedEventHandler(GridItemWrapper_Loaded);
            this.Unloaded -= new RoutedEventHandler(GridItemWrapper_Loaded);
        }

        void GridItemWrapper_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.Template != null)
            {
                IsSelected = false;
                DependencyObject dob = VisualTreeHelper.GetParent(this);
                FrameworkElement ditem = this;
                if (dob is ContentPresenter)
                    ditem = (ContentPresenter)dob;


                DockPanel.SetDock(ditem, Dock.Left);
                
                //if (Id > 0)
                //{
                //    Canvas.SetZIndex(ditem, 1000);// topmost
                //    Canvas.SetTop(ditem, TopLeft >> 16);
                //    Canvas.SetLeft(ditem, (ushort)TopLeft);
                //}
            }
        }
    }
}
