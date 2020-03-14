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
using System;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class EventLogTableMgr
    {
        internal static bool CreateEventLogDBTable()
        {
            int updatedRows = 0;
            SQLiteConnection dbConnection;
            using (dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[EVENT_LOG];" +
                        "CREATE TABLE [EVENT_LOG] (" +
                        "[EventId] INTEGER NOT NULL, " +
                        "[TimeStamp] TEXT NOT NULL, " +
                        "[Category] TEXT NOT NULL, " +
                        "[Message] TEXT NOT NULL, " +
                        "CONSTRAINT[PK_EVENT_LOG] PRIMARY KEY([EventId]));";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool SaveLogEntryToDB(LogEntry logEntry)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [EVENT_LOG] (" +
                        "[EventId], [TimeStamp], [Category], [Message]) " +
                        "VALUES( @eventId, @timeStamp, @category, @message);";
                    cmd.Parameters.Add(new SQLiteParameter("@eventId", SqlDbType.Int) { Value = logEntry.EventId });
                    cmd.Parameters.Add(new SQLiteParameter("@timeStamp", DbType.String) { Value = logEntry.EntryDateTime });
                    cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = Enum.GetName(logEntry.Category.GetType(), logEntry.Category) });
                    cmd.Parameters.Add(new SQLiteParameter("@message", DbType.String) { Value = logEntry.Message });
                    updatedRows += cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }

        internal static bool SaveTrimmedLogToDB(DataTable trimmedLog) // save trimmed log to db
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DELETE FROM EVENT_LOG;";
                    cmd.ExecuteNonQuery();
                    foreach (DataRow dr in trimmedLog.Rows)
                    {
                        cmd.CommandText = "INSERT INTO [EVENT_LOG] (" +
                        "[EventId], [TimeStamp], [Category], [Message]) " +
                        "VALUES( @eventId, @timeStamp, @category, @message);";
                        cmd.Parameters.Add(new SQLiteParameter("@eventId", SqlDbType.Int) { Value = Int32.Parse(dr["EventId"].ToString()) });
                        cmd.Parameters.Add(new SQLiteParameter("@timeStamp", DbType.String) { Value = dr["TimeStamp"].ToString() });
                        cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = dr["Category"].ToString() });
                        cmd.Parameters.Add(new SQLiteParameter("@message", DbType.String) { Value = dr["Message"].ToString() });
                        updatedRows += cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
            }
            return updatedRows == trimmedLog.Rows.Count;
        }

        internal static DataTable GetEventLog()
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                string query;
                if (DBAdmin.TableExists("EVENT_LOG"))
                    query = "SELECT * FROM EVENT_LOG";
                else
                    return null;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, dbConnection))
                {
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            using (var tempDataTable = new DataTable())
                            {
                                tempDataTable.Load(reader);
                                dataTable = tempDataTable;
                            }     
                        }
                    }
                }
            }
            return dataTable;
        }
    }
}
