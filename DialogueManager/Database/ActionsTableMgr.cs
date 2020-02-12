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
    static class ActionsTableMgr
    {
        internal static bool CreateRulesDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[ACTIONS];" +
                        "CREATE TABLE [ACTIONS] (" +
                        "[ActionId] INTEGER PRIMARY KEY, " +
                        "[DeviceName] TEXT NOT NULL, " +
                        "[Category] TEXT NOT NULL, " +
                        "[Label] TEXT NOT NULL, " +
                        "[ActionText] TEXT NOT NULL, " +
                        "[Tooltip] TEXT);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            if (updatedRows == 0)
                return true;
            return false;
        }

        internal static bool AddRule(DeviceAction action)
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
                        cmd.CommandText = "INSERT INTO [ACTIONS] " +
                            "([DeviceName], [Category], [Label], [ActionText], [Tooltip]) " +
                            "VALUES(@deviceName, @category, @label, @actionText, @tooltip);";
                        cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = action.DeviceName });
                        cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = action.Category });
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = action.Label });
                        cmd.Parameters.Add(new SQLiteParameter("@actionText", DbType.String) { Value = action.ActionText });
                        cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = action.Tooltip });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateActions(List<DeviceAction> actions)
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
                        cmd.CommandText = "DELETE FROM [ACTIONS];";
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var action in actions)
                        {
                            cmd.CommandText = "INSERT INTO [ACTIONS] " +
                                "([DeviceName], [Category], [Label], [ActionText], [Tooltip]) " +
                                "VALUES(@deviceName, @category, @label, @actionText, @tooltip);";
                            cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = action.DeviceName });
                            cmd.Parameters.Add(new SQLiteParameter("@category", DbType.String) { Value = action.Category });
                            cmd.Parameters.Add(new SQLiteParameter("@label", DbType.String) { Value = action.Label });
                            cmd.Parameters.Add(new SQLiteParameter("@actionText", DbType.String) { Value = action.ActionText });
                            cmd.Parameters.Add(new SQLiteParameter("@tooltip", DbType.String) { Value = action.Tooltip });
                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == actions.Count;
            }
        }

        internal static bool DeleteRule(string label)
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
                        cmd.CommandText = "DELETE FROM ACTIONS WHERE [Label] = @label";
                        cmd.Parameters.Add(new SQLiteParameter("@label", DbType.Int32) { Value = label });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static DataTable GetActions()
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("ACTIONS"))
                    return null;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM ACTIONS";
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
