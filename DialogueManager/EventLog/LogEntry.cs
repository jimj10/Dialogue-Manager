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
using System.Globalization;

namespace DialogueManager.EventLog
{


    public class LogEntry : PropertyChangedBase
    {
        public DateTime EntryDateTime { get; set; }

        public string Timestamp
        {
            get
            {
                return String.Format("{0} {1}",
                     EntryDateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern), EntryDateTime.ToString("HH:mm:ss"));
            }
            set { Timestamp = value; }
        }

        public int EventId { get; set; }

        public LogCategory Category { get; set; }

        public string Message { get; set; }
    }

    public enum LogCategory
    {
        [Description("INFO")]
        INFO,
        [Description("CONV_AUDIO")]
        CONV_AUDIO,
        [Description("CONV_TEXT")]
        CONV_TEXT,
        [Description("WARN")]
        WARN,
        [Description("ERROR")]
        ERROR,
        [Description("DEBUG")]
        DEBUG
    };
}
