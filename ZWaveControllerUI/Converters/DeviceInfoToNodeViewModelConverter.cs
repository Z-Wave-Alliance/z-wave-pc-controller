/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Utils.UI;
using ZWave.Devices;
using ZWave.Enums;
using ZWave.Xml.Application;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Converters
{
    public class DeviceInfoToNodeViewModelConverter : ZWaveDefinitionReference, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is NodeTag && Network != null && ZWaveDefinition != null)
            {
                var device = (NodeTag)value;
                string deviceType = string.Empty;
                if (ZWaveDefinition != null)
                {
                    BasicDevice basicDevice = ZWaveDefinition.BasicDevices.FirstOrDefault(p => p.KeyId == Network.GetNodeInfo(device).Basic);
                    GenericDevice genericDevice = ZWaveDefinition.GenericDevices.FirstOrDefault(p => p.KeyId == Network.GetNodeInfo(device).Generic);

                    deviceType = genericDevice != null
                        ? genericDevice.Text
                        : basicDevice != null
                            ? basicDevice.Text
                            : "Unknown";
                }

                if (ZWaveDefinition == null)
                    return null;

                var paramValues = GetNodeInfoPayloadValues(device, Network, ZWaveDefinition);
                var result = PrepareTreeViewViewModel(paramValues, 1, ZWaveDefinition);
                var secureCommandClasses = Network.GetSecureCommandClasses(device);

                if (secureCommandClasses != null && secureCommandClasses.Length > 0)
                {
                    AddSupported("Secure Command Classes: ", secureCommandClasses, ZWaveDefinition, result);
                }

                if (Network.GetRoleType(device) != RoleTypes.None)
                {
                    string roleTypeValue = Network.GetRoleType(device).ToString();
                    CommandClass cmdClass = ZWaveDefinition.FindCommandClass("COMMAND_CLASS_ZWAVEPLUS_INFO", 1);
                    if (cmdClass.DefineSet != null)
                    {
                        DefineSet def_set = cmdClass.DefineSet.FirstOrDefault(p => p.Name == "roleType");
                        if (def_set != null && def_set.Define != null)
                        {
                            Define def = def_set.Define.FirstOrDefault(p => p.KeyId == (byte)Network.GetRoleType(device));
                            if (def != null)
                            {
                                roleTypeValue = def.Name;

                            }
                        }
                    }

                    TreeViewItemModel roleTypeItem = new TreeViewItemModel();
                    string roleTypeValueText = roleTypeValue;
                    roleTypeValueText = roleTypeValueText.Replace("ROLE_TYPE_", "");
                    roleTypeValueText.Trim();
                    roleTypeItem.Text = string.Format("Role Type: {0}", roleTypeValueText);
                    roleTypeItem.DepthLevel = 1;
                    roleTypeItem.IconText = "Tag";
                    result.Add(roleTypeItem);
                }
                return result;
            }
            return null;
        }

        private static void AddSupported(string leafText, byte[] cmdClassIds, ZWaveDefinition zWaveDefinition, IList<TreeViewItemModel> result)
        {
            TreeViewItemModel securelySupportedItem = new TreeViewItemModel();
            securelySupportedItem.Text = leafText;
            securelySupportedItem.IsExpanded = true;
            securelySupportedItem.DepthLevel = 1;
            securelySupportedItem.IconText = "Tag";
            securelySupportedItem.Children = new List<TreeViewItemModel>();
            foreach (byte cmdClassId in cmdClassIds)
            {
                string text = string.Format("{0} - ", cmdClassId.ToString("X2"));
                Command r = null;
                CommandClass cmdClass = zWaveDefinition.CommandClasses.Where(p => p.KeyId == cmdClassId).OrderByDescending(x => x.Version).FirstOrDefault();
                if (cmdClass != null)
                {
                    text += cmdClass.Name;
                    text = text.Replace("0x", "");
                    text = text.Replace("COMMAND_CLASS_", "");
                    if (cmdClass.Command != null)
                    {
                        r = cmdClass.Command.FirstOrDefault();
                    }
                    text.Trim();
                }
                securelySupportedItem.Children.Add(new TreeViewItemModel
                {
                    Text = text,
                    DepthLevel = 2,
                    Command = r,
                    IconText = "FileCode"
                });

            }

            result.Add(securelySupportedItem);
        }

        private IList<TreeViewItemModel> PrepareTreeViewViewModel(IEnumerable<ParamValue> paramValues, int depthLevel, ZWaveDefinition zWaveDefinition)
        {
            var result = new List<TreeViewItemModel>();
            foreach (var paramValue in paramValues)
            {
                var itemVm = new TreeViewItemModel
                {
                    DepthLevel = depthLevel,
                    IsExpanded = false,
                    IconText = "Tag"
                };

                string caption = string.Empty;
                if (paramValue.ParamDefinition != null)
                    caption = paramValue.ParamDefinition.Text;
                string text = string.Empty;
                if (paramValue.TextValueList == null || paramValue.TextValueList.Count <= 1)
                    text = paramValue.TextValue;
                else
                {
                    itemVm.Children = itemVm.Children ?? new List<TreeViewItemModel>();

                    foreach (var textVal in paramValue.TextValueList)
                    {
                        string _textVal = textVal;
                        _textVal = _textVal.Replace("COMMAND_CLASS_", "");
                        var child = new TreeViewItemModel();
                        child.Text = _textVal;
                        CommandClass commandClass = zWaveDefinition.CommandClasses.Where(cc => textVal.EndsWith(cc.Name)).OrderByDescending(x => x.Version).FirstOrDefault();
                        if (commandClass != null && commandClass.Command != null)
                        {
                            child.Command = commandClass.Command.FirstOrDefault();
                        }
                        child.DepthLevel = depthLevel + 1;
                        child.IsExpanded = true;
                        child.IconText = "FileCode";
                        itemVm.Children.Add(child);
                    }
                    itemVm.IsExpanded = true;
                }

                text = text.Replace("BASIC_TYPE_", "");
                text = text.Replace("GENERIC_TYPE_", "");
                text = text.Replace("SPECIFIC_TYPE_", "");

                itemVm.Text = string.Format("{0}: {1}", caption, text);

                if (paramValue.InnerValues != null && paramValue.InnerValues.Count > 0)
                {
                    itemVm.Children = itemVm.Children ?? new List<TreeViewItemModel>();

                    var innerValues = PrepareTreeViewViewModel(paramValue.InnerValues, depthLevel + 1, zWaveDefinition);
                    foreach (var innerValue in innerValues)
                        itemVm.Children.Add(innerValue);
                }

                result.Add(itemVm);
            }

            return result;
        }

        private IList<ParamValue> GetNodeInfoPayloadValues(NodeTag device, NetworkViewPoint network, ZWaveDefinition zWaveDefinition)
        {
            CommandClass cmdClass = zWaveDefinition.FindCommandClass("ZWAVE_CMD_CLASS", 1);
            Command cmd = zWaveDefinition.FindCommand("ZWAVE_CMD_CLASS", 1, "NODE_INFO");
            cmd.Param[3].OptionalReference = null;
            List<byte> payload = new List<byte>();
            payload.Add(cmdClass.KeyId);
            payload.Add(cmd.KeyId);
            var nInfo = network.GetNodeInfo(device);
            payload.AddRange((byte[])nInfo);
            if (network.GetCommandClasses(device) != null)
                payload.AddRange(network.GetCommandClasses(device));

            CommandClassValue[] cmdClassValues;
            zWaveDefinition.ParseApplicationObject(payload.ToArray(), out cmdClassValues);
            if (cmdClassValues != null)
                return cmdClassValues[0].CommandValue.ParamValues;

            return null;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TreeViewItemModel : EntityBase
    {
        private Command mCommand;
        public Command Command
        {
            get { return mCommand; }
            set
            {
                mCommand = value;
                Notify("Commands");
            }
        }

        private string mText;
        public string Text
        {
            get { return mText; }
            set
            {
                mText = value;
                Notify("Text");
            }
        }

        private string mIconText;
        public string IconText
        {
            get { return mIconText; }
            set
            {
                mIconText = value;
                Notify("IconText");
            }
        }

        private IList<TreeViewItemModel> mChildren;
        public IList<TreeViewItemModel> Children
        {
            get { return mChildren; }
            set
            {
                mChildren = value;
                Notify("Children");
            }
        }

        private int mDepthLevel;
        public int DepthLevel
        {
            get { return mDepthLevel; }
            set
            {
                mDepthLevel = value;
                Notify("DepthLevel");
            }
        }

        private bool mIsExpanded;
        public bool IsExpanded
        {
            get { return mIsExpanded; }
            set
            {
                mIsExpanded = value;
                Notify("IsExpanded");
            }
        }

        public TreeViewItemModel()
        {
            Children = new List<TreeViewItemModel>();
        }
    }
}