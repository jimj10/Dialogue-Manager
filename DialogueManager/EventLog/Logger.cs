/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 * The Event Log code is derived from code submitted to Stackoverflow by
 * Federico Berasategui
 * https://stackoverflow.com/questions/16743804/implementing-a-log-viewer-with-wpf 
 */
using DialogueManager.Database;
using System;
using System.ComponentModel;
using System.Data;

namespace DialogueManager.EventLog
{
    public static class Logger
    {
        private static int MaxLogEntries = 5000;
        private static LogViewerCtrl logViewerCtrl = new LogViewerCtrl();

        public static LogViewerCtrl LogViewerCtrl { get { return logViewerCtrl; } private set { logViewerCtrl = value; } }

        public static int Index { get; set; } = 1;

        public static void AddLogEntry(LogCategory category, string msg)
        {
            LogEntry logEntry = new LogEntry()
            {
                EventId = Index++,
                EntryDateTime = DateTime.Now,
                Category = category,
                Message = msg
            };
            logViewerCtrl.AddLogEntry(logEntry);
            EventLogTableMgr.SaveLogEntryToDB(logEntry);
        }

        internal static bool LoadLogEntries()
        {
            DataTable dataTable = EventLogTableMgr.GetEventLog();
            if (dataTable != null)
            {
                if (dataTable.Rows.Count > MaxLogEntries)
                    dataTable = TrimLogEntries(dataTable);
                int eventId = 0;
                foreach (DataRow dr in dataTable.Rows)
                {
                    string eventIdStr = dr["EventId"].ToString();
                    if (Int32.TryParse(eventIdStr, out eventId))
                    {
                        logViewerCtrl.AddLogEntry(new LogEntry()
                        {
                            EventId = eventId,
                            EntryDateTime = DateTime.Parse(dr["TimeStamp"].ToString()),
                            Category = GetCategory(dr["Category"].ToString()),
                            Message = dr["Message"].ToString()
                        });
                    }
                    else
                        return false;
                }
                Index = eventId + 1; // set index to next eventId
            }
            else
                return false;
            return true;
        } 
        
        private static DataTable TrimLogEntries(DataTable dataTable)
        {
            DataTable trimmedLog = null;
            using (var tempLog = new DataTable())
            {
                tempLog.Columns.Add("EventId", Type.GetType("System.Int32"));
                tempLog.Columns.Add("TimeStamp", Type.GetType("System.String"));
                tempLog.Columns.Add("Category", Type.GetType("System.String"));
                tempLog.Columns.Add("Message", Type.GetType("System.String"));
                int trimCount = dataTable.Rows.Count - MaxLogEntries + 100; // remove 100 entries
                int count = 0;
                for (int i = trimCount; i < dataTable.Rows.Count - 1; i++)
                {
                    tempLog.Rows.Add(new Object[] { count++.ToString(), dataTable.Rows[i]["TimeStamp"],
                    dataTable.Rows[i]["Category"], dataTable.Rows[i]["Message"] });
                }
                trimmedLog = tempLog;
            } 
            EventLogTableMgr.SaveTrimmedLogToDB(trimmedLog);
            return trimmedLog;
        }

        public static DataTable GetLogEntries(DateTime startDateTime, DateTime endDateTime)
        {
            
            DataTable fullLogTable = EventLogTableMgr.GetEventLog();
            DataTable filteredlog = null;
            using (var tempLog = new DataTable())
            {
                tempLog.Columns.Add("EventId", Type.GetType("System.Int32"));
                tempLog.Columns.Add("TimeStamp", Type.GetType("System.String"));
                tempLog.Columns.Add("Category", Type.GetType("System.String"));
                tempLog.Columns.Add("Message", Type.GetType("System.String"));
                int count = 0;
                DateTime dateTime;
                for (int i = 0; i < fullLogTable.Rows.Count - 1; i++)
                {
                    dateTime = DateTime.Parse(fullLogTable.Rows[i]["TimeStamp"].ToString());
                    if (dateTime >= startDateTime && dateTime <= endDateTime)
                        tempLog.Rows.Add(new Object[] { count++.ToString(), fullLogTable.Rows[i]["TimeStamp"],
                        fullLogTable.Rows[i]["Category"], fullLogTable.Rows[i]["Message"] });
                }
                filteredlog = tempLog;
            }
            return filteredlog;
        }

        private static LogCategory GetCategory(string description)
        {
            foreach (var field in new LogEntry().Category.GetType().GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (LogCategory)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (LogCategory)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
        }
    }
}

