/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.UI;
using Utils;
using Utils.Threading;

namespace ZWaveController.Models
{
    public class NetworkGridLayout : EntityBase
    {
        private BiDictionary<byte, int> mLayout = new BiDictionary<byte, int>();
        private readonly object mLockObject = new object();
        public ushort RowsCount
        {
            get;
            set;
        }
        public ushort ColumnsCount
        {
            get;
            set;
        }
        public NetworkGridLayout()
        {

        }

        public bool GetLayout(byte nodeId, out ushort row, out ushort column)
        {
            bool ret = false;
            lock (mLockObject)
            {
                if (mLayout.ContainsKey(nodeId))
                {
                    int value = mLayout.GetValue(nodeId);
                    row = (ushort)(value >> 16);
                    column = (ushort)value;
                    ret = true;
                }
                else
                {
                    row = 0;
                    column = 0;
                }
            }
            return ret;
        }

        public bool SetLayout(byte nodeId, ushort row, ushort column)
        {
            lock (mLockObject)
            {
                return mLayout.Bind(nodeId, (row << 16) + column, false);
            }
        }

        public bool AddLayout(byte nodeId)
        {
            bool isAdded = false;
            lock (mLockObject)
            {
                for (int r = 0; r < RowsCount; r++)
                {
                    for (int c = 0; c < ColumnsCount; c++)
                    {
                        isAdded = mLayout.Bind(nodeId, (r << 16) + c, false);
                        if (isAdded)
                            break;
                    }
                    if (isAdded)
                        break;
                }
            }
            if (!isAdded)
            {
                RowsCount++;
                ColumnsCount++;
                isAdded = AddLayout(nodeId);
            }
            return isAdded;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            lock (mLockObject)
            {
                sb.AppendFormat("0[{0},{1}]", RowsCount, ColumnsCount);
                for (int i = 1; i < 255; i++)
                {
                    if (mLayout.ContainsKey((byte)i))
                    {
                        int value = mLayout.GetValue((byte)i);
                        sb.AppendFormat("{0}[{1},{2}]", (byte)i, (ushort)(value >> 16), (ushort)value);
                    }
                }
            }
            return sb.ToString();
        }

        public static NetworkGridLayout FromString(string str)
        {
            NetworkGridLayout ret = new NetworkGridLayout();
            if (str != null)
            {
                try
                {
                    foreach (var item in str.Split(';'))
                    {
                        string[] tokens = item.Split(',', '[', ']');
                        byte nodeId = Convert.ToByte(tokens[0]);
                        ushort row = Convert.ToUInt16(tokens[1]);
                        ushort column = Convert.ToUInt16(tokens[2]);
                        if (ret.RowsCount == 0 && ret.ColumnsCount == 0)
                        {
                            ret.RowsCount = row;
                            ret.ColumnsCount = column;
                        }
                        ret.mLayout.Bind(nodeId, (row << 16) + column, false);
                    }
                }
                catch (ArgumentOutOfRangeException)
                { }
            }
            return ret;
        }

        public void Clear()
        {
            lock (mLockObject)
            {
                mLayout.Clear();
            }
        }
    }
}
