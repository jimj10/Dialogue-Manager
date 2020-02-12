/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class OnlineVoicesTableMgr
    {
        internal static bool CreateOnlineVoicesDBTable()
        {
            int updatedRows = 0;
            SQLiteConnection dbConnection;
            using (dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[ONLINE_VOICES];" +
                        "CREATE TABLE [ONLINE_VOICES] (" +
                        "[Voice] TEXT NOT NULL, " +
                        "CONSTRAINT[PK_ONLINE_VOICES] PRIMARY KEY([Voice]));";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool SaveOnlineVoicesToDB(List<string> voices)
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
                        cmd.CommandText = "DELETE FROM [ONLINE_VOICES];";
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var voice in voices)
                        {
                            cmd.CommandText = "INSERT INTO [ONLINE_VOICES] ([Voice]) VALUES(@voice);";
                            cmd.Parameters.Add(new SQLiteParameter("@voice", DbType.String) { Value = voice });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == voices.Count;
            }
        }

        public static bool LoadOnlineVoicesFromDB(List<string> voices)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                string query;
                if (DBAdmin.TableExists("ONLINE_VOICES"))
                {
                    query = "SELECT * FROM ONLINE_VOICES";
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
                foreach (DataRow dr in dataTable.Rows)
                {
                    voices.Add(dr["Voice"].ToString());
                }
                return true;
            }
            else
                return false;
        }
    }
}
