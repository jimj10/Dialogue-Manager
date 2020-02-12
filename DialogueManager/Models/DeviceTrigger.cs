/* 
 * Copyright(c) 2018 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using System;
using System.IO;

namespace DialogueManager.Models
{

    public class DeviceTrigger
    {
        public string DeviceName { get; set; }

        public string Category { get; set; } = "Trigger";

        private string label;
        public string Label {
            get { return label; }
            set {
                label = value;
                Tooltip = label;
            }
        }

        public bool TimeTrigger { get; set; }

        public DateTime TriggerTime { get; set; }

        public string Recurrence { get; set; } // "today", "every day"

        private string triggerText;
        public string TriggerText {
            get { return triggerText; }
            set {
                triggerText = value;
                if (triggerText != null)
                {
                    if (TimeTrigger)
                        TriggerAudioFile = "Triggers\\Time\\" + String.Join("_", triggerText.Split(Path.GetInvalidFileNameChars()));
                    else
                        TriggerAudioFile = "Triggers\\" + String.Join("_", triggerText.Split(Path.GetInvalidFileNameChars()));
                }
            }
        }

        public string TriggerAudioFile { get; set; }

        public bool TriggerAudioFileExists { get; set; } = false;

        public string Tooltip { get; set; }
    }
}
