// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Utils;

namespace ZWaveControllerUI.Converters
{
    public class ByteArrayToStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = value as byte[];

            if (bytes == null)
                return BindingOperations.DoNothing;

            return Tools.GetHex(bytes);
        }

        private List<char> chars = new List<char>(new[]
        {
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0',
            'a', 'b', 'c', 'd', 'e', 'f', 'A', 'B', 'C', 'D', 'E', 'F'
        });

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim()))
            {
                return null;
            }

            var hasSpaceTail = str.EndsWith(" ");

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < str.Length; i++)
            {
                if (chars.Contains(str[i]))
                {
                    sb.Append(str[i]);
                }
                else if (str[i] != ' ')
                {
                    return ConverterResults.Invalid();
                }
            }

            str = sb.ToString();
            if (string.IsNullOrEmpty(str))
            {
                return ConverterResults.Invalid();
            }

            if (str.Length % 2 != 0)
            {
                return ConverterResults.Invalid();
                //return BindingOperations.DoNothing;
            }
            if (hasSpaceTail)
            {
                str = str + " ";
            }

            byte[] ret = Tools.GetBytes(str);
            if (ret != null && ret.Length > 0)
            {
                var strLen = parameter as string;
                if (strLen != null)
                {
                    int len = 0;
                    if (int.TryParse(strLen, out len))
                    {
                        if (len > 0)
                        {
                            if (len > ret.Length)
                            {
                                ret = ret.Concat(new byte[len - ret.Length]).ToArray();
                            }
                            else
                            {
                                ret = ret.Take(len).ToArray();
                            }
                        }
                    }
                }
                return ret;
            }
            else
            {
                return ConverterResults.Invalid();
            }
        }

        #endregion
    }
}