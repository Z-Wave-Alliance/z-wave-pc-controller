/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
using System.Linq;
using System.Collections.Generic;
using Utils;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController;
using ZWaveController.Constants;
using ZWaveController.Configuration;
using ZWave.Devices;

namespace ZWaveControllerUI.Models
{
    public class TopologyMapViewModel : VMBase, ITopologyMapModel
    {
        public TopologyMapReloadCommand TopologyMapReloadCommand => CommandsFactory.CommandControllerSessionGet<TopologyMapReloadCommand>();
        public Dictionary<NodeTag, uint> Legend { get; set; } = new Dictionary<NodeTag, uint>();

        public TopologyMapViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Topology Map";
        }

        private BitMatrix _matrix = new BitMatrix(TopologyMap.MAX_NODES_COUNT, TopologyMap.MAX_NODES_COUNT);
        public BitMatrix Matrix
        {
            get { return _matrix; }
            set
            {
                _matrix = value;
                Notify("Matrix");
            }
        }

        public uint GetGenericDeviceColorArgb(byte genericDeviceId)
        {
            uint result = 0xffffffff; // Default (White)
            if (ApplicationModel.GenericDeviceTypeColorItems != null && ApplicationModel.GenericDeviceTypeColorItems.Count > 0)
            {
                GenericDeviceTypeColorItem tms = ApplicationModel.GenericDeviceTypeColorItems.FirstOrDefault(p => p.GenericDeviceId == genericDeviceId);
                if (tms != null)
                    result = tms.ColorArgb;
            }
            return result;
        }
    }
}