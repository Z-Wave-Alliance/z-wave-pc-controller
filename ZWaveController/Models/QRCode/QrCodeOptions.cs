/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;

namespace ZWaveController.Models
{
    public class QrCodeOptions
    {
        public QrCodeOptions()
        {
            QrHeader = new QrHeader();
            TypesCollection = new List<Tlv>();
        }
        public QrHeader QrHeader { get; set; }
        public List<Tlv> TypesCollection { get; set; }

        /// <summary>
        /// Prepare QrCodeOptions to display on a TreeView.
        /// UI specific method.
        /// </summary>
        /// <param name="IsValidChecksum"></param>
        /// <returns></returns>
        public List<StringFamilyTree> ToTreeView(bool IsValidChecksum)
        {
            List<StringFamilyTree> ret = new List<StringFamilyTree>();
            ret.Add(new StringFamilyTree { Name = "LeadIn", Value = QrHeader.LeadIn, IsValid = true });
            ret.Add(new StringFamilyTree { Name = "Version", Value = QrHeader.Version, IsValid = true });
            ret.Add(new StringFamilyTree { Name = "Checksum", Value = QrHeader.Checksum, IsValid = IsValidChecksum });
            ret.Add(new StringFamilyTree { Name = "RequestedKeys", Value = QrHeader.RequestedKeys, IsValid = true });
            ret.Add(new StringFamilyTree { Name = "DSK", Value = QrHeader.DSK, IsValid = true });
            foreach (var tlv in TypesCollection)
            {
                List<StringFamilyTree> tlvChildren = new List<StringFamilyTree>();
                tlvChildren.Add(new StringFamilyTree { Name = "Type Id", Value = tlv.TypeId.ToString("X2"), IsValid = true });
                tlvChildren.Add(new StringFamilyTree { Name = "Length", Value = tlv.Length.ToString(), IsValid = true });

                if (tlv.IsValid)
                {
                    switch ((TlvTypes)tlv.TypeId)
                    {
                        case TlvTypes.ProductType:
                            var tlv0 = tlv as Tlv0;
                            tlvChildren.Add(new StringFamilyTree { Name = "DeviceType", Value = tlv0.DeviceType, IsValid = true });
                            tlvChildren.Add(new StringFamilyTree { Name = "InstallerIconType", Value = tlv0.InstallerIconType, IsValid = true });
                            break;
                        case TlvTypes.ProductId:
                            var tlv2 = tlv as Tlv2;
                            tlvChildren.Add(new StringFamilyTree { Name = "ManufacturerId", Value = tlv2.ManufacturerId, IsValid = true });
                            tlvChildren.Add(new StringFamilyTree { Name = "ProductType", Value = tlv2.ProductType, IsValid = true });
                            tlvChildren.Add(new StringFamilyTree { Name = "ProductId", Value = tlv2.ProductId, IsValid = true });
                            tlvChildren.Add(new StringFamilyTree { Name = "ApplicationVersion", Value = tlv2.ApplicationVersion, IsValid = true });
                            break;
                        case TlvTypes.MaxInclusionRequestInterval:
                            var tlv4 = tlv as Tlv4;
                            tlvChildren.Add(new StringFamilyTree { Name = "Interval", Value = tlv4.Interval, IsValid = true });
                            break;
                        case TlvTypes.UUID16:
                            var tlv6 = tlv as Tlv6;
                            tlvChildren.Add(new StringFamilyTree { Name = "UUIDPressFormat", Value = tlv6.PresentationFormat, IsValid = true });
                            tlvChildren.Add(new StringFamilyTree { Name = "UUIDData", Value = tlv6.UUID16, IsValid = true });
                            break;
                        case TlvTypes.SupportedProtocols:
                            var tlv8 = tlv as Tlv8;
                            tlvChildren.Add(new StringFamilyTree { Name = "SupportedProtocols", Value = tlv8.SupportedProtocols, IsValid = true });
                            break;
                        case TlvTypes.Name:
                        case TlvTypes.Location:
                        case TlvTypes.SmartStartInclusionSetting:
                        case TlvTypes.AdvancedJoining:
                        case TlvTypes.BootstrappingMode:
                        case TlvTypes.NetworkStatus:
                        default:
                            //Not implemented but display correctly:
                            tlvChildren.Add(new StringFamilyTree { Name = "Value", Value = tlv.Value, IsValid = true });
                            break;
                    }
                }
                else
                {
                    tlvChildren.Add(new StringFamilyTree { Name = "Invalid Value", Value = tlv.Value, IsValid = false });

                }
                ret.Add(new StringFamilyTree { Name = $"{(TlvTypes)tlv.TypeId} (TLV{tlv.Type})", Children = tlvChildren.ToArray(), IsValid = tlv.IsValid });
            }

            return ret;
        }
    }
}
