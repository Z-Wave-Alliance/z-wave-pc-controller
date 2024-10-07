/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.UI.Interfaces;
using ZWaveController.Configuration;
using ZWaveController.Interfaces;

namespace ZWaveController.Commands.Basic.Misc
{
    class ProvisioningListImportCommand : CommandBasicBase
    {
        public ISmartStartModel SmartStartModel => ControllerSession.ApplicationModel.SmartStartModel;
        public ProvisioningListImportCommand(IControllerSession controllerSession) :
          base(controllerSession)
        {
            Text = "Provisioning List Import Command";
            UseBackgroundThread = true;
        }

        protected override void ExecuteInner(object param)
        {
            var openFileDlgVM = ApplicationModel.OpenFileDialogModel;
            openFileDlgVM.Filter = "XML or Text file (*.xml,*.txt)|*.xml; *.txt";
            ((IDialog)openFileDlgVM).ShowDialog();
            if (openFileDlgVM.IsOk && !string.IsNullOrEmpty(openFileDlgVM.FileName) && File.Exists(openFileDlgVM.FileName))
            {
                ControllerSession.StopSmartListener();
                var configurationItemPreKitting = ApplicationModel.ConfigurationItem.PreKitting;
                if (Path.GetExtension(openFileDlgVM.FileName).ToLowerInvariant() == ".xml")
                {
                    string xmlText = File.ReadAllText(openFileDlgVM.FileName);
                    if (!string.IsNullOrEmpty(xmlText))
                    {
                        try
                        {
                            configurationItemPreKitting.ProvisioningList =
                                XmlUtility.XmlStr2Obj<ObservableCollection<ProvisioningItem>>(xmlText);
                        }
                        catch (InvalidOperationException ex)
                        {
                            ex.Message._DLOG();
                            ControllerSession.LogError("DSK import failed", "XML file structure is invalid.");
                        }
                    }
                }
                else
                {
                    StringBuilder duplicateEntriesMessage = new StringBuilder();
                    foreach (var line in File.ReadAllLines(openFileDlgVM.FileName))
                    {
                        var dsk = SmartStartModel.ParseDskFromLine(line);
                        if (dsk != null && dsk.Length == 16)
                        {
                            if (SmartStartModel.ValidateNewEntry(dsk, configurationItemPreKitting).Item1)
                            {
                                var itemToAdd = new ProvisioningItem
                                {
                                    Dsk = dsk,
                                    GrantSchemes = 0x87
                                };
                                ApplicationModel.Invoke(() =>
                                {
                                    if (configurationItemPreKitting.ProvisioningList != null)
                                    {
                                        configurationItemPreKitting.ProvisioningList.Add(itemToAdd);
                                    }
                                    else
                                    {
                                        configurationItemPreKitting.ProvisioningList =
                                            new ObservableCollection<ProvisioningItem>
                                            {
                                                itemToAdd
                                            };
                                    }
                                });
                            }
                            else
                            {
                                duplicateEntriesMessage.AppendLine($"Duplicate entry not added: {Tools.GetDskStringFromBytes(dsk)}");
                            }
                        }
                    }
                    if (duplicateEntriesMessage.Length > 0)
                    {
                        ControllerSession.LogError("File contains duplicate records",
                            duplicateEntriesMessage.ToString());
                    }
                    else
                    {
                        ControllerSession.ApplicationModel.LastCommandExecutionResult = Enums.CommandExecutionResult.OK;

                    }
                }
                ControllerSession.StartSmartListener();
            }

        }
    }
}
