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
using DialogueManager.Database;
using DialogueManager.EventLog;
using DialogueManager.Views;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.IO;

namespace DialogueManager.ViewModels
{
    class SettingsViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<string> Voices {
            get {
                var voices = GoogleTextToSpeechMgr.GetOnlineVoices(SelectedLanguageCode);
                return voices != null
                    ? new ObservableCollection<string>(voices)
                    : null;
            }
        }

        private string selectedVoice = Settings.OnlineVoice;
        public string SelectedVoice {
            get { return selectedVoice; }
            set {
                if (selectedVoice != value)
                {
                    selectedVoice = value;
                    if (selectedVoice != null)
                    {
                        GoogleTextToSpeechMgr.Voice = selectedVoice;
                        SaveSettings("OnlineVoice");
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedVoice)));
                }
            }
        } 

        public ObservableCollection<string> LanguageCodes {
            get {
                var langCodes = GoogleTextToSpeechMgr.GetLanguageCodes();
                return langCodes != null
                    ? new ObservableCollection<string>(GoogleTextToSpeechMgr.GetLanguageCodes())
                    : null;
            }
        }

        private string selectedLanguageCode = Settings.LanguageCode;
        public string SelectedLanguageCode {
            get { return selectedLanguageCode; }
            set {
                if (selectedLanguageCode != value)
                {
                    selectedLanguageCode = value;
                    GoogleTextToSpeechMgr.LanguageCode = selectedLanguageCode;
                    if (SelectedVoice == null || !SelectedVoice.StartsWith(selectedLanguageCode))
                    {
                        // set to null first so SelectedVoice consistently upates (?)
                        SelectedVoice = null;
                        SelectedVoice = Voices[0];
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedLanguageCode)));
                    }
                }
            }
        }

        private string credentialsFile = Settings.CredentialsFile;
        public string CredentialsFile {
            get { return credentialsFile; }
            set {
                credentialsFile = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CredentialsFile)));
            }
        }

        private bool checkAudioFiles = Settings.CheckAudioFiles;
        public bool CheckAudioFiles {
            get { return checkAudioFiles; }
            set {
                checkAudioFiles = value;
                SaveSettings("CheckAudioFiles");
            }
        }

        private Visibility startRecordBtnVisibility = Visibility.Visible;
        public Visibility StartRecordBtnVisibility {
            get { return startRecordBtnVisibility; }
            set {
                if (value != startRecordBtnVisibility)
                {
                    startRecordBtnVisibility = value;
                    if (startRecordBtnVisibility == Visibility.Visible)
                        StopRecordBtnVisibility = Visibility.Collapsed;
                    else
                        StopRecordBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartRecordBtnVisibility)));
                }
            }
        }

        private Visibility stopRecordBtnVisibility = Visibility.Collapsed;
        public Visibility StopRecordBtnVisibility {
            get { return stopRecordBtnVisibility; }
            set {
                if (value != stopRecordBtnVisibility)
                {
                    stopRecordBtnVisibility = value;
                    if (stopRecordBtnVisibility == Visibility.Visible)
                        StartRecordBtnVisibility = Visibility.Collapsed;
                    else
                        StartRecordBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopRecordBtnVisibility)));
                }
            }
        }

        public bool GenerateMissingAudioFiles { get; set; }

        public ObservableCollection<string> AudioDelays {
            get { return new ObservableCollection<string>(AudioMgr.GetDelays()); }
        }

        public string AudioDelay {
            get { return AudioMgr.AudioDelay; }
            set {
                if (AudioMgr.AudioDelay != value)
                    AudioMgr.AudioDelay = value;
            }
        }

        public ObservableCollection<string> AudioDevices {
            get { return new ObservableCollection<string>(AudioMgr.GetAudioDevices()); }
        }

        private string audioDevice = AudioMgr.GetAudioDevices()[0];
        public string AudioDevice {
            get { return audioDevice; }
            set {
                if (audioDevice != value)
                {
                    audioDevice = value;
                    AudioMgr.DeviceNumber = AudioDevices.IndexOf(audioDevice);
                }
            }
        }

        public string MaxLogEntriesTxt { get; set; } = Settings.MaxLogEntries.ToString();
        private int MaxLogEntries = Settings.MaxLogEntries;

        private string currentInputLevelStr = "0";
        public string CurrentInputLevelStr {
            get { return currentInputLevelStr; }
            set {
                currentInputLevelStr = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentInputLevelStr)));
            }
        }

        private float currentInputLevel = 0;
        public float CurrentInputLevel {
            get { return currentInputLevel; }
            set {
                currentInputLevel = value;
                CurrentInputLevelStr = Math.Round(currentInputLevel, MidpointRounding.AwayFromZero).ToString();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentInputLevel)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool CheckingLevels = false;

        public SettingsViewModel()
        {
            if (SelectedVoice == null || !SelectedVoice.StartsWith(selectedLanguageCode))
                SelectedVoice = Voices[0];
            else
                EventSystem.Subscribe<OnlineVoicesLoaded>(OnOnlineVoicesLoaded);
            EventSystem.Subscribe<AudioUpdated>(OnAudioUpdated);
        }

        private void OnAudioUpdated(AudioUpdated audioUpdated)
        {
            CurrentInputLevel = audioUpdated.RecordingLevel;
        }

        private void OnOnlineVoicesLoaded(OnlineVoicesLoaded onlineVoicesLoaded)
        {
           SelectedVoice = Voices[0];
        }

        private void SaveSettings(string setting)
        {
            string msg = String.Empty;
            switch (setting)
            {
                case "CheckAudioFiles":
                    Settings.CheckAudioFiles = CheckAudioFiles;
                    msg = "Check Audio Files option saved.";
                    break;
                case "CredentialsFile":
                    Settings.CredentialsFile = CredentialsFile;
                    msg = "Google Application Credentials file setting saved.";
                    break;
                case "MaxLogEntries":
                    Settings.MaxLogEntries = MaxLogEntries;
                    msg = "Maximum log entries option saved.";
                    break;
                case "OnlineVoice":
                case "LanguageCode":
                    Settings.LanguageCode = SelectedLanguageCode;
                    Settings.OnlineVoice = SelectedVoice;
                    msg = "Online voice options saved.";
                    break;
            }
            if (SettingsTableMgr.SaveSettingsToDB())
            {
                if (!String.IsNullOrEmpty(msg))
                {
                    var messageWin = new MessageWin("Save Settings", msg);
                    messageWin.Show();
                }
                Logger.AddLogEntry(LogCategory.INFO, "App options saved.");
            }
            else
            {
                var messageWin = new MessageWin("Save Settings", "Error: problem saving app options.");
                messageWin.Show();
                Logger.AddLogEntry(LogCategory.ERROR, "SaveSettings: Problem saving app options");
            }
        }

        private ICommand onBrowseCredentialsBtnClick;
        public ICommand OnBrowseCredentialsBtnClick {
            get {
                if (onBrowseCredentialsBtnClick == null)
                    onBrowseCredentialsBtnClick = new RelayCommand(param => BrowseForCredentialsFile(param), param => true);
                return onBrowseCredentialsBtnClick;
            }
            set { onBrowseCredentialsBtnClick = value; }
        }

        private void BrowseForCredentialsFile(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DefaultExt = ".json",
                InitialDirectory = DirectoryMgr.AppDataDirectory,
                Filter = "JSON files | *.json;*.JSON |All files (*.*)|*.*"
            };
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                CredentialsFile = openFileDialog.FileName;
                SaveSettings("CredentialsFile");
            }
        }

        private ICommand onSoundCheckBtnClick;
        public ICommand OnSoundCheckBtnClick {
            get {
                onSoundCheckBtnClick = onSoundCheckBtnClick ?? new RelayCommand(param => SoundCheckBtnClick(param), param => true);
                return onSoundCheckBtnClick;
            }
            set { onSoundCheckBtnClick = value; }
        }

        private void SoundCheckBtnClick(object obj)
        {
            if (AudioMgr.UseRecordings)
                AudioMgr.PlayAudioClip(Path.Combine(DirectoryMgr.AudioClipsDirectory, @"Sound Files\Sound Check.mp3"));
            else
            {
                string audioFile = GoogleTextToSpeechMgr.GenerateSpeech("This is a CONVERSE sound level check. " +
                    "Are you able to hear and understand what I am saying, or would you " +
                    "like the volume or speed changed?");
                AudioMgr.PlayAudioClip(audioFile);
            }
        }

        private ICommand onSaveLogEntriesBtnClick;
        public ICommand OnSaveLogEntriesBtnClick {
            get {
                onSaveLogEntriesBtnClick = onSaveLogEntriesBtnClick ?? new RelayCommand(param => SaveMaxLogEntries(param), param => true);
                return onSaveLogEntriesBtnClick;
            }
            set { onSaveLogEntriesBtnClick = value; }
        }

        private void SaveMaxLogEntries(object obj)
        {
            if (Int32.TryParse(MaxLogEntriesTxt, out int maxEntries))
            {
                if (maxEntries >= 0)
                {
                    MaxLogEntries = maxEntries;
                    SaveSettings("MaxLogEntries");
                }
                else
                    MaxLogEntriesTxt = Settings.MaxLogEntries.ToString();
            }
            else
            {
                MaxLogEntriesTxt = Settings.MaxLogEntries.ToString();
                var messageWin = new MessageWin("App Options", String.Format("{0} is not valid for maximum log entries - please update setting.", MaxLogEntriesTxt));
                messageWin.Show();
            }
        }

        private ICommand toggleRecordingBtnClick;
        public ICommand ToggleRecordingBtnClick {
            get {
                toggleRecordingBtnClick = toggleRecordingBtnClick ?? new RelayCommand(param => ToggleRecording(param), param => true);
                return toggleRecordingBtnClick;
            }
            set { toggleRecordingBtnClick = value; }
        }

        private void ToggleRecording(object obj)
        {
            if (!CheckingLevels)
            {
                if (AudioMgr.StartRecording(true))
                    StartRecordBtnVisibility = Visibility.Collapsed;
            }
            else
            {
                AudioMgr.StopRecording();
                StartRecordBtnVisibility = Visibility.Visible;
                
            }
            CheckingLevels = !CheckingLevels;
        }

        private ICommand refreshBtnClick;
        public ICommand RefreshBtnClick {
            get {
                refreshBtnClick = refreshBtnClick ?? new RelayCommand(param => RefreshVoices(param), param => true);
                return refreshBtnClick;
            }
            set { refreshBtnClick = value; }
        }

        private void RefreshVoices(object obj)
        {
            GoogleTextToSpeechMgr.GetOnlineVoices(SelectedLanguageCode, true);
            if (Voices.Count > 0)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Voices)));
        }   
    }
}
