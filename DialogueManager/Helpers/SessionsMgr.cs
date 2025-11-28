/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using DialogueManager.Database;
using DialogueManager.EventLog;
using DialogueManager.Models;
using System.Collections.Generic;
using System.Linq;

namespace DialogueManager
{
    static class SessionsMgr
    {
        private static List<Session> Sessions = new List<Session>();

        public static bool SessionsLoaded { get; set; } = false;

        public static int AddSession(string sessionName, Ruleset ruleset)
        {
            if (Sessions.FirstOrDefault(x => x.SessionName.Equals(sessionName)) == null)
            {
                var newSession = new Session()
                {
                    SessionName = sessionName,
                    Ruleset = ruleset
                };
                Sessions.Add(newSession);
                // update database
                if (SessionsTableMgr.AddSession(newSession))
                {
                    newSession.SessionId = SessionsTableMgr.GetSessionId(sessionName);
                    SessionsTableMgr.UpdateSession(newSession);
                    Logger.AddLogEntry(LogCategory.INFO, "Added Session: " + sessionName);
                    return newSession.SessionId;
                }
                else
                {
                    return -1;
                }
            }
            return -2; // session name already exits
        }

        public static Session GetSession(string sessionName)
        {
            return Sessions.FirstOrDefault(x => x.SessionName.Equals(sessionName));
        }

        public static Session GetSessionCopy(string sessionName)
        {
            var original = Sessions.FirstOrDefault(x => x.SessionName.Equals(sessionName));
            return original == null ? null : SessionCopy(original);
        }

        public static Session GetSessionCopy(int sessionId)
        {
            var original = Sessions.FirstOrDefault(x => x.SessionId.Equals(sessionId));
            return original == null ? null : SessionCopy(original);
        }

        private static Session SessionCopy(Session original)
        {
            return new Session()
            {
                SessionId = original.SessionId,
                SessionName = original.SessionName,
                SessionAudioClipsList = original.SessionAudioClipsList,
                IsRuleset = original.IsRuleset,
                Ruleset = original.Ruleset,
                CastDisplayEnabled = original.CastDisplayEnabled,
                SpeedRatio = original.SpeedRatio,
                KeepAlive = original.KeepAlive
            };
        }
        public static int UpdateSession(Session sessionCopy)
        {
            var originalSession = Sessions.FirstOrDefault(x => x.SessionId.Equals(sessionCopy.SessionId));
            if (!originalSession.SessionName.Equals(sessionCopy.SessionName))
            {
                if (Sessions.FirstOrDefault(x => x.SessionName.Equals(sessionCopy.SessionName)) != null)
                {
                    return -1; // name already exists
                }
            }
            var index = Sessions.FindIndex(c => c.SessionId == originalSession.SessionId);
            Sessions[index] = sessionCopy;
            if (SessionsTableMgr.UpdateSession(sessionCopy))
            {
                if (!SessionClipsTableMgr.UpdateSessionClips(sessionCopy.SessionId, sessionCopy.SessionAudioClipsList))
                {
                    return -3;
                }
            }
            else
            {
                return -2;
            }

            return sessionCopy.SessionId;
        }

        public static Session GetSession(int sessionId)
        {
            return Sessions.FirstOrDefault(x => x.SessionId.Equals(sessionId));
        }

        public static void DeleteClipFromAllSessions(int clipId)
        {
            foreach (var session in Sessions)
            {
                var audioClip = session.SessionAudioClips.FirstOrDefault(x => x.ClipId.Equals(clipId));
                if (audioClip != null)
                {
                    session.SessionAudioClips.Remove(audioClip);
                    session.SessionAudioClipsList.Remove(clipId);
                    SessionClipsTableMgr.DeleteAudioClip(session.SessionId, clipId);
                    EventSystem.Publish(new SessionInventoryChanged() { SessionName = session.SessionName });
                }
            }
        }

        public static bool DeleteSession(string sessionName)
        {
            var session = Sessions.FirstOrDefault(x => x.SessionName.Equals(sessionName));
            if (session != null)
            {
                Sessions.Remove(session);
                if (SessionsTableMgr.DeleteSession(session.SessionId))
                {
                    Logger.AddLogEntry(LogCategory.INFO, "Deleted Session: " + sessionName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static List<string> GetSessionNames()
        {
            List<string> names = new List<string>();
            foreach (var session in Sessions)
            {
                names.Add(session.SessionName);
            }
            return names;
        }

        public static bool LoadSessionsFromDB()
        {
            Sessions.Clear();
            if (SessionsTableMgr.LoadSessionsFromDB(Sessions))
            {
                SessionsLoaded = true;
                EventSystem.Publish<SessionsLoaded>(new SessionsLoaded { });
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
