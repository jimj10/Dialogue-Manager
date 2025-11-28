/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DialogueManager.Models
{
    public class DeviceRule
    {
        public int RuleNumber { get; set; }

        public string DeviceName { get; set; }

        public AudioClip TriggerClip { get; set; }

        public AudioClip ActionClip { get; set; }

        public bool Complete { get { return (TriggerClip != null && ActionClip != null); } } // rule has all required information

        public string StateText
        {
            get
            {
                if (TriggerClip != null && ActionClip != null)
                {
                    return TriggerClip.StateText + ", " + ActionClip.StateText + ".";
                }
                else
                {
                    return null;
                }
            }
        }

        public string GetAudioText(string activity, int ruleNumber)
        {
            string ruleNumberText = TextHelper.GetNumberText(ruleNumber);
            StringBuilder sb = new StringBuilder();
            if (activity.Equals("State"))
            {
                if (ruleNumber == -1) // New rule
                {
                    sb.Append("New rule. " + StateText);
                }
                else
                {
                    sb.Append("Rule " + ruleNumberText + ". " + StateText);
                }
            }
            else if (activity.Equals("Confirm"))
            {
                if (ruleNumber == -1)
                {
                    sb.Append("OK, new rule added. ");
                }
                else
                {
                    sb.Append("OK, rule " + ruleNumberText + " changed. ");
                }

                if (!ActionClip.Label.Equals("Generic New Rule"))
                {
                    sb.Append(StateText);
                }
            }
            else if (activity.Equals("Check"))
            {
                sb.Append("Let me check I have understood. ");
                if (ruleNumber == -1)
                {
                    sb.Append("Add new rule: ");
                }
                else
                {
                    sb.Append("Change rule " + ruleNumberText + ": ");
                }

                sb.Append(StateText.ToLower());
                sb.Append(" Is this correct?");
            }
            return sb.ToString();

        }

        public string GetRuleAudioFile(string activity, int ruleNumber)
        {
            string ruleNumberText = TextHelper.GetNumberText(ruleNumber);
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
            if (TriggerClip != null && ActionClip != null)
            {
                List<string> clips = new List<string>();
                if (activity.Equals("State"))
                {
                    if (ruleNumber == -1) // New rule
                    {
                        clips.Add(Path.Combine(audioDirectory, "New rule"));
                    }
                    else
                    {
                        clips.Add(Path.Combine(audioDirectory, "Rule " + ruleNumberText));
                    }

                    clips.Add(TriggerClip.StateAudioFile);
                    clips.Add(ActionClip.StateAudioFile);
                }
                else if (activity.Equals("Confirm"))
                {
                    clips.Add(Path.Combine(audioDirectory, "OK"));
                    if (ruleNumber == -1)
                    {
                        clips.Add(Path.Combine(audioDirectory, "New rule added"));
                    }
                    else
                    {
                        clips.Add(Path.Combine(audioDirectory, "Rule " + ruleNumberText + " changed"));
                    }

                    if (!ActionClip.Label.Equals("Generic New Rule"))
                    {
                        clips.Add(TriggerClip.StateAudioFile);
                        clips.Add(ActionClip.StateAudioFile);
                    }
                }
                else if (activity.Equals("Check"))
                {
                    clips.Add(Path.Combine(audioDirectory, "Let me check I have understood"));
                    if (ruleNumber == -1)
                    {
                        clips.Add(Path.Combine(audioDirectory, "Add new rule"));
                    }
                    else
                    {
                        clips.Add(Path.Combine(audioDirectory, "Change rule " + ruleNumberText));
                    }

                    clips.Add(TriggerClip.StateAudioFile);
                    clips.Add(ActionClip.StateAudioFile);
                    clips.Add(Path.Combine(audioDirectory, "Is this correct_"));
                }
                return AudioMgr.CombineAudioClips(clips);
            }
            return null;
        }
    }
}
