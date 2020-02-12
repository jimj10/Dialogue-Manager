/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using System;
using System.IO;

namespace DialogueManager
{
    static class DirectoryMgr
    {
        public static string AppDataDirectory { get; set; }

        public static string AudioClipsDirectory { get; set; }

        public static string TriggerClipsDirectory { get; set; }

        public static string RecordingsDirectory { get; set; }

        public static string StudyLogsDirectory { get; set; }

        public static string SettingsDirectory { get; set; }

        public static string TempDirectory { get; set; }

        internal static void SetAppDirectories()
        {
            SettingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Jackson\Dialogue Manager\"); 
            Directory.CreateDirectory(SettingsDirectory);
            TempDirectory = Path.Combine(Path.GetTempPath(), @"DialogueManager\");
            Directory.CreateDirectory(TempDirectory);
            AppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Dialogue Manager\");
            AudioClipsDirectory = Path.Combine(AppDataDirectory, @"Audio Clips\");
            Directory.CreateDirectory(AudioClipsDirectory);
            TriggerClipsDirectory = Path.Combine(AudioClipsDirectory, @"Triggers\");
            Directory.CreateDirectory(TriggerClipsDirectory);
            RecordingsDirectory = Path.Combine(AppDataDirectory, @"Recordings\");
            Directory.CreateDirectory(RecordingsDirectory);
            StudyLogsDirectory = Path.Combine(AppDataDirectory, @"StudyLogs\");
            Directory.CreateDirectory(StudyLogsDirectory);
        }
    }
}
