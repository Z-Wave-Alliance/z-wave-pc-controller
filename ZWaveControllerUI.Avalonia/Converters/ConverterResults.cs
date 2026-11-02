// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using Avalonia.Data;

namespace ZWaveControllerUI.Converters
{
    internal static class ConverterResults
    {
        /// <summary>
        /// ConvertBack result for text that does not parse. WPF converters return DependencyProperty.UnsetValue,
        /// which keeps the source unchanged and marks the input as invalid; this is the Avalonia equivalent.
        /// </summary>
        public static BindingNotification Invalid(string message = "Invalid value")
        {
            return new BindingNotification(new FormatException(message), BindingErrorType.DataValidationError);
        }
    }
}
