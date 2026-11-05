// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data;
using Avalonia.Input;

namespace ZWaveControllerUI.Controls
{
    /// <summary>
    /// Integer spin box with the WPF build's NumericUpDown API (MinValue, MaxValue, MinorDelta, MajorDelta)
    /// on top of Avalonia's NumericUpDown. Views bind IntValue, the int counterpart of WPF's Value.
    /// </summary>
    public class NumericUpDown : Avalonia.Controls.NumericUpDown
    {
        /// <summary>
        /// Integer value, as the WPF control's Value (Avalonia's Value is a nullable decimal). Bind this one,
        /// so converters and view models see ints as before.
        /// </summary>
        public static readonly StyledProperty<int> IntValueProperty =
            AvaloniaProperty.Register<NumericUpDown, int>(nameof(IntValue), defaultBindingMode: BindingMode.TwoWay);

        public static readonly StyledProperty<int> MinValueProperty =
            AvaloniaProperty.Register<NumericUpDown, int>(nameof(MinValue), 0);

        public static readonly StyledProperty<int> MaxValueProperty =
            AvaloniaProperty.Register<NumericUpDown, int>(nameof(MaxValue), 100000000);

        public static readonly StyledProperty<int> MinorDeltaProperty =
            AvaloniaProperty.Register<NumericUpDown, int>(nameof(MinorDelta), 1);

        public static readonly StyledProperty<int> MajorDeltaProperty =
            AvaloniaProperty.Register<NumericUpDown, int>(nameof(MajorDelta), 10);

        public int IntValue
        {
            get { return GetValue(IntValueProperty); }
            set { SetValue(IntValueProperty, value); }
        }

        public int MinValue
        {
            get { return GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public int MaxValue
        {
            get { return GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public int MinorDelta
        {
            get { return GetValue(MinorDeltaProperty); }
            set { SetValue(MinorDeltaProperty, value); }
        }

        public int MajorDelta
        {
            get { return GetValue(MajorDeltaProperty); }
            set { SetValue(MajorDeltaProperty, value); }
        }

        protected override Type StyleKeyOverride => typeof(Avalonia.Controls.NumericUpDown);

        public NumericUpDown()
        {
            FormatString = "0";
            ParsingNumberStyle = NumberStyles.Integer;
            Minimum = MinValue;
            Maximum = MaxValue;
            Increment = MinorDelta;
            Value = 0;
            ClipValueToMinMax = true;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IntValueProperty)
            {
                if (Value != IntValue)
                    Value = IntValue;
            }
            else if (change.Property == ValueProperty)
            {
                if (!Value.HasValue)
                    return;
                // Out of range input does not reach the view model, as with the WPF control.
                var clamped = (int)Math.Clamp(Value.Value, Minimum, Maximum);
                if (clamped != Value.Value)
                    Value = clamped;
                else if (clamped != IntValue)
                    IntValue = clamped;
            }
            else if (change.Property == MinValueProperty)
                Minimum = MinValue;
            else if (change.Property == MaxValueProperty)
                Maximum = MaxValue;
            else if (change.Property == MinorDeltaProperty)
                Increment = MinorDelta;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                var delta = e.Key == Key.PageUp ? MajorDelta : -MajorDelta;
                Value = Math.Clamp((Value ?? 0) + delta, Minimum, Maximum);
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }
    }
}
