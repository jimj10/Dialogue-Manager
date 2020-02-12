/* 
 * Copyright(c) 2018 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class TriggersTableMgr
    {
        internal static bool CreateTriggersDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[TRIGGERS];" +
                        "CREATE TABLE [TRIGGERS] (" +
                        "[TriggerId] INTEGER PRIMARY KEY, " +
                        "[DeviceName] TEXT NOT NULL, " +
                        "[Category] TEXT NOT NULL, " +
                        "[Label] TEXT NOT NULL, " +
                        "[TimeTrigger] INTEGER NOT NULL, " +
                        "[TriggerText] TEXT, " +
                        "[Recurrence] TEXT, " +
                        "[TriggerAudioFile] TEXT, " +
                        "[TriggerAudioFileExists] INTEGER NOT NULL, " +
                        "[Tooltip] TEXT);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddTrigger(DeviceTrigger trigger)
        {
            lock (DBAdmin.padlock)
            {
                int updatedRows = 0;
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        SQLiteTransaction trans = dbConnection.BeginTransaction();
                        cmd.CommandText = "INSERT INTO [TRIGGERS] ([DeviceName], [Category], " +
                            "[Label], [TimeTrigger], [TriggerText], [Recurrence], [TriggerAudioFile], " +
                            "[TriggerAudioFileExists], [Tooltip]) " +
                                "VALUES(@deviceName, @category, @label,  @timeTrigger, " +
                                "@triggerText, @recurrence, @triggerAudioFile, @triggerAudioFileExists, @tooltip);";
                        cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = trigger.DeviceName });
                        cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = trigger.Category });
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = trigger.Label });
                        cmd.Parameters.Add(new SQLiteParameter("@timeTrigger", DbType.Int32) { Value = trigger.TimeTrigger ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@triggerText", DbType.String) { Value = trigger.TriggerText });
                        cmd.Parameters.Add(new SQLiteParameter("@recurrence", DbType.String) { Value = trigger.Recurrence });
                        cmd.Parameters.Add(new SQLiteParameter("@triggerAudioFile", DbType.String) { Value = trigger.TriggerAudioFile });
                        cmd.Parameters.Add(new SQLiteParameter("@triggerAudioFileExists", DbType.Int32) { Value = trigger.TriggerAudioFileExists ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = trigger.Tooltip });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateTriggers(List<DeviceTrigger> triggers)
        {
            lock (DBAdmin.padlock)
            {
                int updatedRows = 0;
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        SQLiteTransaction trans = dbConnection.BeginTransaction();
                        cmd.CommandText = "DELETE FROM [TRIGGERS];";
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var trigger in triggers)
                        {
                            cmd.CommandText = "INSERT INTO [TRIGGERS] ([DeviceName], [Category], " +
                            "[Label], [TimeTrigger], [TriggerText], [Recurrence], [TriggerAudioFile], " +
                            "[TriggerAudioFileExists], [Tooltip]) " +
                                "VALUES(@deviceName, @category, @label, @timeTrigger, " +
                                "@triggerText, @recurrence, @triggerAudioFile, @triggerAudioFileExists, @tooltip);";
                            cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = trigger.DeviceName });
                            cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = trigger.Category });
                            cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = trigger.Label });
                            cmd.Parameters.Add(new SQLiteParameter("@timeTrigger", DbType.Int32) { Value = trigger.TimeTrigger ? 1 : 0 });
                            cmd.Parameters.Add(new SQLiteParameter("@triggerText", DbType.String) { Value = trigger.TriggerText });
                            cmd.Parameters.Add(new SQLiteParameter("@recurrence", DbType.String) { Value = trigger.Recurrence });
                            cmd.Parameters.Add(new SQLiteParameter("@triggerAudioFile", DbType.String) { Value = trigger.TriggerAudioFile });
                            cmd.Parameters.Add(new SQLiteParameter("@triggerAudioFileExists", DbType.Int32) { Value = trigger.TriggerAudioFileExists ? 1 : 0 });
                            cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = trigger.Tooltip });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == triggers.Count;
            }
        }

        internal static bool DeleteTrigger(string label)
        {
            lock (DBAdmin.padlock)
            {
                int updatedRows = 0;
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        SQLiteTransaction trans = dbConnection.BeginTransaction();
                        cmd.CommandText = "DELETE FROM TRIGGERS WHERE [Label] = @label";
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.Int32) { Value = label });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static DataTable GetTriggers()
        {
            DataTable dataTable = null;
            DataTable tempDataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("TRIGGERS"))
                    return null;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM TRIGGERS";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            using (tempDataTable = new DataTable())
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
