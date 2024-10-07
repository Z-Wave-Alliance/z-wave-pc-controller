/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ZWaveControllerUI.Controls
{
    public class TextBoxDecimalMask : TextBox
    {
        private bool mIsDontReactOnTextChanged;
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            e.Handled = true;

            base.OnTextChanged(e);

            if (mIsDontReactOnTextChanged)
                return;

            if ((e.UndoAction != UndoAction.Redo || e.UndoAction != UndoAction.Undo) &&
                e.Changes.Count > 0)
            {
                foreach (var item in e.Changes)
                {
                    if (item.RemovedLength > 0 && item.AddedLength == 0)
                        return;
                }
            }

            var text = Text ?? string.Empty;
            var caretIndex = CaretIndex;
            var textLenPrev = text.Length;

            var newValueSb = new StringBuilder();
            foreach (var chr in text)
            {
                if (!IsValid(chr))
                    continue;
                newValueSb.Append(Char.ToUpper(chr));
            }

            var newValue = newValueSb.ToString();

            mIsDontReactOnTextChanged = true;
            Text = newValue;
            if (textLenPrev == newValue.Length - 1)
            {
                caretIndex++;
            }
            CaretIndex = caretIndex;

            mIsDontReactOnTextChanged = false;
        }

        private bool IsValid(char chr)
        {
            if (char.IsDigit(chr) || chr==' ')
                return true;

            return false;
        }
    }
}
