/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using DialogueManager.Models;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class SettingsTableMgr
    {
        internal static bool CreateSettingsDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[SETTINGS];" +
                        "CREATE TABLE [SETTINGS] (" +
                        "[CredentialsFile] TEXT, " +
                        "[LanguageCode] TEXT, " +
                        "[OnlineVoice] TEXT, " +
                        "[CheckAudioFiles] INTEGER NOT NULL, " +
                        "[MaxLogEntries] INTEGER NOT NULL);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool SaveSettingsToDB()
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
                        cmd.CommandText = "DELETE FROM SETTINGS;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "INSERT INTO [SETTINGS] (" +
                            "[CredentialsFile], [LanguageCode], [OnlineVoice], [CheckAudioFiles], MaxLogEntries) " +
                            "VALUES(@credentialsFile, @languageCode, @onlineVoice, @checkAudioFiles, @maxLogEntries);";
                        cmd.Parameters.Add(new SQLiteParameter("@credentialsFile", DbType.String) { Value = Settings.CredentialsFile });
                        cmd.Parameters.Add(new SQLiteParameter("@languageCode", DbType.String) { Value = Settings.LanguageCode });
                        cmd.Parameters.Add(new SQLiteParameter("@onlineVoice", DbType.String) { Value = Settings.OnlineVoice });
                        cmd.Parameters.Add(new SQLiteParameter("@checkAudioFiles", SqlDbType.Int) { Value = Settings.CheckAudioFiles ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@maxLogEntries", SqlDbType.Int) { Value = Settings.MaxLogEntries });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static DataTable GetSettings()
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                string query;
                if (DBAdmin.TableExists("SETTINGS"))
                {
                    query = "SELECT * FROM SETTINGS";
                }
                else
                {
                    return null;
                }

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
