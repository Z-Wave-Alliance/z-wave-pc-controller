/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using Utils.UI.Interfaces;
using ZWave.Enums;
using ZWave.Xml.Application;
using ZWaveController.Enums;

namespace ZWaveController.Interfaces
{
    public interface ISetNodeInformationModel
    {
        bool IsReadOnlyView { get; }
        byte ZWavePlusVersion { get; set; }
        bool IsListening { get; set; }
        ISelectableItem<CommandClass>[] CommandClasses { get; set; }
        ISelectableItem<GenericDevice>[] GenericDevices { get; }
        GenericDevice SelectedGenericDevice { get; set; }
        SpecificDevice SelectedSpecificDevice { get; set; }
        DeviceOptionsView DeviceOption { get; set; }
        RoleTypes RoleType { get; set; }
        NodeTypes NodeType { get; set; }
    }
}