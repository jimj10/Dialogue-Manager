/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.EventLog;
using DialogueManager.Views;
using System;
using System.Windows;

namespace DialogueManager
{
    static class AudioFileAuditor
    {
        public static void CheckAudioFiles()
        {
            // Runs at startup if 'Check Audio files' in Settings screen is checked 

            Logger.AddLogEntry(LogCategory.INFO, "Checking audio files...");
            bool allOK = true;
            if (!AudioClipsMgr.CheckAudioFiles())
                allOK = false;
            if (allOK)
                Logger.AddLogEntry(LogCategory.INFO, "Audio files OK");
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var messageWin = new MessageWin("Audio Files Check", "WARNING: One or more audio files is missing\nCheck Event Log for details.");
                    messageWin.Show();
                }));
                Logger.AddLogEntry(LogCategory.ERROR, "One or more audio files is missing");
            }
        }
    }
}
