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
using System;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class RulesetsTableMgr
    {
        internal static bool CreateRulesetsDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[RULESETS];" +
                        "CREATE TABLE [RULESETS] (" +
                        "[RulesetName] TEXT NOT NULL, " +
                        "[RulesetId] INTEGER NOT NULL," +
                        "[DeviceName] TEXT NOT NULL, " +
                        "[CheckDuplicates] INTEGER NOT NULL," +
                        "[CheckConflicts] INTEGER NOT NULL," +
                        "CONSTRAINT[PK_RULESETS] PRIMARY KEY(RulesetId));";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddRuleset(Ruleset ruleset)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [RULESETS] ([RulesetId], [RulesetName], [DeviceName], [CheckDuplicates], [CheckConflicts]) " +
                            "VALUES(@rulesetId, @rulesetName, @deviceName, @checkDuplicates, @checkConflicts); ";
                    cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = ruleset.RulesetId });
                    cmd.Parameters.Add(new SQLiteParameter("@rulesetName", DbType.String) { Value = ruleset.RulesetName });
                    cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = ruleset.DeviceName });
                    cmd.Parameters.Add(new SQLiteParameter("@checkDuplicates", DbType.Int32) { Value = ruleset.CheckDuplicates ? 1 : 0 });
                    cmd.Parameters.Add(new SQLiteParameter("@checkConflicts", DbType.Int32) { Value = ruleset.CheckConflicts ? 1 : 0 });
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }

        internal static bool DeleteRuleset(int rulesetId)
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
                        cmd.CommandText = "DELETE FROM RULESETS WHERE [RulesetId] = @rulesetId";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.String) { Value = rulesetId });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateRuleset(Ruleset ruleset)
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
                        cmd.CommandText = "DELETE FROM [RULESETS] WHERE [RulesetId] = @rulesetId";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = ruleset.RulesetId });
                        updatedRows += cmd.ExecuteNonQuery();
                        cmd.CommandText = "INSERT INTO [RULESETS] ([RulesetId], [RulesetName], [DeviceName], [CheckDuplicates], [CheckConflicts]) " +
                            "VALUES(@rulesetId, @rulesetName, @deviceName, @checkDuplicates, @checkConflicts); ";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = ruleset.RulesetId });
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetName", DbType.String) { Value = ruleset.RulesetName });
                        cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = ruleset.DeviceName });
                        cmd.Parameters.Add(new SQLiteParameter("@checkDuplicates", DbType.Int32) { Value = ruleset.CheckDuplicates ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@checkConflicts", DbType.Int32) { Value = ruleset.CheckConflicts ? 1 : 0 });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        public static bool LoadRulesetFromDB(Session session)
        {
            DataTable ruleSetTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("RULESETS"))
                    return false;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM RULESETS";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            using (var tempDataTable = new DataTable())
                            {
                                tempDataTable.Load(reader);
                                ruleSetTable = tempDataTable;
                            }
                        }
                    }
                }
            }
            if (ruleSetTable != null)
            {
                foreach (DataRow dr in ruleSetTable.Rows)
                {
                    string rulesetName = dr["RulesetName"].ToString();
                    if (rulesetName.Equals(session.SessionName))
                    {
                        string rulesetIdstr = dr["RulesetId"].ToString();
                        string duplicatesCheckstr = dr["CheckDuplicates"].ToString();
                        string conflictsCheckstr = dr["CheckConflicts"].ToString();
                        if (Int32.TryParse(rulesetIdstr, out int rulesetId)
                            && Int32.TryParse(duplicatesCheckstr, out int duplicatesCheck)
                            && Int32.TryParse(conflictsCheckstr, out int conflictsCheck))
                        {
                            string deviceName = dr["DeviceName"].ToString();
                            var ruleset = new Ruleset(session, deviceName, rulesetId)
                            {
                                CheckDuplicates = duplicatesCheck == 1,
                                CheckConflicts = conflictsCheck == 1
                            };
                            ruleset.LoadRulesFromDB();
                            session.Ruleset = ruleset;
                        }
                        break;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
