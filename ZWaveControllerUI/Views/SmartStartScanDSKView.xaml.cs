/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZWaveController.Services;
using ZWaveControllerUI.Models;
using ZXing;
using ZXing.Common;

namespace ZWaveControllerUI.Views
{
    /// <summary>
    /// Interaction logic for SmartStartScanDSKView.xaml
    /// </summary>
    public partial class SmartStartScanDSKView : UserControl
    {
        public SmartStartScanDSKView()
        {
            InitializeComponent();
            this.QRScannerCheckBox.IsChecked = true;
            this.IsVisibleChanged += DialogOpened;
        }

        private WebCamCaptureService _videoCapture;
        private CancellationTokenSource _cts;
        private Bitmap _pictureBoxSource;

        private void DialogOpened(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == true && (bool)e.OldValue == false)
            {
                //StartCamera(); //do we need start and stop on dialog load?
            }
            if ((bool)e.NewValue == false && (bool)e.OldValue == true)
            {
                if (_videoCapture != null)
                {
                    _videoCapture.Stop();
                    _videoCapture.OnNewFrame -= OnNewCameraFrame;
                    _videoCapture.OnError -= OnCameraError;
                    _videoCapture = null;
                }
                _pictureBoxSource = null;
                _cts?.Cancel();
            }
        }

        private void OnCameraError(string error)
        {
            _cts.Cancel();
        }

        void OnNewCameraFrame(int width, int height, uint stride, uint pixelFormat, IntPtr data)
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

        private void StopCamera()
        {
            if (_videoCapture != null)
            {
                _videoCapture.Stop();
                _videoCapture.OnNewFrame -= OnNewCameraFrame;
                _videoCapture.OnError -= OnCameraError;
                _videoCapture = null;
            }
        }

        public void StartCamera()
        {
            _cts = new CancellationTokenSource();
            CancellationToken cancellationToken = _cts.Token;
            StopCamera();
            _videoCapture = new WebCamCaptureService(0);
            _videoCapture.OnNewFrame += OnNewCameraFrame;
            _videoCapture.OnError += OnCameraError;
            _videoCapture.Start();

            Task<string> parseDSKTask = Task.Factory.StartNew(() =>
            {
                string QRCode = string.Empty;
                while (_videoCapture != null && _videoCapture.IsRunning && QRCode == string.Empty)
                {
                    QRCode = string.Empty;
                    Thread.Sleep(300);
                    if (_pictureBoxSource != null)
                    {
                        QRCode = ExtractQRCodeMessageFromImage(_pictureBoxSource);
                    }
                }
                return QRCode;

            }, cancellationToken);

            Task continuationTask = parseDSKTask.ContinueWith((prevTask) =>
            {
                var _model = (SmartStartScanDSKViewModel)DataContext;
                _model.InputQRCode = prevTask.Result;
                StopCamera();
                ClearLocalResources();
            },
            TaskScheduler.FromCurrentSynchronizationContext());
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

        private void QRScannerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void QRScannerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            StopCamera();
            ClearLocalResources();
        }

        private void ClearLocalResources()
        {
            _pictureBoxSource = null;
        }

    }
}
