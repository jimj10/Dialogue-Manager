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
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class TimeTriggerClipsTableMgr
    {
        internal static bool CreateTimeTriggerClipsDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[TIMECLIPS];" +
                        "CREATE TABLE [TIMECLIPS] (" +
                        "[ClipId] INTEGER PRIMARY KEY, " +
                        "[Category] TEXT NOT NULL, " +
                        "[Label] TEXT NOT NULL, " +
                        "[ButtonColour] TEXT NOT NULL, " +
                        "[StatementText] TEXT, " +
                        "[StatementAudioFile] TEXT);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddTimeTriggerClip(AudioClip audioClip)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [TIMECLIPS] ([Category], [Label], [ButtonColour], [StatementText], " +
                        "[StatementAudioFile]) " +
                        "VALUES(@category, @label, @buttonColour, @statementText, @statementAudioFile);";
                    cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = audioClip.Category });
                    cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = audioClip.Label });
                    cmd.Parameters.Add(new SQLiteParameter("@buttonColour", DbType.String) { Value = audioClip.ButtonColour });
                    cmd.Parameters.Add(new SQLiteParameter("@statementText", DbType.String) { Value = audioClip.StateText });
                    cmd.Parameters.Add(new SQLiteParameter("@statementAudioFile", DbType.String) { Value = audioClip.StateAudioFile });
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }

        internal static bool DeleteTimeTriggerClip(string label)
        {
            lock (DBAdmin.padlock)
            {
                SQLiteTransaction trans = null;
                int updatedRows = 0;
                // Note that for DEBUG, Resources directory is bin\Debug\Resources
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        trans = dbConnection.BeginTransaction();
                        cmd.CommandText = "DELETE FROM TIMECLIPS WHERE [Label] = @label;";
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = label });
                        updatedRows = cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateTimeTriggerClips(List<AudioClip> timeTriggerClips)
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
                        cmd.CommandText = "DELETE FROM [TIMECLIPS];";
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var audioClip in timeTriggerClips)
                        {
                            cmd.CommandText = "INSERT INTO [TIMECLIPS] ([Category], [Label], [ButtonColour], [StatementText], " +
                                "[StatementAudioFile]) " +
                                "VALUES(@category, @label, @buttonColour, @statementText, @statementAudioFile);";
                            cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = audioClip.Category });
                            cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = audioClip.Label });
                            cmd.Parameters.Add(new SQLiteParameter("@buttonColour", DbType.String) { Value = audioClip.ButtonColour });
                            cmd.Parameters.Add(new SQLiteParameter("@statementText", DbType.String) { Value = audioClip.StateText });
                            cmd.Parameters.Add(new SQLiteParameter("@statementAudioFile", DbType.String) { Value = audioClip.StateAudioFile });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == timeTriggerClips.Count;
            }
        }

        public static bool LoadTimeTriggerClipsFromDB(List<AudioClip> timeTriggerClips)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                string query;
                if (DBAdmin.TableExists("TIMECLIPS"))
                {
                    query = "SELECT * FROM TIMECLIPS";
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
            }
            if (dataTable != null)
            {
                timeTriggerClips.Clear();
                string[] propsList = { "Category", "Label", "ButtonColour", "StatementText", "StatementAudioFile" };
                bool allOK = true;
                foreach (var prop in propsList)
                {
                    if (!dataTable.Columns.Contains(prop))
                    {
                        Logger.AddLogEntry(LogCategory.ERROR, String.Format("LoadTimeTRiggerClipsFromDB: {0} not found.", prop));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        timeTriggerClips.Add(new AudioClip()
                        {
                            Category = dr["Category"].ToString(),
                            Label = dr["Label"].ToString(),
                            ButtonColour = dr["ButtonColour"].ToString(),
                            StateText = dr["StatementText"].ToString(),
                            StateAudioFile = dr["StatementAudioFile"].ToString(),
                        });
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
    }
}
