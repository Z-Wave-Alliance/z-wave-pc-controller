/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// Thanks!
    /// </summary>
    public class DependencyPropertyProxy : FrameworkElement
    {
        public static readonly DependencyProperty InProperty;
        public static readonly DependencyProperty OutProperty;

        public DependencyPropertyProxy()
        {
            Visibility = Visibility.Collapsed;
        }

        static DependencyPropertyProxy()
        {
            var inMetadata = new FrameworkPropertyMetadata(
              delegate (DependencyObject p, DependencyPropertyChangedEventArgs args)
              {
                  if (null != BindingOperations.GetBinding(p, OutProperty))
                      (p as DependencyPropertyProxy).Out = args.NewValue;
              });

            inMetadata.BindsTwoWayByDefault = false;
            inMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            InProperty = DependencyProperty.Register("In",
                                                     typeof(object),
                                                     typeof(DependencyPropertyProxy),
                                                     inMetadata);

            var outMetadata = new FrameworkPropertyMetadata(
              delegate (DependencyObject p, DependencyPropertyChangedEventArgs args)
              {
                  ValueSource source = DependencyPropertyHelper.GetValueSource(p, args.Property);

                  if (source.BaseValueSource != BaseValueSource.Local)
                  {
                      DependencyPropertyProxy proxy = p as DependencyPropertyProxy;
                      object expected = proxy.In;
                      if (!ReferenceEquals(args.NewValue, expected))
                      {
                          Dispatcher.CurrentDispatcher.BeginInvoke(
                      DispatcherPriority.DataBind, new Action(delegate
                      {
                          proxy.Out = proxy.In;
                      }));
                      }
                  }
              });

            outMetadata.BindsTwoWayByDefault = true;
            outMetadata.DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            OutProperty = DependencyProperty.Register("Out", typeof(object), typeof(DependencyPropertyProxy), outMetadata);
        }

        public object In
        {
            get { return GetValue(InProperty); }
            set { SetValue(InProperty, value); }
        }

        public object Out
        {
            get { return GetValue(OutProperty); }
            set { SetValue(OutProperty, value); }
        }
    }
}