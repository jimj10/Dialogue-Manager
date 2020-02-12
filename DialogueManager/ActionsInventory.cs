/* 
 * Copyright(c) 2018 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.Database;
using DialogueManager.EventLog;
using DialogueManager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DialogueManager
{
    internal static class ActionsInventory
    {
        private static List<DeviceAction> Actions { get; set; } = new List<DeviceAction>();

        public static bool AddAction(DeviceAction action, bool updateDB = true)
        {
            if (Actions.FirstOrDefault(x => x.ActionText.Equals(action.ActionText)) == null)
            {
                Actions.Add(action);
                if (updateDB)
                    ActionsTableMgr.AddRule(action);
                return true;
            }
            return false;
        }

        public static bool UpdateActionsToDB()
        {
            return ActionsTableMgr.UpdateActions(Actions);
        }

        private static bool DeleteAction(string label)
        {
            var action = Actions.FirstOrDefault(x => x.Label.Equals(label));
            if (action != null)
            {
                Actions.Remove(action);
                ActionsTableMgr.DeleteRule(label);
                return true;
            }
            return false;
        }

        public static List<DeviceAction> GetActions(string deviceName = null)
        {
            List<DeviceAction> deviceRules = new List<DeviceAction>();
            foreach (var action in Actions)
            {
                if (String.IsNullOrEmpty(deviceName) || action.DeviceName.Equals(deviceName) || action.DeviceName.Equals("Common"))
                    deviceRules.Add(action);
            }
            return deviceRules;
        }

        public static DeviceAction GetAction(string label)
        {
            var action = Actions.FirstOrDefault(x => x.Label.Equals(label));
            return action;
        }

        public static void LoadActionsFromDB()
        {
            DataTable rules = ActionsTableMgr.GetActions();
            if (rules != null)
            {
                foreach (DataRow row in rules.Rows)
                {
                    var action = new DeviceAction();
                    action.DeviceName = row["DeviceName"].ToString();
                    action.Category = row["Category"].ToString();
                    action.Label = row["Label"].ToString();
                    action.ActionText = row["ActionText"].ToString();
                    action.Tooltip = row["Tooltip"].ToString();
                    Actions.Add(action);
                }
                Logger.AddLogEntry(LogCategory.INFO, "Actions Inventory Loaded");
            }
        }

        public static bool CheckAudioFiles()
        {
            bool allOK = true;
            foreach (var action in Actions)
            {
                action.ActionAudioFileExists = File.Exists(Path.Combine(DirectoryMgr.AudioClipsDirectory, action.ActionAudioFile + ".mp3"));
                if (!action.ActionAudioFileExists)
                {
                    allOK = false;
                    Logger.AddLogEntry(LogCategory.ERROR, String.Format("Action AudioFile {0} not found", action.ActionAudioFile));
                }
            }
            return allOK;
        }
    }
}
