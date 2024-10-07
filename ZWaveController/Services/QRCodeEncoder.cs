/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Utils;
using ZWaveController.Models;

namespace ZWaveController.Services
{
    public class QRCodeEncoder
    {
        const int SIZE_OF_TYPE = 2;
        const int SIZE_OF_LENGTH = 2;
        const int SIZE_OF_QR_HEADER = 52;

        // All magic numbers you can find in file QRCodeEncoder-2 Document "REL14108-1" on hi-stage
        private QrHeader ExtractSmartStartQrHeader(string qrHeader)
        {
            QrHeader QrHeader = new QrHeader();
            try
            {
                if (qrHeader.Length >= 2)
                {
                    QrHeader.LeadIn = $"{qrHeader.Substring(0, 2)} (\"{(char)(int.Parse(qrHeader.Substring(0, 2)))}\")";
                    if (qrHeader.Length >= 4)
                    {
                        QrHeader.Version = qrHeader.Substring(2, 2);
                        if (qrHeader.Length >= 5)
                        {
                            QrHeader.Checksum = qrHeader.Substring(4, 5);
                            if (qrHeader.Length >= 12)
                            {
                                QrHeader.RequestedKeys = qrHeader.Substring(9, 3);
                                if (qrHeader.Length >= 52)
                                {
                                    QrHeader.DSK = qrHeader.Substring(12, 40);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                "ScanDSK corrupted ExtractSmartStartQrHeader: {}"._DLOG(ex.Message.ToString());
            }
            return QrHeader;
        }
        private const int MINIMAL_QRCODE_SIZE = 52;
        private List<Tlv> ExtractTlvExtensions(string tlvExtensions)
        {
            bool canNotRecognize = false;
            List<Tlv> result = new List<Tlv>();
            try
            {
                while (tlvExtensions != string.Empty && !canNotRecognize)
                {
                    int type = int.Parse(tlvExtensions.Substring(0, SIZE_OF_TYPE));
                    switch (type)
                    {
                        case 0:
                            Tlv0 Tlv0 = new Tlv0(tlvExtensions);
                            tlvExtensions = tlvExtensions.Remove(0, Tlv0.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                            result.Add(Tlv0);
                            break;
                        case 2:
                            Tlv2 Tlv2 = new Tlv2(tlvExtensions);
                            tlvExtensions = tlvExtensions.Remove(0, Tlv2.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                            result.Add(Tlv2);
                            break;
                        case 4:
                            Tlv4 Tlv4 = new Tlv4(tlvExtensions);
                            tlvExtensions = tlvExtensions.Remove(0, Tlv4.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                            result.Add(Tlv4);
                            break;
                        case 6:
                            Tlv6 Tlv6 = new Tlv6(tlvExtensions);
                            tlvExtensions = tlvExtensions.Remove(0, Tlv6.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                            result.Add(Tlv6);
                            break;
                        case 8:
                            Tlv8 Tlv8 = new Tlv8(tlvExtensions);
                            tlvExtensions = tlvExtensions.Remove(0, Tlv8.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                            result.Add(Tlv8);
                            break;
                        default:
                            if (tlvExtensions.Length >= SIZE_OF_TYPE)
                            {
                                Tlv tlvDef = new Tlv(tlvExtensions);
                                tlvExtensions = tlvExtensions.Remove(0, tlvDef.Length + SIZE_OF_LENGTH + SIZE_OF_TYPE);
                                result.Add(tlvDef);
                            }
                            else
                            {
                                canNotRecognize = true;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                "ScanDSK corrupted ExtractTlvExtensions: {0}"._DLOG(ex.Message.ToString());
            }
            return result;
        }
        public QrCodeOptions ParseQrCode(string value)
        {
            QrCodeOptions qrCodeOptions = new QrCodeOptions();

            qrCodeOptions.QrHeader = ExtractSmartStartQrHeader(value);

            if (value.Length >= SIZE_OF_QR_HEADER)
            {
                string Extensions = value.Substring(SIZE_OF_QR_HEADER, value.Length - SIZE_OF_QR_HEADER);

                qrCodeOptions.TypesCollection = ExtractTlvExtensions(Extensions);
            }
            return qrCodeOptions;
        }

        public string CalculateCheksum(string input)
        {
            byte[] myByte = System.Text.ASCIIEncoding.Default.GetBytes(input);
            SHA1 sha = new SHA1CryptoServiceProvider();
            var hash = sha.ComputeHash(myByte);
            string Checksum = int.Parse((hash[0].ToString("X2") + hash[1].ToString("X2")), System.Globalization.NumberStyles.HexNumber).ToString().PadLeft(5, '0');
            return Checksum;
        }

    }
}
