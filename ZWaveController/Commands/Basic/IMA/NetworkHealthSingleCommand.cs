/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Linq;
using ZWave.BasicApplication.Devices;
using ZWave.BasicApplication.Operations;
using ZWave.Enums;
using System.Diagnostics;
using System.Threading;
using ZWaveController.Models;
using ZWave.Devices;
using ZWaveController.Enums;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands
{
    public class NetworkHealthSingleCommand : IMACommandBase
    {
        public IIMADeviceInfo ImaItem { get; set; }
        public bool IsFullNetwork { get; set; }

        const int TestIterations = 10;
        const int TestSetsCount = 6;
        /* RSSI defines used in ZW_SendData callback and ZW_GetCurrentNoiseLevel */
        const SByte RSSI_NOT_AVAILABLE = 127;       /* RSSI measurement not available */
        const SByte RSSI_MAX_POWER_SATURATED = 126; /* Receiver saturated. RSSI too high to measure precisely. */
        const SByte RSSI_BELOW_SENSITIVITY = 125;   /* No signal detected. The RSSI is too low to measure precisely. */
        const SByte RSSI_RESERVED_START = 11;    /* All values above and including RSSI_RESERVED_START are reserved,
                                                except those defined above.*/
        const TransmitOptions transmitOptions = TransmitOptions.TransmitOptionAcknowledge | TransmitOptions.TransmitOptionAutoRoute | TransmitOptions.TransmitOptionExplore;
        long Tapp = 0;

        SByte controllerBackgrRSSICh1 = 0;
        SByte controllerBackgrRSSICh2 = 0;
        SByte controllerBackgrRSSICh3 = 0;

        public class LWRdBResult
        {
            public int TestRoundNumber { get; set; }
            public NodeTag Source { get; set; }
            public NodeTag Destination { get; set; }
            public bool? IsSucess { get; set; }
        }

        public NetworkHealthSingleCommand(IControllerSession controllerSession, IIMADeviceInfo imaItem)
            : base(controllerSession)
        {
            IsTargetSensitive = true;
            Text = "Z-Wave Network Health.";
            ImaItem = imaItem;
        }

        protected override bool CanCancelAction(object param)
        {
            return true;
        }

        protected override void ExecuteInner(object param)
        {
            ControllerSession.ApplicationModel.SetBusyMessage("Network Test is in progress");
            var ret = CommandExecutionResult.Failed;
            if (ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
            {
                ControllerSession.Logger.LogFail("IMA Network Test Cancelled for node Id: " + ImaItem.Id);
                ret = CommandExecutionResult.Canceled;
            }
            else
            {
                var basicCM = ControllerSession as BasicControllerSession;
                if (basicCM != null)
                {
                    basicCM.SetWakeupDelayed();
                }
                ImaItem.IsInProgress = true;
                byte networkHealthResult = StartNetworkHealth(ImaItem);
                ControllerSession.IMAFullNetwork.RunningTests--;
                if (IsFullNetwork)
                {
                    ImaViewModel.ControllerNetworkHealth = Math.Min(ImaViewModel.ControllerNetworkHealth, networkHealthResult);
                    if (ControllerSession.IMAFullNetwork.RunningTests == 0)
                    {
                        ((IIMADeviceInfo)ImaViewModel.Items.FirstOrDefault(x => x.Id == ControllerSession.Controller.Network.NodeTag)).NHV = ImaViewModel.ControllerNetworkHealth;
                    }
                }
                ImaItem.IsInProgress = false;
                if (basicCM != null)
                {
                    basicCM.UnSetWakeupDelayed();
                }
                if (IsCancelling || ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
                {
                    ControllerSession.IMAFullNetwork.IsSingleCommandCancelled = true;
                    ControllerSession.Logger.LogWarning("IMA Network Test Cancelled for node Id: " + ImaItem.Id);
                    ret = CommandExecutionResult.Canceled;
                }
                else
                {
                    ret = CommandExecutionResult.OK;
                }
            }
            ControllerSession.ApplicationModel.LastCommandExecutionResult = ret;
        }

        private byte StartNetworkHealth(IIMADeviceInfo imaItem)
        {
            var currentNode = imaItem.Id;
            ControllerSession.ApplicationModel.Invoke(() => imaItem.ClearIMATestResult());
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            var getSucNodeRes = (SessionDevice as Controller).GetSucNodeId();
            if (getSucNodeRes)
            {
                sw.Stop();
                Tapp = sw.ElapsedMilliseconds;
            }

            Log("Network Health Started for Node " + currentNode);
            //temp counters
            bool isPowerLevelPossible = false;

            long sumLatency = 0;
            List<LWRdBResult> lwrdBResults = new List<LWRdBResult>();
            byte[] prevRoute = new byte[] { 0x00 };

            for (int testRound = 0; testRound < TestSetsCount; testRound++)
            {
                if (IsCancelling || ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
                {
                    break;
                }
                IIMATestResult imaRes = imaItem.CreateTestResult(testRound + 1);
                string NodeNH = "RED";
                bool firstTry = true;
                prevRoute = new byte[] { 0x00 };
                long prevLatency = 0;
                SendDataResult lastNOP = null;


                var routingInfoRes = (SessionDevice as Controller).GetRoutingInfo(currentNode, 0, 1);
                if (routingInfoRes)
                {
                    imaRes.NB = (byte)routingInfoRes.RoutingNodes.Length;
                }

                #region NOP "TestIterations" times
                for (int i = 0; i < TestIterations; i++)
                {
                    if (IsCancelling || ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
                    {
                        break;
                    }
                    sw.Reset();
                    sw.Start();
                    //NOP 
                    var sendNOPRes = SessionDevice.SendData(currentNode, new byte[10], transmitOptions, null, out _token);
                    sw.Stop();
                    if (sendNOPRes && sendNOPRes.TransmitStatus == TransmitStatuses.CompleteOk)
                    {
                        lastNOP = sendNOPRes;
                        long latencyMs = sendNOPRes.TransmitTicks == 0 ? sw.ElapsedMilliseconds : (sendNOPRes.TransmitTicks * 10);
                        Log(string.Format("NOP Power with Ack complete to NodeId: {0} Time Taken:{1}", currentNode, latencyMs));
                        sumLatency += latencyMs;
                        byte[] currentRoute = new byte[0];

                        var lwrRes = (SessionDevice as Controller).GetPriorityRoute(currentNode);
                        if (lwrRes)
                        {
                            Log("LWR complete to NodeId: " + currentNode);
                        }

                        if (lastNOP.HasTxTransmitReport)
                        {
                            currentRoute = lastNOP.Repeaters;
                        }
                        else
                        {
                            currentRoute = lwrRes.PriorityRoute.Select(x => (byte)x.Id).ToArray();
                        }

                        if (firstTry)
                        {
                            imaRes.RC = 0;
                            firstTry = false;
                        }
                        else
                        {
                            if (currentRoute != null && !prevRoute.SequenceEqual(currentRoute)) //RC ++
                            {
                                imaRes.RC++;
                            }
                            else if (latencyMs - prevLatency > Tapp + 150)
                            {
                                imaRes.RC++;
                            }
                        }
                        prevLatency = latencyMs;
                        prevRoute = currentRoute ?? prevRoute;
                    }
                    else
                    {
                        imaRes.PER++;
                        Log(string.Format("NOP Power Failed to NodeId: {0} Time Taken:{1}", currentNode, sw.ElapsedMilliseconds));
                    }
                }
                #endregion

                if (lastNOP != null)
                {
                    #region Network Health Status and Value calculation
                    if (imaRes.PER == 10) //RED
                    {
                        NodeNH = "RED";
                        imaRes.NHV = 0;
                    }
                    else if (imaRes.PER > 0)
                    {
                        if (2 < imaRes.PER && imaRes.PER < 10)
                        {
                            NodeNH = "RED";
                            imaRes.NHV = 1;
                        }
                        else if (imaRes.PER == 2)
                        {
                            NodeNH = "RED";
                            imaRes.NHV = 2;
                        }
                        else if (imaRes.PER == 1)
                        {
                            NodeNH = "RED";
                            imaRes.NHV = 3;
                        }
                    }
                    else if (imaRes.PER == 0) //YELLOW OR GREEN
                    {
                        if (imaRes.RC > 1) // Yellow
                        {
                            if (imaRes.RC <= 4)
                            {
                                NodeNH = "YELLOW";
                                imaRes.NHV = 5;
                            }
                            else if (imaRes.RC > 4)
                            {
                                NodeNH = "YELLOW";
                                imaRes.NHV = 4;
                            }
                        }
                        else
                        {
                            if (imaRes.RC > 1) // Yellow
                            {
                                if (imaRes.RC <= 4)
                                {
                                    NodeNH = "YELLOW";
                                    imaRes.NHV = 5;
                                }
                                else if (imaRes.RC > 4)
                                {
                                    NodeNH = "YELLOW";
                                    imaRes.NHV = 4;
                                }
                            }
                            else	// Green
                            {
                                if ((imaRes.RC == 0) &&
                                    (imaRes.NB > 2))
                                {
                                    NodeNH = "Green";
                                    imaRes.NHV = 10;
                                }
                                else if ((imaRes.RC == 1) &&
                                         (imaRes.NB > 2))
                                {
                                    NodeNH = "Green";
                                    imaRes.NHV = 9;
                                }
                                else
                                {
                                    NodeNH = "Green";
                                    imaRes.NHV = 8;
                                }
                            }
                        }
                    }
                    #endregion

                    if (imaRes.NHV > 7)
                    {
                        #region Powerlevel test for all hops in route
                        bool isLwrdBPass = false;
                        var pLRepeaters = prevRoute.Length > 0 ? prevRoute : lastNOP.Repeaters;

                        isPowerLevelPossible = IsPowerLevelSupportedForNode(currentNode);
                        if (isPowerLevelPossible) //check for all repeaters if PowerLevel Supported
                        {
                            foreach (var rep in pLRepeaters)
                            {
                                if (rep == 0)
                                {
                                    continue;
                                }
                                isPowerLevelPossible = IsPowerLevelSupportedForNode(new NodeTag(rep));
                                if (!isPowerLevelPossible)
                                {
                                    break;
                                }
                            }
                        }

                        if (isPowerLevelPossible)
                        {
                            var plNodesList = new List<byte>();
                            plNodesList.Add((byte)ControllerSession.Controller.Id);
                            plNodesList.AddRange(pLRepeaters);
                            plNodesList.Add((byte)currentNode.Id);
                            plNodesList.RemoveAll(x => x == 0);
                            isLwrdBPass = PerformPowerlevelTest(plNodesList.Select(x=> new NodeTag(x)).ToArray(), lwrdBResults, testRound);
                            imaRes.LWRdB = isLwrdBPass;
                        }
                        #endregion

                        bool isLwrRSSIPass = false;
                        if (lastNOP.HasTxTransmitReport)
                        {

                            #region RSSI Calculations
                            CalculateBackgroundRSSI();
                            SByte lwrRSSI = GetLwrRSSI(currentNode);
                            imaRes.LWRRSSI = lwrRSSI;
                            isLwrRSSIPass = lwrRSSI >= 17;
                            #endregion

                        }

                        if (isPowerLevelPossible || lastNOP.HasTxTransmitReport)
                        {
                            if (!isPowerLevelPossible)
                            {
                                isLwrdBPass = true; //hack to prevent more IFs'
                            }
                            if (!lastNOP.HasTxTransmitReport)
                            {
                                isLwrRSSIPass = true; //hack to prevent more IFs'
                            }
                            if (imaRes.RC <= 1 && imaRes.NB <= 2
                                && (!isLwrdBPass
                               || !isLwrRSSIPass))
                            {
                                NodeNH = "YELLOW";
                                imaRes.NHV = 6;
                            }
                            else if (imaRes.RC <= 1 && imaRes.NB > 2
                                       && (!isLwrdBPass
                                       || !isLwrRSSIPass))
                            {
                                NodeNH = "YELLOW";
                                imaRes.NHV = 7;
                            }
                            else if (imaRes.RC == 0 && imaRes.NB > 2
                                && isLwrdBPass
                               && isLwrRSSIPass) // Green
                            {
                                NodeNH = "Green";
                                imaRes.NHV = 10;
                            }
                            else if ((imaRes.RC == 1) && (imaRes.NB > 2)
                              && isLwrdBPass
                             && isLwrRSSIPass)
                            {
                                NodeNH = "Green";
                                imaRes.NHV = 9;
                            }
                            else
                            {
                                NodeNH = "Green";
                                imaRes.NHV = 8;
                            }
                        }
                    }
                }
                ControllerSession.ApplicationModel.Invoke(() => imaItem.AddIMATestResult(imaRes));

                Log("Round " + testRound + " Completed for Node " + currentNode + " With status: " + NodeNH + " and NHV: " + imaRes.NHV);
            }

            string overallNH = "FAIL";
            if (imaItem.NHV > 0)
            {
                overallNH = "RED";
                if (imaItem.NHV > 3)
                {
                    overallNH = "YELLOW";

                    if (imaItem.NHV > 7)
                    {
                        overallNH = "GREEN";
                    }
                }
            }
            Log("Network Health Completed for Node " + currentNode + " With status: " + overallNH + " Network Health Value: " + imaItem.NHV);
            Log("RC  = " + imaItem.RC);
            Log("imaItem.PER  = " + imaItem.PER);
            Log("imaItem.NB  = " + imaItem.NB);
            Log("lwrDb " + ((imaItem.LWRdB == null || imaItem.LWRdB == false) ? "<" : "≥") + " 6dB");
            Log("lwrRssi = " + imaItem.LWRRSSI == null ? "didn't measured" : (imaItem.LWRRSSI + "dB"));

            var nodesList = new List<byte>();
            nodesList.Add((byte)ControllerSession.Controller.Id);
            nodesList.AddRange(prevRoute);
            nodesList.Add((byte)currentNode.Id);
            nodesList.RemoveAll(x => x == 0);
            DrawLines(nodesList.Select(x=>new NodeTag(x)).ToArray());
            //end of alg
            return (byte)imaItem.NHV;
        }

        private bool PerformPowerlevelTest(NodeTag[] nodes, List<LWRdBResult> lwrdBResults, int testRound)
        {
            bool ret = false;
            byte minPowLevelMargin = (byte)0x06;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (IsCancelling || ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
                {
                    break;
                }

                NodeTag source = NodeTag.Empty;
                NodeTag destination = NodeTag.Empty;
                bool? isSuccess = null;
                if (i == 0)
                {
                    source = nodes[i + 1]; //No exception since we added at least 2 items
                    destination = nodes[i];
                    isSuccess = ResultToBool(ControllerSession.StartPowerLevelTest(source, destination, minPowLevelMargin, _token));

                    lwrdBResults.Add(new LWRdBResult
                    {
                        TestRoundNumber = testRound,
                        Source = source,
                        Destination = destination,
                        IsSucess = isSuccess
                    });
                    Log(string.Format("LWR 6 dB {0} - {1} Status: {2}", source, destination, isSuccess));
                }
                else if (nodes.Length > i + 1) //check if last and we already checked it
                {
                    source = nodes[i + 1];
                    destination = nodes[i];
                    isSuccess = ResultToBool(ControllerSession.StartPowerLevelTest(source, destination, minPowLevelMargin, _token));
                    lwrdBResults.Add(new LWRdBResult
                    {
                        TestRoundNumber = testRound,
                        Source = source,
                        Destination = destination,
                        IsSucess = isSuccess
                    });
                    Log(string.Format("LWR 6 dB {0} - {1} Status: {2}", source, destination, isSuccess));
                }
            }
            if (IsCancelling || ControllerSession.IMAFullNetwork.IsSingleCommandCancelled)
            {
                ret = false;
            }
            else
            {
                ret = !(lwrdBResults.Any(x => x.TestRoundNumber == testRound && (x.IsSucess != true))); //NOT
            }

            return ret;

        }

        private static bool? ResultToBool(CommandExecutionResult res)
        {
            bool? ret = null;
            if (res == CommandExecutionResult.OK)
                ret = true;
            else if (res == CommandExecutionResult.Failed)
                ret = false;
            return ret;
        }

        private bool IsPowerLevelSupportedForNode(NodeTag node)
        {
            bool ret = false;
            ret = ControllerSession.Controller.Network.HasCommandClass(node, 0x73);
            if (!ret)
            {
                Log("PowerLevel command not supported for Node: " + node.Id);
            }
            return ret;
        }

        private void CalculateBackgroundRSSI()
        {
            Log("Calculating background RSSI");
            List<sbyte> backgrRssiCh1List = new List<sbyte>();
            List<sbyte> backgrRssiCh2List = new List<sbyte>();
            List<sbyte> backgrRssiCh3List = new List<sbyte>();
            for (int i = 0; i < 10; i++)
            {
                var getBackgroundRssiRes = SessionDevice.GetBackgroundRSSI();
                if (getBackgroundRssiRes)
                {
                    byte[] backgrRSSI = SessionDevice.BackgroundRSSILevels;
                    if (backgrRSSI.Length > 2)
                    {
                        backgrRssiCh1List.Add(ConvertRSSITodBValue((sbyte)backgrRSSI[0]));
                        backgrRssiCh2List.Add(ConvertRSSITodBValue((sbyte)backgrRSSI[1]));
                        backgrRssiCh3List.Add(ConvertRSSITodBValue((sbyte)backgrRSSI[2]));
                    }
                    else if (backgrRSSI.Length > 0)
                    {
                        backgrRssiCh1List.Add(ConvertRSSITodBValue((sbyte)backgrRSSI[0]));
                        backgrRssiCh2List.Add(ConvertRSSITodBValue((sbyte)backgrRSSI[1]));
                    }

                }
                Thread.Sleep(50);
            }
            if (backgrRssiCh1List.Count > 0)
            {
                controllerBackgrRSSICh1 = (sbyte)backgrRssiCh1List.OrderBy(x => x).Take(5).Average(x => x);
            }
            if (backgrRssiCh2List.Count > 0)
            {
                controllerBackgrRSSICh2 = (sbyte)backgrRssiCh2List.OrderBy(x => x).Take(5).Average(x => x);
            }
            if (backgrRssiCh3List.Count > 0)
            {
                controllerBackgrRSSICh3 = (sbyte)backgrRssiCh3List.OrderBy(x => x).Take(5).Average(x => x);
            }
            Log(string.Format("Background RSSI: CH0: {0} dB, CH1: {1} dB, CH2: {2} dB.", controllerBackgrRSSICh1, controllerBackgrRSSICh2, controllerBackgrRSSICh3));
        }

        private sbyte GetLwrRSSI(NodeTag node)
        {
            sbyte result = 0;
            //NOP for RSSI
            var sendNOPRes = SessionDevice.SendData(node, new byte[] { 0x00 }, TransmitOptions.TransmitOptionAcknowledge, null, out _token);
            if (sendNOPRes)
            {
                var lwrRSSIList = sendNOPRes.RssiValuesIncoming.Take(sendNOPRes.RepeatersCount + 1).ToArray();
                List<int> RSSIHops = new List<int>();
                foreach (var item in lwrRSSIList)
                {
                    int resRSSI = 0;
                    if (item == RSSI_NOT_AVAILABLE)
                    {
                        resRSSI = 255;
                    }
                    else
                    {
                        switch (sendNOPRes.AckChannelNo)
                        {
                            case 0:
                                resRSSI = (sbyte)(ConvertRSSITodBValue(item) - controllerBackgrRSSICh1);
                                break;
                            case 1:
                                resRSSI = (sbyte)(ConvertRSSITodBValue(item) - controllerBackgrRSSICh2);
                                break;
                            case 2:
                                resRSSI = (sbyte)(ConvertRSSITodBValue(item) - controllerBackgrRSSICh3);
                                break;
                            default:
                                break;
                        }
                    }
                    RSSIHops.Add(resRSSI);
                }
                result = (sbyte)RSSIHops.Min();
            }
            Log(string.Format("Background RSSI: CH0: {0} dB, CH1: {1} dB, CH2: {2} dB.", controllerBackgrRSSICh1, controllerBackgrRSSICh2, controllerBackgrRSSICh3));
            Log(string.Format("LWR RSSI for Node {0} (CH{1}): {2} dB.", node, sendNOPRes.AckChannelNo, result));
            return result;
        }

        private sbyte ConvertRSSITodBValue(sbyte val)
        {
            if (val == RSSI_MAX_POWER_SATURATED)
            {
                return 1; /* Treat saturated as 1 dBm */
            }
            else if (val == RSSI_BELOW_SENSITIVITY)
            {
                return -100;
            }
            else if (val == RSSI_NOT_AVAILABLE)
            {
                return val; /* making it explicit that we don't change NOT_AVAILABLE */
            }
            else
            {
                return val;
            }
        }

        private readonly object mLockObject = new object();
        private void DrawLines(NodeTag[] nodes)
        {
            if (nodes != null && nodes.Length > 0)
            {
                ControllerSession.ApplicationModel.Invoke(() =>
                {
                    lock (mLockObject)
                    {
                        IIMALine prevLine = null;
                        for (int i = 0; i < nodes.Length; i++)
                        {
                            IIMADeviceInfo di = ImaViewModel.Items.FirstOrDefault((x) => x.Id == nodes[i]) as IIMADeviceInfo;
                            IIMADeviceInfo diNext = null;
                            if (i < nodes.Length - 1)
                            {
                                diNext = ImaViewModel.Items.FirstOrDefault((x) => x.Id == nodes[i + 1]) as IIMADeviceInfo;
                                if (di != null && diNext != null)
                                {
                                    IIMALine line = ImaViewModel.CreateIMALine(ImaViewModel.Layout, di.Id, diNext.Id, nodes.Last());
                                    // new IMALine(ImaViewModel.Layout, di.Id, diNext.Id, nodeIds.Last());
                                    if (prevLine != null)
                                    {
                                        line.PreviousLine = prevLine;
                                    }
                                    prevLine = line;
                                    di.StartLines.Add(line);
                                    diNext.EndLines.Add(line);
                                    ImaViewModel.Items.Add(line);
                                }
                            }
                        }
                    }
                });
            }
        }
    }
}
