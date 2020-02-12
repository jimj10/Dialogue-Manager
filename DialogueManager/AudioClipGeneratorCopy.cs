/* 
 * Copyright(c) 2018 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

// Generates audio clips for use in offline mode.
// Methods are called from the  'Audioclips' section of the Settings tab

using DialogueManager.EventLog;
using System;
using System.Diagnostics;
using System.IO;

namespace DialogueManager
{
    static class AudioClipGenerator
    {

        public static void GenerateRulesetAudioClips(string rulesetName)
        {
            switch (rulesetName)
            {
                case "Bedroom Light":
                    GenerateBedroomLightAudioClips();
                    break;
                case "Kitchen Radio":
                    GenerateKitchenRadioAudioClips();
                    break;
                default:
                    Logger.AddLogEntry(LogCategory.ERROR, String.Format("GenerateRulesetAudioClips: Ruleset name {0} not found.", rulesetName));
                    break;
            }

        }

        public static void GenerateTriggerAudioClips()
        {
            string audioText = String.Empty;
            string deviceName = "Triggers\\Time";
            for (int hours = 1; hours < 13; hours++)
            {
                for (int minutes = 0; minutes < 60; minutes++)
                {
                    audioText = "At " + hours.ToString() + ":" + minutes.ToString("D2") + "am today";
                    Debug.WriteLine("audioText: " + audioText);
                    TextToSpeechMgr.TextToAudiofile(audioText, deviceName);
                    TextToSpeechMgr.TextToAudiofile(audioText.Replace("today", "every day"), deviceName);
                    TextToSpeechMgr.TextToAudiofile(audioText.Replace("am", "pm"), deviceName);
                    TextToSpeechMgr.TextToAudiofile(audioText.Replace("today", "every day").Replace("am", "pm"), deviceName);
                }
            }
            deviceName = "Triggers";
            audioText = "If someone enters the room";
            TextToSpeechMgr.TextToAudiofile(audioText, deviceName);
            audioText = "If the room is empty";
            TextToSpeechMgr.TextToAudiofile(audioText, deviceName);
        }

        public static void GenerateAudioClipsFromFile(string phrasesFileName)
        {
            string deviceName = "Other";
            var lines = File.ReadLines(phrasesFileName);
            foreach (var line in lines)
            {
                TextToSpeechMgr.TextToAudiofile(line, deviceName);
            }
        }

        public static void GenerateCommonAudioClips()
        {
            string audioText = String.Empty;
            string fileName = String.Empty;
            string deviceName = "Common";
            string[] audioPhrases = new string[]
            {
              "A similar rule already exists",
              "Add new rule",
              "At what time should this happen?",
              "Change rule six",
              "Change rule five",
              "Change rule four",
              "Change rule three",
              "Change rule two",
              "Change rule one",
              "Would you like to change rule six?",
              "Would you like to change rule five?",
              "Would you like to change rule four?",
              "Would you like to change rule three?",
              "Would you like to change rule two?",
              "Would you like to change rule one?",
              "Rule six changed",
              "Rule five changed",
              "Rule four changed",
              "Rule three changed",
              "Rule two changed",
              "Rule one changed",
              "Rule six deleted",
              "Rule five deleted",
              "Rule four deleted",
              "Rule three deleted",
              "Rule two deleted",
              "Rule one deleted",
              "Rule six",
              "Rule five",
              "Rule four",
              "Rule three",
              "Rule two",
              "Rule one",
              "Do you want to delete rule six?",
              "Do you want to delete rule five?",
              "Do you want to delete rule four?",
              "Do you want to delete rule three?",
              "Do you want to delete rule two?",
              "Do you want to delete rule one?",
              "This new rule conflicts with rule six",
              "This new rule conflicts with rule five",
              "This new rule conflicts with rule four",
              "This new rule conflicts with rule three",
              "This new rule conflicts with rule two",
              "This new rule conflicts with rule one",
              "This new rule is a duplicate of rule six",
              "This new rule is a duplicate of rule five",
              "This new rule is a duplicate of rule four",
              "This new rule is a duplicate of rule three",
              "This new rule is a duplicate of rule two",
              "This new rule is a duplicate of rule one",
              "How can I help?",
              "I'm sorry, I'm not yet able to help with that",
              "I'm sorry, I don't understand",
              "Is this correct?",
              "Let me check I have understood",
              "New rule added",
              "New rule",
              "OK, new rule added",
              "OK",
              "On which day or days should this happen?",
              "Run new rule",
              "This rule is redundant",
              "What time would you like to set?",
              "What would you like me to do?",
              "What would you like to change?",
              "OK. What's the new rule?",
              "Which rule would you like to change?",
              "Yes, I can do that"
            };
            foreach (var phrase in audioPhrases)
            {
                TextToSpeechMgr.TextToAudiofile(phrase, deviceName);
            }

            // Add audioclips with set filenames

            audioText = "Hi. I’m CONVERSE. I’m a virtual assistant that can help you " +
                "manage smart devices in your home. I can list the rules I have for a device; add new " +
                "rules, and change or delete rules. If you want to know the rules I have for a device, " +
                "simply ask “What rules do you have for” and give the device name. Why not give it a try?";
            fileName = "Converse intro";
            TextToSpeechMgr.TextToAudiofile(audioText, deviceName, fileName);
            audioText = "I'm sorry, I'm not yet able to help with that. I can list " +
                "the rules I have for a device; add new rules, and change or delete rules. " +
                "What would you like me to do?";
            fileName = "I can't help; list functions";
            TextToSpeechMgr.TextToAudiofile(audioText, deviceName, fileName);
            audioText = "This is a CONVERSE sound level check. Are you able to hear and understand what I am " +
                "saying, or would you like the volume or speed changed?";
            fileName = "Sound Check";
            TextToSpeechMgr.TextToAudiofile(audioText, deviceName, fileName);
        }

        private static void GenerateBedroomLightAudioClips()
        {
            string deviceName = "Bedroom Light";
            string[] audioPhrases = new string[]
            {
              "change the bedroom light colour to blue",
              "change the bedroom light colour to red",
              "change the bedroom light colour to white",
              "I have six rules for the bedroom light",
              "I have five rules for the bedroom light",
              "I have four rules for the bedroom light",
              "I have three rules for the bedroom light",
              "I have two rules for the bedroom light",
              "I have one rule for the bedroom light",
              "Currently I have no rules for the bedroom light",
              "turn on the bedroom light and set the colour blue",
              "turn on the bedroom light and set the colour red",
              "turn on the bedroom light and set the colour white",
              "turn off the bedroom light",
              "What colour should the light be set to?"
            };
            foreach (var phrase in audioPhrases)
            {
                TextToSpeechMgr.TextToAudiofile(phrase, deviceName);
            }
        }

        private static void GenerateKitchenRadioAudioClips()
        {
            string audioText = String.Empty;
            string deviceName = "Kitchen Radio";
            string[] audioPhrases = new string[]
            {
              "change the kitchen radio station to Radio 1",
              "change the kitchen radio station to Radio 2",
              "change the kitchen radio station to Radio 3",
              "change the kitchen radio station to Radio 4",
              "I have six rules for the kitchen radio",
              "I have five rules for the kitchen radio",
              "I have four rules for the kitchen radio",
              "I have three rules for the kitchen radio",
              "I have two rules for the kitchen radio",
              "I have one rule for the kitchen radio",
              "Currently I have no rules for the kitchen radio",
              "turn on the kitchen radio and select Radio 1",
              "turn on the kitchen radio and select Radio 2",
              "turn on the kitchen radio and select Radio 3",
              "turn on the kitchen radio and select Radio 4",
              "turn off the kitchen radio",
              "What station should be selected?"
            };
            foreach (var phrase in audioPhrases)
            {
                TextToSpeechMgr.TextToAudiofile(phrase, deviceName);
            }
        }

        public static void GenerateAudioFiles(AudioClip audioClip)
        {
            string audiofile = String.Empty;
            if (!String.IsNullOrEmpty(audioClip.StatementAudioFile))
            {
                audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, audioClip.StatementAudioFile + ".mp3");
                if (!File.Exists(audiofile))
                    TextToSpeechMgr.TextToAudiofile(audioClip.StatementText, audioClip.DeviceName, audioClip.StatementAudioFile);
            }
            if (!String.IsNullOrEmpty(audioClip.ConfirmationAudioFile))
            {
                audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, audioClip.ConfirmationAudioFile + ".mp3");
                if (!File.Exists(audiofile))
                    TextToSpeechMgr.TextToAudiofile(audioClip.ConfirmationText, audioClip.DeviceName, audioClip.ConfirmationAudioFile);
            }
            if (!String.IsNullOrEmpty(audioClip.CheckAudioFile))
            {
                audiofile = Path.Combine(DirectoryMgr.AudioClipsDirectory, audioClip.CheckAudioFile + ".mp3");
                if (!File.Exists(audiofile))
                    TextToSpeechMgr.TextToAudiofile(audioClip.CheckText, audioClip.DeviceName, audioClip.CheckAudioFile);
            }
        }
    }
}
