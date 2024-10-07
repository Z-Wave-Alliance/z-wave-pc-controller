/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Linq;

namespace ZWaveController.Models
{
    /// <summary>
    /// Type-Length-Value types.
    /// Table 1, Provisioning List TLV Block:: Type encoding
    /// </summary>
    public enum TlvTypes
    {
        ProductType = 0x00,
        ProductId = 0x01,
        MaxInclusionRequestInterval = 0x02,
        UUID16 = 0x03,
        SupportedProtocols = 0x04,
        /*0x05..0x31 (5..49) Reserved for QR code compatible provisioning information types */
        Name = 0x32,
        Location = 0x33,
        SmartStartInclusionSetting = 0x34,
        AdvancedJoining = 0x35,
        BootstrappingMode = 0x36,
        NetworkStatus = 0x37,
    }


    /// <summary>
    /// Type-Length-Value.
    /// Provisioning Information Types to be specified in QR codes or delivered by provisioning applications.
    /// </summary>
    public class Tlv
    {
        public const int SIZE_OF_TYPE = 2;
        public const int SIZE_OF_LENGTH = 2;

        public byte Type { get; set; }
        public byte Length { get; set; }
        public string Value { get; set; }

        public byte TypeId { get => (byte)(Type >> 1 & 0x7F); }
        public bool IsCritical { get => (Type >> 0 & 0x01) != 0; } //(i | 0xFE) != 0xFE;
        public bool IsValid { get; set; }

        public Tlv(string data)
        {
            IsValid = !string.IsNullOrEmpty(data);
            if (IsValid)
            {
                IsValid = !data.Any(x => !char.IsDigit(x)); // contains only digits.
                IsValid = IsValid && data.Length >= SIZE_OF_TYPE;

                if (IsValid)
                {
                    var type = data.Substring(0, SIZE_OF_TYPE);
                    Type = byte.Parse(type);
                }

                IsValid = IsValid && data.Length >= SIZE_OF_TYPE + SIZE_OF_LENGTH;
                if (IsValid)
                {
                    Length = byte.Parse(data.Substring(SIZE_OF_TYPE, SIZE_OF_LENGTH));
                }

                IsValid = IsValid && data.Length - SIZE_OF_TYPE - SIZE_OF_LENGTH >= Length;
                if (IsValid)
                {
                    Value = data.Substring(SIZE_OF_TYPE + SIZE_OF_LENGTH, Length);
                }
            }
        }
    }

    /// <summary>
    /// ProductType Information Type (3.1.2.1).
    /// This Information Type is used to advertise the product type data of a supporting node.
    /// </summary>
    public class Tlv0 : Tlv
    {
        /// <summary>
        /// Length = 0x04
        /// </summary>
        const int DATA_TEXT_LEN = 10;

        public string DeviceType { get; set; }
        public string InstallerIconType { get; set; }

        public Tlv0(string data) : base(data)
        {
            IsValid = IsValid && Length == DATA_TEXT_LEN;
            if (IsValid)
            {
                DeviceType = ushort.TryParse(Value.Substring(0, 5), out ushort devTypeVal) ?
                    $"{Value.Substring(0, 5)} (0x{devTypeVal:x4})" :
                    Value.Substring(0, 5);
                InstallerIconType = ushort.TryParse(Value.Substring(5), out ushort insIconTypeVal) ?
                    $"{Value.Substring(5)} (0x{insIconTypeVal:x4})" :
                    InstallerIconType = Value.Substring(5);
            }
        }
    }

    /// <summary>
    /// ProductId Information Type (3.1.2.2).
    /// This Information Type is used to advertise the product identifying data of a supporting node.
    /// </summary>
    public class Tlv2 : Tlv
    {
        /// <summary>
        /// Length = 0x08
        /// </summary>
        const int DATA_TEXT_LEN = 20;

        public string ManufacturerId { get; set; }
        public string ProductType { get; set; }
        public string ProductId { get; set; }
        public string ApplicationVersion { get; set; }
        public Tlv2(string data) : base(data)
        {
            IsValid = IsValid && Length == DATA_TEXT_LEN;
            if (IsValid)
            {
                ManufacturerId = ushort.TryParse(Value.Substring(0, 5), out ushort manIdVal) ?
                    $"{Value.Substring(0, 5)} (0x{manIdVal:x4})" :
                    ManufacturerId = Value.Substring(0, 5);
                ProductType = ushort.TryParse(Value.Substring(5, 5), out ushort prodTypeVal) ?
                    $"{Value.Substring(5, 5)} (0x{prodTypeVal:x4})" :
                    ProductType = Value.Substring(5, 5);
                ProductId = ushort.TryParse(Value.Substring(10, 5), out ushort prodIdVal) ?
                    $"{Value.Substring(10, 5)} (0x{prodIdVal:x4})" :
                    ProductId = Value.Substring(10, 5);
                ApplicationVersion = ushort.TryParse(Value.Substring(15, 5), out ushort appVerVal) ?
                    $"{Value.Substring(15, 5)} (0x{(byte)(appVerVal >> 8):x2}.0x{(byte)appVerVal:x2})" :
                    ApplicationVersion = Value.Substring(15, 5);
            }
        }
    }

    /// <summary>
    /// MaxInclusion RequestInterval Information Type (3.1.2.3).
    /// This Information Type is used to advertise if a power constrained SmartStart node will issue inclusion request at a higher interval value than the default 512 seconds.
    /// </summary>
    public class Tlv4 : Tlv
    {
        public string Interval { get; set; }
        public Tlv4(string data) : base(data)
        {
            if (IsValid)
            {
                Interval = Value;
            }
        }

    }

    /// <summary>
    /// UUID16 Information Type (3.1.2.4).
    /// </summary>
    public class Tlv6 : Tlv
    {
        /// <summary>
        /// Length = 0x11
        /// </summary>
        const int DATA_TEXT_LEN = 32;

        public string PresentationFormat { get; set; }
        public string UUID16 { get; set; }
        public Tlv6(string data) : base(data)
        {
            //IsValid = IsValid && Length > DATA_TEXT_LEN;
            if (IsValid)
            {
                PresentationFormat = Value.Substring(0, 2);
                if (Value.Length > 2)
                {
                    UUID16 = data.Substring(2);
                }
            }
        }
    }

    /// <summary>
    /// Supported protocols (3.1.2.5).
    /// This Information Type is used to advertise which protocols are supported by the device.
    /// </summary>
    public class Tlv8 : Tlv
    {
        /// <summary>
        /// Length = N
        /// </summary>

        public string SupportedProtocols { get; set; }

        public Tlv8(string data) : base(data)
        {
            if (IsValid)
            {
                SupportedProtocols = Value;
            }
        }
    }

}
