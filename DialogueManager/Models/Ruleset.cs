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
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace DialogueManager.Models
{

    public class Ruleset
    {
        public int RulesetId { get; set; }

        public string RulesetName { get; set; }

        public string DeviceName { get; set; }

        public List<string> Devices { get; set; } = new List<string>();

        public List<DeviceRule> Rules { get; }

        public List<AudioClip> TriggerClips { get; }

        public List<AudioClip> AudioClips { get; set; }

        public List<AudioClip> ActionClips { get; }

        private bool checkDuplicates;
        public bool CheckDuplicates {
            get { return checkDuplicates; }
            set {
                if (checkDuplicates != value)
                {
                    checkDuplicates = value;
                    RulesetsTableMgr.UpdateRuleset(this);
                }
            }
        }

        private bool checkConflicts;
        public bool CheckConflicts {
            get { return checkConflicts; }
            set {
                if (checkConflicts != value)
                {
                    checkConflicts = value;
                    RulesetsTableMgr.UpdateRuleset(this);
                }
            }
        }

        public Ruleset(Session session, string deviceName, int rulesetId)
        {
            RulesetName = session.SessionName;
            DeviceName = deviceName;
            Devices.Add(deviceName);
            RulesetId = rulesetId;
            AudioClips = session.SessionAudioClips;
            ActionClips = new List<AudioClip>(AudioClipsMgr.GetAudioClips(DeviceName, "Action"));
            TriggerClips = new List<AudioClip>(AudioClipsMgr.GetAudioClips(DeviceName, "Trigger"));
            Rules = new List<DeviceRule>();
            UpdateListRulesAudioClip();
            UpdateWhichRuleAudioClip();
        }

        public bool AddRule(DeviceRule rule)
        {
            if (rule.TriggerClip != null && rule.ActionClip != null)
            {
                var existingRule = Rules.Where((x) =>
                    x.TriggerClip.StateText.Equals(rule.TriggerClip.StateText)
                    && x.ActionClip.StateText.Equals(rule.ActionClip.StateText)
                    ).FirstOrDefault();
                if (rule != null && existingRule == null)
                {
                    Rules.Add(rule);
                    rule.RuleNumber = Rules.Count;
                    UpdateListRulesAudioClip();
                    UpdateWhichRuleAudioClip();
                    return RulesTableMgr.AddRule(RulesetId, rule);
                }
            }
            return false;
        }

        public bool DeleteRule(int ruleNumber)
        {
            var ruleToDelete = Rules.Where((x) => x.RuleNumber == ruleNumber).FirstOrDefault();
            if (ruleToDelete != null)
            {
                Rules.Remove(ruleToDelete);
                RenumberRules();
                UpdateListRulesAudioClip();
                UpdateWhichRuleAudioClip();
                return RulesTableMgr.DeleteRule(RulesetId, ruleNumber);
            }
            return false;
        }

        public bool ChangeRule(int ruleNumber, DeviceRule newRule)
        {
            var ruleToChange = Rules.Where((x) => x.RuleNumber == ruleNumber).FirstOrDefault();
            if (newRule != null && ruleToChange != null)
            {
                newRule.RuleNumber = ruleNumber;
                int indx = Rules.IndexOf(ruleToChange);
                Rules[indx] = newRule;
                UpdateListRulesAudioClip();
                UpdateWhichRuleAudioClip();
                return RulesTableMgr.UpdateRules(RulesetId, Rules);
            }
            return false;
        }

        private void RenumberRules()
        {
            int ruleNumber = 1;
            foreach (var rule in Rules)
                rule.RuleNumber = ruleNumber++;
            RulesTableMgr.UpdateRules(RulesetId, Rules);
        }

        public List<string> GetRuleNumbers()
        {
            List<string> rules = new List<string>();
            rules.Add("New");
            for (int i = 1; i < Rules.Count + 1; i++)
            {
                rules.Add(i.ToString());
            }
            return rules;
        }

        public int CheckForDuplicates(DeviceRule deviceRule)
        {
            foreach (var rule in Rules)
            {
                if (rule.TriggerClip.StateText.Equals(deviceRule.TriggerClip.StateText)
                    && rule.ActionClip.StateText.Equals(deviceRule.ActionClip.StateText))
                {
                    var audioClip = AudioClips.Where((x) => x.Label.Equals("Rule is a duplicate")).FirstOrDefault();
                    if (audioClip != null)
                        RuleClipGenerator.UpdateDuplicateWarning(audioClip, rule.RuleNumber);
                    return rule.RuleNumber;
                }
            }
            return -1;
        }

        public int CheckForConflicts(DeviceRule deviceRule)
        {
            foreach (var rule in Rules)
            {
                if (rule.TriggerClip.StateText.Equals(deviceRule.TriggerClip.StateText)
                    && !rule.ActionClip.StateText.Equals(deviceRule.ActionClip.StateText))
                {
                    var audioClip = AudioClips.Where((x) => x.Label.Equals("Rule conflicts with another rule")).FirstOrDefault();
                    if (audioClip != null)
                        RuleClipGenerator.UpdateConflictWarning(audioClip, rule.RuleNumber);
                    return rule.RuleNumber;
                }
            }
            return -1;
        }

        public string GetRulesetText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DeviceName + " Rules\n");
            if (Rules.Count == 0)
                sb.Append("No rules found.");
            else
                foreach (var rule in Rules)
                    sb.Append(rule.RuleNumber.ToString() + ") " + rule.StateText + "\n");
            return sb.ToString();
        }

        public void UpdateStateSelectedRuleAudioClip(int selectedRule)
        {
            var audioClip = AudioClips.Where((x) => x.Label.Equals("State selected rule")).FirstOrDefault();
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
            if (audioClip == null)
            {
                audioClip = new AudioClip()
                {
                    Category = "Ruleset",
                    Label = "State selected rule",
                    Tooltip = "State selected rule",
                    ButtonColour = ColourHelper.StatementColour
                };
                AudioClips.Add(audioClip);
            }
            List<string> clips = new List<string>();
            StringBuilder sb = new StringBuilder();
            string ruleText = TextHelper.GetRuleNumberText(selectedRule - 1);
            sb.Append(ruleText + ": ");
            clips.Add(Path.Combine(audioDirectory, ruleText));
            sb.Append(Rules[selectedRule - 1].StateText);
            clips.Add(Rules[selectedRule - 1].TriggerClip.StateAudioFile);
            clips.Add(Rules[selectedRule - 1].ActionClip.StateAudioFile);
            audioClip.StateText = sb.ToString();
            audioClip.StateAudioFile = AudioMgr.CombineAudioClips(clips);
        }

        public void UpdateListRulesAudioClip()
        {
            var audioClip = AudioClips.Where((x) => x.Label.Equals("List rules")).FirstOrDefault();
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
            string deviceDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, DeviceName);
            if (audioClip == null)
            {
                audioClip = new AudioClip()
                {
                    Category = "Ruleset",
                    Label = "List rules",
                    Tooltip = "List rules",
                    ButtonColour = ColourHelper.StatementColour
                };
                AudioClips.Add(audioClip);
            }
            List<string> clips = new List<string>();
            StringBuilder sb = new StringBuilder();
            switch (Rules.Count)
            {
                case 0:
                    sb.Append("Currently I have no rules for the " + DeviceName);
                    clips.Add(Path.Combine(deviceDirectory, String.Format("Currently I have no rules for the {0}", DeviceName)));
                    break;
                case 1:
                    sb.Append("I have one rule for the " + DeviceName + ".\n");
                    sb.Append(Rules[0].StateText);
                    clips.Add(Path.Combine(deviceDirectory, String.Format("I have one rule for the {0}", DeviceName)));
                    clips.Add(Rules[0].TriggerClip.StateAudioFile);
                    clips.Add(Rules[0].ActionClip.StateAudioFile);
                    break;
                default:
                    string numberTxt = TextHelper.GetNumberText(Rules.Count);
                    sb.Append("I have " + numberTxt + " rules for the " + DeviceName + ".\n");
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        sb.Append(TextHelper.GetRuleNumberText(i) + ": ");
                        sb.Append(Rules[i].StateText + "\n");
                    }
                    clips.Add(Path.Combine(deviceDirectory, String.Format("I have " + numberTxt + " rules for the {0}", DeviceName)));
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        clips.Add(Path.Combine(audioDirectory, TextHelper.GetRuleNumberText(i)));
                        clips.Add(Rules[i].TriggerClip.StateAudioFile);
                        clips.Add(Rules[i].ActionClip.StateAudioFile);
                    }
                    break;
            }
            audioClip.StateText = sb.ToString();
            audioClip.StateAudioFile = AudioMgr.CombineAudioClips(clips);
        }

        public void UpdateWhichRuleAudioClip()
        {
            StringBuilder sb = new StringBuilder();
            var listRulesAudioClip = AudioClips.Where((x) => x.Label.Equals("List rules; which rule to change?")).FirstOrDefault();
            var questionOnlyAudioClip = AudioClips.Where((x) => x.Label.Equals("Which rule to change?")).FirstOrDefault();
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
            string deviceDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, DeviceName);
            if (listRulesAudioClip == null)
            {
                listRulesAudioClip = new AudioClip()
                {
                    Category = "Ruleset",
                    Label = "List rules; which rule to change?",
                    Tooltip = "List rules; which rule to change?",
                    ButtonColour = ColourHelper.QuestionColour
                };
                AudioClips.Add(listRulesAudioClip);
            }
            if (questionOnlyAudioClip == null)
            {
                questionOnlyAudioClip = new AudioClip()
                {
                    Category = "Ruleset",
                    Label = "Which rule to change?",
                    Tooltip = "Which rule to change?",
                    ButtonColour = ColourHelper.QuestionColour
                };
                AudioClips.Add(questionOnlyAudioClip);
            }
            List<string> clips = new List<string>();
            switch (Rules.Count)
            {
                case 0:
                    sb.Append("Currently I have no rules for the " + DeviceName + ".\n");
                    clips.Add(Path.Combine(deviceDirectory, String.Format("Currently I have no rules for the {0}", DeviceName)));
                    break;
                case 1:
                    sb.Append("OK, I have one rule for the " + DeviceName + ".\n");
                    sb.Append(Rules[0].StateText + "\n");
                    sb.Append("What would you like to change?");
                    clips.Add(Path.Combine(audioDirectory, "OK"));
                    clips.Add(Path.Combine(deviceDirectory, String.Format("I have one rule for the {0}", DeviceName)));
                    clips.Add(Rules[0].TriggerClip.StateAudioFile);
                    clips.Add(Rules[0].ActionClip.StateAudioFile);
                    clips.Add(Path.Combine(audioDirectory, "What would you like to change_"));
                    break;
                default:
                    string numberTxt = TextHelper.GetNumberText(Rules.Count);
                    sb.Append("I have " + numberTxt + " rules for the " + DeviceName + ".\n");
                    sb.Append("Which rule would you like to change?");
                    questionOnlyAudioClip.StateText = sb.ToString();
                    questionOnlyAudioClip.StateAudioFile = AudioMgr.CombineAudioClips(clips);
                    sb.Clear();
                    sb.Append("OK, I have " + numberTxt + " rules for the " + DeviceName + ".\n");
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        sb.Append(TextHelper.GetRuleNumberText(i) + ": ");
                        sb.Append(Rules[i].StateText + "\n");
                    }
                    sb.Append("Which rule would you like to change?");
                    clips.Add(Path.Combine(audioDirectory, "OK"));
                    clips.Add(Path.Combine(deviceDirectory, String.Format("I have " + numberTxt + " rules for the {0}", DeviceName)));
                    for (int i = 0; i < Rules.Count; i++)
                    {
                        clips.Add(Path.Combine(audioDirectory, TextHelper.GetRuleNumberText(i)));
                        clips.Add(Rules[i].TriggerClip.StateAudioFile);
                        clips.Add(Rules[i].ActionClip.StateAudioFile);
                    }
                    clips.Add(Path.Combine(audioDirectory, "Which rule would you like to change_"));
                    break;
            }
            listRulesAudioClip.StateText = sb.ToString();
            listRulesAudioClip.StateAudioFile = AudioMgr.CombineAudioClips(clips);
        }

        public void UpdateRuleDeletedAudioClip(int ruleNumber)
        {
            var audioClip = AudioClips.Where((x) => x.Label.Equals("OK, rule deleted")).FirstOrDefault();
            if (audioClip != null)
                RuleClipGenerator.UpdateRuleDeletedAudioClip(audioClip, ruleNumber);
        }

        public bool LoadRulesFromDB()
        {
            if (RulesTableMgr.LoadRulesFromDB(RulesetId, DeviceName, Rules))
            {
                UpdateListRulesAudioClip();
                UpdateWhichRuleAudioClip();
                return true;
            }
            return false;
        }
    }
}
