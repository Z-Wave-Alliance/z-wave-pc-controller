/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Globalization;
using System.Windows.Controls;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class BytesValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult ret = ValidationResult.ValidResult;
            string strValue = value as string;
            strValue = strValue.Trim();
            bool isInputValid = true;
            if (!string.IsNullOrEmpty(strValue))
            {

                var commaSeparatedValues = strValue.Split(',', ' ', ';', '-');
                foreach (var separateValue in commaSeparatedValues)
                {
                    var separateValueTrimmed = separateValue.Trim();
                    if (separateValueTrimmed.Length == 0)
                    {
                        continue;
                    }
                    if (!IsValueInByte(separateValueTrimmed))
                    {
                        isInputValid = false;
                        break;
                    }
                }

                if (!isInputValid)
                {
                    ret = new ValidationResult(false, Tools.FormatStr("Incorrect format. \nInput string must contain byte values in decimal format. Allowed delimiters: space, comma, semicolon. \nExample: 2 3 231 232", value));
                }
            }
            return ret;
        }

        private bool IsValueInByte(string separateValue)
        {
            bool ret = false;
            byte byteValue;
            if (byte.TryParse(separateValue, out byteValue))
                ret = true;
            return ret;
        }
    }
}
