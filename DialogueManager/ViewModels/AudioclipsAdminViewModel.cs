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
using DialogueManager.Views;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogueManager.ViewModels
{
    class AudioclipsAdminViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AudioClip> AudioClips { get; private set; }

        public ObservableCollection<string> CategoryNames { get { return new ObservableCollection<string>() { "Standard", "Ruleset", "Trigger", "Action" }; } }

        public ObservableCollection<string> VisibilityOptions { get { return new ObservableCollection<string>() { "Visible", "Hidden" }; } }

        private string selectedCategory = "Standard";
        public string SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                selectedCategory = value;
                if (selectedCategory.Equals("Trigger"))
                {
                    AudioFileDirectory = DirectoryMgr.TriggerClipsDirectory;
                }
            }
        }

        public string ClipVisibility { get; set; } = "Visible";

        public ICollectionView AudioclipsView { get; set; }

        private string screenTitle = "Standard Audio Clips";
        public string ScreenTitle
        {
            get { return screenTitle; }
            set
            {
                screenTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenTitle)));
            }
        }

        private string audioClipLabel;
        public string AudioClipLabel
        {
            get { return audioClipLabel; }
            set
            {
                audioClipLabel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClipLabel)));
            }
        }

        private Visibility rulesetOnlyVisibility = Visibility.Collapsed;
        public Visibility RulesetOnlyVisibility
        {
            get { return rulesetOnlyVisibility; }
            set
            {
                rulesetOnlyVisibility = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RulesetOnlyVisibility)));
            }
        }

        private string audioClipText;
        public string AudioClipText
        {
            get { return audioClipText; }
            set
            {
                audioClipText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClipText)));
            }
        }

        private string audioFileName;
        public string AudioFileName
        {
            get { return audioFileName; }
            set
            {
                audioFileName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioFileName)));
            }
        }

        private bool includeRulesetClips = false;
        public bool IncludeRulesetClips
        {
            get { return includeRulesetClips; }
            set
            {
                includeRulesetClips = value;
                RulesetOnlyVisibility = includeRulesetClips ? Visibility.Visible : Visibility.Collapsed;
                ScreenTitle = includeRulesetClips ? "Ruleset Audio Clips" : "Standard Audio Clips";
                if (AudioclipsView != null)
                {
                    AudioclipsView.Refresh();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
                }
            }
        }

        private bool includeHiddenClips = false;
        public bool IncludeHiddenClips
        {
            get { return includeHiddenClips; }
            set
            {
                includeHiddenClips = value;
                RulesetOnlyVisibility = includeHiddenClips ? Visibility.Visible : Visibility.Collapsed;
                if (AudioclipsView != null)
                {
                    AudioclipsView.Refresh();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
                }
            }
        }

        private string audioFileDirectory = String.Empty;

        public string AudioFileDirectory
        {
            get { return audioFileDirectory; }
            set
            {
                if (!audioFileDirectory.Equals(value))
                {
                    audioFileDirectory = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioFileDirectory)));
                }
            }
        }

        private AudioClip SelectedAudioClip;
        private bool newAudioClip = false;
        private string ButtonColour = ColourHelper.StatementColour;
        private MessageDialogWin messageDialogWin;
        internal Button LightBlueSquare;
        internal Button BeigeSquare;
        internal Button OrangeSquare;
        internal Button LightGreenSquare;
        private Button SelectedBtn;
        public event PropertyChangedEventHandler PropertyChanged;

        public AudioclipsAdminViewModel()
        {
            if (AudioClipsMgr.AudioclipsLoaded)
            {
                RefreshAudioClips();
            }
            else
            {
                EventSystem.Subscribe<AudioclipsLoaded>(OnAudioclipsLoaded);
            }

            SelectedAudioClip = new AudioClip();
            newAudioClip = true;

        }

        private void OnAudioclipsLoaded(AudioclipsLoaded al)
        {
            RefreshAudioClips();
        }

        private void RefreshAudioClips()
        {
            // Order alphabetically
            AudioClips = new ObservableCollection<AudioClip>(AudioClipsMgr.AudioClips
                .OrderBy(AudioClip => !AudioClip.IsVisible)
                .ThenBy(AudioClip => AudioClip.Label));
            AudioclipsView = CollectionViewSource.GetDefaultView(AudioClips);
            AudioclipsView.Filter = AudioClipsFilter;
            AudioclipsView.Refresh();
        }

        private bool AudioClipsFilter(object item)
        {
            AudioClip audioClip = item as AudioClip;
            if (IncludeRulesetClips)
            {
                if (!IncludeHiddenClips && !audioClip.IsVisible)
                {
                    return false;
                }
                else
                {
                    return (!audioClip.Label.Contains("OK, rule deleted")
                        && !audioClip.Label.Contains("List rules")
                        && !audioClip.Label.Contains("State selected rule"));
                }
            }
            else
            {
                return audioClip.Category.Equals("Standard");
            }
        }

        private ICommand newBtnClick;
        public ICommand NewBtnClick
        {
            get
            {
                if (newBtnClick == null)
                {
                    newBtnClick = new RelayCommand(param => NewClip(param), param => true);
                }

                return newBtnClick;
            }
            set { newBtnClick = value; }
        }

        private void NewClip(object obj)
        {
            SelectedAudioClip = new AudioClip();
            AudioClipLabel = String.Empty;
            AudioClipText = String.Empty;
            AudioFileName = String.Empty;
            SelectedAudioClip.ButtonColour = ColourHelper.StatementColour;
            LightBlueSquare.BorderThickness = new Thickness(2);
            newAudioClip = true;
        }

        private ICommand deleteBtnClick;
        public ICommand DeleteBtnClick
        {
            get
            {
                if (deleteBtnClick == null)
                {
                    deleteBtnClick = new RelayCommand(param => DeleteClipConfirm(param), param => true);
                }

                return deleteBtnClick;
            }
            set { deleteBtnClick = value; }
        }

        public void DeleteClipConfirm(object obj)
        {
            messageDialogWin = new MessageDialogWin("Deleting audio clip",
                String.Format("This will delete audio clip \"{0}\" from all sessions. Are you sure?", SelectedAudioClip.Label), "Yes", "No");
            messageDialogWin.MessageResponse += DeleteMessageResponse;
            messageDialogWin.Show();
        }

        private void DeleteMessageResponse(object sender, string e)
        {
            messageDialogWin.MessageResponse -= DeleteMessageResponse;
            if (e.Equals("Yes"))
            {
                SessionsMgr.DeleteClipFromAllSessions(SelectedAudioClip.ClipId);
                AudioClipsMgr.DeleteAudioClip(SelectedAudioClip.Label, out string outcome);
                RefreshAudioClips();
                SelectedAudioClip = new AudioClip();
                newAudioClip = true;
                AudioclipsView.Refresh();
                Reset();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
                EventSystem.Publish(new AudioClipsInventoryChanged());
            }
            messageDialogWin.Close();
        }

        private ICommand cancelBtnClick;
        public ICommand CancelBtnClick
        {
            get
            {
                if (cancelBtnClick == null)
                {
                    cancelBtnClick = new RelayCommand(param => CancelChanges(param), param => true);
                }

                return cancelBtnClick;
            }
            set { cancelBtnClick = value; }
        }

        private void CancelChanges(object obj)
        {
            Reset();
        }

        private void Reset()
        {
            SelectedAudioClip = null;
            AudioClipLabel = String.Empty;
            AudioClipText = String.Empty;
            AudioFileName = String.Empty;
            AudioFileDirectory = String.Empty;
            newAudioClip = false;
        }

        private ICommand loadBtnClick;
        public ICommand LoadBtnClick
        {
            get
            {
                if (loadBtnClick == null)
                {
                    loadBtnClick = new RelayCommand(param => LoadAudioFile(param), param => true);
                }

                return loadBtnClick;
            }
            set { loadBtnClick = value; }
        }

        private void LoadAudioFile(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Path.Combine(DirectoryMgr.AppDataDirectory, "Audio Clips"),
                DefaultExt = ".mp3",
                Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|AAC Files (*.aac)|*.aac|MP4 Files (*.mp4)|*.mp4"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                AudioFileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                AudioFileDirectory = Path.GetDirectoryName(openFileDialog.FileName);
            }
        }

        private ICommand saveBtnClick;
        public ICommand SaveBtnClick
        {
            get
            {
                if (saveBtnClick == null)
                {
                    saveBtnClick = new RelayCommand(param => SaveChanges(param), param => true);
                }

                return saveBtnClick;
            }
            set { saveBtnClick = value; }
        }

        private void SaveChanges(object obj)
        {
            if (AudioClipDataIsComplete())
            {
                int returnValue;
                string msg = "Audio clip saved.";
                if (SelectedAudioClip == null)
                {
                    SelectedAudioClip = new AudioClip();
                    newAudioClip = true;
                }
                SelectedAudioClip.Label = AudioClipLabel;
                SelectedAudioClip.StateText = AudioClipText;
                SelectedAudioClip.StateAudioFile = Path.Combine(AudioFileDirectory, AudioFileName);
                SelectedAudioClip.Category = SelectedCategory;
                SelectedAudioClip.IsVisible = ClipVisibility.Equals("Visible") ? true : false;
                if (SelectedAudioClip.IsVisible)
                {
                    SelectedAudioClip.ButtonColour = ButtonColour;
                }
                else
                {
                    SelectedAudioClip.ButtonColour = ColourHelper.HiddenColour;
                }

                if (newAudioClip)
                {
                    returnValue = AudioClipsMgr.AddAudioClip(SelectedAudioClip);
                    if (returnValue > 0)
                    {
                        newAudioClip = false;
                    }
                }
                else
                {
                    returnValue = AudioClipsMgr.UpdateAudioClipToDB(SelectedAudioClip);
                }

                if (returnValue > 0)
                {
                    RefreshAudioClips();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
                    EventSystem.Publish(new AudioClipsInventoryChanged());
                }
                if (returnValue == -1)
                {
                    msg = "Error: Audio clip label already exists - please choose another label.";
                }
                else if (returnValue == -2)
                {
                    msg = "Error: Problem saving audioClip to database.";
                }

                var messageWin = new MessageWin("Save Audioclip", msg);
                messageWin.Show();
            }
        }

        private bool AudioClipDataIsComplete(bool fileGenerationOnly = false)
        {
            bool allOK = true;
            StringBuilder sb = new StringBuilder();
            if (!fileGenerationOnly && String.IsNullOrEmpty(AudioClipLabel))
            {
                sb.AppendLine("Please give the audioclip a label.");
                allOK = false;
            }
            if (String.IsNullOrEmpty(AudioClipText))
            {
                sb.AppendLine("Please set the audioclip text.");
                allOK = false;
            }
            if (String.IsNullOrEmpty(AudioFileDirectory))
            {
                sb.AppendLine("Please set a directory for the audio clip.");
                allOK = false;
            }
            if (String.IsNullOrEmpty(AudioFileName))
            {
                sb.AppendLine("Please set a filename.");
                allOK = false;
            }
            if (!allOK)
            {
                var messageWin = new MessageWin("Save Audioclip", sb.ToString());
                messageWin.Show();
            }
            return allOK;
        }

        public void OnAudioClipBtnClick(Button btn)
        {
            if (SelectedBtn != null)
            {
                // Clear selection colours
                SelectedBtn.Foreground = new SolidColorBrush(Colors.Black);
                SelectedBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(SelectedBtn.Background.ToString()));
            }
            AudioClipLabel = btn.Content.ToString();
            if (SelectedAudioClip != null)
            {
                ResetColourButtonBorders();
            }

            SelectedAudioClip = AudioClipsMgr.GetAudioClipCopy(AudioClipLabel);
            AudioClipText = SelectedAudioClip.StateText;
            AudioFileName = Path.GetFileName(SelectedAudioClip.StateAudioFile);
            AudioFileDirectory = Path.GetDirectoryName(SelectedAudioClip.StateAudioFile);
            SelectedCategory = SelectedAudioClip.Category;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCategory)));
            ClipVisibility = SelectedAudioClip.IsVisible ? "Visible" : "Hidden";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipVisibility)));
            newAudioClip = false;
            switch (btn.Background.ToString())
            {
                case ColourHelper.StatementColour:
                    LightBlueSquare.BorderThickness = new Thickness(2);
                    break;
                case ColourHelper.QuestionColour:
                    BeigeSquare.BorderThickness = new Thickness(2);
                    break;
                case ColourHelper.ActionColour:
                    OrangeSquare.BorderThickness = new Thickness(2);
                    break;
                case ColourHelper.ConditionColour:
                    LightGreenSquare.BorderThickness = new Thickness(2);
                    break;
            }
            SelectedBtn = btn;
            SelectedBtn.Foreground = new SolidColorBrush(Colors.White);
            SelectedBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(SelectedBtn.Background.ToString()));
        }

        private ICommand onGenerateBtnClick;
        public ICommand OnGenerateBtnClick
        {
            get
            {
                if (onGenerateBtnClick == null)
                {
                    onGenerateBtnClick = new RelayCommand(param => GenerateAudioFile(param), param => true);
                }

                return onGenerateBtnClick;
            }
            set { onGenerateBtnClick = value; }
        }

        private void GenerateAudioFile(object obj)
        {
            bool filePathSet = true;
            bool fileNameSet = true;
            bool textSet = true;
            AudioFileName = Path.GetFileName(AudioFileName);
            if (String.IsNullOrEmpty(AudioClipText))
            {
                textSet = false;
            }

            if (String.IsNullOrEmpty(AudioFileDirectory))
            {
                filePathSet = false;
            }

            if (filePathSet && String.IsNullOrEmpty(AudioFileName))
            {
                if (!String.IsNullOrEmpty(AudioClipLabel))
                {
                    AudioFileName = AudioClipLabel;
                }
                else
                {
                    filePathSet = false;
                }
            }
            if (!filePathSet || !textSet)
            {
                string msg;
                if (!textSet && !fileNameSet)
                {
                    msg = "Please complete the directory, filename and audio text fields.";
                }
                else if (!textSet)
                {
                    msg = "Please enter audio text.";
                }
                else
                {
                    msg = "Please complete the directory and filename fields.";
                }

                var msgWin = new MessageWin("Generate Audio File", msg);
                msgWin.Show();
                return;
            }
            try
            {
                Directory.CreateDirectory(AudioFileDirectory);
            }
            catch (Exception)
            {
                var msgWin = new MessageWin("Generate Audio File", "Please enter a valid directory.");
                msgWin.Show();
                return;
            }
            string fileName = Path.Combine(AudioFileDirectory, AudioFileName);
            if (GoogleTextToSpeechMgr.GenerateAudiofile(AudioClipText, fileName))
            {
                SelectedAudioClip.StateAudioFile = fileName;
                AudioFileName = Path.GetFileName(fileName);
                SelectedAudioClip.Label = AudioClipLabel;
                SelectedAudioClip.StateText = AudioClipText;
                SelectedAudioClip.ButtonColour = ColourHelper.StatementColour;
                var messageWin = new MessageWin("Generate Audio clip", String.Format("Audio clip {0} generated", fileName));
                messageWin.Show();
                Logger.AddLogEntry(LogCategory.INFO, String.Format("Audio clip {0} generated", fileName));
            }
            else
            {
                var messageWin = new MessageWin("Generate Audio clips", String.Format("Errors encountered generating \"{0}\" audio clip.", SelectedAudioClip.Label));
                messageWin.Show();
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("Error generating \"{0}\" audio clip", SelectedAudioClip.Label));
            }
        }

        private ICommand browseBtnClick;
        public ICommand BrowseBtnClick
        {
            get
            {
                if (browseBtnClick == null)
                {
                    browseBtnClick = new RelayCommand(param => BrowseForAudioClipDirectory(param), param => true);
                }

                return browseBtnClick;
            }
            set { browseBtnClick = value; }
        }

        private void BrowseForAudioClipDirectory(object obj)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.SelectedPath = DirectoryMgr.AudioClipsDirectory;
                if (!String.IsNullOrEmpty(dlg.SelectedPath))
                {
                    AudioFileDirectory = dlg.SelectedPath;
                }
                else
                {
                    Logger.AddLogEntry(LogCategory.WARN, String.Format("BrowseForAudioClipDirectory: Directory not selected"));
                }
            }
        }

        public void OnColourBtnClick(Button btn)
        {
            if (SelectedAudioClip != null)
            {
                ResetColourButtonBorders();
                switch (btn.Tag)
                {
                    case "LightBlue":
                        SelectedAudioClip.ButtonColour = ColourHelper.StatementColour;
                        LightBlueSquare.BorderThickness = new Thickness(2);
                        break;
                    case "Beige":
                        SelectedAudioClip.ButtonColour = ColourHelper.QuestionColour;
                        BeigeSquare.BorderThickness = new Thickness(2);
                        break;
                    case "Orange":
                        SelectedAudioClip.ButtonColour = ColourHelper.ActionColour;
                        OrangeSquare.BorderThickness = new Thickness(2);
                        break;
                    case "LightGreen":
                        SelectedAudioClip.ButtonColour = ColourHelper.ConditionColour;
                        LightGreenSquare.BorderThickness = new Thickness(2);
                        break;
                }
                ButtonColour = SelectedAudioClip.ButtonColour;
                AudioclipsView.Refresh();
            }
        }

        private ICommand onTimeTriggersBtnClick;
        public ICommand OnTimeTriggersBtnClick
        {
            get
            {
                if (onTimeTriggersBtnClick == null)
                {
                    onTimeTriggersBtnClick = new RelayCommand(param => ConfirmTimeTriggersGeneration(param), param => true);
                }

                return onTimeTriggersBtnClick;
            }
            set { onTimeTriggersBtnClick = value; }
        }

        public void ConfirmTimeTriggersGeneration(object obj)
        {
            messageDialogWin = new MessageDialogWin("Generating Time Trigger Clips",
                String.Format("This will generate all time trigger clips using the voice selected in Settings. Are you sure?", SelectedAudioClip.Label), "Yes", "No");
            messageDialogWin.MessageResponse += TimeTriggerMessageResponse;
            messageDialogWin.Show();
        }

        private void TimeTriggerMessageResponse(object sender, string e)
        {
            messageDialogWin.MessageResponse -= TimeTriggerMessageResponse;
            if (e.Equals("Yes"))
            {
                MessageWin msgWin;
                if (AudioFileGenerator.GenerateTimeTriggerAudioFiles())
                {
                    msgWin = new MessageWin("Generate Time Trigger Clips", "Time Trigger clips generated.");
                }
                else
                {
                    msgWin = new MessageWin("Generate Time Trigger Clips", "Problem generating Time Trigger clips.");
                }

                msgWin.Show();
            }
            messageDialogWin.Close();
        }

        private ICommand onRulesetBtnClick;
        public ICommand OnRulesetBtnClick
        {
            get
            {
                if (onRulesetBtnClick == null)
                {
                    onRulesetBtnClick = new RelayCommand(param => ConfirmRulesetClipsGeneration(param), param => true);
                }

                return onRulesetBtnClick;
            }
            set { onRulesetBtnClick = value; }
        }

        public void ConfirmRulesetClipsGeneration(object obj)
        {
            messageDialogWin = new MessageDialogWin("Generating Common Ruleset Clips",
                String.Format("This will generate the common ruleset clips using the voice selected in Settings. Are you sure?", SelectedAudioClip.Label), "Yes", "No");
            messageDialogWin.MessageResponse += RulesetMessageResponse;
            messageDialogWin.Show();
        }

        private void RulesetMessageResponse(object sender, string e)
        {
            messageDialogWin.MessageResponse -= RulesetMessageResponse;
            if (e.Equals("Yes"))
            {
                MessageWin msgWin;
                if (AudioFileGenerator.GenerateCommonRulesetAudioFiles())
                {
                    msgWin = new MessageWin("Generate Common Ruleset Clips", "Common Ruleset Clips generated.");
                }
                else
                {
                    msgWin = new MessageWin("Generate Common Ruleset Clips", "Problem generating Common Ruleset Clips.");
                }

                msgWin.Show();
            }
            messageDialogWin.Close();
        }

        private void ResetColourButtonBorders()
        {
            LightBlueSquare.BorderThickness = new Thickness(1);
            BeigeSquare.BorderThickness = new Thickness(1);
            OrangeSquare.BorderThickness = new Thickness(1);
            LightGreenSquare.BorderThickness = new Thickness(1);
        }
    }
}
