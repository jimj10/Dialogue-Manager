/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using System.Windows;

namespace DialogueManager.Models
{
    public class AudioClip
    {
        public int ClipId { get; set; }
        public string DeviceName { get; set; }
        public string Category { get; set; } = "Standard"; // 'Standard' or rule-related ('Ruleset', 'Action', 'Trigger', 'TimeTrigger')

        private string label;
        public string Label {
            get { return label; }
            set {
                label = value;
                Tooltip = label;
            }
        }

        public bool IsVisible { get; set; } = true;

        public string Recurrence { get; set; } // TimeTriggers: "today", "every day"

        public string StateText { get; set; }

        public string StateAudioFile { get; set; }

        public string ConfirmText { get; set; }
        public string ConfirmAudioFile { get; set; }

        public string CheckText { get; set; }
        public string CheckAudioFile { get; set; }

        public string Tooltip { get; set; }
        public bool AudioFilesExist { get; set; }
        public string ButtonColour { get; set; }

    }
}
