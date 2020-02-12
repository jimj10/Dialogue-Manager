/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 * The Event Log code code is derived from code submitted to Stackoverflow by
 * Federico Berasategui (thank-you)
 * https://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf 
 */
using System;
using System.ComponentModel;

namespace DialogueManager.EventLog
{
    public enum LogCategory
    {
        [Description("INFO")]
        INFO,
        [Description("CONVERSE_AUDIO")]
        CONVERSE_AUDIO,
        [Description("CONVERSE_TEXT")]
        CONVERSE_TEXT,
        [Description("WARN")]
        WARN,
        [Description("ERROR")]
        ERROR,
        [Description("DEBUG")]
        DEBUG
    };

    public class LogEntry : PropertyChangedBase
    {
        public DateTime DateTime { get; set; }

        public string TimeStamp { get { return DateTime.ToString("dd/MM/yyyy HH:mm:ss"); } set { TimeStamp = value; } }

        public int EventId { get; set; }

        public LogCategory Category { get; set; }

        public string Message { get; set; }
    }
}
