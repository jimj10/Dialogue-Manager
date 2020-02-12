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
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace DialogueManager.Database
{
    static class SessionsTableMgr
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
                    cmd.CommandText = "DROP TABLE IF EXISTS[SESSIONS];" +
                        "CREATE TABLE [SESSIONS] (" +
                        "[SessionId] INTEGER PRIMARY KEY AUTOINCREMENT," + 
                        "[SessionName] TEXT NOT NULL, " +
                        "[IsRuleset] INT NOT NULL," +
                        "[EnableCastDisplay] INTEGER NOT NULL," +
                        "[KeepAlive] INT NOT NULL," +
                        "[SpeedRatio] REAL NOT NULL);";
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 0;
        }

        internal static bool AddSession(Session session)
        {
            int updatedRows = 0;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    SQLiteTransaction trans = dbConnection.BeginTransaction();
                    cmd.CommandText = "INSERT INTO [SESSIONS] ([SessionName], [IsRuleset], " +
                            "[EnableCastDisplay], [KeepAlive], [SpeedRatio]) " +
                            "VALUES(@sessionName, @isRuleset, @enableCastDisplay, @keepAlive, @speedRatio ); ";
                    cmd.Parameters.Add(new SQLiteParameter("@sessionName", DbType.String) { Value = session.SessionName });
                    cmd.Parameters.Add(new SQLiteParameter("@isRuleset", DbType.Int32) { Value = session.IsRuleset ? 1 : 0 });
                    cmd.Parameters.Add(new SQLiteParameter("@enableCastDisplay", DbType.Int32) { Value = session.CastDisplayEnabled ? 1 : 0 });
                    cmd.Parameters.Add(new SQLiteParameter("@keepAlive", DbType.Int32) { Value = session.KeepAlive ? 1 : 0 });
                    cmd.Parameters.Add(new SQLiteParameter("@speedRatio", DbType.Double) { Value = session.SpeedRatio });
                    updatedRows = cmd.ExecuteNonQuery();
                    trans.Commit();
                }
            }
            return updatedRows == 1;
        }

        internal static bool DeleteSession(int sessionId)
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
                        cmd.CommandText = "DELETE FROM SESSIONS WHERE [SessionId] = @sessionId";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.String) { Value = sessionId });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static int GetSessionId(string sessionName)
        {
            DataTable sessionTable = null;
            lock (DBAdmin.padlock)
            {
                using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
                {
                    dbConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                    {
                        cmd.CommandText = "SELECT SessionId FROM [SESSIONS] WHERE [SessionName] = @sessionName";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionName", DbType.String) { Value = sessionName });
                        using (SQLiteDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                using (sessionTable = new DataTable())
                                {
                                    sessionTable.Load(reader);
                                }
                            }
                        }
                    }
                }
            }
            if (sessionTable != null)
            {
                string sessionIdstr = sessionTable.Rows[0]["SessionId"].ToString();
                if (Int32.TryParse(sessionIdstr, out int sessionId))
                {
                    return sessionId;
                }
            }
            return -1;
        }


        internal static bool UpdateSession(Session session)
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
                        cmd.CommandText = "UPDATE [SESSIONS] SET SessionName=@sessionName, IsRuleset=@isRuleset, " +
                            "EnableCastDisplay=@enableCastDisplay, KeepAlive=@keepAlive, SpeedRatio=@speedRatio " +
                            "WHERE SessionId=@sessionId;";
                        cmd.Parameters.Add(new SQLiteParameter("@sessionId", DbType.Int64) { Value = session.SessionId });
                        cmd.Parameters.Add(new SQLiteParameter("@sessionName", DbType.String) { Value = session.SessionName });
                        cmd.Parameters.Add(new SQLiteParameter("@isRuleset", DbType.Int32) { Value = session.IsRuleset ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@enableCastDisplay", DbType.Int32) { Value = session.CastDisplayEnabled ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@keepAlive", DbType.Int32) { Value = session.KeepAlive ? 1 : 0 });
                        cmd.Parameters.Add(new SQLiteParameter("@speedRatio", DbType.Double) { Value = session.SpeedRatio });
                        updatedRows = cmd.ExecuteNonQuery();
                        trans.Commit();
                    }
                }
                return updatedRows == 1;
            }
        }

        internal static DataTable GetSessions()
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("SESSIONS"))
                    return null;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM SESSIONS";
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

        internal static DataTable GetSession(string sessionName)
        {
            DataTable dataTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("SESSIONS"))
                    return null;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM SESSIONS WHERE [SessionName] = @sessionName";
                    cmd.Parameters.Add(new SQLiteParameter("@sessionName", DbType.String) { Value = sessionName });
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

        public static bool LoadSessionsFromDB(List<Session> sessions)
        {
            DataTable sessionsTable = null;
            using (SQLiteConnection dbConnection = DBAdmin.GetSQLConnection())
            {
                if (!DBAdmin.TableExists("SESSIONS"))
                    return false;
                dbConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(dbConnection))
                {
                    cmd.CommandText = "SELECT * FROM SESSIONS";
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            using (sessionsTable = new DataTable())
                            {
                                sessionsTable.Load(reader);
                            }   
                        }
                    }
                }
            }
            if (sessionsTable != null)
            {
                foreach (DataRow dr in sessionsTable.Rows)
                {
                    string sessionIdstr = dr["SessionId"].ToString();
                    string enableCastDisplaystr = dr["EnableCastDisplay"].ToString();
                    string isRulesetStr = dr["IsRuleset"].ToString();
                    string keepAliveStr = dr["KeepAlive"].ToString();
                    string speedRatioStr = dr["SpeedRatio"].ToString();
                    
                    if (Int32.TryParse(sessionIdstr, out int sessionId)
                        && Int32.TryParse(isRulesetStr, out int isRuleset)
                        && Int32.TryParse(enableCastDisplaystr, out int enableCastDisplay)
                        && Int32.TryParse(keepAliveStr, out int keepAlive)
                        && Double.TryParse(speedRatioStr, out double speedRatio))
                    {
                        string sessionName = dr["SessionName"].ToString();
                        var session = new Session()
                        {
                            SessionId = sessionId,
                            SessionName = sessionName,
                            CastDisplayEnabled = enableCastDisplay == 1,
                            KeepAlive = keepAlive == 1,
                            SpeedRatio = speedRatio,
                            IsRuleset = isRuleset == 1
                        };
                        SessionClipsTableMgr.LoadAudioClipsListFromDB(session.SessionId, session.SessionAudioClipsList);
                        if (isRuleset == 1)
                            RulesetsTableMgr.LoadRulesetFromDB(session);
                        sessions.Add(session);
                    }
                }
                return true;
            }
            return false;
        }
    }
}
