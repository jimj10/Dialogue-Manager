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
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class SessionClipsTableMgr
    {

        internal static bool CreateSessionsDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[SESSION_CLIPS];" +
                        "CREATE TABLE [SESSION_CLIPS] (" +
                        "[SessionClipId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "[AudioClipId] INT NOT NULL, " +
                        "[SessionId] INT NOT NULL);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddAudioClip(int sessionId, int audioClipId)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [SESSION_CLIPS] ([SessionId], [AudioClipId]) VALUES(@sessionId, @audioClipId);";
                    cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int32) { Value = sessionId });
                    cmd.Parameters.Add(new SQLiteParameter("@audioClipId", DbType.Int32) { Value = audioClipId });
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }


        internal static bool DeleteAudioClip(int sessionId, int audioClipId)
        {
            lock (DBAdmin.padlock)
            {
                SQLiteTransaction trans = null;
                int updatedRows = 0;
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        trans = dbConnection.BeginTransaction();
                        cmd.CommandText = "DELETE FROM SESSION_CLIPS WHERE [SessionId] = @sessionId AND [AudioClipId] = @audioClipId;";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int32) { Value = sessionId });
                        cmd.Parameters.Add(new SQLiteParameter("@audioClipId", DbType.Int32) { Value = audioClipId });
                        updatedRows = cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateSessionClips(int sessionId, List<int> audioClipsList)
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
                        cmd.CommandText = "DELETE FROM [SESSION_CLIPS] WHERE [SessionId] = @sessionId;";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int32) { Value = sessionId });
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var audioClipId in audioClipsList)
                        {
                            cmd.CommandText = "INSERT INTO [SESSION_CLIPS] ([SessionId], [AudioClipId]) VALUES(@sessionId, @audioClipId);";
                            cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int32) { Value = sessionId });
                            cmd.Parameters.Add(new SQLiteParameter("@audioClipId", DbType.Int32) { Value = audioClipId });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return true;
            }
        }

        internal static void LoadAudioClipsListFromDB(int sessionId, List<int> actionClipsList)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (DBAdmin.TableExists("SESSION_CLIPS"))
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        cmd.CommandText = "SELECT * FROM SESSION_CLIPS WHERE [SessionId] = @sessionId;";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int32) { Value = sessionId });
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
                foreach (DataRow row in dataTable.Rows)
                {
                    if (Int32.TryParse(row["AudioClipId"].ToString(), out int audioClipId))
                    {
                        actionClipsList.Add(audioClipId);
                    }
                }
                Logger.AddLogEntry(LogCategory.INFO, "ActionClips Inventory Loaded");
            }
        }
    }
}
