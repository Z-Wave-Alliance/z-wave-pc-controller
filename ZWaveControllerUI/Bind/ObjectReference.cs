/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System.Windows;

namespace ZWaveControllerUI.Bind
{
    public class ObjectReference : Freezable
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof (object), typeof (ObjectReference), new PropertyMetadata(default(object)));

        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new ObjectReference();
        }
    }
}