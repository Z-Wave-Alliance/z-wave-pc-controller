/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Runtime.InteropServices;

namespace ZWaveController.Services
{
    public class WebCamCaptureService
    {
        #region DllImports

        private static bool Is64Bit { get { return Environment.Is64BitProcess; } }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void OnErrorDelegate([In, MarshalAs(UnmanagedType.LPStr)] string pszError);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public delegate void OnFrameInputDelegate([In, MarshalAs(UnmanagedType.I4)] int width,
            [In, MarshalAs(UnmanagedType.I4)] int height,
            [In, MarshalAs(UnmanagedType.U4)] uint stride,
            [In, MarshalAs(UnmanagedType.U4)] uint pixelFormat,
            IntPtr data);

        [DllImport("wcvcap64", EntryPoint = "OpenWebCam", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool OpenWebCam64([In, MarshalAs(UnmanagedType.U4)] uint deviceNo,
           [MarshalAs(UnmanagedType.FunctionPtr)] OnFrameInputDelegate onFrameInput,
           [MarshalAs(UnmanagedType.FunctionPtr)] OnErrorDelegate onError);

        [DllImport("wcvcap32", EntryPoint = "OpenWebCam", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private extern static bool OpenWebCam32([In, MarshalAs(UnmanagedType.U4)] uint deviceNo,
            [MarshalAs(UnmanagedType.FunctionPtr)] OnFrameInputDelegate onFrameInput,
            [MarshalAs(UnmanagedType.FunctionPtr)] OnErrorDelegate onError);

        public static bool OpenWebCam(uint deviceNo, OnFrameInputDelegate onFrameInput, OnErrorDelegate onError)
        {
            return Is64Bit ? 
                OpenWebCam64(deviceNo, onFrameInput, onError) :
                OpenWebCam32(deviceNo, onFrameInput, onError);
        }

        [DllImport("wcvcap64", EntryPoint = "CloseWebCam", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void CloseWebCam64([In, MarshalAs(UnmanagedType.U4)] uint deviceNo);

        [DllImport("wcvcap32", EntryPoint = "CloseWebCam", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private extern static void CloseWebCam32([In, MarshalAs(UnmanagedType.U4)] uint deviceNo);

        public static void CloseWebCam(uint deviceNo)
        {
            if (Is64Bit)
            {
                CloseWebCam64(deviceNo);
            }
            else
            {
                CloseWebCam32(deviceNo);
            }
        }

        #endregion

        public event Action<int, int, uint, uint, IntPtr> OnNewFrame;
        public event Action<string> OnError;

        private int _deviceNo;
        private OnFrameInputDelegate _onFrameInput;
        private OnErrorDelegate _onError;

        public bool IsRunning { get; private set; }

        public WebCamCaptureService(int deviceNo)
        {
            _deviceNo = deviceNo;
            _onFrameInput = new OnFrameInputDelegate(OnFrameInput);
            _onError = new OnErrorDelegate(OnInternalError);
        }
    
        public bool Start()
        {
            IsRunning = OpenWebCam((uint)_deviceNo, _onFrameInput, _onError);
            return IsRunning;
        }

        public void Stop()
        {
            if (IsRunning)
            {
                CloseWebCam((uint)_deviceNo);
                IsRunning = false;
            }
        }

        private void OnFrameInput(int width, int height, uint stride, uint pixelFormat, IntPtr data)
        {
            OnNewFrame?.Invoke(width, height, stride, pixelFormat, data);
        }

        private void OnInternalError(string error)
        {
            OnError?.Invoke(error);
        }
    }
}
