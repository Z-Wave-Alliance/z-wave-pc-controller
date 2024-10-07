/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils.UI;
using Utils;
using Utils.Threading;
using ZWave.Devices;

namespace ZWaveController.Models
{
    public class NetworkCanvasLayout : EntityBase
    {
        private Dictionary<NodeTag, int> mLayout = new Dictionary<NodeTag, int>();
        private readonly object mLockObject = new object();
        public ushort MaxY { get; set; }
        public ushort MaxX { get; set; }
        public int DeltaY { get; set; }
        public int DeltaX { get; set; }
        public int NodesPerLine { get; set; }
        public NetworkCanvasLayout()
        {
            MaxY = 1000;
            MaxX = 1000;
            DeltaY = 40;
            DeltaX = 40;
            NodesPerLine = 8;
        }

        public int Get(NodeTag nodeId)
        {
            int ret = 0;
            ushort x, y;
            Get(nodeId, out x, out y);
            ret = (y << 16) + x;
            return ret;
        }

        public bool Get(NodeTag nodeId, out ushort x, out ushort y)
        {
            bool ret = false;
            lock (mLockObject)
            {
                if (mLayout.ContainsKey(nodeId))
                {
                    int value = mLayout[nodeId];
                    y = (ushort)(value >> 16);
                    x = (ushort)value;
                    ret = true;
                }
                else if (nodeId.Id > 0)
                {
                    y = (ushort)(((nodeId.Id - 1) / NodesPerLine) * DeltaY);
                    x = (ushort)(((nodeId.Id - 1) % NodesPerLine) * DeltaX);
                }
                else
                {
                    y = 0;
                    x = 0;
                }
            }
            return ret;
        }

        public void Set(NodeTag nodeId, int topLeft)
        {
            lock (mLockObject)
            {
                if (mLayout.ContainsKey(nodeId))
                    mLayout[nodeId] = topLeft;
                else
                    mLayout.Add(nodeId, topLeft);
            }
        }

        public void Set(NodeTag nodeId, ushort x, ushort y)
        {
            Set(nodeId, (y << 16) + x);
        }

        public void Add(NodeTag nodeId)
        {
            lock (mLockObject)
            {

                mLayout.Add(nodeId, 0);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            lock (mLockObject)
            {
                sb.AppendFormat("0[{0},{1}];", MaxY, MaxX);
                for (ushort i = 1; i < 1024; i++)
                {
                    var nd = new NodeTag(i);
                    if (mLayout.ContainsKey(nd))
                    {
                        int value = mLayout[nd];
                        sb.AppendFormat("{0}[{1},{2}];", i, (ushort)(value >> 16), (ushort)value);
                    }
                }
            }
            return sb.ToString();
        }

        public static NetworkCanvasLayout FromString(string str)
        {
            NetworkCanvasLayout ret = new NetworkCanvasLayout();
            if (str != null)
            {
                try
                {
                    foreach (var item in str.Split(';'))
                    {
                        string[] tokens = item.Split(',', '[', ']');
                        if (tokens.Length > 2)
                        {
                            NodeTag nodeId = new NodeTag(Convert.ToUInt16(tokens[0]));
                            ushort y = Convert.ToUInt16(tokens[1]);
                            ushort x = Convert.ToUInt16(tokens[2]);
                            if (ret.MaxY == 0 && ret.MaxX == 0)
                            {
                                ret.MaxY = y;
                                ret.MaxX = x;
                            }
                            ret.mLayout.Add(nodeId, (y << 16) + x);
                        }
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
