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
using DialogueManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DialogueManager
{
    static class RuleClipGenerator
    {
        public static void UpdateRuleDeletedAudioClip(AudioClip audioClip, int ruleNumber)
        {
            if (ruleNumber < 7)
            {
                if (ruleNumber == 0)
                {
                    audioClip.ConfirmText = "*** ERROR: Cannot delete a new rule ***";
                    audioClip.CheckText = "*** ERROR: Cannot delete a new rule ***";
                    audioClip.StateAudioFile = String.Empty;
                    audioClip.ConfirmAudioFile = String.Empty;
                    audioClip.CheckAudioFile = String.Empty;
                }
                else
                {
                    List<string> clips = new List<string>();
                    string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Let me check I have understood. "); 
                    string numberTxt = TextHelper.GetNumberText(ruleNumber); 
                    audioClip.ConfirmText = "OK. Rule " + numberTxt + " deleted";
                    sb.Append("Do you want to delete rule " + numberTxt + "?");
                    clips.Add(Path.Combine(audioDirectory, "OK"));
                    clips.Add(Path.Combine(audioDirectory, "Rule " + numberTxt + " deleted"));
                    audioClip.ConfirmAudioFile = AudioMgr.CombineAudioClips(clips);
                    clips.Clear();
                    clips.Add(Path.Combine(audioDirectory, "Let me check I have understood"));
                    clips.Add(Path.Combine(audioDirectory, "Do you want to delete rule " + numberTxt + "_"));
                    audioClip.CheckText = sb.ToString();
                    audioClip.CheckAudioFile = AudioMgr.CombineAudioClips(clips);
                }
            }
            else
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("RuleClipGenerator.SetRuleDeletedAudioClip(): ruleNumber {0} out of range.", ruleNumber));
        }

        public static void UpdateDuplicateWarning(AudioClip audioClip, int ruleNumber)
        {
            if (ruleNumber < 7)
            {
                string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset"); 
                string numberTxt = TextHelper.GetNumberText(ruleNumber);
                audioClip.StateText = "This new rule is a duplicate of rule " + numberTxt + ".";
                audioClip.StateAudioFile = Path.Combine(audioDirectory, "This new rule is a duplicate of rule " + numberTxt + ".mp3");
            }
            else
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("RuleClipGenerator.UpdateDuplicateWarning(): ruleNumber {0} out of range.", ruleNumber));
        }

        public static void UpdateConflictWarning(AudioClip audioClip, int ruleNumber)
        {
            if (ruleNumber < 7)
            {
                List<string> clips = new List<string>();
                string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset"); 
                string numberTxt = TextHelper.GetNumberText(ruleNumber);
                audioClip.StateText = "This new rule conflicts with rule " + numberTxt + ". Would you like to change rule one?";
                clips.Add(Path.Combine(audioDirectory, "This new rule conflicts with rule " + numberTxt));
                clips.Add(Path.Combine(audioDirectory, "Would you like to change rule " + numberTxt + "_"));
                audioClip.StateAudioFile = AudioMgr.CombineAudioClips(clips);
            }
            else
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("RuleClipGenerator.UpdateConflictWarning(): ruleNumber {0} out of range.", ruleNumber));
        }
    }
}
