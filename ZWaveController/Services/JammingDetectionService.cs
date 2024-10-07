/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave.BasicApplication.Enums;
using ZWaveController.Interfaces;
using ZWaveController.Models;

namespace ZWaveController.Services
{
    public class JammingDetectionService : IJammingDetectionService
    {
        private const byte INVALID = 0x7F;
        private const int WARNING_LEVEL_DB = -30;
        private const int MEASUMEASUREMENTS_CAPACITY = 10;
        private const int MEASUREMENTS_COUNT = 3;
        private const int MEASURE_PERIOD_MS = 500;

        private BasicControllerSession _controllerSession;
        private INetworkStatisticsModel NetworkStatisticsModel => _controllerSession.ApplicationModel.NetworkStatisticsModel;

        private Task _timerTickTask;
        private bool _isRunning = false;

        private List<byte[]> _rssiLevelResults = new List<byte[]>(MEASUMEASUREMENTS_CAPACITY);

        public bool IsServiceSupported
        {
            get
            {
                bool ret = false;
                var device = _controllerSession._device;
                if (device != null)
                {
                    ret = device.SupportedSerialApiCommands.Contains((byte)CommandTypes.CmdGetBackgroundRSSI);
                }
                return ret;
            }
        }

        public JammingDetectionService(BasicControllerSession controllerSession)
        {
            _controllerSession = controllerSession;
        }

        public void GetBackgroundRSSI()
        {
            GetBackgroundRSSIInternal();
        }

        public void Start()
        {
            if (_timerTickTask == null && IsServiceSupported && NetworkStatisticsModel.IsJammingDetectionOn)
            {
                _isRunning = true;
                _rssiLevelResults.Clear();
                _timerTickTask = Task.Run(() => PollingTimerTick());
            }
        }

        public void Stop()
        {
            if (_timerTickTask != null)
            {
                _isRunning = false;
                _timerTickTask.Wait();
                _timerTickTask = null;
            }
        }

        private void PollingTimerTick()
        {
            while (_isRunning)
            {
                GetBackgroundRSSIInternal();
                MeasureLevels();
                Thread.Sleep(MEASURE_PERIOD_MS);
            }
        }

        private void GetBackgroundRSSIInternal()
        {
            var getBgRssi = _controllerSession._device.GetBackgroundRSSI();
            if (getBgRssi)
            {
                var currentLevels = getBgRssi.BackgroundRSSILevels;
                //RES | RSSI Ch0 | RSSI Ch1 | RSSI Ch2 | | RSSI Ch3
                NetworkStatisticsModel.RSSI_Ch0 = (sbyte)(currentLevels.Length > 0 ? currentLevels[0] : INVALID);
                NetworkStatisticsModel.RSSI_Ch1 = (sbyte)(currentLevels.Length > 1 ? currentLevels[1] : INVALID);
                NetworkStatisticsModel.RSSI_Ch2 = (sbyte)(currentLevels.Length > 2 ? currentLevels[2] : INVALID);
                NetworkStatisticsModel.RSSI_Ch3 = (sbyte)(currentLevels.Length > 3 ? currentLevels[3] : INVALID);
                if (_rssiLevelResults.Count >= MEASUMEASUREMENTS_CAPACITY)
                {
                    _rssiLevelResults.RemoveAt(0);
                }
                _rssiLevelResults.Add(currentLevels);
            }
        }

        /// <summary>
        /// Measure average value to skip impulses
        /// </summary>
        private void MeasureLevels()
        {
            if (_rssiLevelResults != null && _rssiLevelResults.Count() > MEASUREMENTS_COUNT)
            {
                var measureItems = _rssiLevelResults.Skip(Math.Max(0, _rssiLevelResults.Count() - MEASUREMENTS_COUNT)).ToArray();
                var levelCount = measureItems.Max(x => x.Length);
                var averageValues = new double[levelCount];
                var warnIds = new List<byte>();
                for (byte i = 0; i < levelCount; i++)
                {
                    averageValues[i] = measureItems.Average(x => (sbyte)x[i]);
                    if (averageValues[i] > WARNING_LEVEL_DB && averageValues[i] != INVALID)
                    {
                        warnIds.Add(i);
                    }
                }
                if (warnIds.Count() > 0) // move warning to callback?
                {
                    var restr = string.Join(", ", averageValues.Select(x => ((sbyte)x).ToString()).ToArray());
                    var warnChs = string.Join(", ", warnIds);
                    string msg = $"Jamming detected on channel(s):{warnChs}, RSSI levels:[{restr}] higher ther -30";
                    _controllerSession.Logger.LogWarning(msg);
                }
            }
        }

    }
}
