/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;
using Utils.UI.Interfaces;
using ZWave.Enums;
using ZWave.Xml.Application;
using ZWaveController;
using ZWaveController.Commands;
using ZWaveController.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Services;

namespace ZWaveControllerUI.Models
{
    public class SetNodeInformationViewModel : VMBase, ISetNodeInformationModel
    {
        public bool IsReadOnlyView
        {
            get
            {
                return ApplicationModel.ConfigurationItem.Nodes.Count == 1 || ApplicationModel.Controller?.Id == 0;
            }
        }

        #region Commands
        public SetNodeInformationCommand SetNodeInformationCommand => CommandsFactory.CommandControllerSessionGet<SetNodeInformationCommand>();
        public SelectDefaultCommandClassesCommand SelectDefaultCommandClassesCommand => CommandsFactory.CommandControllerSessionGet<SelectDefaultCommandClassesCommand>();
        public CommandBase ClearActiveCommandClassesCommand => CommandsFactory.CommandBaseGet<CommandBase>(
              (x) => CommandClasses.Where(v => v.IsSelected).ToList().ForEach(v => v.IsSelected = false),
              (y) => CommandClasses.Where(v => v.IsSelected).Count() > 0);
        #endregion

        #region Properties
        private ISelectableItem<CommandClass>[] _commandClasses;
        public ISelectableItem<CommandClass>[] CommandClasses
        {
            get { return _commandClasses; }
            set
            {
                _commandClasses = value;
                Notify("CommandClasses");
            }
        }

        public ISelectableItem<GenericDevice>[] GenericDevices { get; private set; }
        private GenericDevice _selectedGenericDevice;
        public GenericDevice SelectedGenericDevice
        {
            get { return _selectedGenericDevice; }
            set
            {
                _selectedGenericDevice = value;
                Notify("SelectedGenericDevice");
            }
        }

        private SpecificDevice _selectedSpecificDevice;
        public SpecificDevice SelectedSpecificDevice
        {
            get { return _selectedSpecificDevice; }
            set
            {
                _selectedSpecificDevice = value;
                Notify("SelectedSpecificDevice");
            }
        }

        private DeviceOptionsView _deviceOption;
        public DeviceOptionsView DeviceOption
        {
            get { return _deviceOption; }
            set
            {
                _deviceOption = value;
                Notify("DeviceOption");
            }
        }

        private bool _listening;

        public bool IsListening
        {
            get { return _listening; }
            set
            {
                _listening = value;
                Notify("DeviceOption");
            }
        }

        private byte _zWavePlusVersion;
        public byte ZWavePlusVersion
        {
            get { return _zWavePlusVersion; }
            set
            {
                _zWavePlusVersion = value;
                Notify("ZWavePlusVersion");
            }
        }

        private RoleTypes _roleType;
        public RoleTypes RoleType
        {
            get { return _roleType; }
            set
            {
                _roleType = value;
                Notify("RoleType");
            }
        }

        private NodeTypes _nodeType;
        public NodeTypes NodeType
        {
            get { return _nodeType; }
            set
            {
                _nodeType = value;
                Notify("NodeType");
            }
        }
        #endregion

        public SetNodeInformationViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Set Node Information";
            Description = "List of supported command classes (securelly supported will be managed by PC Controller)";

            CommandClasses = ApplicationModel.ZWaveDefinition.CommandClasses.
                                Where(x => x.KeyId >= 0x20).
                                OrderByDescending(x => x.Version).
                                GroupBy(x => x.KeyId).
                                Select(x =>
                                {
                                    var ret = new SelectableItem<CommandClass>(x.First());
                                    ret.IsEnabled = NodeInformationService.DefaultCommandClasses.Contains(x.First().KeyId);
                                    return ret;
                                }).
                                ToArray();
            GenericDevices = ApplicationModel.ZWaveDefinition.GenericDevices.Select(x => new SelectableItem<GenericDevice>(x)).ToArray();
            GenericDevices.FirstOrDefault(p => p.Item.Name == "GENERIC_TYPE_STATIC_CONTROLLER").IsSelected = true;
            SelectedGenericDevice = GenericDevices.First(p => p.IsSelected).Item;
            SelectedSpecificDevice = SelectedGenericDevice.SpecificDevice.FirstOrDefault(p => p.Name == "SPECIFIC_TYPE_PC_CONTROLLER");
        }
    }
}
