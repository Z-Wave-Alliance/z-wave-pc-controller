/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class AsciiToByteArrayConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                List<byte> list = new List<byte>();
                if (value is byte)
                {
                    list.Add((byte)value);
                }
                else if (value is byte[])
                {
                    list.AddRange((byte[])value);
                }
                else
                {
                    return "";
                }

                string encodingString = null;
                if (parameter != null && parameter is string)
                {
                    encodingString = parameter as string;
                }
                else if (parameter != null && parameter is ObjectReference)
                {
                    var encodingParam = parameter as ObjectReference;
                    if (encodingParam.Value != null)
                    {
                        encodingString = encodingParam.Value as string;
                    }
                }

                if (encodingString != null)
                {
                    var encoding = Encoding.GetEncoding(encodingString);
                    return encoding.GetString(list.ToArray());
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string encodingString = null;
            if (parameter != null && parameter is string)
            {
                encodingString = parameter as string;
            }
            else if (parameter != null && parameter is ObjectReference)
            {
                var encodingParam = parameter as ObjectReference;
                if (encodingParam.Value != null)
                {
                    encodingString = encodingParam.Value as string;
                }
            }

            if (encodingString != null)
            {
                var encoding = Encoding.GetEncoding(encodingString);
                return encoding.GetBytes((string)value);
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
