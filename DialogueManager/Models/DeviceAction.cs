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
    public class DeviceAction
    {
        public string DeviceName { get; set; }

        public string Category { get; set; } = "Action";

        private string label;
        public string Label {
            get { return label; }
            set {
                label = value;
                Tooltip = label;
            }
        }

        private string actionText;
        public string ActionText {
            get { return actionText; }
            set {
                actionText = value;
                if (actionText != null)
                {
                    ActionAudioFile = DeviceName + "\\" + String.Join("_", actionText.Split(Path.GetInvalidFileNameChars()));
                }
            }
        }

        public string ActionAudioFile { get; set; }

        public bool ActionAudioFileExists { get; set; }

        public string Tooltip { get; set; }
    }
}
