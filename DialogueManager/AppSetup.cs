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
using DialogueManager.Models;
using System.Threading.Tasks;
using System.Windows;

namespace DialogueManager
{
    static class AppSetup
    {

        public static Window MainWindow { get; set; }

        public static bool InitialiseApp() // called from MainWindow
        {
            DirectoryMgr.SetAppDirectories();
            if (DBAdmin.DefaultDatabaseExists())
                LoadDatabaseTables();
            else
            {
                CreateDatabaseTables();
                LoadDatabaseTables();
            }
            // Check audio files exist for loaded audioclips
            if (Settings.CheckAudioFiles)
                Task.Factory.StartNew(() => AudioFileAuditor.CheckAudioFiles());
            return true;
        }

        private static void LoadDatabaseTables()
        {
            Logger.LoadLogEntries();
            Logger.AddLogEntry(LogCategory.INFO, "NEW SESSION STARTED...");
            Settings.LoadSettingsFromDB();
            AudioClipsMgr.LoadAudioClipsFromDB();
            AudioClipsMgr.LoadTimeTriggerClipsFromDB();
            GoogleTextToSpeechMgr.LoadOnlineVoicesFromDB();
            SessionsMgr.LoadSessionsFromDB();
            AudioMgr.GetAudioDevices();
        }

        private static void CreateDatabaseTables()
        {
            string filename = DBAdmin.CreateDBFile();
            if (EventLogTableMgr.CreateEventLogDBTable())
                Logger.AddLogEntry(LogCategory.INFO, "Created Database file \'" + filename + "\'.");
            SettingsTableMgr.CreateSettingsDBTable();
            AudioClipsTableMgr.CreateAudioClipsDBTable();
            TimeTriggerClipsTableMgr.CreateTimeTriggerClipsDBTable();
            RulesetsTableMgr.CreateRulesetsDBTable();
            RulesTableMgr.CreateRulesetRulesDBTable();
            SessionsTableMgr.CreateSessionsDBTable();
            SessionClipsTableMgr.CreateSessionsDBTable();
            OnlineVoicesTableMgr.CreateOnlineVoicesDBTable();
        }
    }
}
