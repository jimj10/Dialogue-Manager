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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
    class DashboardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public IList<AudioClip> AudioClips { get; private set; }
        public ICollectionView AudioClipsView { get; private set; }

        private string screenTitle = "Standard Dashboard";
        public string ScreenTitle {
            get { return screenTitle; }
            set {
                screenTitle = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScreenTitle)));
            }
        }

        public string DeviceName { get; set; }
        
        private bool statePlayBtnEnabled = true;
        public bool StatePlayBtnEnabled {
            get { return statePlayBtnEnabled; }
            set {
                if (value != statePlayBtnEnabled)
                {
                    statePlayBtnEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatePlayBtnEnabled)));
                }
            }
        }

        private bool confirmPlayBtnEnabled = true;
        public bool ConfirmPlayBtnEnabled {
            get { return confirmPlayBtnEnabled; }
            set {
                if (value != confirmPlayBtnEnabled)
                {
                    confirmPlayBtnEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConfirmPlayBtnEnabled)));
                }
            }
        }

        private bool checkPlayBtnEnabled = true;
        public bool CheckPlayBtnEnabled {
            get { return checkPlayBtnEnabled; }
            set {
                if (value != checkPlayBtnEnabled)
                {
                    checkPlayBtnEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheckPlayBtnEnabled)));
                }
            }
        }

        private bool rulesetComponentsEnabled = true;
        public bool RulesetComponentsEnabled {
            get { return rulesetComponentsEnabled; }
            set {
                if (value != rulesetComponentsEnabled)
                {
                    rulesetComponentsEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RulesetComponentsEnabled)));
                }
            }
        }

        private bool playBtnEnabled = true;
        public bool PlayBtnEnabled {
            get { return playBtnEnabled; }
            set {
                if (value != playBtnEnabled)
                {
                    playBtnEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayBtnEnabled)));
                }
            }
        }

        private bool audioTextBoxIsReadOnly = true;
        public bool AudioTextBoxIsReadOnly {
            get { return audioTextBoxIsReadOnly; }
            set {
                if (value != audioTextBoxIsReadOnly)
                {
                    audioTextBoxIsReadOnly = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioTextBoxIsReadOnly)));
                }
            }
        }

        public bool KeepAlive {
            get { return AudioMgr.KeepAlive; }
            set {
                AudioMgr.KeepAlive = value;
                if (AudioMgr.KeepAlive)
                    KeepAliveOnBtnVisibility = Visibility.Visible;
                else
                    KeepAliveOnBtnVisibility = Visibility.Collapsed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeepAlive)));
                if (CurrentSession != null)
                {
                    CurrentSession.KeepAlive = AudioMgr.KeepAlive;
                    SessionsTableMgr.UpdateSession(CurrentSession);
                }
            }
        }

        private Visibility keepAliveOnBtnVisibility = Visibility.Visible;
        public Visibility KeepAliveOnBtnVisibility {
            get { return keepAliveOnBtnVisibility; }
            set {
                keepAliveOnBtnVisibility = value;
                if (keepAliveOnBtnVisibility == Visibility.Visible)
                    KeepAliveOffBtnVisibility = Visibility.Collapsed;
                else
                    KeepAliveOffBtnVisibility = Visibility.Visible;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeepAliveOnBtnVisibility)));
            }
        }

        private Visibility keepAliveOffBtnVisibility = Visibility.Visible;
        public Visibility KeepAliveOffBtnVisibility {
            get { return keepAliveOffBtnVisibility; }
            set {
                if (value != keepAliveOffBtnVisibility)
                {
                    keepAliveOffBtnVisibility = value;
                    if (keepAliveOffBtnVisibility == Visibility.Visible)
                        KeepAliveOnBtnVisibility = Visibility.Collapsed;
                    else
                        KeepAliveOnBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KeepAliveOffBtnVisibility)));
                }
            }
        }

        private Visibility audioOnBtnVisibility = Visibility.Visible;
        public Visibility AudioOnBtnVisibility {
            get { return audioOnBtnVisibility; }
            set {
                if (value != audioOnBtnVisibility)
                {
                    audioOnBtnVisibility = value;
                    if (audioOnBtnVisibility == Visibility.Visible)
                        AudioOffBtnVisibility = Visibility.Collapsed;
                    else
                        AudioOffBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioOnBtnVisibility)));
                }
            }
        }

        private Visibility audioOffBtnVisibility = Visibility.Collapsed;
        public Visibility AudioOffBtnVisibility {
            get { return audioOffBtnVisibility; }
            set {
                if (value != audioOffBtnVisibility)
                {
                    audioOffBtnVisibility = value;
                    if (audioOffBtnVisibility == Visibility.Visible)
                        AudioOnBtnVisibility = Visibility.Collapsed;
                    else
                        AudioOnBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioOffBtnVisibility)));
                }
            }
        }

        private Visibility castOnBtnVisibility = Visibility.Collapsed;
        public Visibility CastOnBtnVisibility {
            get { return castOnBtnVisibility; }
            set {
                if (value != castOnBtnVisibility)
                {
                    castOnBtnVisibility = value;
                    if (castOnBtnVisibility == Visibility.Visible)
                        CastOffBtnVisibility = Visibility.Collapsed;
                    else
                        CastOffBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CastOnBtnVisibility)));
                }
            }
        }

        private Visibility castOffBtnVisibility = Visibility.Visible;
        public Visibility CastOffBtnVisibility {
            get { return castOffBtnVisibility; }
            set {
                if (value != castOffBtnVisibility)
                {
                    castOffBtnVisibility = value;
                    if (castOffBtnVisibility == Visibility.Visible)
                        CastOnBtnVisibility = Visibility.Collapsed;
                    else
                        CastOnBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CastOffBtnVisibility)));
                }
            }
        }

        private Visibility playRuleBtnVisibility = Visibility.Collapsed;
        public Visibility PlayRuleBtnVisibility {
            get { return playRuleBtnVisibility; }
            set {
                if (value != playRuleBtnVisibility)
                {
                    playRuleBtnVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayRuleBtnVisibility)));
                }
            }
        }

        private Visibility playBtnVisibility = Visibility.Visible;
        public Visibility PlayBtnVisibility {
            get { return playBtnVisibility; }
            set {
                if (value != playBtnVisibility)
                {
                    playBtnVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayBtnVisibility)));
                }
            }
        }

        private Visibility startRecordingBtnVisibility = Visibility.Visible;
        public Visibility StartRecordingBtnVisibility {
            get { return startRecordingBtnVisibility; }
            set {
                if (value != startRecordingBtnVisibility)
                {
                    startRecordingBtnVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StartRecordingBtnVisibility)));
                }
            }
        }

        private Visibility stopRecordingBtnVisibility = Visibility.Collapsed;
        public Visibility StopRecordingBtnVisibility {
            get { return stopRecordingBtnVisibility; }
            set {
                if (value != stopRecordingBtnVisibility)
                {
                    stopRecordingBtnVisibility = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StopRecordingBtnVisibility)));
                }
            }
        }

        private bool recordingEnabled = false;
        public bool RecordingEnabled {
            get { return recordingEnabled; }
            set {
                if (value != recordingEnabled)
                {
                    recordingEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecordingEnabled)));
                }
            }
        }

        private Visibility offlineBtnVisibility = Visibility.Visible;
        public Visibility OfflineBtnVisibility {
            get { return offlineBtnVisibility; }
            set {
                if (value != offlineBtnVisibility)
                {
                    offlineBtnVisibility = value;
                    if (offlineBtnVisibility == Visibility.Visible)
                        OnlineBtnVisibility = Visibility.Collapsed;
                    else
                        OnlineBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OfflineBtnVisibility)));
                }
            }
        }

        private Visibility onlineBtnVisibility = Visibility.Collapsed;
        public Visibility OnlineBtnVisibility {
            get { return onlineBtnVisibility; }
            set {
                if (value != onlineBtnVisibility)
                {
                    onlineBtnVisibility = value;
                    if (onlineBtnVisibility == Visibility.Visible)
                        OfflineBtnVisibility = Visibility.Collapsed;
                    else
                        OfflineBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OnlineBtnVisibility)));
                }
            }
        }

        private Visibility aMBtnVisibility = Visibility.Visible;
        public Visibility AMBtnVisibility {
            get { return aMBtnVisibility; }
            set {
                if (value != aMBtnVisibility)
                {
                    aMBtnVisibility = value;
                    if (aMBtnVisibility == Visibility.Visible)
                        PMBtnVisibility = Visibility.Collapsed;
                    else
                        PMBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AMBtnVisibility)));
                }
            }
        }

        private Visibility pMBtnVisibility = Visibility.Collapsed;
        public Visibility PMBtnVisibility {
            get { return pMBtnVisibility; }
            set {
                if (value != pMBtnVisibility)
                {
                    pMBtnVisibility = value;
                    if (pMBtnVisibility == Visibility.Visible)
                        AMBtnVisibility = Visibility.Collapsed;
                    else
                        AMBtnVisibility = Visibility.Visible;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PMBtnVisibility)));
                }
            }
        }

        public DateTime SelectedTime { get; set; }

        private bool timeTriggerChecked = true;
        public bool TimeTriggerChecked {
            get { return timeTriggerChecked; }
            set {
                timeTriggerChecked = value;
                UpdateSelectedTime();
                AudioClipsView.Refresh();
            }
        }

        public List<string> DaysList { get; set; } = new List<string>(2) { "Today", "Every Day" };

        private string selectedDays = "Today";
        public string SelectedDays {
            get { return selectedDays; }
            set {
                selectedDays = value;
                UpdateSelectedTime();
            }
        }

        private bool amSelected = true;
        public bool AMSelected {
            get { return amSelected; }
            set {
                if (value != amSelected)
                {
                    amSelected = value;
                    UpdateSelectedTime();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AMSelected)));
                }
            }
        }

        private string hourTextStr = "06";
        public string HourTextStr {
            get { return hourTextStr; }
            set {
                if (value != hourTextStr)
                {
                    if (Int32.TryParse(value, out int selectedhour))
                    {
                        if (selectedhour >= 1 && selectedhour <= 12)
                        {
                            if (CheckSelectedTime(value, MinuteTextStr))
                            {
                                hourTextStr = value;
                                UpdateSelectedTime();
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HourTextStr)));
                            }
                        }
                    }
                }
            }
        }

        private string minuteTextStr = "00";
        public string MinuteTextStr {
            get { return minuteTextStr; }
            set {
                if (value != minuteTextStr)
                {
                    if (Int32.TryParse(value, out int selectedMinute))
                    {
                        if (selectedMinute >= 0 && selectedMinute <= 59)
                        {
                            if (CheckSelectedTime(HourTextStr, value))
                            {
                                minuteTextStr = value;
                                UpdateSelectedTime();
                                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MinuteTextStr)));
                            }
                        }    
                    }
                }
            }
        }

        private ObservableCollection<string> ruleNumbers = new ObservableCollection<string>(new List<string>(2) { "New" });
        public ObservableCollection<string> RuleNumbers {
            get { return ruleNumbers; }
            set {
                ruleNumbers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RuleNumbers)));
            }
        }

        public ObservableCollection<string> SessionNames {
            get { return new ObservableCollection<string>(SessionsMgr.GetSessionNames()); }
        }

        private int SelectedRuleNumber = -1; // -1 when "New" rule selected
        private string selectedRuleNumberStr = "New";
        public string SelectedRuleNumberStr {
            get { return selectedRuleNumberStr; }
            set {
                if (value != selectedRuleNumberStr)
                {
                    selectedRuleNumberStr = value ?? "New";
                    if (Int32.TryParse(selectedRuleNumberStr, out SelectedRuleNumber))
                    {
                        if (ActiveRuleset != null)
                        {
                            ActiveRuleset.UpdateStateSelectedRuleAudioClip(SelectedRuleNumber);
                        }
                    }
                    else
                        SelectedRuleNumber = -1;
                    if (AudioClipSelected && ActiveAudioClip.Label.Equals("OK, rule deleted"))
                        ActiveRuleset.UpdateRuleDeletedAudioClip(SelectedRuleNumber);
                    else if (SelectedRuleNumberStr.Equals("New") && ActiveRule.Complete)
                        DuplicateAndConflictCheck();
                    DisplayAudioText(CurrentActivity);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedRuleNumberStr)));
                }     
            }
        }

        private bool ShowCastText = false; // controlled by CastDisplayOnBtn, CastDisplayOffBtn

        private string rulesetText = String.Empty;
        public string CastText
        {
            get { return rulesetText; }
            set {
                rulesetText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CastText)));
            }
        }

        private SolidColorBrush audioTextColour = new SolidColorBrush(Colors.Black);
        public SolidColorBrush AudioTextColour {
            get { return audioTextColour; }
            set {
                audioTextColour = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioTextColour)));
            }
        }

        private SolidColorBrush castTextColour = new SolidColorBrush(Colors.White);
        public SolidColorBrush CastTextColour {
            get { return castTextColour; }
            set {
                castTextColour = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CastTextColour)));
            }
        }

        private string audioText = String.Empty;
        public string AudioText {
            get { return audioText; }
            set {
                audioText = value;
                if (AudioTextUpdating)
                    AudioTextUpdated = true;
                PlayBtnEnabled = !String.IsNullOrEmpty(audioText);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioText)));
            }
        }

        private string sessionName;
        public string SessionName {
            get { return sessionName; }
            set {
                if (sessionName != value)
                {
                    sessionName = value;
                    ChangeSession(SessionName);
                }
            }
        }

        public double MinSpeedRatio { get; } = 0.8;
        public string MinSpeedRatioStr { get; } = "0.8";
        public double MaxSpeedRatio { get; } = 1.2;
        public string MaxSpeedRatioStr { get; } = "1.2";

        private double speedRatio = 1.0;
        public double SpeedRatio {
            get { return speedRatio; }
            set {
                if (speedRatio != value)
                {
                    speedRatio = value;
                    if (CurrentSession != null)
                    {
                        CurrentSession.SpeedRatio = speedRatio;
                        SessionsTableMgr.UpdateSession(CurrentSession);
                    }
                    AudioMgr.SpeedRatio = speedRatio;
                }
            }
        }

        private bool duplicateCheck;
        public bool DuplicateCheck {
            get { return duplicateCheck; }
            set {
                duplicateCheck = value;
                if (CurrentSession != null)
                {
                    CurrentSession.Ruleset.CheckDuplicates = duplicateCheck;
                    AudioClipsView.Refresh();
                }
                    

            }
        }

        private bool conflictCheck;
        public bool ConflictCheck {
            get { return conflictCheck; }
            set {
                conflictCheck = value;
                if (CurrentSession != null)
                {
                    CurrentSession.Ruleset.CheckConflicts = conflictCheck;
                    AudioClipsView.Refresh();
                }
            }
        }

        private bool castDisplayEnabled;
        public bool CastDisplayEnabled {
            get { return castDisplayEnabled; }
            set {
                if (castDisplayEnabled != value)
                {
                    castDisplayEnabled = value;
                    CurrentSession.CastDisplayEnabled = castDisplayEnabled;
                    SessionsTableMgr.UpdateSession(CurrentSession);
                    if (castDisplayEnabled)
                    {
                        if (CastDisplay == null)
                            CastDisplay = new DisplayWinView();
                        CastDisplay.Show();
                        CastOnBtnVisibility = Visibility.Collapsed;
                        CastOffBtnVisibility = Visibility.Visible;
                    }
                    else
                    {
                        DisplayCastText(false);
                        CastTextColour = new SolidColorBrush(Colors.White);
                        ShowCastText = false;
                        CastDisplay?.Hide();
                        CastOnBtnVisibility = Visibility.Collapsed;
                        CastOffBtnVisibility = Visibility.Visible;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CastDisplayEnabled)));
                }  
            }
        }

        private Button ActiveActionBtn;
        private Button ActiveTriggerBtn;
        private Button ActiveAudioClipBtn;
        private int ConflictRuleNumber = -1;
        private int DuplicateRuleNumber = -1;
        private AudioClip ActiveAudioClip = null;
        private DeviceRule ActiveRule = null;
        private AudioClip SelectedTriggerClip = null;
        private AudioClip SelectedActionClip = null;
        private bool AudioClipSelected = false;
        private string CurrentActivity = "State";
        private bool IsRecording = false;
        private string SelectedTimeStr;
        private DisplayWinView CastDisplay;
        public Session CurrentSession;
        public Ruleset ActiveRuleset;
        private bool AudioTextUpdating = false;
        private bool AudioTextUpdated = false;
        public DashboardViewModel()
        {
            if (AudioMgr.RecordingDeviceExists)
                RecordingEnabled = true;
            else
                RecordingEnabled = false;
            CreateNewActiveRule();
            if (SessionsMgr.SessionsLoaded && SessionsMgr.GetSessionNames().Count > 0)
            {
                sessionName = SessionsMgr.GetSessionNames()[0];
                ChangeSession(SessionName);
            }
            else
                EventSystem.Subscribe<SessionsLoaded>(OnSessionsLoaded);
            EventSystem.Subscribe<AudioMuted>(OnAudioMuted);
            EventSystem.Subscribe<SessionChanged>(OnSessionChanged);
            EventSystem.Subscribe<SessionInventoryChanged>(OnSessionInventoryChanged);
            EventSystem.Subscribe<SessionsInventoryChanged>(OnSessionsInventoryChanged);
            EventSystem.Subscribe<AudioClipsInventoryChanged>(OnAudioClipsInventoryChanged);
            UpdateSelectedTime();
        }

        private bool SessionClipsFilter(object item)
        {
            AudioClip audioClip = item as AudioClip;
            if (!audioClip.IsVisible)
                return false;
            if (TimeTriggerChecked && audioClip.Category.Equals("Trigger"))
                return false;
            if (!ActiveRuleset.CheckConflicts && audioClip.Label.Equals("Rule conflicts with another rule")) 
                return false;
            else if (!ActiveRuleset.CheckDuplicates && audioClip.Label.Equals("Rule is a duplicate"))
                return false;
            else
                return true;
        }

        private void OnSessionsLoaded(SessionsLoaded sl)
        {
            if (SessionsMgr.GetSessionNames().Count > 0)
            {
                sessionName = SessionsMgr.GetSessionNames()[0];
                ChangeSession(SessionName);
            }
        }

        private void OnAudioClipsInventoryChanged(AudioClipsInventoryChanged ac)
        {
            RefreshAudioClips();
        }

        private void OnSessionChanged(SessionChanged sc)
        {
            if (sc.SessionName.Equals(SessionName))
                ChangeSession(SessionName);
        }

        private void OnSessionInventoryChanged(SessionInventoryChanged sic)
        {
            if (sic.SessionName.Equals(SessionName))
                RefreshAudioClips();
        }

        private void OnSessionsInventoryChanged(SessionsInventoryChanged sic)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionNames)));
        }

        private void RefreshAudioClips()
        {
            if (CurrentSession != null)
            {
                AudioClips = CurrentSession.SessionAudioClips;
                AudioClipsView = CollectionViewSource.GetDefaultView(AudioClips);
                if (ActiveRuleset != null)
                {
                    AudioClipsView.Filter = SessionClipsFilter;
                    AudioClipsView.Refresh();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
            }
        }

        public void OnAudioClipBtnClick(Button btn)
        {
            AudioTextUpdating = false;
            AudioTextUpdated = false;
            string label = btn.Content?.ToString();
            switch (btn.Tag)
            {
                case "Trigger":
                    if (ActiveTriggerBtn != null)
                    {
                        ActiveTriggerBtn.Focusable = true;
                        ActiveTriggerBtn.Foreground = new SolidColorBrush(Colors.Black);
                        ActiveTriggerBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveTriggerBtn.Background.ToString()));
                    }
                    if (ActiveAudioClipBtn != null)
                    {
                        ActiveAudioClipBtn.Focusable = true;
                        ActiveAudioClipBtn.Foreground = new SolidColorBrush(Colors.Black);
                        ActiveAudioClipBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveAudioClipBtn.Background.ToString()));
                    }
                    ActiveTriggerBtn = btn;
                    ActiveTriggerBtn.Focusable = false;
                    ActiveTriggerBtn.Foreground = new SolidColorBrush(Colors.White);
                    ActiveTriggerBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveTriggerBtn.Background.ToString()));
                    LoadTrigger(label);
                    break;
                case "Action":
                    if (ActiveActionBtn != null)
                    {
                        ActiveActionBtn.Focusable = true;
                        ActiveActionBtn.Foreground = new SolidColorBrush(Colors.Black);
                        ActiveActionBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveActionBtn.Background.ToString()));
                    }
                    if (ActiveAudioClipBtn != null)
                    {
                        ActiveAudioClipBtn.Focusable = true;
                        ActiveAudioClipBtn.Foreground = new SolidColorBrush(Colors.Black);
                        ActiveAudioClipBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveAudioClipBtn.Background.ToString()));
                    }
                    ActiveActionBtn = btn;
                    ActiveActionBtn.Focusable = false;
                    ActiveActionBtn.Foreground = new SolidColorBrush(Colors.White);
                    ActiveActionBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveActionBtn.Background.ToString()));
                    LoadAction(label);
                    break;
                default:
                    if (btn.Content.Equals("State selected rule") && SelectedRuleNumberStr.Equals("New"))
                    {
                        var messageWin = new MessageWin("Select Audio Clip", "Please select a rule number.");
                        messageWin.Show();
                    }
                    else
                    {
                        if (ActiveAudioClipBtn != null)
                        {
                            ActiveAudioClipBtn.Focusable = true;
                            ActiveAudioClipBtn.Foreground = new SolidColorBrush(Colors.Black);
                            ActiveAudioClipBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveAudioClipBtn.Background.ToString()));

                        }
                        if (ActiveActionBtn != null)
                        {
                            ActiveActionBtn.Focusable = true;
                            ActiveActionBtn.Foreground = new SolidColorBrush(Colors.Black);
                            ActiveActionBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveActionBtn.Background.ToString()));
                        }
                        if (ActiveTriggerBtn != null)
                        {
                            ActiveTriggerBtn.Focusable = true;
                            ActiveTriggerBtn.Foreground = new SolidColorBrush(Colors.Black);
                            ActiveTriggerBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveTriggerBtn.Background.ToString()));
                        }
                        ActiveAudioClipBtn = btn;
                        ActiveAudioClipBtn.Focusable = false;
                        ActiveAudioClipBtn.Foreground = new SolidColorBrush(Colors.White);
                        ActiveAudioClipBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(ActiveAudioClipBtn.Background.ToString()));
                        LoadAudioClip(label);
                    }
                    break;
            }
            // PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
        private void OnAudioMuted(AudioMuted audioMuted)
        {
            AudioTextColour = new SolidColorBrush(Colors.Black);
        }

        public string CheckHours(string hours)
        {
            if (Int32.TryParse(hours, out int selectedHour))
            {
                if (selectedHour >= 0 && selectedHour <= 12)
                {
                    hourTextStr = hours;
                    UpdateSelectedTime();
                }
            }
            return HourTextStr;
        }

        public string CheckMinutes(string minutesStr)
        {
            if (Int32.TryParse(minutesStr, out int selectedMinute))
            {
                if (selectedMinute >= 0 && selectedMinute <= 59)
                {
                    // if single digit between "1" and "5", assume tens of minutes
                    if (selectedMinute > 0 && selectedMinute <= 5 && !minutesStr.StartsWith("0"))
                        minuteTextStr = minutesStr + "0";
                    else
                        minuteTextStr = minutesStr;
                    UpdateSelectedTime();
                }
            }
            return MinuteTextStr;
        } 

        public void LoadTrigger(string label)
        {
            AudioClipSelected = false;
            SelectedTriggerClip = TimeTriggerChecked 
                ? AudioClipsMgr.GetTimeTriggerClip(label) 
                : AudioClips?.Where((x) => x.Label.Equals(label)).FirstOrDefault();
            if (SelectedTriggerClip != null)
            {
                ActiveRule.TriggerClip = SelectedTriggerClip;
                if (ActiveRule.Complete)
                {
                    if (SelectedRuleNumberStr.Equals("New"))
                    {
                        DuplicateAndConflictCheck();
                        UpdatePlayBtnsStatus();
                    }
                    AudioTextColour = new SolidColorBrush(Colors.Black);
                    AudioText = GetAudioText();
                }
            }   
        }

        public void LoadAction(string label)
        {
            AudioClipSelected = false;
            ActiveAudioClip = null;
            SelectedActionClip = AudioClips.Where((x) => x.Label.Equals(label)).FirstOrDefault();
            if (SelectedActionClip != null)
            {
                ActiveRule.ActionClip = SelectedActionClip;
                if (ActiveRule.Complete)
                {
                    if (SelectedRuleNumberStr.Equals("New"))
                        DuplicateAndConflictCheck();
                    UpdatePlayBtnsStatus();
                    AudioTextColour = new SolidColorBrush(Colors.Black);
                    AudioText = GetAudioText();
                }   
            }
        }

        public void LoadAudioClip(string label)
        {
            AudioClipSelected = true;
            SelectedActionClip = null;
            ActiveRule.ActionClip = null;
            ActiveAudioClip = AudioClips.Where((x) => x.Label.Equals(label)).FirstOrDefault();
            CurrentActivity = "State";
            if (CurrentSession.IsRuleset)
            {
                ActiveRuleset = CurrentSession.Ruleset;
                switch (label)
                {
                    case "OK, rule deleted":
                        CurrentActivity = "Confirm";
                        if (!SelectedRuleNumberStr.Equals("New"))
                            ActiveRuleset.UpdateRuleDeletedAudioClip(SelectedRuleNumber);
                        StatePlayBtnEnabled = false;
                        ConfirmPlayBtnEnabled = true;
                        CheckPlayBtnEnabled = true;
                        break;
                    case "OK, New rule added":
                        CurrentActivity = "Confirm";
                        StatePlayBtnEnabled = false;
                        ConfirmPlayBtnEnabled = true;
                        CheckPlayBtnEnabled = false;
                        break;
                    case "Which rule to change?":
                        ActiveRuleset.UpdateWhichRuleAudioClip();
                        StatePlayBtnEnabled = true;
                        ConfirmPlayBtnEnabled = false;
                        CheckPlayBtnEnabled = false;
                        break;
                    default:
                        StatePlayBtnEnabled = true;
                        ConfirmPlayBtnEnabled = false;
                        CheckPlayBtnEnabled = false;
                        break;
                }
            }
            AudioTextColour = new SolidColorBrush(Colors.Black);
            AudioText = GetAudioText();
        }

        public void DisplayAudioText(string activity)
        {
            CurrentActivity = activity;
            if (!AudioTextUpdating)
                AudioText = GetAudioText();
            AudioTextColour = new SolidColorBrush(Colors.Black);
        }

        public void UpdateSelectedTime()
        {
            if (TimeTriggerChecked)
            {
                if (HourTextStr.Length == 1)
                    HourTextStr = "0" + HourTextStr;
                if (MinuteTextStr.Length == 1)
                    MinuteTextStr = "0" + MinuteTextStr;
                SelectedTimeStr = AMSelected
                    ? HourTextStr + ":" + MinuteTextStr + " AM"
                    : HourTextStr + ":" + MinuteTextStr + " PM";
                SelectedTime = DateTime.ParseExact(SelectedTimeStr, "hh:mm tt", CultureInfo.InvariantCulture);
                string triggerText = SelectedTimeStr
                            .Replace(" AM", "am " + SelectedDays.ToLower())
                            .Replace(" PM", "pm " + SelectedDays.ToLower());
                triggerText = triggerText.StartsWith("0") ? "At " + triggerText.Substring(1) : "At " + triggerText;
                LoadTrigger(triggerText);
            }  
        }

        private bool CheckSelectedTime(string hours, string minutes)
        {
            string triggerText = AMSelected 
                ? hours + ":" + minutes + "am " + SelectedDays.ToLower() 
                : hours + ":" + minutes + "pm " + SelectedDays.ToLower();
            triggerText = triggerText.StartsWith("0") 
                ? "At " + triggerText.Substring(1)
                : "At " + triggerText;
            return AudioClipsMgr.GetTimeTriggerClip(triggerText) != null ? true : false;
        }

        private string GetAudioText()
        {
            if (!AudioClipSelected && SelectedTriggerClip != null && SelectedActionClip != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(ActiveRule.GetAudioText(CurrentActivity, SelectedRuleNumber));
                if (ActiveRuleset.CheckConflicts && ConflictRuleNumber > 0)
                    sb.Append("\n\n**** CONFLICTS WITH RULE " + ConflictRuleNumber.ToString() + " ****");
                else if (ActiveRuleset.CheckDuplicates && DuplicateRuleNumber > 0)
                    sb.Append("\n\n**** DUPLICATE OF RULE " + DuplicateRuleNumber.ToString() + " ****");
                return sb.ToString();
            }
            else if (AudioClipSelected && ActiveAudioClip != null)
            {
                if (ActiveAudioClip.Label.Equals("OK, rule deleted") && SelectedRuleNumberStr.Equals("New"))
                    return "*** ERROR: Cannot delete a new rule ***";
                else if (CurrentActivity.Equals("State"))
                    return ActiveAudioClip.StateText;
                else if (CurrentActivity.Equals("Confirm"))
                    return ActiveAudioClip.ConfirmText;
                else if (CurrentActivity.Equals("Check"))
                    return ActiveAudioClip.CheckText;
            }
            return String.Empty;
        }

        public void PlayRuleAudio()
        {
            string audioFile = AudioMgr.UseRecordings
                ? ActiveRule.GetRuleAudioFile(CurrentActivity, SelectedRuleNumber)
                : GoogleTextToSpeechMgr.GenerateSpeech(AudioText);
            if (File.Exists(audioFile))
            {
                AudioMgr.PlayAudioClip(audioFile);
                AudioTextColour = new SolidColorBrush(Colors.Red);
                Logger.AddLogEntry(LogCategory.CONV_AUDIO, AudioText);
            }
            else
            {
                string msg = String.Format("PlayRuleAudio: File \'{0}\' not found", audioFile);
                Logger.AddLogEntry(LogCategory.ERROR, msg);
                var messageWin = new MessageWin("Play Audio", msg);
                messageWin.Show();
            }    
        }

        public void PlayAudioClip()
        {
            string audioFile = String.Empty;
            if (!AudioMgr.UseRecordings)
            {
                audioFile = GoogleTextToSpeechMgr.GenerateSpeech(AudioText);
                if (String.IsNullOrEmpty(audioFile))
                    return; // GenerateSpeech displays error message
            }
            else if (ActiveAudioClip != null)
            {
                if (CurrentActivity.Equals("State"))
                {
                    audioFile = ActiveAudioClip.StateAudioFile;
                }
                else if (CurrentActivity.Equals("Confirm"))
                {
                    if (ActiveAudioClip.Label.Equals("OK, rule deleted")
                        || ActiveAudioClip.Label.Equals("OK, New rule added")
                        || ActiveAudioClip.Label.Equals("OK"))
                    {
                        audioFile = ActiveAudioClip.ConfirmAudioFile;
                    }
                }
                else if (CurrentActivity.Equals("Check"))
                    audioFile = ActiveAudioClip.CheckAudioFile;
            }
            if (File.Exists(audioFile) || File.Exists(audioFile + ".mp3"))
            {
                AudioMgr.PlayAudioClip(audioFile);
                AudioTextColour = new SolidColorBrush(Colors.Red);
                Logger.AddLogEntry(LogCategory.CONV_AUDIO, AudioText);
            }
            else
            {
                string msg = String.Format("PlayAudioClip: File \'{0}\' not found", audioFile);
                Logger.AddLogEntry(LogCategory.ERROR, msg);
                var messageWin = new MessageWin("Play Audio", msg);
                messageWin.Show();
            }
        }

        private void CreateNewActiveRule()
        {
            ActiveRule = new DeviceRule
            {
                DeviceName = DeviceName,
                TriggerClip = SelectedTriggerClip
            };
            SelectedActionClip = null;
            ActiveRule.ActionClip = SelectedActionClip;
        }

        private void UpdateRules()
        {
            if (AudioClipSelected)
            {
                if (ActiveAudioClip.Label.Equals("OK, rule deleted"))
                {
                    ActiveRuleset.DeleteRule(SelectedRuleNumber);
                    CastText = ActiveRuleset?.GetRulesetText();
                    UpdateCastDisplayWindow();
                    ActiveAudioClip = null;
                    AudioClipSelected = false;
                    SelectedRuleNumberStr = "New";
                    RuleNumbers = new ObservableCollection<string>(ActiveRuleset.GetRuleNumbers());
                    // Disable Play buttons
                    StatePlayBtnEnabled = false;
                    ConfirmPlayBtnEnabled = false;
                    CheckPlayBtnEnabled = false;
                    // Clear any highlighting on buttons
                    if (ActiveAudioClipBtn != null)
                    {
                        ActiveAudioClipBtn.Focusable = true;
                        ActiveAudioClipBtn.Foreground = new SolidColorBrush(Colors.Black);
                        ActiveAudioClipBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.StatementColour));
                        ActiveAudioClipBtn = null;
                    }
                }
            }
            else if (ActiveRule != null && ActiveRule.Complete)
            {
                if (SelectedRuleNumberStr.Equals("New"))
                {
                    if (DuplicateRuleNumber > 0 || ConflictRuleNumber > 0)
                        return;
                    else
                    {
                        ActiveRuleset.AddRule(ActiveRule);
                        RuleNumbers = new ObservableCollection<string>(ActiveRuleset.GetRuleNumbers());
                    }
                }
                else if (SelectedRuleNumber > 0 && SelectedRuleNumber < 7)
                {
                    ActiveRuleset.ChangeRule(SelectedRuleNumber, ActiveRule);
                }
                else
                {
                    Logger.AddLogEntry(LogCategory.ERROR, "Rule number exceeds six");
                    return;
                }
                CreateNewActiveRule();
                CastText = ActiveRuleset?.GetRulesetText();
                UpdateCastDisplayWindow();
                if (!TimeTriggerChecked)
                    SelectedTriggerClip = null;
                SelectedActionClip = null;

                // Clear highlighting on buttons
                if (ActiveActionBtn != null)
                {
                    ActiveActionBtn.Focusable = true;
                    ActiveActionBtn.Foreground = new SolidColorBrush(Colors.Black);
                    ActiveActionBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.ActionColour));
                    ActiveActionBtn = null;
                }
                if (ActiveTriggerBtn != null)
                {
                    ActiveTriggerBtn.Focusable = true;
                    ActiveTriggerBtn.Foreground = new SolidColorBrush(Colors.Black);
                    ActiveTriggerBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.ConditionColour));
                    ActiveTriggerBtn = null;
                } 
            }
        }

        private void UpdatePlayBtnsStatus()
        {
            StatePlayBtnEnabled = true;
            if ((ActiveRuleset.CheckConflicts && ConflictRuleNumber > 0) 
                || (ActiveRuleset.CheckDuplicates && DuplicateRuleNumber > 0))
            {
                ConfirmPlayBtnEnabled = false;
                CheckPlayBtnEnabled = false;
            }
            else
            {
                ConfirmPlayBtnEnabled = true;
                CheckPlayBtnEnabled = true;
            }
        }

        private void DuplicateAndConflictCheck()
        {
            if (ActiveRuleset.CheckDuplicates)
                DuplicateRuleNumber = ActiveRuleset.CheckForDuplicates(ActiveRule);
            if (ActiveRuleset.CheckConflicts)
                ConflictRuleNumber = ActiveRuleset.CheckForConflicts(ActiveRule);
        }

        public void DisplayCastText(bool cast)
        {
            ShowCastText = cast;
            CastTextColour = ShowCastText ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.White);
            if (ShowCastText)
                Logger.AddLogEntry(LogCategory.INFO, String.Format("Rules displayed"));
            else
                Logger.AddLogEntry(LogCategory.INFO, String.Format("Rules hidden"));
            UpdateCastDisplayWindow();
        }

        private void UpdateCastDisplayWindow()
        {
            if (CastDisplay != null)
            {
                if (CastDisplayEnabled && ShowCastText)
                    CastDisplay.SetText(CastText);
                else
                    CastDisplay.SetText(String.Empty);
            }
        }

        private ICommand displayRulesetBtnClick;
        public ICommand DisplayRulesetBtnClick
        {
            get
            {
                displayRulesetBtnClick = displayRulesetBtnClick ?? new RelayCommand(param => DisplayRuleset(param), param => true);
                return displayRulesetBtnClick;
            }
            set { displayRulesetBtnClick = value; }
        }

        private void DisplayRuleset(object obj)
        {
            ShowCastText = !ShowCastText;
            CastTextColour = ShowCastText ? new SolidColorBrush(Colors.Orange) : new SolidColorBrush(Colors.White);
            UpdateCastDisplayWindow();
        }

        private ICommand audioBtnClick;
        public ICommand AudioBtnClick {
            get {
                audioBtnClick = audioBtnClick ?? new RelayCommand(param => DisplayText(param), param => true);
                return audioBtnClick;
            }
            set { audioBtnClick = value; }
        }
        private void DisplayText(object obj)
        {
            if (!(AudioClipSelected && ActiveAudioClip.Label.Equals("OK, rule deleted") && SelectedRuleNumberStr.Equals("New")))
            {
                CurrentActivity = obj as String;
                DisplayAudioText(CurrentActivity);
            }
        }

        private ICommand playBtnClick;
        public ICommand PlayBtnClick {
            get {
                playBtnClick = playBtnClick ?? new RelayCommand(param => PlayClip(param), param => true);
                return playBtnClick;
            }
            set { playBtnClick = value; }
        }

        private void PlayClip(object obj)
        {
            string param = obj as String;
            if (AudioTextUpdated && true || (param.Equals("State") || param.Equals("Play")))
            {
                PlayAudioClip();
                AudioTextUpdated = false;
                AudioTextUpdating = false;
            }
            else if (AudioTextUpdating)
            {
                AudioTextUpdating = false;
                AudioText = "";
            }
            else if (!(AudioClipSelected && ActiveAudioClip.Label.Equals("OK, rule deleted") && SelectedRuleNumberStr.Equals("New")))
            {
                DisplayAudioText(CurrentActivity);
                if (!AudioMgr.AudioMuted)
                {
                    if (AudioClipSelected)
                        PlayAudioClip();
                    else
                        PlayRuleAudio();
                }
                if (CurrentActivity.Equals("Confirm"))
                    UpdateRules();
            }
        }

        private ICommand chimesBtnClick;
        public ICommand ChimesBtnClick {
            get {
                chimesBtnClick = chimesBtnClick ?? new RelayCommand(param => PlayChimes(param), param => true);
                return chimesBtnClick;
            }
            set { chimesBtnClick = value; }
        }

        public void PlayChimes(object obj)
        {
            if (!AudioMgr.AudioMuted)
                AudioMgr.PlayAudioClip(Path.Combine(DirectoryMgr.AudioClipsDirectory, @"Sound Files\Chimes.mp3"));
        }

        private ICommand toggleTimePeriodBtnClick;
        public ICommand ToggleTimePeriodBtnClick {
            get {
                toggleTimePeriodBtnClick = toggleTimePeriodBtnClick ?? new RelayCommand(param => ToggleTimePeriod(param), param => true);
                return toggleTimePeriodBtnClick;
            }
            set { toggleTimePeriodBtnClick = value; }
        }

        private void ToggleTimePeriod(object obj)
        {
            if (AMSelected)
                AMBtnVisibility = Visibility.Collapsed;
            else
                PMBtnVisibility = Visibility.Collapsed;
            AMSelected = !AMSelected;
        }

        private ICommand toggleCastDisplayBtnClick;
        public ICommand ToggleCastDisplayBtnClick {
            get {
                toggleCastDisplayBtnClick = toggleCastDisplayBtnClick ?? new RelayCommand(param => ToggleCastDisplay(param), param => true);
                return toggleCastDisplayBtnClick;
            }
            set { toggleCastDisplayBtnClick = value; }
        }

        private void ToggleCastDisplay(object obj)
        {
            if (obj.ToString().Equals("true"))
            {
                CastOnBtnVisibility = Visibility.Visible;
                DisplayCastText(true);
            }
            else
            {
                CastOnBtnVisibility = Visibility.Collapsed;
                DisplayCastText(false);
            }
        }

        private ICommand toggleKeepAliveBtnClick;
        public ICommand ToggleKeepAliveBtnClick {
            get {
                toggleKeepAliveBtnClick = toggleKeepAliveBtnClick ?? new RelayCommand(param => ToggleKeepAlive(param), param => true);
                return toggleKeepAliveBtnClick;
            }
            set { toggleKeepAliveBtnClick = value; }
        }

        private void ToggleKeepAlive(object obj)
        {
            KeepAlive = !KeepAlive;
        }

        private ICommand toggleAudioBtnClick;
        public ICommand ToggleAudioBtnClick {
            get {
                toggleAudioBtnClick = toggleAudioBtnClick ?? new RelayCommand(param => ToggleAudio(param), param => true);
                return toggleAudioBtnClick;
            }
            set { toggleAudioBtnClick = value; }
        }

        private void ToggleAudio(object obj)
        {
            if (AudioMgr.AudioMuted)
                AudioOnBtnVisibility = Visibility.Visible;
            else
                AudioOnBtnVisibility = Visibility.Collapsed;
            AudioMgr.AudioMuted = !AudioMgr.AudioMuted;
        }

        private ICommand toggleOnlineModeBtnClick;
        public ICommand ToggleOnlineModeBtnClick {
            get {
                toggleOnlineModeBtnClick = toggleOnlineModeBtnClick ?? new RelayCommand(param => ToggleOnlineMode(param), param => true);
                return toggleOnlineModeBtnClick;
            }
            set { toggleOnlineModeBtnClick = value; }
        }

        private void ToggleOnlineMode(object obj)
        {
            if (AudioMgr.UseRecordings)
                OfflineBtnVisibility = Visibility.Collapsed;
            else
                OfflineBtnVisibility = Visibility.Visible;
            AudioMgr.UseRecordings = !AudioMgr.UseRecordings;
            AudioTextBoxIsReadOnly = !AudioTextBoxIsReadOnly;
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
            if (!IsRecording)
            {
                if (AudioMgr.StartRecording())
                {
                    StartRecordingBtnVisibility = Visibility.Collapsed;
                    StopRecordingBtnVisibility = Visibility.Visible;
                    IsRecording = true;
                }
            }
            else
            {
                AudioMgr.StopRecording();
                StopRecordingBtnVisibility = Visibility.Collapsed;
                StartRecordingBtnVisibility = Visibility.Visible;
                IsRecording = false;
            }
        }

        public void OnAudioTextHasFocus()
        {
            if (!AudioTextBoxIsReadOnly)
                AudioTextUpdating = true;
        }

        public void ChangeSession(string name)
        {
            if (SessionsMgr.SessionsLoaded)
            {
                CurrentSession = SessionsMgr.GetSession(name);
                if (CurrentSession != null)
                {
                    ShowCastText = false;
                    CastDisplayEnabled = CurrentSession.CastDisplayEnabled;
                    KeepAlive = CurrentSession.KeepAlive;
                    SpeedRatio = CurrentSession.SpeedRatio;
                    AudioClips = CurrentSession.SessionAudioClips;
                    AudioClipsView = CollectionViewSource.GetDefaultView(AudioClips);
                    ActiveAudioClip = null;
                    ActiveTriggerBtn = null;
                    AudioClipSelected = false;
                    AudioMgr.SessionName = name;
                    AudioText = String.Empty;
                    if (CurrentSession.IsRuleset)
                    {
                        PlayRuleBtnVisibility = Visibility.Visible;
                        PlayBtnVisibility = Visibility.Collapsed;
                        ActiveRuleset = CurrentSession.Ruleset;
                        CreateNewActiveRule();
                        if (ActiveRuleset != null)
                        {
                            ActiveRule.DeviceName = ActiveRuleset.DeviceName;
                            DeviceName = ActiveRuleset.DeviceName;
                            AudioClipsView.Filter = SessionClipsFilter;
                            AudioClipsView.Refresh();
                            RuleNumbers = new ObservableCollection<string>(ActiveRuleset.GetRuleNumbers());
                            DuplicateRuleNumber = -1;
                            ConflictRuleNumber = -1;
                            SelectedRuleNumberStr = "New";
                            ActiveRuleset.UpdateListRulesAudioClip();
                            ActiveRuleset.UpdateWhichRuleAudioClip();
                            CastText = ActiveRuleset.GetRulesetText();
                            RulesetComponentsEnabled = true;
                            ScreenTitle = "Ruleset Dashboard";
                        }
                    }
                    else
                    {
                        DeviceName = String.Empty; 
                        PlayRuleBtnVisibility = Visibility.Collapsed;
                        PlayBtnVisibility = Visibility.Visible;
                        DuplicateRuleNumber = -1;
                        ConflictRuleNumber = -1;
                        ActiveRuleset = null;
                        CastText = String.Empty;
                        StatePlayBtnEnabled = false;
                        ConfirmPlayBtnEnabled = false;
                        CheckPlayBtnEnabled = false;
                        RulesetComponentsEnabled = false;
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
                    Logger.AddLogEntry(LogCategory.INFO, String.Format("Session changed to \"{0}\"", SessionName));
                }
            }
        }
    }
}
