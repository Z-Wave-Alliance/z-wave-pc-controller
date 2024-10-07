/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Globalization;
using System.Linq;
using Utils;
using Utils.UI;
using ZWaveController.Interfaces;

namespace ZWaveControllerUI.Models
{
    public class ScanDSK : UserInput, IScanDSKModel
    {
        private bool _isHexInputChecked;
        public bool IsHexInputChecked
        {
            get
            {
                return _isHexInputChecked;
            }
            set
            {
                _isHexInputChecked = value;
                InputData = string.Empty;
                Notify("IsHexInputChecked");
                Notify("InputData");
            }
        }
    }

    public class ScanDSKViewModel : DialogVMBase, IScanDSKDialog
    {
        public IScanDSKModel State { get; set; }
        public ScanDSKViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {            
            Title = "Enter DSK";
            Description = "Enter first five digits and verify all other numbers";            
            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;
        }


        /// <summary>
        /// Parses input as hex or uint. Returns null if input is incorrect
        /// </summary>
        /// <param name="bytesCount"></param>
        /// <returns> Null if input is incorrect </returns>

        public byte[] GetBytesFromInput(int bytesCount = 2)
        {
            var ret = new byte[bytesCount];

            uint inputValue = 0;
            if (State.IsHexInputChecked)
            {
                var cleanInput = RemoveDelimiters(State.InputData);

                if (cleanInput.Length > bytesCount * 2 || !uint.TryParse(cleanInput, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out inputValue))
                {
                    return null;
                }
            }
            else
            {
                var strInput = RemoveDelimiters(State.InputData);
                if (strInput.Length <= 5)
                {
                    if (uint.TryParse(strInput, out uint val))
                        inputValue = val;
                }
                else if (strInput.Length <= 10)
                {
                    var strVal1 = strInput.Substring(0, 5);
                    var strVal2 = strInput.Substring(5);
                    if (uint.TryParse(strVal1, out uint val1) && uint.TryParse(strVal2, out uint val2))
                        inputValue = (val1 << 16) + val2;
                }
                else
                {
                    return null;
                }
            }

            ret = Tools.GetBytes(inputValue);

            if (bytesCount == 2)
            {
                ret = ret.Skip(2).Take(bytesCount).ToArray();
            }

            return ret;
        }

        private string RemoveDelimiters(string input)
        {
            return input.Trim().Replace(" ", "").Replace(",", "").Replace(";", "").Replace("-", "");
        }
    }
}