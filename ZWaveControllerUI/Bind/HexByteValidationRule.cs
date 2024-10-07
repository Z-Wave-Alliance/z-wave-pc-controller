/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Globalization;
using System.Windows.Controls;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class HexByteValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult ret = ValidationResult.ValidResult;
            string strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                byte[] bytes = Tools.GetBytes(strValue);
                string toValue = Tools.GetHexShort(bytes);
                string fromValue = strValue.ToUpper(cultureInfo).Replace("0X", "").Replace(" ", "").Replace(",", "").Replace(";", "");
                if (toValue == null || fromValue == null || !toValue.Equals(fromValue))
                    ret = new ValidationResult(false, Tools.FormatStr("Incorrect format. \nInput string must contain byte values in hex format. Allowed delimiters: space, comma, semicolon. \nExample: 0A BD 12 44", value));
            }
            return ret;
        }
    }
}
