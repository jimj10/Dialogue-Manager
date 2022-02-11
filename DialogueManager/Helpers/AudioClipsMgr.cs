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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DialogueManager
{
    static class AudioClipsMgr
    { 
        public static List<AudioClip> AudioClips = new List<AudioClip>();
        public static List<AudioClip> TimeTriggerClips = new List<AudioClip>();
        public static bool AudioclipsLoaded = false;

        public static int AddAudioClip(AudioClip newClip)
        {
            if (AudioClips.FirstOrDefault(x => x.Label.Equals(newClip.Label)) == null)
            {
                AudioClips.Add(newClip);
                if (AudioClipsTableMgr.AddAudioClip(newClip))
                {
                    newClip.ClipId = AudioClipsTableMgr.GetClipId(newClip.Label);
                    Logger.AddLogEntry(LogCategory.INFO, "Added AudioClip: " + newClip.StateAudioFile);
                    return newClip.ClipId;
                }
                else
                    return -2; // problem updating database
            }
            return -1; // audioclip label already exits
        }

        public static bool AddTimeTriggerClip(AudioClip newClip, bool updateDB = true)
        {
            if (TimeTriggerClips.FirstOrDefault(x => x.Label.Equals(newClip.Label)) == null)
            {
                TimeTriggerClips.Add(newClip);
                if (updateDB)
                {
                    if (TimeTriggerClipsTableMgr.AddTimeTriggerClip(newClip))
                    {
                        Logger.AddLogEntry(LogCategory.INFO, "Added TimeTriggerClip: " + newClip.StateAudioFile);
                        return true;
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public static bool UpdateTimeTriggerClipsToDB()
        {
            return TimeTriggerClipsTableMgr.UpdateTimeTriggerClips(TimeTriggerClips);
        }

        public static bool UpdateAudioClipsToDB()
        {
            return AudioClipsTableMgr.UpdateAudioClips(AudioClips);
        }

        public static int UpdateAudioClipToDB(AudioClip clipCopy)
        {
            var originalClip = AudioClips.FirstOrDefault(x => x.ClipId.Equals(clipCopy.ClipId));
            if (!originalClip.Label.Equals(clipCopy.Label))
            {
                if (AudioClips.FirstOrDefault(x => x.Label.Equals(clipCopy.Label)) != null)
                    return -1; // name already exists
            }
            var index = AudioClips.FindIndex(c => c.ClipId == originalClip.ClipId);
            AudioClips[index] = clipCopy;
            if (!AudioClipsTableMgr.UpdateAudioClip(clipCopy))
                return -2; // problem updating database
            else
                return clipCopy.ClipId;
        }

        public static AudioClip GetAudioClipCopy(string label)
        {
            var original = AudioClips.FirstOrDefault(x => x.Label.Equals(label));
            return original == null ? null : AudioClipCopy(original);
        }

        public static AudioClip GetAudioClipCopy(int clipId)
        {
            var original = AudioClips.FirstOrDefault(x => x.ClipId.Equals(clipId));
            return original == null ? null : AudioClipCopy(original);
        }

        private static AudioClip AudioClipCopy(AudioClip original)
        {
            return new AudioClip()
            {
                ClipId = original.ClipId,
                Label = original.Label,
                Category = original.Category,
                DeviceName = original.DeviceName,
                ButtonColour = original.ButtonColour,
                IsVisible = original.IsVisible,
                StateText = original.StateText,
                ConfirmText = original.ConfirmText,
                CheckText = original.CheckText,
                Recurrence = original.Recurrence,
                StateAudioFile = original.StateAudioFile,
                ConfirmAudioFile = original.ConfirmAudioFile,
                CheckAudioFile = original.CheckAudioFile,
                Tooltip = original.Tooltip,
            };
        }

        public static bool DeleteAudioClip(string label, out string outcome)
        {
            var clip = AudioClips.FirstOrDefault(x => x.Label.Equals(label));
            if (clip != null)
            {
                AudioClips.Remove(clip);
                if (AudioClipsTableMgr.DeleteAudioClip(label))
                {
                    Logger.AddLogEntry(LogCategory.INFO, "Deleted AudioClip: " + label);
                    outcome = "AudioClip deleted";
                    return true;
                }
                else
                {
                    outcome = "Error deleting AudioClip from database";
                    return false;
                }
            }
            outcome = "AudioClip name \'" + label + "\' not found.";
            return false;
        }


        public static List<AudioClip> GetAudioClips(string sessionName, string category)
        {
            List<AudioClip> audioClips = new List<AudioClip>();
            var session = SessionsMgr.GetSession(sessionName);
            if (session != null)
            {
                var sessionClips = GetAudioClips(session.SessionAudioClipsList);
                foreach (var sessionClip in sessionClips)
                {
                    if (sessionClip.Category.Equals(category))
                        audioClips.Add(sessionClip);
                }
            }
            return audioClips;
        }

        public static List<AudioClip> GetAudioClips(List<int> audioClipsList)
        {
            List<AudioClip> audioClips = new List<AudioClip>();
            foreach (var clipId in audioClipsList)
            {
                audioClips.Add(AudioClips.FirstOrDefault(x => x.ClipId.Equals(clipId)));
            }
            return audioClips;
        }

        public static AudioClip GetAudioClip(string label)
        {
            var audioClip = AudioClips.FirstOrDefault(x => x.Label.Equals(label));
            return audioClip;
        }

        public static AudioClip GetTimeTriggerClip(string label)
        {
            var timeTriggerClip = TimeTriggerClips.FirstOrDefault(x => x.Label.Equals(label));
            return timeTriggerClip;
        }

        public static bool LoadAudioClipsFromDB()
        {
            AudioclipsLoaded = true;
            EventSystem.Publish<AudioclipsLoaded>(new AudioclipsLoaded { });
            return AudioClipsTableMgr.LoadAudioClipsFromDB(AudioClips);
        }

        public static bool LoadTimeTriggerClipsFromDB()
        {
            return TimeTriggerClipsTableMgr.LoadTimeTriggerClipsFromDB(TimeTriggerClips);
        }

        public static bool CheckAudioFiles()
        {
            bool allOK = true;
            foreach (var clip in AudioClips.Concat(TimeTriggerClips))
            {
                // Called from AudioFileAuditor.CheckAudioFiles()

                string audiofile;
                if (!String.IsNullOrEmpty(clip.StateAudioFile))
                {
                    audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, clip.StateAudioFile + ".mp3");
                    if (!File.Exists(audiofile))
                    {
                        clip.AudioFilesExist = false;
                        if (!audiofile.Contains("AppData\\Local\\Temp"))
                        {
                            allOK = false;
                            Logger.AddLogEntry(LogCategory.ERROR, String.Format("Audio clip {0}: audio file {1} not found", clip.Label, clip.StateAudioFile));
                        }
                    }
                }
                if (!String.IsNullOrEmpty(clip.ConfirmAudioFile))
                {
                    audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, clip.ConfirmAudioFile + ".mp3");
                    if (!File.Exists(audiofile))
                    {
                        clip.AudioFilesExist = false;
                        allOK = false;
                        Logger.AddLogEntry(LogCategory.ERROR, String.Format("Audio clip {0}: audio file {1} not found", clip.Label, clip.ConfirmAudioFile));
                    }
                }
                if (!String.IsNullOrEmpty(clip.CheckAudioFile))
                {
                    audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, clip.CheckAudioFile + ".mp3");
                    if (!File.Exists(audiofile))
                    {
                        clip.AudioFilesExist = false;
                        if (!audiofile.Contains("AppData\\Local\\Temp"))
                        {
                            allOK = false;
                            Logger.AddLogEntry(LogCategory.ERROR, String.Format("Audio clip {0}: audio file {1} not found", clip.Label, clip.CheckAudioFile));
                        }  
                    }
                }
            }
            return allOK;
        }
    }
}
