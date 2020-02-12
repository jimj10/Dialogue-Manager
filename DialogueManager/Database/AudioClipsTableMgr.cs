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
using System.Diagnostics;

namespace DialogueManager.Database
{
    static class AudioClipsTableMgr
    {
        internal static bool CreateAudioClipsDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[AUDIOCLIPS];" +
                        "CREATE TABLE [AUDIOCLIPS] (" +
                        "[ClipId] INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "[Category] TEXT NOT NULL, " +
                        "[Label] TEXT NOT NULL, " +
                        "[DeviceName] TEXT, " +
                        "[ButtonColour] TEXT NOT NULL, " +
                        "[IsVisible] INT NOT NULL, " +
                        "[StatementText] TEXT, " +
                        "[ConfirmationText] TEXT, " +
                        "[CheckText] TEXT, " +
                        "[StatementAudioFile] TEXT, " +
                        "[ConfirmationAudioFile] TEXT, " +
                        "[CheckAudioFile] TEXT, " +
                        "[Tooltip] TEXT);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddAudioClip(AudioClip audioClip)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [AUDIOCLIPS] ([Category], [Label], [DeviceName], [ButtonColour], [IsVisible], [StatementText], " +
                        "[ConfirmationText], [CheckText], [StatementAudioFile], [ConfirmationAudioFile], [CheckAudioFile], [Tooltip]) " +
                        "VALUES(@category, @label, @deviceName, @buttonColour, @isVisible, @statementText, @confirmationText, @checkText, " +
                        "@statementAudioFile, @confirmationAudioFile, @checkAudioFile, @tooltip);";
                    cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = audioClip.Category });
                    cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = audioClip.Label });
                    cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = audioClip.DeviceName });
                    cmd.Parameters.Add(new SQLiteParameter("@buttonColour", DbType.String) { Value = audioClip.ButtonColour });
                    cmd.Parameters.Add(new SQLiteParameter("@isVisible", DbType.Int32) { Value = audioClip.IsVisible ? 1 : 0 });
                    cmd.Parameters.Add(new SQLiteParameter("@statementText", DbType.String) { Value = audioClip.StateText });
                    cmd.Parameters.Add(new SQLiteParameter("@confirmationText", DbType.String) { Value = audioClip.ConfirmText });
                    cmd.Parameters.Add(new SQLiteParameter("@checkText", DbType.String) { Value = audioClip.CheckText });
                    cmd.Parameters.Add(new SQLiteParameter("@statementAudioFile", DbType.String) { Value = audioClip.StateAudioFile });
                    cmd.Parameters.Add(new SQLiteParameter("@confirmationAudioFile", DbType.String) { Value = audioClip.ConfirmAudioFile });
                    cmd.Parameters.Add(new SQLiteParameter("@checkAudioFile", DbType.String) { Value = audioClip.CheckAudioFile });
                    cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = audioClip.Tooltip });
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }

        internal static bool DeleteAudioClip(string label)
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
                        cmd.CommandText = "DELETE FROM AUDIOCLIPS WHERE [Label] = @label;";
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = label });
                        updatedRows = cmd.ExecuteNonQuery();
                    }
                    trans.Commit();
                }
                return updatedRows == 1;
            }
        }

        internal static int GetClipId(string label)
        {
            DataTable audioClipsTable = null;
            lock (DBAdmin.padlock)
            {
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        cmd.CommandText = "SELECT ClipId FROM [AUDIOCLIPS] WHERE [Label] = @label";
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = label });
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                using (audioClipsTable = new DataTable())
                                {
                                    audioClipsTable.Load(reader);
                                }
                            }
                        }
                    }
                }
            }
            if (audioClipsTable != null)
            {
                string clipIdstr = audioClipsTable.Rows[0]["ClipId"].ToString();
                if (Int32.TryParse(clipIdstr, out int clipId))
                {
                    return clipId;
                }
            }
            return -1;
        }

        internal static bool UpdateAudioClips(List<AudioClip> audioClips)
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
                        cmd.CommandText = "DELETE FROM [AUDIOCLIPS];";
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var audioClip in audioClips)
                        {
                            cmd.CommandText = "INSERT INTO [AUDIOCLIPS] ([Category], [Label], [DeviceName], [ButtonColour], [IsVisible], [StatementText], " +
                                "[ConfirmationText], [CheckText], [StatementAudioFile], [ConfirmationAudioFile], [CheckAudioFile], [Tooltip]) " +
                                "VALUES(@category, @label, @deviceName, @buttonColour, @isVisible, @statementText, @confirmationText, @checkText, " +
                                "@statementAudioFile, @confirmationAudioFile, @checkAudioFile, @tooltip);";
                            cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = audioClip.Category });
                            cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = audioClip.Label });
                            cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = audioClip.DeviceName });
                            cmd.Parameters.Add(new SQLiteParameter("@buttonColour", DbType.String) { Value = audioClip.ButtonColour });
                            cmd.Parameters.Add(new SQLiteParameter("@isVisible", DbType.Int32) { Value = audioClip.IsVisible ? 1 : 0 });
                            cmd.Parameters.Add(new SQLiteParameter("@statementText", DbType.String) { Value = audioClip.StateText });
                            cmd.Parameters.Add(new SQLiteParameter("@confirmationText", DbType.String) { Value = audioClip.ConfirmText });
                            cmd.Parameters.Add(new SQLiteParameter("@checkText", DbType.String) { Value = audioClip.CheckText });
                            cmd.Parameters.Add(new SQLiteParameter("@statementAudioFile", DbType.String) { Value = audioClip.StateAudioFile });
                            cmd.Parameters.Add(new SQLiteParameter("@confirmationAudioFile", DbType.String) { Value = audioClip.ConfirmAudioFile });
                            cmd.Parameters.Add(new SQLiteParameter("@checkAudioFile", DbType.String) { Value = audioClip.CheckAudioFile });
                            cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = audioClip.Tooltip });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == audioClips.Count;
            }
        }

        internal static bool UpdateAudioClip(AudioClip audioClip)
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
                        cmd.CommandText = "DELETE FROM [AUDIOCLIPS];";
                        cmd.CommandText = "UPDATE [AUDIOCLIPS] SET Category=@category, Label=@label, " +
                            "DeviceName=@deviceName, ButtonColour=@buttonColour, IsVisible=@isVisible, StatementText=@statementText, " +
                            "ConfirmationText=@confirmationText, CheckText=@checkText, StatementAudioFile=@statementAudioFile, " +
                            "ConfirmationAudioFile=@confirmationAudioFile, CheckAudioFile=@checkAudioFile, Tooltip=@tooltip " +
                            "WHERE ClipId=@clipId;";
                        cmd.Parameters.Add(new SQLiteParameter("@clipId", DbType.Int64) { Value = audioClip.ClipId });
                        cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = audioClip.Category });
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = audioClip.Label });
                        cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = audioClip.DeviceName });
                        cmd.Parameters.Add(new SQLiteParameter("@buttonColour", DbType.String) { Value = audioClip.ButtonColour });
                        cmd.Parameters.Add(new SQLiteParameter("@isVisible", DbType.Int32) { Value = audioClip.IsVisible ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@statementText", DbType.String) { Value = audioClip.StateText });
                        cmd.Parameters.Add(new SQLiteParameter("@confirmationText", DbType.String) { Value = audioClip.ConfirmText });
                        cmd.Parameters.Add(new SQLiteParameter("@checkText", DbType.String) { Value = audioClip.CheckText });
                        cmd.Parameters.Add(new SQLiteParameter("@statementAudioFile", DbType.String) { Value = audioClip.StateAudioFile });
                        cmd.Parameters.Add(new SQLiteParameter("@confirmationAudioFile", DbType.String) { Value = audioClip.ConfirmAudioFile });
                        cmd.Parameters.Add(new SQLiteParameter("@checkAudioFile", DbType.String) { Value = audioClip.CheckAudioFile });
                        cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = audioClip.Tooltip });
                        updatedRows += cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        public static bool LoadAudioClipsFromDB(List<AudioClip> audioClips)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                string query;
                if (DBAdmin.TableExists("AUDIOCLIPS"))
                {
                    query = "SELECT * FROM AUDIOCLIPS";
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
                audioClips.Clear();
                string[] propsList = { "ClipId", "Category", "Label", "DeviceName", "ButtonColour", "IsVisible", 
                    "StatementText", "ConfirmationText", "CheckText", "StatementAudioFile", "ConfirmationAudioFile", 
                    "CheckAudioFile", "Tooltip" };
                bool allOK = true;
                foreach (var prop in propsList)
                {
                    if (!dataTable.Columns.Contains(prop))
                    {
                        Logger.AddLogEntry(LogCategory.ERROR, String.Format("LoadAudioClipsFromDB: {0} not found.", prop));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    foreach (DataRow dr in dataTable.Rows)
                    {
                        string clipIdstr = dr["ClipId"].ToString();
                        string isVisiblestr = dr["IsVisible"].ToString();
                        if (Int32.TryParse(clipIdstr, out int clipId) && Int32.TryParse(isVisiblestr, out int isVisible))
                        {
                            audioClips.Add(new AudioClip()
                            {
                                ClipId = clipId,
                                Category = dr["Category"].ToString(),
                                Label = dr["Label"].ToString(),
                                DeviceName = dr["DeviceName"].ToString(),
                                ButtonColour = dr["ButtonColour"].ToString(),
                                IsVisible = isVisible == 1,
                                StateText = dr["StatementText"].ToString(),
                                ConfirmText = dr["ConfirmationText"].ToString(),
                                CheckText = dr["CheckText"].ToString(),
                                StateAudioFile = dr["StatementAudioFile"].ToString(),
                                ConfirmAudioFile = dr["ConfirmationAudioFile"].ToString(),
                                CheckAudioFile = dr["CheckAudioFile"].ToString(),
                                Tooltip = dr["Tooltip"].ToString(),
                            });
                        }
                    }
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
    }
}
