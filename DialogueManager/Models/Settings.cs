/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.Database;
using DialogueManager.EventLog;
using System;
using System.Data;
using System.IO;


namespace DialogueManager.Models
{
    static class Settings
    {
        public static bool CheckAudioFiles = false;

        public static string CredentialsFile { get; set; }

        public static string LanguageCode { get; set; } = "en-GB";

        public static string OnlineVoice { get; set; } = "en-GB-Wavenet-A";

        public static int MaxLogEntries { get; set; } = 2000;

        public static bool LoadSettingsFromDB()
        {
            bool returnValue = false;
            DataTable dataTable = SettingsTableMgr.GetSettings();
            string[] optionsList = { "CredentialsFile", "CheckAudioFiles", "MaxLogEntries",
                "LanguageCode", "OnlineVoice" };
            if (dataTable != null)
            {
                returnValue = true;
                foreach (var option in optionsList)
                {
                    if (dataTable.Columns.Contains(option))
                    {
                        switch (option)
                        {
                            case "CredentialsFile":
                                CredentialsFile = dataTable.Rows[0][option].ToString();
                                break;
                            case "LanguageCode":
                                LanguageCode = dataTable.Rows[0][option].ToString();
                                break;
                            case "OnlineVoice":
                                OnlineVoice = dataTable.Rows[0][option].ToString();
                                break;
                            case "CheckAudioFiles":
                                string intString = dataTable.Rows[0][option].ToString();
                                if (Int32.TryParse(intString, out int intValue))
                                    CheckAudioFiles = intValue == 1 ? true : false;
                                else
                                {
                                    returnValue = false;
                                    Logger.AddLogEntry(LogCategory.ERROR, String.Format("LoadOptionsFromDB: Could not parse {0}.", option));
                                }
                                break;
                            case "MaxLogEntries":
                                intString = dataTable.Rows[0][option].ToString();
                                if (Int32.TryParse(intString, out intValue))
                                    MaxLogEntries = intValue;
                                else
                                {
                                    returnValue = false;
                                    Logger.AddLogEntry(LogCategory.ERROR, String.Format("LoadOptionsFromDB: Could not parse {0}.", option));
                                }
                                break;
                            default:
                                returnValue = false;
                                Logger.AddLogEntry(LogCategory.ERROR, String.Format("LoadOptionsFromDB: {0} not found.", option));
                                break;

                        }
                    }
                }
            }
            return returnValue;
        }
    }
}
