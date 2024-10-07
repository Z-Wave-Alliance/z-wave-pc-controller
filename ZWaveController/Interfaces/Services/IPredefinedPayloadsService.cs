/// SPDX-License-Identifier: BSD-3-Clause
/// SPDX-FileCopyrightText: Silicon Laboratories Inc. https://www.silabs.com
﻿using System.Collections.Generic;
using ZWaveController.Enums;
using ZWaveController.Models;

namespace ZWaveController.Interfaces
{
    /// <summary>
    /// IPredefinedPayloadsService - manages how to fill Predefined Commands Model
    /// </summary>
    public interface IPredefinedPayloadsService
    {
        //Actions with Groups:
        /// <summary>
        /// Creates default group
        /// Used on the application start.
        /// </summary>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult Initialize();
        /// <summary>
        /// Load Group from JSON text.
        /// </summary>
        /// <param name="groupName">New Group Name.</param>
        /// <param name="json">Source JSON text.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult LoadGroup(string groupName, string json);
        /// <summary>
        /// Save Group selected by name to destination path.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="filePath">Destiantion to save.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult SaveGroup(string groupName, string filePath);
        /// <summary>
        /// Add new group.
        /// </summary>
        /// <param name="groupName">New group name.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult AddGroup(string groupName);
        /// <summary>
        /// Delete selected group by name.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult DeleteGroup(string groupName);
        /// <summary>
        /// Delete all groups.
        /// </summary>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult DeleteGroups(); //SaveGroups ?
        /// <summary>
        /// Copy Group selected by name to <paramref name="newGroupName"./>
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="newGroupName">New Group Name.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult CloneGroup(string groupName, string newGroupName = null);
        /// <summary>
        /// Update Selected group name.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="newGroupName">New Group Name.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult RenameGroup(string groupName, string newGroupName);

        //Actions with Items:

        /// <summary>
        /// Add new item to the group selected by name.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="newItem">New <see cref="PayloadItem"/> to add .</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult AddItemToGroup(string groupName, PayloadItem newItem);
        /// <summary>
        /// Delete selected item by <paramref name="id"/> from the selected group <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="id">Selected item Id.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult DeleteItem(string groupName, uint id);
        /// <summary>
        /// Move Up selected item by <paramref name="id"/> inside selected group <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="id">Selected item Id.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult MoveUp(string groupName, uint id);
        /// <summary>
        /// Move Down selected item by <paramref name="id"/> inside selected group <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="id">Selected item Id.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult MoveDown(string groupName, uint id);
        /// <summary>
        /// Make a copy of selected item by <paramref name="id"/> inside selected group <paramref name="groupName"/>.
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        /// <param name="id">Selected item Id.</param>
        /// <returns>CommandExecutionResult <see cref="CommandExecutionResult"/>.</returns>
        CommandExecutionResult CopyItem(string groupName, uint id);

        //CommandExecutionResult ReverseList(string groupName)?

        /// <summary>
        /// Sequently Send commands from selected Group
        /// </summary>
        /// <param name="groupName">Selected Group Name.</param>
        void RunGroup(string groupName);
        void RunCommands(string groupName, List<uint> ids);

        //Review is it still needed:
        //void AddItemsToGroup(string groupName, List<ISelectableItem<PredefinedPayload>> favoriteItem);
        //IPredefinedCommandGroup UploadGroup(string name, List<ISelectableItem<PredefinedPayload>> favoriteItems);
        //ISelectableItem<PredefinedPayload> CloneItem(string groupName, uint id)
        //void UpdateItem(string groupName, ISelectableItem<PredefinedPayload> favoriteItem);

        //void RunSelectedCommands(string groupName); == void RunGroup(string name);

    }
}
