/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Utils;

namespace ZWaveController.Models
{
    public class WakeupIntervalSetChecker
    {
        const int BYTES_COUNT = 32;
        const int NODES_COUNT = 256;
        private static BitArray mWakeupIntervalSets;
        private static WakeupIntervalSetChecker instance;


        private WakeupIntervalSetChecker()
        {
            mWakeupIntervalSets = new BitArray(NODES_COUNT);
            mWakeupIntervalSets.SetAll(false);
        }

        public static WakeupIntervalSetChecker Init()
        {
            if (instance == null)
                instance = new WakeupIntervalSetChecker();
            return instance;
        }

        public void SetNodeState(byte nodeId, bool state)
        {
            mWakeupIntervalSets.Set(nodeId, state);
        }

        public string ConvertToStr()
        {
            string ret = string.Empty;
            byte[] tmpByteArr = new byte[BYTES_COUNT];
            mWakeupIntervalSets.CopyTo(tmpByteArr, 0);
            ret = Tools.GetHexShort(tmpByteArr);
            return ret;
        }

        public void ConvertFromStr(string input)
        {
            byte[] inputByteArr = Tools.GetBytes(input);
            byte[] tmpByteArr = new byte[BYTES_COUNT];
            Array.Copy(inputByteArr, 0, tmpByteArr, 0, inputByteArr.Length > BYTES_COUNT ? BYTES_COUNT : inputByteArr.Length);
            mWakeupIntervalSets = new BitArray(tmpByteArr);
        }

        public bool IsAssigned(byte nodeId)
        {
            bool ret = mWakeupIntervalSets[nodeId];
            return ret;
        }

        public void Clear()
        {
            mWakeupIntervalSets.SetAll(false);
        }

    }
}
