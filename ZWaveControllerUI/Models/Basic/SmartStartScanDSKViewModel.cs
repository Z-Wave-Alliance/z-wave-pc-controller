/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWaveController.Interfaces;
using ZWaveController.Models;
using ZWaveController.Services;

namespace ZWaveControllerUI.Models
{
    public class SmartStartScanDSKViewModel : DialogVMBase
    {
        private const int LEAD_IN_LENGTH = 2;
        private const int VERSION_LENGTH = 2;
        private const int CHEKSUM_LENGTH = 5;

        private string _inputQRCode;
        public string InputQRCode
        {
            get
            {
                return _inputQRCode;
            }

            set
            {
                _inputQRCode = value;
                Notify("InputQRCode");

                QrCodeOptions = _qRCodeEncoder.ParseQrCode(_inputQRCode);
                if (QrCodeOptions.QrHeader.DSK != null && QrCodeOptions.QrHeader.DSK.Length == 40)//(!string.IsNullOrEmpty(_inputQRCode))
                {
                    var cheksum = _qRCodeEncoder.CalculateCheksum(_inputQRCode.Substring(9, _inputQRCode.Length - 9));
                    IsValidCheksum = cheksum == _inputQRCode.Substring(LEAD_IN_LENGTH + VERSION_LENGTH, CHEKSUM_LENGTH);
                }
                else
                {
                    IsValidCheksum = true;
                }
                ParsedQR = QrCodeOptions.ToTreeView(IsValidCheksum);
            }
        }

        private QrCodeOptions _qrCodeOptions;
        public QrCodeOptions QrCodeOptions
        {
            get
            {
                return _qrCodeOptions;
            }
            set
            {
                _qrCodeOptions = value;
                Notify("QrCodeOptions");
            }
        }

        private List<StringFamilyTree> _parsedQR;
        public List<StringFamilyTree> ParsedQR
        {
            get
            {
                return _parsedQR;
            }
            set
            {
                _parsedQR = value;
                Notify("ParsedQR");
            }
        }

        private bool _isValidCheksum;
        public bool IsValidCheksum
        {
            get
            {
                return _isValidCheksum;
            }

            set
            {
                _isValidCheksum = value;
                Notify("IsValidCheksum");
            }
        }


        private QRCodeEncoder _qRCodeEncoder;
        public SmartStartScanDSKViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "Enter DSK";
            Description = "Enter DSK of the joining node";

            DialogSettings.IsModal = true;
            DialogSettings.IsTopmost = true;
            _qRCodeEncoder = new QRCodeEncoder();

        }
    }
}
