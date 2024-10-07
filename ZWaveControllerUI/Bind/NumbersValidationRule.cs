/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Globalization;
using System.Windows.Controls;
using Utils;

namespace ZWaveControllerUI.Bind
{
    public class NumbersValidationRule : ValidationRule
    {
        public bool IsHexInput { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult ret = ValidationResult.ValidResult;
            string strValue = value as string;
            bool isInputValid = true;
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
                        isInputValid = false;
                        break;
                    }
                }

                if (!isInputValid)
                {
                    ret = new ValidationResult(false, Tools.FormatStr("Incorrect format. \nInput string must contain values in decimal format. Allowed delimiters: space, comma, semicolon, dash. \nExample: 1234 12345 231 232", value));
                }
            }
            return ret;
        }

        private bool IsValueNum(string separateValue)
        {
            bool ret = false;
            if (IsHexInput)
            {
                if (uint.TryParse(separateValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint intValue))
                    ret = true;
            }
            else
            {
                if (uint.TryParse(separateValue, out uint intValue))
                    ret = true;
            }

            return ret;
        }
    }
}
