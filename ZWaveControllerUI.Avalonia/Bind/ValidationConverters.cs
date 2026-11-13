// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Globalization;
using System.Net;
using Avalonia.Data.Converters;
using Utils;
using ZWaveControllerUI.Converters;

namespace ZWaveControllerUI.Bind
{
    /// <summary>
    /// The WPF build's ValidationRules, as converters: invalid text is reported as a data validation
    /// error on the text box and is not written to the view model.
    /// </summary>
    public class IPAddressValidationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string ipString = value as string;
            if (!IPAddress.TryParse(ipString, out _))
                return ConverterResults.Invalid(Tools.FormatStr("Invalid IP address: {0}", value));
            return ipString;
        }
    }

    public class NumbersValidationConverter : IValueConverter
    {
        public bool IsHexInput { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                var commaSeparatedValues = strValue.Trim().Split(',', ' ', ';', '-');
                foreach (var separateValue in commaSeparatedValues)
                {
                    var separateValueTrimmed = separateValue.Trim();
                    if (separateValueTrimmed.Length == 0)
                    {
                        continue;
                    }
                    if (!IsValueNum(separateValueTrimmed))
                    {
                        return ConverterResults.Invalid("Incorrect format. \nInput string must contain values in decimal format. Allowed delimiters: space, comma, semicolon, dash. \nExample: 1234 12345 231 232");
                    }
                }
            }
            return strValue;
        }

        private bool IsValueNum(string separateValue)
        {
            if (IsHexInput)
                return uint.TryParse(separateValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);
            return uint.TryParse(separateValue, out _);
        }
    }
}
