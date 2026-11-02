// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Utils;

namespace ZWaveControllerUI.Controls
{
    /// <summary>
    /// Hex input box. With SymbolsCount set it is a fixed width field ("00 00" for 4 symbols) that is filled
    /// with zeros on focus and edited in overwrite mode, as in the WPF build; without it, a plain text box.
    /// </summary>
    public class TextBoxHexMask : TextBox
    {
        public static readonly StyledProperty<int> SymbolsCountProperty =
            AvaloniaProperty.Register<TextBoxHexMask, int>(nameof(SymbolsCount));

        public int SymbolsCount
        {
            get { return GetValue(SymbolsCountProperty); }
            set { SetValue(SymbolsCountProperty, value); }
        }

        public string HexMask
        {
            get { return SymbolsCount > 0 ? new byte[SymbolsCount / 2].GetHex() : string.Empty; }
        }

        protected override Type StyleKeyOverride => typeof(TextBox);

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == SymbolsCountProperty)
                MaxLength = HexMask.Length;
        }

        protected override void OnGotFocus(FocusChangedEventArgs e)
        {
            base.OnGotFocus(e);
            if (string.IsNullOrEmpty(Text) && HexMask.Length > 0)
            {
                Text = HexMask;
                CaretIndex = 0;
            }
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            // Overwrite mode for fixed width fields: the typed symbol replaces the one at the caret.
            var text = Text ?? string.Empty;
            if (HexMask.Length > 0 && SelectionStart == SelectionEnd && !string.IsNullOrEmpty(e.Text))
            {
                int caret = CaretIndex;
                if (caret < text.Length && IsSeparator(text[caret]))
                    caret++;
                if (caret < text.Length)
                {
                    SelectionStart = caret;
                    SelectionEnd = caret + 1;
                }
            }
            base.OnTextInput(e);
            // Step over separators so the next symbol overwrites a digit.
            text = Text ?? string.Empty;
            if (HexMask.Length > 0 && CaretIndex < text.Length && IsSeparator(text[CaretIndex]))
                CaretIndex++;
        }

        private static bool IsSeparator(char chr)
        {
            return chr == ' ' || chr == ':' || chr == '/';
        }
    }

    /// <summary>
    /// Decimal input box: keeps digits and spaces only.
    /// </summary>
    public class TextBoxDecimalMask : TextBox
    {
        protected override Type StyleKeyOverride => typeof(TextBox);

        protected override void OnTextInput(TextInputEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Text))
                return;
            foreach (var chr in e.Text)
            {
                if (!char.IsDigit(chr) && chr != ' ')
                {
                    e.Handled = true;
                    return;
                }
            }
            base.OnTextInput(e);
        }
    }
}
