/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.IO;
using System.Linq;
using Utils;
using Utils.Events;
using ZWave;
using ZWave.Layers;
using ZWaveController.Interfaces;
using ZWaveController.Constants;
using System.Threading.Tasks;

namespace ZWaveController.Models
{
    public class TraceCaptureModel : ITraceCapture
    {
        private object _traceCaptureLockObject = new object();
        public StorageHeader _storageHeader = new StorageHeader();
        private StorageWriter _storageWriter = new StorageWriter();
        private string _traceCaptureFileName;
        private DateTime _traceCaptureCreatedAt;
        private string _dataSourceId = string.Empty;
        public ITraceCaptureSettingsModel TraceCaptureSettingsModel { get; set; }

        public void Close()
        {
            lock (_traceCaptureLockObject)
            {
                _storageWriter.Close();
            }
        }

        public void Open()
        {
            _storageWriter.Open(_traceCaptureFileName, _storageHeader);
        }

        public void DeleteOld()
        {
            string dataSourceId = _dataSourceId.Replace('.', '_').Replace(':', '_');
            string[] tempFiles = Directory.GetFiles(TraceCaptureSettingsModel.TraceCaptureFolder, string.Concat(Trace.DefaultName, "_", dataSourceId, "*", Trace.Ext)).ToArray();
            if (tempFiles.Count() >= TraceCaptureSettingsModel.TraceCaptureSplitCount)
            {
                var files = tempFiles.Select(x => new FileInfo(x)).OrderByDescending(x => x.CreationTime).Skip(TraceCaptureSettingsModel.TraceCaptureSplitCount).ToArray();
                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (IOException ioex)
                    {
                        ioex.Message?._DLOG();
                    }
                }
            }
        }

        public void Init(string sourceId, bool? readDefault = true)
        {
            _dataSourceId = sourceId;
            if (TraceCaptureSettingsModel.IsTraceCapturing)
            {
                if (string.IsNullOrEmpty(TraceCaptureSettingsModel.TraceCaptureFolder))
                {
                    //Log(string.Format("Trace Dump Folder '{0}' is not set", Settings.Default.TraceCaptureFolder),
                    //LogLevels.Warning, LogIndents.None, LogIndents.None);
                }
                else
                {
                    if (!Directory.Exists(TraceCaptureSettingsModel.TraceCaptureFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(TraceCaptureSettingsModel.TraceCaptureFolder);
                        }
                        catch (Exception) { }
                    }
                    if (!Directory.Exists(TraceCaptureSettingsModel.TraceCaptureFolder))
                    {
                        //Log(string.Format("Trace Dump Folder '{0}' doesn't exists!", Settings.Default.TraceCaptureFolder),
                        //LogLevels.Warning, LogIndents.None, LogIndents.None);
                    }
                    else
                    {
                        if (Tools.FolderHasAccess(TraceCaptureSettingsModel.TraceCaptureFolder))
                        {
                            if (readDefault == true)
                            {
                                TraceCaptureSettingsModel.ReadDefault();
                            }
                            PrepareCaptureTraceFile();
                        }
                        else
                        {
                            // You don't have write permissions
                            //Log(string.Format("You don't have permission to write to the folder - '{0}'", Settings.Default.TraceCaptureFolder),
                            //LogLevels.Warning, LogIndents.None, LogIndents.None);
                        }
                    }
                }
            }
        }

        public void SaveTransmittedData(object sender, EventArgs<DataChunk> e)
        {
            if (TraceCaptureSettingsModel.IsTraceCapturing)
            {
                if (TraceCaptureSettingsModel.IsTraceCaptureAutoSplit && (DurationExceeded() || FileSizeExceeded()))
                {
                    PrepareCaptureTraceFile();
                }
                _storageWriter.Write(e.Value);
            }
        }

        private bool FileSizeExceeded()
        {
            return TraceCaptureSettingsModel.TraceCaptureSplitSizeInBytes > 0
                && (_storageWriter.TotalBytes > TraceCaptureSettingsModel.TraceCaptureSplitSizeInBytes);
        }

        private bool DurationExceeded()
        {
            return TraceCaptureSettingsModel.TraceCaptureSplitDuration > 0
                && ((DateTime.Now - _traceCaptureCreatedAt).TotalMinutes > TraceCaptureSettingsModel.TraceCaptureSplitDuration);
        }

        private void PrepareCaptureTraceFile()
        {
            lock (_traceCaptureLockObject)
            {
                Close();
                string dataSourceId = new string(_dataSourceId.Select(x => char.IsLetterOrDigit(x) ? x : '_').ToArray() ?? new[] { '_' });
                _traceCaptureCreatedAt = DateTime.Now;
                _traceCaptureFileName = Path.Combine(TraceCaptureSettingsModel.TraceCaptureFolder, string.Concat(Trace.DefaultName, "_", dataSourceId, Trace.Ext));
                if (!TraceCaptureSettingsModel.IsTraceCaptureOverwriteFile)
                {
                    int counter = 1;
                    var _traceCaptureFileNameBackup = _traceCaptureFileName;
                    while (File.Exists(_traceCaptureFileNameBackup))
                    {
                        _traceCaptureFileNameBackup = Path.Combine(TraceCaptureSettingsModel.TraceCaptureFolder, string.Concat(Trace.DefaultName, "_", dataSourceId, "_", counter++, Trace.Ext));
                    }
                    if (_traceCaptureFileName != _traceCaptureFileNameBackup)
                    {
                        bool isMoved = false;
                        try
                        {
                            File.Move(_traceCaptureFileName, _traceCaptureFileNameBackup);
                            isMoved = true;
                        }
                        catch (IOException ex)
                        {
                            ex.Message._DLOG();
                        }
                        if (!isMoved)
                        {
                            _traceCaptureFileName = _traceCaptureFileNameBackup;
                        }

                        if (TraceCaptureSettingsModel.TraceCaptureSplitCount > 0)
                        {
                            Task.Run(() => DeleteOld());
                        }
                    }
                }
                Open();
            }
        }
    }
}
