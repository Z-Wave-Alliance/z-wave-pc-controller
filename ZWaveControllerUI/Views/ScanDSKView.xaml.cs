/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Common;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using ZWaveController.Services;
using ZWaveController.Models;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for ScanDSKView.xaml
    /// </summary>
    public partial class ScanDSKView : UserControl
    {
        public ScanDSKView()
        {
            InitializeComponent();
            this.IsVisibleChanged += DialogOpened;
        }

        private CancellationTokenSource _cts;
        private Bitmap _pictureBoxSource;
        private WebCamCaptureService _videoCapture;

        private void DialogOpened(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && (bool)e.OldValue == false)
            {
                if (QRScannerCheckBox.IsChecked.HasValue && !QRScannerCheckBox.IsChecked.Value)
                    StartCamera();
            }
            if ((bool)e.NewValue == false && (bool)e.OldValue == true)
            {
                StopCamera();
                _pictureBoxSource = null;
                if (_cts != null)
                {
                    _cts.Cancel();
                }
            }
        }

        private void ShowImage_Collapsed(object sender, RoutedEventArgs e)
        {
            if (_videoCapture != null)
            {
                _videoCapture.Stop();
                _videoCapture.OnNewFrame -= CaptureVideo;
                _videoCapture.OnError -= OnCameraError;
                _videoCapture = null;
                ClearLocalResources();
            }
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }

        private void ShowImage_Expanded(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void ClearLocalResources()
        {
            _pictureBoxSource = null;
        }

        private void OnCameraError(string error)
        {
            _cts.Cancel();
        }

        void CaptureVideo(int width, int height, uint stride, uint pixelFormat, IntPtr data)
        {
            if (!Enum.TryParse(pixelFormat.ToString(), out PixelFormat format))
            {
                throw new ArgumentException("Invalid PixelFormat received from video capture library!");
            }

            var cameraFrame = new Bitmap(width, height, (int)stride, format, data);
            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new MemoryStream())
            {
                formatter.Serialize(stream, cameraFrame);
                stream.Seek(0, SeekOrigin.Begin);
                _pictureBoxSource = (Bitmap)formatter.Deserialize(stream);
            }
            try
            {
                System.Drawing.Image img = cameraFrame;

                var ms = new MemoryStream();
                img.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();

                Application.Current.Dispatcher.BeginInvoke
                (new ThreadStart(delegate
                {
                    this.ImageFromCamera.Source = bi;
                }));
            }
            catch (Exception)
            {
                // ignore
            }
        }

        public void StartCamera()
        {
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;
            StopCamera();
            _videoCapture = new WebCamCaptureService(0);
            _videoCapture.OnNewFrame += CaptureVideo;
            _videoCapture.OnError += OnCameraError;
            _videoCapture.Start();

            Task<string> parseDSK = Task.Factory.StartNew(() =>
            {
                string barcodeResult = string.Empty;
                while (_videoCapture != null &&
                       _videoCapture.IsRunning &&
                       barcodeResult == string.Empty)
                {
                    barcodeResult = string.Empty;
                    System.Threading.Thread.Sleep(300);
                    if (_pictureBoxSource != null)
                    {
                        barcodeResult = ExtractQRCodeMessageFromImage(_pictureBoxSource);
                    }
                }
                return barcodeResult;

            }, cancellationToken);

            Task continuationTask = parseDSK.ContinueWith((prevTask) =>
               {
                   var qrEncoder = new QRCodeEncoder();
                   var ParsedQR = qrEncoder.ParseQrCode(prevTask.Result);
                   if (ParsedQR.QrHeader.DSK != null && ParsedQR.QrHeader.DSK.Length == 40)
                   {
                       string Pin = ParsedQR.QrHeader.DSK.Substring(0, 5);
                       this.InputTextBox.Text = Pin;
                       StopCamera();
                       ClearLocalResources();
                   }
               }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private string ExtractQRCodeMessageFromImage(Bitmap bitmap)
        {
            string message = string.Empty;
            try
            {
                var reader = new BarcodeReader
                    (null, newbitmap => new BitmapLuminanceSource(bitmap), luminance => new GlobalHistogramBinarizer(luminance));

                reader.AutoRotate = true;
                reader.Options = new DecodingOptions { TryHarder = true, TryInverted = true };

                var result = reader.Decode(bitmap);

                if (result != null)
                {
                    return result.ToString();
                }
                else
                {
                    return message;
                }
            }
            catch (Exception)
            {
                return message;
            }
        }

        private void StopCamera()
        {
            if (_videoCapture != null)
            {
                _videoCapture.Stop();
                _videoCapture.OnNewFrame -= CaptureVideo;
                _videoCapture.OnError -= OnCameraError;
                _videoCapture = null;
            }
        }
        private const int LEAD_IN_LENGTH = 2;
        private const int VERSION_LENGTH = 2;
        private const int CHEKSUM_LENGTH = 5;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;
            bool IsValidCheksum = false;

            string QRCode = ((TextBox)sender).Text;

            var qrEncoder = new QRCodeEncoder();

            if (QRCode.Length > LEAD_IN_LENGTH + VERSION_LENGTH + CHEKSUM_LENGTH)
            {
                var cheksum = qrEncoder.CalculateCheksum(QRCode.Substring(9, QRCode.Length - 9));
                IsValidCheksum = cheksum == QRCode.Substring(LEAD_IN_LENGTH + VERSION_LENGTH, CHEKSUM_LENGTH);
            }

            //if (IsValidCheksum)
            //{
            Task<QrCodeOptions> parseDSKTask = Task.Factory.StartNew(() =>
            {
                string barcodeResult = string.Empty;
                var parsedQR = new QrCodeOptions();
                if (QRCode != string.Empty)
                {
                    parsedQR = qrEncoder.ParseQrCode(QRCode);
                }
                return parsedQR;
            }, cancellationToken);

            Task continuationTask = parseDSKTask.ContinueWith((prevTask) =>
            {
                QrCodeOptions ParsedQR = prevTask.Result;
                if (ParsedQR.QrHeader.DSK != null && ParsedQR.QrHeader.DSK.Length == 40)
                {
                    string Pin = ParsedQR.QrHeader.DSK.Substring(0, 5);
                    this.InputTextBox.Text = Pin;
                    StopCamera();
                    ClearLocalResources();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
            //}
            //else
            //{
            //    this.InputTextBox.Text = "00000";
            //}
        }

        private void QRScannerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            StopCamera();
            ClearLocalResources();
        }

        private void QRScannerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void IsHexInputCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }

        private void IsHexInputCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            InputTextBox.Focus();
        }
    }
}
