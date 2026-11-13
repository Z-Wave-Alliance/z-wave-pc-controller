// SPDX-License-Identifier: BSD-3-Clause
// SPDX-FileCopyrightText: Silicon Laboratories Inc. <https://www.silabs.com>
// SPDX-FileCopyrightText: Z-Wave Alliance <https://z-wavealliance.org>
using Avalonia.Controls;
using ZWaveController.Services;
using ZWaveControllerUI.Bind;

namespace ZWaveControllerUI.Views
{
    public partial class ScanDSKView : UserControl
    {
        public ScanDSKView()
        {
            InitializeComponent();
            var validation = (NumbersValidationConverter)Resources["DskValidation"];
            IsHexInputCheckBox.IsCheckedChanged += (s, e) =>
            {
                validation.IsHexInput = IsHexInputCheckBox.IsChecked == true;
                InputTextBox.Focus();
            };
            QrCodeInput.TextChanged += (s, e) => TakePinFromQrCode(QrCodeInput.Text);
            Loaded += (s, e) =>
            {
                validation.IsHexInput = IsHexInputCheckBox.IsChecked == true;
                InputTextBox.Focus();
            };
        }

        /// <summary>
        /// The PIN is the first five digits of the DSK in the QR code.
        /// </summary>
        private void TakePinFromQrCode(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
                return;
            try
            {
                var parsedQR = new QRCodeEncoder().ParseQrCode(qrCode.Trim());
                if (parsedQR?.QrHeader?.DSK != null && parsedQR.QrHeader.DSK.Length == 40)
                    InputTextBox.Text = parsedQR.QrHeader.DSK.Substring(0, 5);
            }
            catch (System.Exception)
            {
                // Incomplete or invalid code: keep waiting for input.
            }
        }
    }
}
