/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using Utils.UI.Bind;
using ZWave.Devices;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    public enum IMAEntityTypes
    {
        None,
        Line,
        RoutingLine,
        Item
    }

    public interface IIMAFullNetworkModel
    {
        byte[] BackgroundColor { get; set; }
        string BackgroundImagePath { get; set; }
        byte ControllerNetworkHealth { get; set; }
        NodeTag DestinationNode { get; set; }
        bool HasSelectedItems { get; }
        ISubscribeCollection<IIMAEntity> Items { get; set; }
        NetworkCanvasLayout Layout { get; }
        ISubscribeCollection<IIMAEntity> SelectedDevices { get; set; }
        NodeTag SourceNode { get; set; }
        bool UseBackgroundColor { get; set; }

        void SetLayout(NetworkCanvasLayout networkCanvasLayout);
        void ClearRoutingLines();
        List<IIMAEntity> GetItems();
        List<IIMAEntity> GetItems(bool isIncludeNotSelected, bool isIncludeNonListeningNodes, bool isIncludeController);
        List<IIMAEntity> GetSelectedItems();
        IIMALine CreateIMALine(NetworkCanvasLayout layout, NodeTag fromId, NodeTag told, NodeTag routeId);
    }
}