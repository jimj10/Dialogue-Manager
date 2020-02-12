/* 
 * Copyright(c) 2018 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.Models;
using DialogueManager.Database;
using DialogueManager.EventLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace DialogueManager
{
    static class TriggersInventory
    {
        public static List<DeviceTrigger> Triggers = new List<DeviceTrigger>();

        public static bool AddTrigger(DeviceTrigger newTrigger, out string outcome, bool updateDB = true)
        {
            if (Triggers.FirstOrDefault(x => x.Label.Equals(newTrigger.Label)) == null)
            {
                newTrigger.TriggerAudioFileExists = File.Exists(Path.Combine(DirectoryMgr.AudioClipsDirectory, newTrigger.TriggerAudioFile + ".mp3"));
                Triggers.Add(newTrigger);
                if (updateDB)
                {
                    if (TriggersTableMgr.AddTrigger(newTrigger))
                    {
                        outcome = "DeviceTrigger added";
                        return true;
                    }
                    else
                    {
                        outcome = "Error adding DeviceTrigger to database";
                        Logger.AddLogEntry(LogCategory.ERROR, "Error adding DeviceTrigger to database: " + newTrigger.Label);
                        return false;
                    }
                }
            }
            outcome = "DeviceTrigger name \'" + newTrigger.Label + "\' already exits.\nPlease choose another name.";
            return false;
        }

        public static bool UpdateTriggersToDB()
        {
            return TriggersTableMgr.UpdateTriggers(Triggers);
        }

        public static bool CheckAudioFiles()
        {
            bool allOK = true;
            foreach (var trigger in Triggers)
            {
                trigger.TriggerAudioFileExists = File.Exists(Path.Combine(DirectoryMgr.AudioClipsDirectory, 
                    trigger.TriggerAudioFile + ".mp3"));
                if (!trigger.TriggerAudioFileExists)
                {
                    allOK = false;
                    Logger.AddLogEntry(LogCategory.ERROR, 
                        String.Format("TriggerAudioFile {0} not found", trigger.TriggerAudioFile));
                }
            }
            return allOK;
        }

        public static bool DeleteTrigger(string label, out string outcome)
        {
            var trigger = Triggers.FirstOrDefault(x => x.Label.Equals(label));
            if (trigger != null)
            {
                Triggers.Remove(trigger);
                if (TriggersTableMgr.DeleteTrigger(label))
                {
                    Logger.AddLogEntry(LogCategory.INFO, "Deleted DeviceTrigger: " + label);
                    outcome = "DeviceTrigger deleted";
                    return true;
                }
                else
                {
                    outcome = "Error deleting DeviceTrigger from database";
                    return false;
                }
            }
            outcome = "DeviceTrigger name \'" + label + "\' not found.";
            return false;
        }

        public static List<DeviceTrigger> GetTriggers(string deviceName = null, bool includeTimeTriggers = false)
        {
            if (deviceName == null)
            {
                if (includeTimeTriggers)
                    return Triggers;
                else
                {
                    List<DeviceTrigger> triggers = new List<DeviceTrigger>();
                    foreach (var trigger in Triggers)
                    {
                        if (!trigger.TimeTrigger)
                            triggers.Add(trigger);
                    }
                    return triggers;
                }
            }  
            else
            {
                List<DeviceTrigger> triggers = new List<DeviceTrigger>();
                if (includeTimeTriggers)
                {
                    foreach (var trigger in Triggers)
                    {
                        if (trigger.DeviceName.Equals("Common") || trigger.DeviceName.Equals(deviceName))
                            triggers.Add(trigger);
                    }
                    return triggers;
                }
                else
                {
                    foreach (var trigger in Triggers)
                    {
                        if (!trigger.TimeTrigger && (trigger.DeviceName.Equals("Common") || trigger.DeviceName.Equals(deviceName)))
                            triggers.Add(trigger);
                    }
                    return triggers;
                }     
            }
        }

        public static DeviceTrigger GetTrigger(string label)
        {
            return Triggers.FirstOrDefault(x => x.Label.Equals(label));
        }

        public static bool LoadTriggersFromDB()
        {
            DataTable dataTable = TriggersTableMgr.GetTriggers();
            Triggers.Clear();
            string[] propsList = { "Category", "Label", "DeviceName", "TimeTrigger",
                "TriggerText", "Recurrence", "TriggerAudioFile", "TriggerAudioFileExists", "Tooltip" };
            if (dataTable != null)
            {
                bool allOK = true;
                foreach (var prop in propsList)
                {
                    if (!dataTable.Columns.Contains(prop))
                    {
                        Logger.AddLogEntry(LogCategory.ERROR, 
                            String.Format("LoadTriggersFromDB: {0} not found.", prop));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        string timeTriggerStr = dr["TimeTrigger"].ToString();
                        string triggerAudioFileExistsStr = dr["TimeTrigger"].ToString();
                        if (Int32.TryParse(timeTriggerStr, out int timeTrigger)
                            && Int32.TryParse(triggerAudioFileExistsStr, out int triggerAudioFileExists))
                        {
                            Triggers.Add(new DeviceTrigger()
                            {
                                Category = dr["Category"].ToString(),
                                Label = dr["Label"].ToString(),
                                DeviceName = dr["DeviceName"].ToString(),
                                TimeTrigger = (timeTrigger == 1),
                                TriggerText = dr["TriggerText"].ToString(),
                                Recurrence = dr["Recurrence"].ToString(),
                                TriggerAudioFile = dr["TriggerAudioFile"].ToString(),
                                TriggerAudioFileExists = (triggerAudioFileExists == 1),
                                Tooltip = dr["Tooltip"].ToString(),
                            });
                        }
                        else
                            Logger.AddLogEntry(LogCategory.ERROR, 
                                String.Format("LoadTriggersFromDB: Could not parse entry for timeTriggerStr {0}", timeTriggerStr));
                    }
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
    }
}
