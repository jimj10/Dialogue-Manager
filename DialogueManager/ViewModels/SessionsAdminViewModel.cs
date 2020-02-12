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
using DialogueManager.Models;
using DialogueManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DialogueManager.ViewModels
{
    
    class SessionsAdminViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AudioClip> AudioClips { get; private set; }
        public ICollectionView AudioclipsView { get; set; }

        public ObservableCollection<AudioClip> SessionClips { get; private set; }

        private string selectedSessionName;
        public string SelectedSessionName {
            get { return selectedSessionName; }
            set {
                if (selectedSessionName != value)
                {
                    selectedSessionName = value;
                    SessionName = value;
                    ChangeSession(SelectedSessionName);
                }
            }
        }

        private string sessionName;
        public string SessionName {
            get { return sessionName; }
            set {
                if (sessionName != value)
                {
                    sessionName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionName)));
                }
            }
        }

        public ObservableCollection<string> SessionNames { get { return new ObservableCollection<string>(SessionsMgr.GetSessionNames()); } }

        public ICollectionView SessionClipsView { get; set; }

        private bool isRuleset;

        public bool IsRuleset {
            get { return isRuleset; }
            set {
                isRuleset = value;
                if (AudioclipsView != null)
                {
                    AudioclipsView.Refresh();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRuleset)));
                }
            }
        }

        private List<Button> SelectedAudioClipButtons = new List<Button>();
        private List<Button> SelectedSessionClipButtons = new List<Button>();
        private AudioClip SelectedAudioClip;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool newSession = true;
        private string originalSessionName;
        private MessageDialogWin messageDialogWin;
        private Session SelectedSession;
        public SessionsAdminViewModel()
        {
            if (SessionsMgr.GetSessionNames().Count > 0)
            {
                ChangeSession(SessionsMgr.GetSessionNames()[0]);
                selectedSessionName = SessionsMgr.GetSessionNames()[0];
                SessionName = SelectedSessionName;
            }
            if (SelectedSession == null)
                ChangeSession(null);
            EventSystem.Subscribe<AudioClipsInventoryChanged>(OnAudioClipsInventoryChanged);
        }

        private void OnAudioClipsInventoryChanged(AudioClipsInventoryChanged ac)
        {
            SessionClips = new ObservableCollection<AudioClip>(SelectedSession.SessionAudioClips);
            SessionClipsView = CollectionViewSource.GetDefaultView(SessionClips);
            UpdateAudioClips();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionClips)));
        }

        private void ChangeSession(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                SelectedSession = null;
                SessionName = String.Empty;
                selectedSessionName = null;
                newSession = true;
                IsRuleset = false;
                SessionClips = new ObservableCollection<AudioClip>();
                SessionClipsView = CollectionViewSource.GetDefaultView(SessionClips);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSessionName)));
            } 
            else
            {
                SelectedSession = SessionsMgr.GetSessionCopy(name);
                originalSessionName = name;
                selectedSessionName = name;
                SessionName = name;
                if (SelectedSession != null)
                {
                    IsRuleset = SelectedSession.IsRuleset;
                    newSession = false;
                    SessionClips = new ObservableCollection<AudioClip>(SelectedSession.SessionAudioClips);
                    SessionClipsView = CollectionViewSource.GetDefaultView(SessionClips);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionNames)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSessionName)));
                }
            }
            UpdateAudioClips();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionClips)));
        }

        private void UpdateAudioClips()
        {
            // Order alphabetically
            AudioClips = new ObservableCollection<AudioClip>(AudioClipsMgr.AudioClips.OrderBy(AudioClip => AudioClip.Label));
            AudioclipsView = CollectionViewSource.GetDefaultView(AudioClips);
            AudioclipsView.Filter = AudioClipsFilter;
            AudioclipsView.Refresh();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AudioClips)));
        }

        private bool AudioClipsFilter(object item)
        {
            AudioClip audioClip = item as AudioClip;
            if (SessionClips != null && SessionClips.FirstOrDefault(x => x.Label.Equals(audioClip.Label)) != null)
                return false; 
            if (!IsRuleset)
                return  audioClip.Category.Equals("Standard");
            else
                return (!audioClip.Label.Contains("OK, rule deleted")
                    && !audioClip.Label.Contains("List rules")
                    && !audioClip.Label.Contains("State selected rule"));
        }

        public void OnAudioClipBtnClick(Button btn)
        {
            var audioClipLabel = btn.Content?.ToString();
            if (!SelectedAudioClipButtons.Contains(btn))
            {
                if (SelectedAudioClipButtons.Count == 0 
                    || Keyboard.IsKeyDown(Key.LeftShift) 
                    || Keyboard.IsKeyDown(Key.RightShift))
                {
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString()));
                    SelectedAudioClipButtons.Add(btn);
                }
                else if (SelectedAudioClipButtons.Count > 0)
                {
                    foreach (var button in SelectedAudioClipButtons)
                    {
                        button.Foreground = new SolidColorBrush(Colors.Black); 
                        button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(button.Background.ToString()));
                    }
                    SelectedAudioClipButtons.Clear();
                    btn.Foreground = new SolidColorBrush(Colors.White);
                    btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString())));
                    SelectedAudioClipButtons.Add(btn);
                }
                
            }
            else if (!(SelectedAudioClipButtons.Contains(btn) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
            {
                foreach (var button in SelectedAudioClipButtons)
                {
                    button.Foreground = new SolidColorBrush(Colors.White); 
                    button.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString())));
                }
                SelectedAudioClipButtons.Clear();
            }
            SelectedAudioClip = AudioClips.Single(x => x.Label.Equals(audioClipLabel));
        }

        public void OnSessionClipBtnClick(Button btn)
        {
            var audioClipLabel = btn.Content?.ToString();
            SelectedAudioClip = AudioClips.Single(x => x.Label.Equals(audioClipLabel));
            if (!SelectedSessionClipButtons.Contains(btn))
            {
                if (SelectedSessionClipButtons.Count == 0 || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    btn.Foreground = new SolidColorBrush(Colors.White); 
                    btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString()));
                    SelectedSessionClipButtons.Add(btn);
                }
                else if (SelectedSessionClipButtons.Count > 0)
                {
                    foreach (var button in SelectedSessionClipButtons)
                    {
                        button.Foreground = new SolidColorBrush(Colors.Black); 
                        button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(button.Background.ToString()));
                    }
                    SelectedSessionClipButtons.Clear();
                    btn.Foreground = new SolidColorBrush(Colors.White); 
                    btn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString()));
                    SelectedSessionClipButtons.Add(btn);
                }

            }
            else if (!(SelectedSessionClipButtons.Contains(btn) && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))))
            {
                foreach (var button in SelectedSessionClipButtons)
                {
                    button.Foreground = new SolidColorBrush(Colors.White); 
                    button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString()));
                }
                SelectedSessionClipButtons.Clear();
            }
        }

        private ICommand newBtnClick;
        public ICommand NewBtnClick {
            get {
                newBtnClick = newBtnClick ?? new RelayCommand(param => SetNewSession(param), param => true);
                return newBtnClick;
            }
            set { newBtnClick = value; }
        }

        private void SetNewSession(object obj)
        {
            ChangeSession(null);
        }

        private ICommand deleteBtnClick;
        public ICommand DeleteBtnClick {
            get {
                deleteBtnClick = deleteBtnClick ?? new RelayCommand(param => DeleteSessionConfirm(param), param => true);
                return deleteBtnClick;
            }
            set { deleteBtnClick = value; }
        }

        public void DeleteSessionConfirm(object obj)
        {
            messageDialogWin = new MessageDialogWin("Deleting Session", String.Format("Deleting session \"{0}\". Are you sure?", SelectedSession.SessionName), "Yes", "No");
            messageDialogWin.MessageResponse += MessageResponse;
            messageDialogWin.Show();
        }

        private void MessageResponse(object sender, string e)
        {
            messageDialogWin.MessageResponse -= MessageResponse;
            if (e.Equals("Yes"))
            {
                SessionsMgr.DeleteSession(SelectedSession.SessionName);
                SessionsMgr.DeleteSession(SessionName);
                EventSystem.Publish(new SessionsInventoryChanged());
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionNames)));
            }
            messageDialogWin.Close();
        }

        private ICommand addBtnClick;
        public ICommand AddBtnClick {
            get {
                addBtnClick = addBtnClick ?? new RelayCommand(param => AddAudioClips(param), param => true);
                return addBtnClick;
            }
            set { addBtnClick = value; }
        }


        private void AddAudioClips(object obj)
        {
            foreach (var btn in SelectedAudioClipButtons)
            {
                if (btn.DataContext != BindingOperations.DisconnectedSource)
                {
                    btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString())));
                    var audioClip = AudioClips.Single(x => x.Label.Equals(btn.Content?.ToString()));
                    if (!SessionClips.Contains(audioClip))
                    {
                        SessionClips.Add(audioClip);
                        AudioclipsView.Refresh();
                    }
                }
            }
        }

        private ICommand removeBtnClick;
        public ICommand RemoveBtnClick {
            get {
                removeBtnClick = removeBtnClick ?? new RelayCommand(param => RemoveAudioClips(param), param => true);
                return removeBtnClick;
            }
            set { removeBtnClick = value; }
        }

        private void RemoveAudioClips(object obj)
        {
            foreach (var btn in SelectedSessionClipButtons)
            {
                if (btn.DataContext != BindingOperations.DisconnectedSource)
                {
                    btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(ColourHelper.GetBtnInverseColour(btn.Background.ToString())));
                    var sessionClip = SessionClips.Single(x => x.Label.Equals(btn.Content?.ToString()));
                    SessionClips.Remove(sessionClip);
                    AudioclipsView.Refresh();
                }
            }
        }

        private ICommand moveUpBtnClick;
        public ICommand MoveUpBtnClick {
            get {
                moveUpBtnClick = moveUpBtnClick ?? new RelayCommand(param => MoveSessionClipUp(param), param => true);
                return moveUpBtnClick;
            }
            set { moveUpBtnClick = value; }
        }

        private void MoveSessionClipUp(object obj)
        {
            var index = SessionClips.IndexOf(SelectedAudioClip);
            if (index > 0)
                SessionClips.Move(index, index - 1);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionClips)));
        }

        private ICommand moveDownBtnClick;
        public ICommand MoveDownBtnClick {
            get {
                moveDownBtnClick = moveDownBtnClick ?? new RelayCommand(param => MoveSessionClipDown(param), param => true);
                return moveDownBtnClick;
            }
            set { moveDownBtnClick = value; }
        }

        private void MoveSessionClipDown(object obj)
        {
            var index = SessionClips.IndexOf(SelectedAudioClip);
            if (index != -1)
                SessionClips.Move(index, index + 1);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionClips)));
        }

        private ICommand saveBtnClick;
        public ICommand SaveBtnClick {
            get {
                saveBtnClick = saveBtnClick ?? new RelayCommand(param => SaveSession(param), param => true);
                return saveBtnClick;
            }
            set { saveBtnClick = value; }
        }

        private void SaveSession(object obj)
        {
            MessageWin messageWin;
            var msg = "Session saved.";
            int ret;
            if (newSession)
            {
                ret = SessionsMgr.AddSession(SessionName, null);
                if (ret < 0)
                {
                    msg = ret == -1
                        ? "Error: Problem saving session to database." :
                        "Error: Session name already exists - please choose another name.";
                    messageWin = new MessageWin("Save Session", msg);
                    messageWin.Show();
                    return;
                }
                else
                {
                    SelectedSession = SessionsMgr.GetSessionCopy(SessionName);
                    msg = "New session saved.";
                }
            }
            SelectedSession.IsRuleset = IsRuleset;
            string newSessionName = SessionName;
            SelectedSession.SessionName = SessionName;
            selectedSessionName = SessionName;
            SelectedSession.SessionAudioClipsList.Clear();
            if (SessionClips != null)
            {
                foreach (var sessionClip in SessionClips)
                    SelectedSession.SessionAudioClipsList.Add(sessionClip.ClipId);
            }
            ret = SessionsMgr.UpdateSession(SelectedSession);
            if (ret > 0)
            {
                EventSystem.Publish(new SessionsInventoryChanged());
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SessionNames)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSessionName)));
                ChangeSession(newSessionName);
                EventSystem.Publish(new SessionInventoryChanged() { SessionName = newSessionName }); 
                    
            }
            else if (ret == -1)
                msg = "Error: Session name already exists - please choose another name.";
            else if (ret == -2)
                msg = "Error: Problem saving session to database.";
            else
                msg = "Error: Problem saving session audioClips to database.";
            messageWin = new MessageWin("Save Session", msg);
            messageWin.Show();
        }

        private ICommand cancelBtnClick;
        public ICommand CancelBtnClick {
            get {
                cancelBtnClick = cancelBtnClick ?? new RelayCommand(param => CancelChanges(param), param => true);
                return cancelBtnClick;
            }
            set { cancelBtnClick = value; }
        }

        private void CancelChanges(object obj)
        {
            if (SelectedSession != null)
                ChangeSession(originalSessionName);
        }
        
    }
}
