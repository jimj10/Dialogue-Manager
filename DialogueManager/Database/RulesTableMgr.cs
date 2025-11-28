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
    static class RulesTableMgr
    {
        internal static bool CreateRulesetRulesDBTable()
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "DROP TABLE IF EXISTS[RULESET_RULES];" +
                        "CREATE TABLE [RULESET_RULES] (" +
                        "[RulesetId] INTEGER NOT NULL, " +
                        "[RuleNumber] INTEGER NOT NULL, " +
                        "[DeviceName] TEXT NOT NULL, " +
                        "[TriggerLabel] TEXT, " +
                        "[ActionLabel] TEXT, " +
                        "CONSTRAINT[RULESET_RULES] PRIMARY KEY(RulesetId, RuleNumber));";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddRule(int rulesetId, DeviceRule rule)
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
                        cmd.CommandText = "INSERT INTO [RULESET_RULES] ([RulesetId], [RuleNumber], " +
                            "[DeviceName], [TriggerLabel], [ActionLabel]) " +
                            "VALUES(@rulesetId, @ruleNumber, @deviceName, @triggerLabel, @actionLabel);";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = rulesetId });
                        cmd.Parameters.Add(new SQLiteParameter("@ruleNumber", DbType.Int32) { Value = rule.RuleNumber });
                        cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = rule.DeviceName });
                        if (rule.TriggerClip != null)
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@triggerLabel", DbType.String) { Value = rule.TriggerClip.Label });
                        }
                        else
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@triggerLabel", DbType.String) { Value = null });
                        }

                        if (rule.ActionClip != null)
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@actionLabel", DbType.String) { Value = rule.ActionClip.Label });
                        }
                        else
                        {
                            cmd.Parameters.Add(new SQLiteParameter("@actionLabel", DbType.String) { Value = null });
                        }

                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static bool DeleteRule(int rulesetId, int ruleNumber)
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
                        cmd.CommandText = "DELETE FROM [RULESET_RULES] WHERE [RulesetId] = @rulesetId AND [RuleNumber] = @ruleNumber";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = rulesetId });
                        cmd.Parameters.Add(new SQLiteParameter("@ruleNumber", DbType.Int32) { Value = ruleNumber });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static bool UpdateRules(int rulesetId, List<DeviceRule> activeRules)
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
                        cmd.CommandText = "DELETE FROM [RULESET_RULES] WHERE [RulesetId] = @rulesetId";
                        cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = rulesetId });
                        updatedRows += cmd.ExecuteNonQuery();
                        foreach (var rule in activeRules)
                        {
                            cmd.CommandText = "INSERT INTO [RULESET_RULES] ([RulesetId], [RuleNumber], " +
                            "[DeviceName], [TriggerLabel], [ActionLabel]) " +
                            "VALUES(@rulesetId, @ruleNumber, @deviceName, @triggerLabel, @actionLabel);";
                            cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = rulesetId });
                            cmd.Parameters.Add(new SQLiteParameter("@ruleNumber", DbType.Int32) { Value = rule.RuleNumber });
                            cmd.Parameters.Add(new SQLiteParameter("@deviceName", DbType.String) { Value = rule.DeviceName });
                            if (rule.TriggerClip != null)
                            {
                                cmd.Parameters.Add(new SQLiteParameter("@triggerLabel", DbType.String) { Value = rule.TriggerClip.Label });
                            }
                            else
                            {
                                cmd.Parameters.Add(new SQLiteParameter("@triggerLabel", DbType.String) { Value = null });
                            }

                            if (rule.ActionClip != null)
                            {
                                cmd.Parameters.Add(new SQLiteParameter("@actionLabel", DbType.String) { Value = rule.ActionClip.Label });
                            }
                            else
                            {
                                cmd.Parameters.Add(new SQLiteParameter("@actionLabel", DbType.String) { Value = null });
                            }

                            updatedRows += cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static DataTable GetRulesetRules(int rulesetId)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("RULESET_RULES"))
                {
                    return null;
                }

                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM [RULESET_RULES] WHERE [RulesetId] = @rulesetId";
                    cmd.Parameters.Add(new SQLiteParameter("@rulesetId", DbType.Int32) { Value = rulesetId });
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

        internal static bool LoadRulesFromDB(int rulesetId, string deviceName, List<DeviceRule> rules)
        {
            DataTable rulesTable = GetRulesetRules(rulesetId);
            string[] propsList = { "RuleNumber", "TriggerLabel", "ActionLabel" };
            if (rulesTable != null)
            {
                bool allOK = true;
                foreach (var prop in propsList)
                {
                    if (!rulesTable.Columns.Contains(prop))
                    {
                        Logger.AddLogEntry(LogCategory.ERROR,
                            String.Format("LoadActiveRulesFromDB: {0} not found.", prop));
                        allOK = false;
                    }
                }
                if (allOK)
                {
                    foreach (DataRow dr in rulesTable.Rows)
                    {
                        string ruleNumberStr = dr["RuleNumber"].ToString();
                        string actionLabel = dr["ActionLabel"].ToString();
                        string triggerLabel = dr["TriggerLabel"].ToString();
                        if (Int32.TryParse(ruleNumberStr, out int ruleNumber))
                        {
                            DeviceRule rule = new DeviceRule
                            {
                                RuleNumber = ruleNumber,
                                DeviceName = deviceName
                            };
                            if (triggerLabel != null)
                            {
                                rule.TriggerClip = AudioClipsMgr.GetAudioClip(triggerLabel);
                            }

                            if (rule.TriggerClip == null) // if null, it's a TimeTrigger
                            {
                                rule.TriggerClip = AudioClipsMgr.GetTimeTriggerClip(triggerLabel);
                            }

                            if (actionLabel != null)
                            {
                                rule.ActionClip = AudioClipsMgr.GetAudioClip(actionLabel);
                            }

                            rules.Add(rule);
                        }
                        else
                        {
                            Logger.AddLogEntry(LogCategory.ERROR,
                                String.Format("LoadActiveRulesFromDB: Could not parse entry for ruleNumber {0}", ruleNumberStr));
                        }
                    }
                    Logger.AddLogEntry(LogCategory.INFO, "Active Rules Loaded");
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
