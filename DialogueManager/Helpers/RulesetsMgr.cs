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
using System.Collections.ObjectModel;
using System.Linq;

namespace DialogueManager
{
    internal static class RulesetsMgr
    {
        public static ObservableCollection<Ruleset> Rulesets { get; set; } = new ObservableCollection<Ruleset>();

        public static bool RulesetsLoaded { get; set; } = false;

        public static bool AddRuleset(Session session, string deviceName)
        {
            if (Rulesets.FirstOrDefault(x => x.RulesetName.Equals(session.SessionName)) == null)
            {
                var newRuleset = new Ruleset(session, deviceName, Rulesets.Count);

                Rulesets.Add(newRuleset);
                if (RulesetsTableMgr.AddRuleset(newRuleset))
                {
                    Logger.AddLogEntry(LogCategory.INFO, "Added Ruleset: " + session.SessionName);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static Ruleset GetRuleset(string rulesetName)
        {
            return Rulesets.FirstOrDefault(x => x.RulesetName.Equals(rulesetName));
        }
    }
}