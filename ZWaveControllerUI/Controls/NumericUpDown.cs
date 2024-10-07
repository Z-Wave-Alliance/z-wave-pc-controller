/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Globalization;

namespace ZWaveControllerUI.Controls
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
    public class NumericUpDown : Control
    {
        #region Properties

        #region Value

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(Int32), typeof(NumericUpDown),
                                        new PropertyMetadata((int)0, OnValueChanged, CoerceValue));

        public Int32 Value
        {
            get { return (Int32)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValueChanged(DependencyObject element,
                                           DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)element;

            if (control != null && control.TextBox != null)
            {
                control.TextBox.UndoLimit = 0;
                control.TextBox.UndoLimit = 1;
            }
        }

        private static object CoerceValue(DependencyObject element, object baseValue)
        {
            var control = (NumericUpDown)element;
            var value = (Int32)baseValue;

            control.CoerceValueToBounds(ref value);

            if (control.TextBox != null)
            {
                control.TextBox.Text = value.ToString();
            }

            return value;
        }

        #endregion

        #region MaxValue

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(Int32), typeof(NumericUpDown),
                                        new PropertyMetadata((int)100000000, OnMaxValueChanged,
                                                             CoerceMaxValue));

        public Int32 MaxValue
        {
            get { return (Int32)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        private static void OnMaxValueChanged(DependencyObject element,
                                              DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)element;
            var maxValue = (Int32)e.NewValue;

            // If maxValue steps over MinValue, shift it
            if (maxValue < control.MinValue)
            {
                control.MinValue = maxValue;
            }

            if (maxValue <= control.Value)
            {
                control.Value = maxValue;
            }
        }

        private static object CoerceMaxValue(DependencyObject element, Object baseValue)
        {
            var maxValue = (Int32)baseValue;

            if (maxValue == Int32.MaxValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return maxValue;
        }

        #endregion

        #region MinValue

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(Int32), typeof(NumericUpDown),
                                        new PropertyMetadata((int)0, OnMinValueChanged,
                                                             CoerceMinValue));

        public Int32 MinValue
        {
            get { return (Int32)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        private static void OnMinValueChanged(DependencyObject element,
                                              DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)element;
            var minValue = (Int32)e.NewValue;

            // If minValue steps over MaxValue, shift it
            if (minValue > control.MaxValue)
            {
                control.MaxValue = minValue;
            }

            if (minValue >= control.Value)
            {
                control.Value = minValue;
            }
        }

        private static object CoerceMinValue(DependencyObject element, Object baseValue)
        {
            var minValue = (Int32)baseValue;

            if (minValue == Int32.MinValue)
            {
                return DependencyProperty.UnsetValue;
            }

            return minValue;
        }

        #endregion

        #region MinorDelta

        public static readonly DependencyProperty MinorDeltaProperty =
            DependencyProperty.Register("MinorDelta", typeof(Int32), typeof(NumericUpDown),
                                        new PropertyMetadata((int)1, OnMinorDeltaChanged,
                                                             CoerceMinorDelta));

        public Int32 MinorDelta
        {
            get { return (Int32)GetValue(MinorDeltaProperty); }
            set { SetValue(MinorDeltaProperty, value); }
        }

        private static void OnMinorDeltaChanged(DependencyObject element,
                                                DependencyPropertyChangedEventArgs e)
        {
            var minorDelta = (Int32)e.NewValue;
            var control = (NumericUpDown)element;

            if (minorDelta > control.MajorDelta)
            {
                control.MajorDelta = minorDelta;
            }
        }

        private static object CoerceMinorDelta(DependencyObject element, Object baseValue)
        {
            var minorDelta = (Int32)baseValue;

            return minorDelta;
        }

        #endregion

        #region MajorDelta

        public static readonly DependencyProperty MajorDeltaProperty =
            DependencyProperty.Register("MajorDelta", typeof(Int32), typeof(NumericUpDown),
                                        new PropertyMetadata((int)10, OnMajorDeltaChanged,
                                                             CoerceMajorDelta));

        public Int32 MajorDelta
        {
            get { return (Int32)GetValue(MajorDeltaProperty); }
            set { SetValue(MajorDeltaProperty, value); }
        }

        private static void OnMajorDeltaChanged(DependencyObject element,
                                                DependencyPropertyChangedEventArgs e)
        {
            var majorDelta = (Int32)e.NewValue;
            var control = (NumericUpDown)element;

            if (majorDelta < control.MinorDelta)
            {
                control.MinorDelta = majorDelta;
            }
        }

        private static object CoerceMajorDelta(DependencyObject element, Object baseValue)
        {
            var majorDelta = (Int32)baseValue;

            return majorDelta;
        }

        #endregion

        #region IsAutoSelectionActive

        public static readonly DependencyProperty IsAutoSelectionActiveProperty =
            DependencyProperty.Register("IsAutoSelectionActive", typeof(Boolean), typeof(NumericUpDown),
                                        new PropertyMetadata(false));

        public Boolean IsAutoSelectionActive
        {
            get { return (Boolean)GetValue(IsAutoSelectionActiveProperty); }
            set { SetValue(IsAutoSelectionActiveProperty, value); }
        }

        #endregion

        #region IsValueWrapAllowed

        public static readonly DependencyProperty IsValueWrapAllowedProperty =
            DependencyProperty.Register("IsValueWrapAllowed", typeof(Boolean), typeof(NumericUpDown),
                                        new PropertyMetadata(false));

        public Boolean IsValueWrapAllowed
        {
            get { return (Boolean)GetValue(IsValueWrapAllowedProperty); }
            set { SetValue(IsValueWrapAllowedProperty, value); }
        }

        #endregion

        #endregion

        #region Fields

        protected readonly CultureInfo Culture;
        protected RepeatButton DecreaseButton;
        protected RepeatButton IncreaseButton;
        protected TextBox TextBox;

        #endregion

        #region Commands

        private readonly RoutedUICommand _minorDecreaseValueCommand =
            new RoutedUICommand("MinorDecreaseValue", "MinorDecreaseValue", typeof(NumericUpDown));

        private readonly RoutedUICommand _minorIncreaseValueCommand =
            new RoutedUICommand("MinorIncreaseValue", "MinorIncreaseValue", typeof(NumericUpDown));

        private readonly RoutedUICommand _majorDecreaseValueCommand =
            new RoutedUICommand("MajorDecreaseValue", "MajorDecreaseValue", typeof(NumericUpDown));

        private readonly RoutedUICommand _majorIncreaseValueCommand =
            new RoutedUICommand("MajorIncreaseValue", "MajorIncreaseValue", typeof(NumericUpDown));

        private readonly RoutedUICommand _updateValueStringCommand =
            new RoutedUICommand("UpdateValueString", "UpdateValueString", typeof(NumericUpDown));

        private readonly RoutedUICommand _cancelChangesCommand =
            new RoutedUICommand("CancelChanges", "CancelChanges", typeof(NumericUpDown));

        #endregion

        #region Constructors

        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown),
                                                     new FrameworkPropertyMetadata(
                                                         typeof(NumericUpDown)));
        }

        public NumericUpDown()
        {
            Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

            Loaded += OnLoaded;
        }

        #endregion

        #region Event handlers

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();
            AttachCommands();
        }

        private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateValue();
        }

        private void TextBoxOnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (IsAutoSelectionActive)
            {
                TextBox.SelectAll();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InvalidateProperty(ValueProperty);
        }

        private void ButtonOnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            Value = 0;
        }

        #endregion

        #region Utility Methods

        #region Attachment

        private void AttachToVisualTree()
        {
            AttachTextBox();
            AttachIncreaseButton();
            AttachDecreaseButton();
        }

        private void AttachTextBox()
        {
            var textBox = GetTemplateChild("PART_TextBox") as TextBox;

            // A null check is advised
            if (textBox != null)
            {
                TextBox = textBox;
                TextBox.LostFocus += TextBoxOnLostFocus;
                TextBox.PreviewMouseLeftButtonUp += TextBoxOnPreviewMouseLeftButtonUp;

                TextBox.UndoLimit = 1;
                TextBox.IsUndoEnabled = true;
            }
        }

        private void AttachIncreaseButton()
        {
            var increaseButton = GetTemplateChild("PART_IncreaseButton") as RepeatButton;
            if (increaseButton != null)
            {
                IncreaseButton = increaseButton;
                IncreaseButton.Focusable = false;
                IncreaseButton.Command = _minorIncreaseValueCommand;
                IncreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
                IncreaseButton.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
            }
        }

        private void AttachDecreaseButton()
        {
            var decreaseButton = GetTemplateChild("PART_DecreaseButton") as RepeatButton;
            if (decreaseButton != null)
            {
                DecreaseButton = decreaseButton;
                DecreaseButton.Focusable = false;
                DecreaseButton.Command = _minorDecreaseValueCommand;
                DecreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
                DecreaseButton.PreviewMouseRightButtonDown += ButtonOnPreviewMouseRightButtonDown;
            }
        }

        private void AttachCommands()
        {
            CommandBindings.Add(new CommandBinding(_minorIncreaseValueCommand, (a, b) => IncreaseValue(true)));
            CommandBindings.Add(new CommandBinding(_minorDecreaseValueCommand, (a, b) => DecreaseValue(true)));
            CommandBindings.Add(new CommandBinding(_majorIncreaseValueCommand, (a, b) => IncreaseValue(false)));
            CommandBindings.Add(new CommandBinding(_majorDecreaseValueCommand, (a, b) => DecreaseValue(false)));
            CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (a, b) => UpdateValue()));
            CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (a, b) => CancelChanges()));

            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_majorIncreaseValueCommand,
                                                                    new KeyGesture(Key.PageUp)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_majorDecreaseValueCommand,
                                                                    new KeyGesture(Key.PageDown)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox),
                                                     new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
        }

        #endregion

        #region Data retrieval and deposit

        private Int32 ParseStringToDecimal(String source)
        {
            Int32 value;
            Int32.TryParse(source, out value);

            return value;
        }

        #endregion

        #region SubCoercion

        private void CoerceValueToBounds(ref Int32 value)
        {
            if (value < MinValue)
            {
                value = MinValue;
            }
            else if (value > MaxValue)
            {
                value = MaxValue;
            }
        }

        #endregion

        #endregion

        #region Methods

        private void UpdateValue()
        {
            var val = ParseStringToDecimal(TextBox.Text);
            CoerceValueToBounds(ref val);
            Value = val;
        }

        private void CancelChanges()
        {
            TextBox.Undo();
        }

        private void RemoveFocus()
        {
            // Passes focus here and then just deletes it
            Focusable = true;
            Focus();
            Focusable = false;
        }

        private void IncreaseValue(Boolean minor)
        {
            // Get the value that's currently in the _textBox.Text
            int value = ParseStringToDecimal(TextBox.Text);

            // Only change the value if it has any meaning
            if (value >= MinValue)
            {
                if (minor)
                {
                    if (IsValueWrapAllowed && value + MinorDelta > MaxValue)
                    {
                        value = MinValue;
                    }
                    else
                    {
                        value += MinorDelta;
                    }
                }
                else
                {
                    if (IsValueWrapAllowed && value + MajorDelta > MaxValue)
                    {
                        value = MinValue;
                    }
                    else
                    {
                        value += MajorDelta;
                    }
                }

                // Coerce the value to min/max
                CoerceValueToBounds(ref value);

            }

            Value = value;
        }

        private void DecreaseValue(Boolean minor)
        {
            // Get the value that's currently in the _textBox.Text
            int value = ParseStringToDecimal(TextBox.Text);

            // Coerce the value to min/max
            CoerceValueToBounds(ref value);

            // Only change the value if it has any meaning
            if (value <= MaxValue)
            {
                if (minor)
                {
                    if (IsValueWrapAllowed && value - MinorDelta < MinValue)
                    {
                        value = MaxValue;
                    }
                    else
                    {
                        if (value - MinorDelta >= MinValue)
                            value -= MinorDelta;
                    }
                }
                else
                {
                    if (IsValueWrapAllowed && value - MajorDelta < MinValue)
                    {
                        value = MaxValue;
                    }
                    else
                    {
                        value -= MajorDelta;
                    }
                }
            }

            Value = value;
        }

        #endregion
    }
}
