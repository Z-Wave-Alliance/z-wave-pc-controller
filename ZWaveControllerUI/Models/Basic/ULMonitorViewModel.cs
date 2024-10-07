/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using System.Linq;
using ZWave;
using ZWave.CommandClasses;
using ZWave.BasicApplication.Devices;
using Utils.UI;
using ZWaveController.Commands;
using ZWaveController.Interfaces;
using ZWaveController;
using ZWave.Devices;
using ZWave.Enums;

namespace ZWaveControllerUI.Models
{
    public class ULMonitorViewModel : VMBase, IULMonitorModel
    {
        const byte SYSTEM_NOTIFICATION_EVENT_ID = 0x09;
        public CommandBase ClearCommand { get; set; }
        public CommandBase StartULMonitorCommand { get; set; }
        public CommandBase StopULMonitorCommand { get; set; }

        private ActionToken responseMonitorActionToken = null;
        private List<ULMonitorItem> mULMonitorItems;
        public List<ULMonitorItem> ULMonitorItems
        {
            get { return mULMonitorItems; }
            set
            {
                mULMonitorItems = value;
                Notify("ULMonitorItems");
            }
        }


        public ULMonitorViewModel(IApplicationModel applicationModel) : base(applicationModel)
        {
            Title = "UL Tool Monitor";

            ClearCommand = new CommandBase(ResetCounters);
            //StartULMonitorCommand = new CommandBase(StartULMonitor, obj => responseMonitorActionToken == null);
            //StopULMonitorCommand = new CommandBase(StopULMonitor, obj => responseMonitorActionToken != null);
            ULMonitorInitValues();
        }


        public void ULMonitorInitValues()
        {
            ULMonitorItems = new List<ULMonitorItem>();
            ULMonitorItems.Add(new ULMonitorItem(0x07, "Home Security", 0x01, "INTRUSION"));
            ULMonitorItems.Add(new ULMonitorItem(0x07, "Home Security", 0x03, "TAMPERING_COVERING_REMOVED"));
            ULMonitorItems.Add(new ULMonitorItem(0x08, "Power management", 0x02, "AC_MAINS_DISCONNECED"));
            ULMonitorItems.Add(new ULMonitorItem(0x08, "Power management", 0x0A, "REPLACE_BATTERY_SOON"));
            ULMonitorItems.Add(new ULMonitorItem(0x0A, "Emergency", 0x01, "ALARM_CONTACT_POLICE"));
            ULMonitorItems.Add(new ULMonitorItem(0x01, "Smoke Alarm", 0x02, "SMODE_DETECTED_UNKNOWN_LOCATION"));

            ULMonitorItem systemItem = new ULMonitorItem(SYSTEM_NOTIFICATION_EVENT_ID, "System", 0x03, "HARDWARE_FAILURE_WITH_MANUFACTURER_PROPRIETARY_FAILURE_CODE");
            systemItem.IsSystem = true;
            systemItem.IsJammingChannel0 = true;
            systemItem.IsJammingChannel1 = true;
            systemItem.IsJammingChannel2 = true;
            ULMonitorItems.Add(systemItem);
        }

        private void ResetCounters(object o)
        {
            prevSeqNo = -1;
            ULMonitorItems.ForEach(x => x.Counter = 0);
            var systemItem = ULMonitorItems.First(x => x.NotificationTypeId == SYSTEM_NOTIFICATION_EVENT_ID);
            systemItem.IsJammingChannel0 = true;
            systemItem.IsJammingChannel1 = true;
            systemItem.IsJammingChannel2 = true;
        }

        public static bool IsULMonitorEnabled = true;
        public void StartULMonitor(object o)
        {
            if (IsULMonitorEnabled)
            {
                responseMonitorActionToken = (ApplicationModel.Controller as Device)
                .ResponseData(OnNotificationReport, ControllerSessionsContainer.Config.TxOptions, new COMMAND_CLASS_NOTIFICATION_V5.NOTIFICATION_REPORT());
            }
        }

        public void StopULMonitor(object o)
        {
            if (IsULMonitorEnabled)
            {
                if (responseMonitorActionToken != null)
                {
                    (ApplicationModel.Controller as Device).Cancel(responseMonitorActionToken);
                    responseMonitorActionToken.WaitCompletedSignal();
                    responseMonitorActionToken = null;
                }
            }
        }

        private int prevSeqNo = -1;
        private byte[] OnNotificationReport(ReceiveStatuses options, NodeTag destNodeId, NodeTag srcNodeId, byte[] data)
        {
            COMMAND_CLASS_NOTIFICATION_V5.NOTIFICATION_REPORT report = data;
            if (report.sequenceNumber != prevSeqNo)
            {
                var ulMonitorItem = ULMonitorItems.FirstOrDefault(x => x.NotificationTypeId == report.notificationType && x.EvntId == report.mevent);
                if (ulMonitorItem != null)
                {
                    if (ulMonitorItem.NotificationTypeId == SYSTEM_NOTIFICATION_EVENT_ID &&
                        ulMonitorItem.EvntId == 0x03 &&
                        report.eventParameter != null &&
                        report.eventParameter.Count > 0)
                    {
                        byte jammingValue = report.eventParameter.First();

                        ulMonitorItem.IsJammingChannel0 = (jammingValue & 0x01) == 0;
                        ulMonitorItem.IsJammingChannel1 = (jammingValue & 0x02) == 0;
                        ulMonitorItem.IsJammingChannel2 = (jammingValue & 0x04) == 0;
                    }
                    else
                    {
                        ulMonitorItem.Counter++;
                    }
                }
                prevSeqNo = report.sequenceNumber;
            }
            return null;
        }
    }

    public class ULMonitorItem : EntityBase
    {
        public ULMonitorItem(byte notificetionTypeId, string notificationTypeName, byte eventId, string eventName)
        {
            NotificationTypeId = notificetionTypeId;
            NotificationType = notificationTypeName;
            EvntId = eventId;
            EventName = eventName;
            Counter = 0;
            IsSystem = false;
            IsJammingChannel0 = false;
            IsJammingChannel1 = false;
            IsJammingChannel2 = false;
        }

        private byte mNotificationTypeId;
        public byte NotificationTypeId
        {
            get { return mNotificationTypeId; }
            set
            {
                mNotificationTypeId = value;
                Notify("NotificationTypeId");
            }
        }

        private string mNotificationType;
        public string NotificationType
        {
            get { return mNotificationType; }
            set
            {
                mNotificationType = value;
                Notify("NotificationType");
            }
        }

        private byte mEvntId;
        public byte EvntId
        {
            get { return mEvntId; }
            set
            {
                mEvntId = value;
                Notify("EvntId");
            }
        }

        private string mEventName;
        public string EventName
        {
            get { return mEventName; }
            set
            {
                mEventName = value;
                Notify("EventName");
            }
        }

        private int mCounter;
        public int Counter
        {
            get { return mCounter; }
            set
            {
                mCounter = value;
                Notify("Counter");
            }
        }

        private bool mIsSystem;
        public bool IsSystem
        {
            get { return mIsSystem; }
            set
            {
                mIsSystem = value;
                Notify("IsSystem");
            }
        }

        public bool IsCounter
        {
            get { return !IsSystem; }
        }

        private bool mIsJammingChannel0;
        public bool IsJammingChannel0
        {
            get { return mIsJammingChannel0; }
            set
            {
                mIsJammingChannel0 = value;
                Notify("IsJammingChannel0");
            }
        }

        private bool mIsJammingChannel1;
        public bool IsJammingChannel1
        {
            get { return mIsJammingChannel1; }
            set
            {
                mIsJammingChannel1 = value;
                Notify("IsJammingChannel1");
            }
        }

        private bool mIsJammingChannel2;
        public bool IsJammingChannel2
        {
            get { return mIsJammingChannel2; }
            set
            {
                mIsJammingChannel2 = value;
                Notify("IsJammingChannel2");
            }
        }
    }
}
