/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Utils;
using Utils.UI;
using Utils.UI.Interfaces;
using ZWave.BasicApplication.Security;
using ZWave.Configuration;
using ZWave.Devices;
using ZWave.Enums;

namespace ZWaveController.Configuration
{
    public partial class ConfigurationItem : EntityBase
    {
        public readonly static string CONFIG_PATH = Path.Combine("Silicon Labs", "Z-Wave PC Controller 5", "Saved Items");
        [XmlIgnore]
        public Func<NodeTag, ISelectableItem<NodeTag>> CreateSelectedNodeItem;
        [XmlIgnore]
        public NetworkViewPoint Network { get; set; }
        private ObservableCollection<ISelectableItem<NodeTag>> _nodes = new ObservableCollection<ISelectableItem<NodeTag>>();
        [XmlIgnore]
        public ObservableCollection<ISelectableItem<NodeTag>> Nodes
        {
            get
            {
                return _nodes;
            }
            set
            {
                _nodes = value;
                Notify("Nodes");
            }
        }

        public virtual void RefreshNodes()
        {
            Notify("Nodes");
            foreach (var item in Nodes)
            {
                item.RefreshBinding();
            }
        }

        public ConfigurationItem()
        {
            SecuritySettings = new SecuritySettings();
            PreKitting = new PreKitting();
            Node = new Collection<Node>();
            ViewSettings = new ViewSettings();
        }

        public ConfigurationItem(Func<NodeTag, ISelectableItem<NodeTag>> createSelectedNodeItem) : this()
        {
            CreateSelectedNodeItem = createSelectedNodeItem;
        }

        public ConfigurationItem(NetworkViewPoint network, Func<NodeTag, ISelectableItem<NodeTag>> createSelectedNodeItem)
            : this(createSelectedNodeItem)
        {
            Network = network;
        }

        public ConfigurationItem(ConfigurationItem configurationItem, Func<NodeTag, ISelectableItem<NodeTag>> createSelectedNodeItem)
            : this(createSelectedNodeItem)
        {
            AppVersion = GetCurrentAssemblyShortVersion();
            Node = configurationItem.Node;
            PreKitting = configurationItem.PreKitting;
            SecuritySettings = configurationItem.SecuritySettings;
            ViewSettings = configurationItem.ViewSettings;
        }

        public static string GetCurrentAssemblyShortVersion()
        {
            string ret = string.Empty;
            Assembly asm = Assembly.GetEntryAssembly();
            if (asm != null)
            {
                Version version = asm.GetName().Version;
                if (version.Build > 0)
                    ret = string.Format("{0}.{1:00}.{2:00}", version.Major, version.Minor, version.Build);
                else
                    ret = string.Format("{0}.{1:00}", version.Major, version.Minor);
            }
            return ret;
        }

        public void ToConfigurationItem(ConfigurationItem configurationItem)
        {
            configurationItem.Node = Node;
            configurationItem.PreKitting = PreKitting;
            configurationItem.ViewSettings = ViewSettings;
            configurationItem.SecuritySettings = SecuritySettings;
        }

        public static ConfigurationItem Load(NetworkViewPoint network, Func<NodeTag, ISelectableItem<NodeTag>> createSelectedNodeItem)
        {
            var ret = new ConfigurationItem(network, createSelectedNodeItem);
            string itemFileName = GetItemFileName(network.HomeId, network.NodeTag);
            if (File.Exists(itemFileName))
            {
                string xmlText = File.ReadAllText(itemFileName);
                if (!string.IsNullOrEmpty(xmlText))
                {
                    try
                    {
                        var configurationItem = XmlUtility.XmlStr2Obj<ConfigurationItem>(xmlText);
                        configurationItem.ToConfigurationItem(ret);
                    }
                    catch (InvalidOperationException)
                    {
                        "ConfigurationItem Load Parse Error, XmlStr2Obj"._DLOG();
                    }
                }
            }
            else
            {
                network.SetRoleType(RoleTypes.CONTROLLER_CENTRAL_STATIC);
                network.SetNodeType(NodeTypes.ZWAVEPLUS_NODE);
            }
            return ret;
        }

        public static string GetItemFileName(byte[] homeId, NodeTag node)
        {
            var shortName = string.Concat(((byte)node.Id).ToString("000"), "_", string.Concat(homeId.Select(x => x.ToString("x2").ToUpper()).ToArray()), ".xml");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CONFIG_PATH);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, shortName);
        }

        public void RemoveNode(NodeTag node)
        {
            //From Node List View
            if (Nodes.Count > 0)
            {
                var toRemove = Nodes.Where(n => n.Item.Parent == node).Select(n => n.Item).ToList();
                for (int i = Nodes.Count - 1; i >= 0; i--)
                {
                    var removeNode = toRemove.FirstOrDefault(rNode => rNode == Nodes[i].Item);
                    if (removeNode != NodeTag.Empty)
                    {
                        Nodes.RemoveAt(i);
                        toRemove.Remove(removeNode);
                    }
                }
            }
            //From Confirutaion file
            if (Node.Count > 0)
            {
                var toRemove = Node.Where(n => n.NodeTag.Parent == node).Select(n => n.NodeTag).ToList();
                for (int i = Node.Count - 1; i >= 0; i--)
                {
                    var removeNode = toRemove.FirstOrDefault(rNode => rNode == Node[i].NodeTag);
                    if (removeNode != NodeTag.Empty)
                    {
                        Node.RemoveAt(i);
                        toRemove.Remove(removeNode);
                    }
                }
            }

            ProvisioningItem[] removePItems = this.PreKitting.ProvisioningList.Where(p => p.NodeId == node.Id).ToArray();
            foreach (var item in removePItems)
            {
                item.NodeId = 0;
                item.State = PreKittingState.Pending;
                PreKitting.PendingProvisioningCount++;
            }
            Save();
        }

        public void AddOrUpdateNode(NodeTag node)
        {
            var cnode = Node.FirstOrDefault(x => x.NodeTag == node);
            if (cnode == null)
            {
                cnode = new Node(node);
                Node.Add(cnode);
            }

            var snode = Nodes.FirstOrDefault(x => x.Item == node);
            if (snode == null)
            {
                snode = CreateSelectedNodeItem(node);
                Nodes.Add(snode);
            }
            else
            {
                if (snode.Item == NodeTag.Empty)
                {
                    Nodes.Remove(snode);
                }
                else
                {
                    RemoveNode(node);
                }
                Nodes.Add(snode);
            }

            cnode.NodeInfo = Network.GetNodeInfo(node);
            cnode.CommandClasses = Network.GetCommandClasses(node);
            cnode.RoleType = (byte)Network.GetRoleType(node);
            cnode.RoleTypeSpecified = true;
            cnode.NodeType = (byte)Network.GetNodeType(node);
            cnode.NodeTypeSpecified = true;
            cnode.IsVirtual = Network.IsVirtual(node);
            cnode.IsVirtualSpecified = Network.IsVirtual(node);
            cnode.IsWakeupIntervalSet = Network.GetWakeupInterval(node);
            cnode.IsWakeupIntervalSetSpecified = Network.GetWakeupInterval(node);
            cnode.WakeupIntervalSetValue = Network.GetWakeupIntervalValue(node);
            cnode.WakeupIntervalSetValueSpecified = Network.GetWakeupIntervalValue(node) > -1;
            var secureCommandClasses = Network.GetSecureCommandClasses(node);
            var schemes = Network.GetSecuritySchemes(node);
            cnode.SecurityExtension.Clear();
            if (schemes != null && schemes.Length > 0)
            {
                NetworkKeyS2Flags keysMask = NetworkKeyS2Flags.None;
                foreach (var scheme in schemes)
                {
                    switch (scheme)
                    {
                        case SecuritySchemes.NONE:
                            break;
                        case SecuritySchemes.S2_UNAUTHENTICATED:
                            keysMask |= NetworkKeyS2Flags.S2Class0;
                            break;
                        case SecuritySchemes.S2_AUTHENTICATED:
                            keysMask |= NetworkKeyS2Flags.S2Class1;
                            break;
                        case SecuritySchemes.S2_ACCESS:
                            keysMask |= NetworkKeyS2Flags.S2Class2;
                            break;
                        case SecuritySchemes.S0:
                            keysMask |= NetworkKeyS2Flags.S0;
                            break;
                        case SecuritySchemes.S2_TEMP:
                            break;
                        default:
                            break;
                    }
                }
                cnode.SecurityExtension.Add(new SecurityExtension(keysMask, secureCommandClasses));
            }

            snode.RefreshBinding();
            Save();
        }

        public virtual void FillNodes(NodeTag[] nodecs)
        {
            var saveEndPoints = Nodes.Where(x => x.Item.EndPointId > 0).ToArray();
            Nodes.Clear();
            if (nodecs != null)
            {
                foreach (var nodec in nodecs)
                {
                    var nodes = FillNode(nodec);
                    foreach (var node in nodes)
                    {
                        AddOrUpdateNode(node);
                    }
                    foreach (var item in saveEndPoints.Where(x => x.Item == nodec))
                    {
                        AddOrUpdateNode(item.Item);
                    }
                }
            }
            for (int i = Node.Count - 1; i >= 0; i--)
            {
                if (!Nodes.Where(x => x.Item == Node[i].NodeTag).Any())
                {
                    Node.RemoveAt(i);
                }
            }
        }

        private NodeTag[] FillNode(NodeTag nodec)
        {
            try
            {
                var cnodes = Node.Where(x => x.Id == nodec.Id);
                if (cnodes.Any())
                {
                    var nodes = new List<NodeTag>();
                    foreach (var cnode in cnodes)
                    {
                        var node = cnode.NodeTag;
                        //Network.Add(nodeId);
                        Network.SetNodeInfo(node, cnode.NodeInfo);
                        Network.SetCommandClasses(node, cnode.CommandClasses);
                        Network.SetRoleType(node, (RoleTypes)cnode.RoleType);
                        Network.SetNodeType(node, (NodeTypes)cnode.NodeType);
                        Network.SetVirtual(node, cnode.IsVirtual);
                        if (cnode.IsWakeupIntervalSetSpecified)
                        {
                            Network.SetWakeupInterval(node, cnode.IsWakeupIntervalSet);
                            if (cnode.WakeupIntervalSetValueSpecified)
                            {
                                Network.SetWakeupIntervalValue(node, cnode.WakeupIntervalSetValue);
                            }
                        }
                        if (cnode.SecurityExtension != null)
                        {
                            List<SecuritySchemes> tmpList = new List<SecuritySchemes>();
                            foreach (var scheme in cnode.SecurityExtension)
                            {
                                if (scheme.KeysValue.HasFlag(NetworkKeyS2Flags.S2Class2))
                                {
                                    tmpList.Add(SecuritySchemes.S2_ACCESS);
                                }
                                if (scheme.KeysValue.HasFlag(NetworkKeyS2Flags.S2Class1))
                                {
                                    tmpList.Add(SecuritySchemes.S2_AUTHENTICATED);
                                }
                                if (scheme.KeysValue.HasFlag(NetworkKeyS2Flags.S2Class0))
                                {
                                    tmpList.Add(SecuritySchemes.S2_UNAUTHENTICATED);
                                }
                                if (scheme.KeysValue.HasFlag(NetworkKeyS2Flags.S0))
                                {
                                    tmpList.Add(SecuritySchemes.S0);
                                }
                            }
                            var schemes = tmpList.Distinct().ToArray();
                            if (schemes != null && schemes.Length > 0)
                            {
                                Network.SetSecuritySchemes(node, schemes);
                            }
                            else
                            {
                                Network.SetSecuritySchemes(node, null);
                            }
                            var secureCommandClasses = cnode.SecurityExtension.Select(x => x.CommandClasses);
                            if (secureCommandClasses.Any())
                            {
                                Network.SetSecureCommandClasses(node, secureCommandClasses.FirstOrDefault(x => x != null));
                            }
                        }
                        nodes.Add(node);
                    }
                    return nodes.ToArray();
                }
                else
                {
                    return new[] { nodec };
                }
            }
            catch (Exception ex)
            {
                "Failed config {0}"._DLOG(ex.Message);
#if DEBUG
                throw ex;
#else
                return new[] { nodec };
#endif
            }
        }

        public void Save()
        {
            SecuritySettings.IsCsaEnabled = Network.IsCsaEnabled;
            SecuritySettings.IsCsaEnabledSpecified = SecuritySettings.IsCsaEnabled;

            SecuritySettings.IsEnabledS0 = Network.IsEnabledS0;
            SecuritySettings.IsEnabledS2_UNAUTHENTICATED = Network.IsEnabledS2_UNAUTHENTICATED;
            SecuritySettings.IsEnabledS2_AUTHENTICATED = Network.IsEnabledS2_AUTHENTICATED;
            SecuritySettings.IsEnabledS2_ACCESS = Network.IsEnabledS2_ACCESS;

            SecuritySettings.NetworkKey.Clear();
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[7]);
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[0]);
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[1]);
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[2]);
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[3]);
            SecuritySettings.NetworkKey.Add(SecuritySettings.NetworkKeys[4]);

            var ItemFileName = GetItemFileName(Network.HomeId, Network.NodeTag);
            try
            {
                var wrapper = new ConfigurationItem(this, CreateSelectedNodeItem);
                File.WriteAllText(ItemFileName, XmlUtility.Obj2XmlStr(wrapper));
            }
            catch (Exception ex)
            {
                ex.Message._DLOG();
#if DEBUG
                throw ex;
#endif
            }
        }
    }

    public partial class PreKitting
    {
        private int _pendingProvisioningCount;
        [XmlIgnore]
        public int PendingProvisioningCount
        {
            get { return _pendingProvisioningCount; }
            set
            {
                _pendingProvisioningCount = value;
                RaisePropertyChanged("PendingProvisioningCount");
            }
        }

        public PreKitting()
        {
            ProvisioningList = new ObservableCollection<ProvisioningItem>();
            ProvisioningList.CollectionChanged += ProvisioningList_CollectionChanged;
        }

        private void ProvisioningList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            var pitem = item as ProvisioningItem;
                            if (pitem != null && pitem.State == PreKittingState.Pending)
                            {
                                PendingProvisioningCount++;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            var pitem = item as ProvisioningItem;
                            if (pitem != null && pitem.State == PreKittingState.Pending)
                            {
                                PendingProvisioningCount--;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            var pitem = item as ProvisioningItem;
                            if (pitem != null && pitem.State == PreKittingState.Pending)
                            {
                                PendingProvisioningCount++;
                            }
                        }
                    }
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            var pitem = item as ProvisioningItem;
                            if (pitem != null && pitem.State == PreKittingState.Pending)
                            {
                                PendingProvisioningCount--;
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    PendingProvisioningCount = 0;
                    break;
                default:
                    break;
            }
        }

        internal void UpdateProvisioningItem(byte[] dsk, NodeTag node, PreKittingState preKittingState)
        {
            var item = ProvisioningList.FirstOrDefault(pl => pl.Dsk != null && dsk != null && pl.Dsk.Take(16).SequenceEqual(dsk.Take(16)));
            if (item != null)
            {
                item.NodeId = node.Id;
                item.State = preKittingState;
                if (item.State == PreKittingState.Pending)
                {
                    PendingProvisioningCount++;
                }
                else
                {
                    PendingProvisioningCount--;
                }
            }
        }
    }

    public partial class ProvisioningItem
    {
        [XmlIgnore]
        public string Text { get { return ToString(); } }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("DSK: ");
            if (Dsk != null)
            {
                for (int i = 0; i < 16; i += 2)
                {
                    if (i >= 14)
                    {
                        if (i < Dsk.Length)
                        {
                            sb.Append(Tools.GetInt32(Dsk.Skip(i).Take(2).ToArray()).ToString("00000"));
                        }
                        else
                        {
                            sb.Append("xxxxx");
                        }
                    }
                    else
                    {
                        if (i < Dsk.Length)
                        {
                            sb.Append(Tools.GetInt32(Dsk.Skip(i).Take(2).ToArray()).ToString("00000") + "-");
                        }
                        else
                        {
                            sb.Append("xxxxx-");
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }

    struct StoredKey
    {
        public string SecurityId;
        public string Key;
    }

    public partial class SecuritySettings : EntityBase
    {
        private NetworkKey[] _networkKeys;
        [XmlIgnore]
        public NetworkKey[] NetworkKeys
        {
            get
            {
                return _networkKeys;
            }
            set
            {
                _networkKeys = value;
                Notify("NetworkKeys");
            }
        }

        public SecuritySettings()
        {
            TestS2Settings = new TestS2Settings();
            TestS0Settings = new TestS0Settings();
            NetworkKey = new Collection<NetworkKey>();
            _networkKeys = new NetworkKey[8];
            _networkKeys[7] = new NetworkKey { Class = 7 };
            _networkKeys[0] = new NetworkKey { Class = 0 };
            _networkKeys[1] = new NetworkKey { Class = 1 };
            _networkKeys[2] = new NetworkKey { Class = 2 };
            _networkKeys[3] = new NetworkKey { Class = 3 };
            _networkKeys[4] = new NetworkKey { Class = 4 };
        }

        [XmlIgnore]
        public bool IsEnabledS2_UNAUTHENTICATED
        {
            get { return GetNetworkKey(SecuritySchemes.S2_UNAUTHENTICATED).IsEnabled; }
            set { GetNetworkKey(SecuritySchemes.S2_UNAUTHENTICATED).IsEnabled = value; }
        }

        [XmlIgnore]
        public bool IsEnabledS2_AUTHENTICATED
        {
            get { return GetNetworkKey(SecuritySchemes.S2_AUTHENTICATED).IsEnabled; }
            set { GetNetworkKey(SecuritySchemes.S2_AUTHENTICATED).IsEnabled = value; }
        }

        [XmlIgnore]
        public bool IsEnabledS2_ACCESS
        {
            get { return GetNetworkKey(SecuritySchemes.S2_ACCESS).IsEnabled; }
            set { GetNetworkKey(SecuritySchemes.S2_ACCESS).IsEnabled = value; }
        }

        [XmlIgnore]
        public bool IsEnabledS0
        {
            get { return GetNetworkKey(SecuritySchemes.S0).IsEnabled; }
            set { GetNetworkKey(SecuritySchemes.S0).IsEnabled = value; }
        }

        public bool IsParameterS2Set(ParameterS2Type type, out TestParametersS2Settings param)
        {
            param = TestS2Settings.Parameters.FirstOrDefault(item => item.ParameterTypeV == type);
            if (param != null)
            {
                return param.IsEnabled;
            }
            return false;
        }

        public NetworkKey GetNetworkKey(SecuritySchemes scheme)
        {
            var ret = NetworkKeys[SecurityManagerInfo.GetNetworkKeyIndex(scheme, false)];
            return ret;
        }
        public NetworkKey GetNetworkKeyLR(SecuritySchemes scheme)
        {
            var ret = NetworkKeys[SecurityManagerInfo.GetNetworkKeyIndex(scheme, true)];
            return ret;
        }
    }

    public partial class TestS2Settings : EntityBase
    {
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                Notify("IsActive");
            }
        }

        public TestS2Settings()
        {
            Parameters = new ObservableCollection<TestParametersS2Settings>();
            Frames = new ObservableCollection<TestFrameS2Settings>();
            Extensions = new ObservableCollection<TestExtensionS2Settings>();
        }
    }

    public partial class TestS0Settings : EntityBase
    {
        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                Notify("IsActive");
            }
        }

        [XmlIgnore]
        public bool NonceInNetworkKeySetSpecified { get; set; }
        [XmlIgnore]
        public bool MACInNetworkKeySetSpecified { get; set; }
        [XmlIgnore]
        public bool ValueInNetworkKeySetSpecified { get; set; }
        [XmlIgnore]
        public bool NonceInNetworkKeyVerifySpecified { get; set; }
        [XmlIgnore]
        public bool MACInNetworkKeyVerifySpecified { get; set; }
        [XmlIgnore]
        public bool ValueInNetworkKeyVerifySpecified { get; set; }

    }

    public partial class ViewSettings
    {
        public ViewSettings()
        {
            this.EncryptDecryptView = new EncryptDecryptView();
            this.IMAView = new IMAView();
            this.SecurityView = new SecurityView();
        }
    }
}
