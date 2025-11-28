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
using DialogueManager.Views;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.TextToSpeech.V1;
using System;
using System.Collections.Generic;
using System.IO;

namespace DialogueManager
{
    public static class GoogleTextToSpeechMgr
    {
        private static List<string> Voices; // holds voice + (gender)
        private static List<string> LanguageCodes;
        private static string voice = "en-GB-Wavenet-C";
        private static SsmlVoiceGender Gender = SsmlVoiceGender.Female;

        public static string Voice
        {
            get { return voice; }
            set
            {
                if (value != null)
                {
                    if (value.EndsWith("(female)"))
                    {
                        Gender = SsmlVoiceGender.Female;
                    }
                    else
                    {
                        Gender = SsmlVoiceGender.Male;
                    }

                    voice = value.Substring(0, value.IndexOf("(") - 1);
                }
            }
        }

        public static bool OnlineVoicesLoaded { get; set; } = false;

        public static string LanguageCode { get; set; } = "en-GB";

        private static string GetCredentialsFile()
        {
            string credentialsFile = Settings.CredentialsFile;
            if (!File.Exists(credentialsFile))
            {
                credentialsFile = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                    EnvironmentVariableTarget.User);
                if (!File.Exists(credentialsFile))
                {
                    var messageWin = new MessageWin("TextToSpeechMgr",
                        String.Format("TextToSpeechMgr Error: Google credentials file not found"));
                    messageWin.Show();
                    Logger.AddLogEntry(LogCategory.ERROR,
                        String.Format("TextToSpeechMgr: Google credentials file: not found"));
                    return null;
                }
            }
            return credentialsFile;
        }

        public static bool GenerateAudiofile(string audioText, string fileName)
        {
            /*
             * Method generates audio clips for use offline
             */
            string credentialsFile = GetCredentialsFile();
            if (!String.IsNullOrEmpty(credentialsFile))
            {
                try
                {
                    GoogleCredential credentials = GoogleCredential.FromFile(credentialsFile);
                    SynthesizeSpeechResponse response;
                    using (var textToSpeechClient = TextToSpeechClient.Create(credentials))
                    {
                        response = textToSpeechClient.SynthesizeSpeech(
                        new SynthesisInput()
                        {
                            Text = audioText
                        },
                        new VoiceSelectionParams()
                        {
                            LanguageCode = LanguageCode,
                            Name = Voice,
                            SsmlGender = Gender
                        },
                        new AudioConfig()
                        {
                            AudioEncoding = AudioEncoding.Mp3,
                            SpeakingRate = (float?)0.95, // default speed set to 0.95
                            SampleRateHertz = 24000
                        }
                    );
                    }
                    // check all required directories exist
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    string speechFile = Path.Combine(fileName + ".mp3");
                    File.WriteAllBytes(speechFile, response.AudioContent);
                    return true;
                }
                catch (Exception e)
                {
                    string msg = String.Format("Error reading Google credentials file - audio file not generated.\n{0} ", e.ToString());
                    var messageWin = new MessageWin("TextToSpeechMgr", msg);
                    messageWin.Show();
                    Logger.AddLogEntry(LogCategory.ERROR, msg);
                }
            }
            return false;
        }

        public static string GenerateSpeech(string audioText)
        {
            /*
             * Generates audio when online; 
             * Default fileName is audio text with illegal characters replaced by "_" 
             * FileName can be specified to avoid long filenames
             * 
             */
            if (!String.IsNullOrEmpty(audioText))
            {
                string credentialsFile = GetCredentialsFile();
                if (!String.IsNullOrEmpty(credentialsFile))
                {
                    try
                    {
                        GoogleCredential credentials = GoogleCredential.FromFile(credentialsFile);
                        SynthesizeSpeechResponse response;
                        using (var textToSpeechClient = TextToSpeechClient.Create(credentials))
                        {
                            response = textToSpeechClient.SynthesizeSpeech(
                            new SynthesisInput()
                            {
                                Text = audioText
                            },
                            new VoiceSelectionParams()
                            {
                                LanguageCode = LanguageCode,
                                Name = Voice,
                                SsmlGender = Gender
                            },
                            new AudioConfig()
                            {
                                AudioEncoding = AudioEncoding.Mp3,
                                SpeakingRate = (float?)(0.95 * AudioMgr.SpeedRatio),  // default speed set to 0.95
                                SampleRateHertz = 24000
                            }
                        );
                        }

                        string speechFile = Path.Combine(DirectoryMgr.TempDirectory, Guid.NewGuid().ToString() + ".mp3");
                        File.WriteAllBytes(speechFile, response.AudioContent);
                        return speechFile;
                    }
                    catch (Exception e)
                    {
                        Logger.AddLogEntry(LogCategory.ERROR,
                            String.Format("TextToSpeechMgr: Error reading Google credentials file: {0} ", e.ToString()));
                        var messageWin = new MessageWin("TextToSpeechMgr",
                            String.Format("Error reading Google credentials file - audio not generated."));
                        messageWin.Show();
                        return null;
                    }
                }
            }
            return null;
        }

        public static void LoadOnlineVoices()
        {
            string credentialsFile = GetCredentialsFile();
            if (!String.IsNullOrEmpty(credentialsFile))
            {
                try
                {
                    GoogleCredential credentials = GoogleCredential.FromFile(credentialsFile);
                    using (var textToSpeechClient = TextToSpeechClient.Create(credentials))
                    {
                        Voices = new List<string>();
                        LanguageCodes = new List<string>();
                        var voices = textToSpeechClient.ListVoices();
                        foreach (var voice in voices)
                        {
                            Voices.Add(String.Format("{0} ({1})", voice.Name, voice.SsmlGender.ToLower()));
                            int index = voice.Name.IndexOf('-', voice.Name.IndexOf('-') + 1);
                            string languageCode = voice.Name.Substring(0, index);
                            if (!LanguageCodes.Contains(languageCode))
                            {
                                LanguageCodes.Add(languageCode);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.AddLogEntry(LogCategory.ERROR,
                        String.Format("TextToSpeechMgr: Error reading Google credentials file: {0} ", e.ToString()));
                    var messageWin = new MessageWin("TextToSpeechMgr",
                        String.Format("Error reading Google credentials file - voice list not generated."));
                    messageWin.Show();
                }
                if (Voices != null)
                {
                    OnlineVoicesTableMgr.SaveOnlineVoicesToDB(Voices);
                }
            }
        }

        public static bool LoadOnlineVoicesFromDB()
        {
            var voices = new List<string>();
            if (OnlineVoicesTableMgr.LoadOnlineVoicesFromDB(voices))
            {
                LanguageCodes = new List<string>();
                foreach (var voice in voices)
                {
                    int index = voice.IndexOf('-', voice.IndexOf('-') + 1);
                    string languageCode = voice.Substring(0, index);
                    if (!LanguageCodes.Contains(languageCode))
                    {
                        LanguageCodes.Add(languageCode);
                    }
                }
                if (voices.Count > 0)
                {
                    Voices = voices;
                    OnlineVoicesLoaded = true;
                    EventSystem.Publish<OnlineVoicesLoaded>(new OnlineVoicesLoaded { });
                    return true;
                }
            }
            return false;
        }

        public static List<string> GetOnlineVoices(string languageCode, bool refresh = false)
        {
            List<string> voices = new List<string>();
            if (Voices == null || refresh)
            {
                LoadOnlineVoices();
            }

            if (Voices != null)
            {
                foreach (var voice in Voices)
                {
                    if (voice.StartsWith(languageCode))
                    {
                        voices.Add(voice);
                    }
                }
            }
            return voices;
        }

        public static List<string> GetLanguageCodes()
        {
            if (LanguageCodes == null)
            {
                LoadOnlineVoices();
            }

            return LanguageCodes;
        }
    }
}
