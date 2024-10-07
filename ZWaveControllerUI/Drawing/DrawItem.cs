/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Utils.UI.Wrappers;
using System.Windows.Input;
using System.Windows.Media;
using ZWaveController.Models;
using System.Windows.Shapes;
using ZWave.Devices;

namespace ZWaveControllerUI.Drawing
{
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_IsSourceDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_IsDestinationDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class DrawItem : ContentControl, ISelectableExt
    {
        #region Id Property
        public static readonly DependencyProperty IdProperty = DependencyProperty.
            Register("Id", typeof(NodeTag), typeof(DrawItem), new FrameworkPropertyMetadata(NodeTag.Empty));

        public NodeTag Id
        {
            get { return (NodeTag)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }
        #endregion

        #region TopLeft Property
        public static readonly DependencyProperty TopLeftProperty = DependencyProperty.
            Register("TopLeft", typeof(int), typeof(DrawItem), new FrameworkPropertyMetadata(0));

        public int TopLeft
        {
            get { return (int)GetValue(TopLeftProperty); }
            set { SetValue(TopLeftProperty, value); }
        }
        #endregion


        #region IsSelected Property
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.
            Register("IsSelected", typeof(bool), typeof(DrawItem), new FrameworkPropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        #endregion

        #region IsSelectedAsSource Property
        public static readonly DependencyProperty IsSelectedAsSourceProperty = DependencyProperty.
            Register("IsSelectedAsSource", typeof(bool), typeof(DrawItem), new FrameworkPropertyMetadata(false));

        public bool IsSelectedAsSource
        {
            get { return (bool)GetValue(IsSelectedAsSourceProperty); }
            set { SetValue(IsSelectedAsSourceProperty, value); }
        }
        #endregion

        #region IsSelectedAsDestination Property
        public static readonly DependencyProperty IsSelectedAsDestinationProperty = DependencyProperty.
            Register("IsSelectedAsDestination", typeof(bool), typeof(DrawItem), new FrameworkPropertyMetadata(false));

        public bool IsSelectedAsDestination
        {
            get { return (bool)GetValue(IsSelectedAsDestinationProperty); }
            set { SetValue(IsSelectedAsDestinationProperty, value); }
        }
        #endregion

        #region DragThumbTemplate Property
        public static readonly DependencyProperty DragThumbTemplateProperty = DependencyProperty.
            RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DrawItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }
        #endregion

        static DrawItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.
                OverrideMetadata(typeof(DrawItem), new FrameworkPropertyMetadata(typeof(DrawItem)));
        }

        public DrawItem()
        {
            this.Loaded += new RoutedEventHandler(DrawItem_Loaded);
            this.Unloaded -= new RoutedEventHandler(DrawItem_Loaded);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            if (parent != null)
            {
                Panel designer = parent as Panel;
                if (designer == null)
                {
                    if (parent is ContentPresenter)
                    {
                        parent = VisualTreeHelper.GetParent(parent);
                        designer = parent as Panel;
                    }
                }
                if (designer != null && designer is ISelector)
                {
                    if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                        if (this.IsSelected)
                        {
                            ((ISelector)designer).SelectionService.RemoveFromSelection(this);
                        }
                        else
                        {
                            ((ISelector)designer).SelectionService.AddToSelection(this);
                        }
                    else if (!this.IsSelected)
                    {
                        ((ISelector)designer).SelectionService.SelectItem(this);
                    }
                    Focus();
                }
            }

            e.Handled = false;
        }

        void DrawItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.Template != null)
            {
                IsSelected = false;
                DependencyObject dob = VisualTreeHelper.GetParent(this);
                FrameworkElement ditem = this;
                if (dob is ContentPresenter)
                    ditem = (ContentPresenter)dob;

                if (Id.Id > 0)
                {
                    Canvas.SetZIndex(ditem, 1000);// topmost
                    Canvas.SetTop(ditem, TopLeft >> 16);
                    Canvas.SetLeft(ditem, (ushort)TopLeft);
                }

                ContentPresenter contentPresenter = this.Template.
                    FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null && VisualTreeHelper.GetChildrenCount(contentPresenter) > 0)
                {
                    UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    if (contentVisual != null)
                    {
                        DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                        if (thumb != null)
                        {
                            ControlTemplate template =
                                DrawItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                thumb.Template = template;
                        }
                    }
                }
            }
        }
    }
}
