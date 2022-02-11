/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

/*
 * Generates audio clips and audio fragments that are linked together for playback.
 * Includes methods to generate audio files from AudioClip instances and from 
 * phrases in a text file (each phrase on a separate line)
 */

using DialogueManager.EventLog;
using System;
using System.IO;

namespace DialogueManager
{
    static class AudioFileGenerator
    {

        public static bool GenerateTimeTriggerAudioFiles()
        {
            string fileName;
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Triggers\\Time");
            string audiofile;
            bool allOK = true;
            string audioText;
            for (int hours = 1; hours < 13; hours++)
            {
                for (int minutes = 0; minutes < 60; minutes++)
                {
                    audioText = "At " + hours.ToString() + ":" + minutes.ToString("D2") + "am today";
                    fileName = String.Join("_", audioText.Split(Path.GetInvalidFileNameChars()));
                    audiofile = Path.Combine(audioDirectory, fileName);
                    if (!GoogleTextToSpeechMgr.GenerateAudiofile(audioText, audiofile))
                        return false; // bale out if first call fails
                    fileName = String.Join("_", audioText.Replace("today", "every day").Split(Path.GetInvalidFileNameChars()));
                    audiofile = Path.Combine(audioDirectory, fileName);
                    allOK = GoogleTextToSpeechMgr.GenerateAudiofile(audioText.Replace("today", "every day"), audiofile) ? allOK : false;
                    fileName = String.Join("_", audioText.Replace("am", "pm").Split(Path.GetInvalidFileNameChars()));
                    audiofile = Path.Combine(audioDirectory, fileName);
                    allOK = GoogleTextToSpeechMgr.GenerateAudiofile(audioText.Replace("am", "pm"), audiofile) ? allOK : false;
                    fileName = String.Join("_", audioText.Replace("today", "every day").Replace("am", "pm").Split(Path.GetInvalidFileNameChars()));
                    audiofile = Path.Combine(audioDirectory, fileName);
                    allOK = GoogleTextToSpeechMgr.GenerateAudiofile(audioText.Replace("today", "every day").Replace("am", "pm"), audiofile) ? allOK : false;
                }
            }
            audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Triggers");
            audioText = "If someone enters the room";
            fileName = String.Join("_", audioText.Split(Path.GetInvalidFileNameChars()));
            audiofile = Path.Combine(audioDirectory, fileName);
            if (!File.Exists(audiofile + ".mp3"))
                allOK = GoogleTextToSpeechMgr.GenerateAudiofile(audioText, audiofile) ? allOK : false;
            audioText = "If the room is empty";
            fileName = String.Join("_", audioText.Split(Path.GetInvalidFileNameChars()));
            audiofile = Path.Combine(audioDirectory, fileName);
            if (!File.Exists(audiofile + ".mp3"))
                allOK = GoogleTextToSpeechMgr.GenerateAudiofile(audioText, audiofile) ? allOK : false;
            return allOK;
        }

        public static bool GenerateCommonRulesetAudioFiles()
        {
            string[] audioPhrases = new string[]
            {
              "Add new rule",
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
              "Is this correct?",
              "Let me check I have understood",
              "New rule added",
              "New rule",
              "OK",
              "Run new rule",
              "What would you like me to do?",
              "Which rule would you like to change?",
            };
            string fileName;
            string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, "Ruleset");
            string audiofile;
            foreach (var phrase in audioPhrases)
            {
                fileName = String.Join("_", phrase.Split(Path.GetInvalidFileNameChars()));
                audiofile = Path.Combine(audioDirectory, fileName);
                if (!File.Exists(audiofile + ".mp3") && !GoogleTextToSpeechMgr.GenerateAudiofile(phrase, audiofile))
                    return false;
            }

            // Generate audio files with non-default filenames

            string audioText = "This is a CONVERSE sound level check. Are you able to hear and understand what I am " +
                "saying, or would you like the volume or speed changed?";
            fileName = "Sound Check";
            audiofile = Path.Combine(audioDirectory, fileName);
            if (!File.Exists(audiofile + ".mp3"))
                return GoogleTextToSpeechMgr.GenerateAudiofile(audioText, audiofile);
            return true;
        }

    }
}
