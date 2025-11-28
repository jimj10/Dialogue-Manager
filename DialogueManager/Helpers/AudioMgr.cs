/*
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * https://opensource.org/licenses/MS-PL
 *
 */

// Takes care of audio devices. Plays audioclips, Keepalive
// and study session audio recordings.

using DialogueManager.EventLog;
using NAudio.Lame;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DialogueManager
{
    internal static class AudioMgr
    {
        public static string SessionName { get; set; }
        public static bool RecordingDeviceExists
        { get { return WaveIn.DeviceCount > 0; } }

        public static float CurrentInputLevel { get; set; }

        public static int DeviceNumber { get; set; } = 0;

        private static bool audioMuted = false;

        public static bool AudioMuted
        {
            get { return audioMuted; }
            set
            {
                audioMuted = value;
                if (AudioMuted)
                {
                    if (MediaPlaying)
                    {
                        MPlayer.Stop();
                        MPlayer.MediaEnded -= MediaEndedEventHandler;
                        MediaPlaying = false;
                    }
                    if (KeepAlive)
                    {
                        KeepAlivePlayer.Stop();
                    }

                    EventSystem.Publish(new AudioMuted());
                    Logger.AddLogEntry(LogCategory.INFO, String.Format("Audio muted"));
                }
                else
                {
                    if (KeepAlive)
                    {
                        PlayKeepAlive();
                    }

                    Logger.AddLogEntry(LogCategory.INFO, String.Format("Audio enabled"));
                }
            }
        }

        public static bool UseRecordings { get; set; } = true;

        public static double speedRatio = 1.0;

        public static double SpeedRatio
        {
            get { return speedRatio; }
            set
            {
                speedRatio = value;
                if (MediaPlaying)
                {
                    MPlayer.Stop();
                    EventSystem.Publish(new AudioMuted());
                    MPlayer.MediaEnded -= MediaEndedEventHandler;
                }
                if (KeepAlive)
                {
                    KeepAlivePlayer.Stop();
                }

                MPlayer.SpeedRatio = speedRatio;
                Logger.AddLogEntry(LogCategory.INFO, String.Format("SpeedRatio set to ") + speedRatio.ToString());
                if (KeepAlive)
                {
                    PlayKeepAlive();
                }
            }
        }

        public static string AudioDelay { get; set; } = "Auto";

        private static bool keepAlive = true; // needed for many Bluetooth connections - without it can lose start of audio playback

        public static bool KeepAlive
        {
            get { return keepAlive; }
            set
            {
                keepAlive = value;
                if (keepAlive)
                {
                    PlayKeepAlive();
                }
                else
                {
                    KeepAlivePlayer.Stop();
                }
            }
        }

        private static WaveInEvent WaveSource = null;
        private static WaveFileWriter WaveFile = null;
        private static string DateStamp;
        private static string WaveAudioFile;
        private static string Mp3AudioFile;
        private static readonly int waveInDevices = WaveIn.DeviceCount;
        private static bool Recording = false;
        public static MediaPlayer MPlayer = new MediaPlayer();
        public static MediaPlayer KeepAlivePlayer = new MediaPlayer();
        private static bool MediaPlaying = false;

        public static List<string> GetAudioDevices()
        {
            List<string> audioDevices = new List<string>();
            for (int waveInDevice = 0; waveInDevice < WaveIn.DeviceCount; waveInDevice++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
                audioDevices.Add(String.Format("{0}: {1}, {2} ch",
                waveInDevice, deviceInfo.ProductName, deviceInfo.Channels));
            }
            return audioDevices;
        }

        public static void PlayAudioClip(string audioFile)
        {
            if (!Path.IsPathRooted(audioFile))
            {
                audioFile = Path.Combine(DirectoryMgr.AudioClipsDirectory, audioFile);
            }

            if (SpeedRatio != 1.0 && UseRecordings)
            {
                /*
                 * Changing playback speed doesn't work well with mp3 codec, so change to wav
                 * when using pre-recorded clips (online clip speed is set when audio generated).
                 * A buffer of 'silence' (low volume subsonic tones) needs to be inserted at
                 * start of clip to ensure proper playback.
                 *
                 */

                var clips = new List<string>();
                if (!AudioDelay.Equals("0"))
                {
                    string audioDirectory = Path.Combine(DirectoryMgr.AudioClipsDirectory, @"Sound Files\");
                    if (AudioDelay.Equals("Auto"))
                    {
                        if (SpeedRatio < 0.9)
                        {
                            clips.Add(Path.Combine(audioDirectory, "Silence600"));
                        }
                        else
                        {
                            clips.Add(Path.Combine(audioDirectory, "Silence500"));
                        }
                    }
                    else
                    {
                        clips.Add(Path.Combine(audioDirectory, "Silence" + AudioDelay));
                    }
                }
                clips.Add(audioFile);
                var mp3file = CombineAudioClips(clips);
                var wavFile = Path.Combine(DirectoryMgr.TempDirectory, Guid.NewGuid().ToString() + ".wav");
                MP3ToWav(mp3file, wavFile);
                audioFile = wavFile;
            }
            if (!audioFile.EndsWith("mp3") && !audioFile.EndsWith("wav"))
            {
                audioFile += ".mp3";
            }

            if (File.Exists(audioFile))
            {
                if (MediaPlaying)
                {
                    MPlayer.Stop();
                    EventSystem.Publish(new AudioMuted());
                    MPlayer.MediaEnded -= MediaEndedEventHandler;
                }
                MPlayer.Close();
                MPlayer = new MediaPlayer();
                if (UseRecordings) // desired speed set when clip generated online
                {
                    MPlayer.SpeedRatio = SpeedRatio;
                }

                MPlayer.MediaEnded += MediaEndedEventHandler;
                MediaPlaying = true;
                MPlayer.Open(new Uri(audioFile));
                MPlayer.Play();
            }
            else
            {
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("PlayAudioClip: AudioFile not found"));
            }
        }

        private static void MediaEndedEventHandler(object sender, EventArgs e)
        {
            MPlayer.MediaEnded -= MediaEndedEventHandler;
            MediaPlaying = false;
            EventSystem.Publish(new AudioMuted());
        }

        private static void KeepAliveEndedEventHandler(object sender, EventArgs e)
        {
            KeepAlivePlayer.MediaEnded -= KeepAliveEndedEventHandler;
            if (KeepAlive)
            {
                PlayKeepAlive();
            }
        }

        public static string CombineAudioClips(List<string> clips)
        {
            if (clips == null)
            {
                Logger.AddLogEntry(LogCategory.ERROR, String.Format("CombineFiles: Audio clips list is null"));
                return null;
            }
            string outputFile;
            if (clips.Count == 10)
            {
                if (clips[0].EndsWith("mp3"))
                {
                    outputFile = clips[0];
                }
                else
                {
                    outputFile = clips[0] + ".mp3";
                }

                if (File.Exists(outputFile))
                {
                    return outputFile;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                outputFile = Path.Combine(DirectoryMgr.TempDirectory, Guid.NewGuid().ToString() + ".mp3");
                byte[] buffer;
                using (var fs = File.OpenWrite(outputFile))
                {
                    foreach (var clip in clips)
                    {
                        string file = clip.EndsWith("mp3") ? clip : clip + ".mp3";
                        if (File.Exists(file))
                        {
                            buffer = File.ReadAllBytes(file);
                            fs.Write(buffer, 0, buffer.Length);
                        }
                        else
                        {
                            Logger.AddLogEntry(LogCategory.ERROR, String.Format("CombineFiles: File {0} not found", file));
                            return null;
                        }
                    }
                    fs.Flush();
                }
                return outputFile;
            }
        }

        public static List<string> GetDelays()
        {
            var delays = new List<string>
            {
                "Auto",
                "0"
            };
            for (int i = 400; i < 1100; i += 100)
            {
                delays.Add(i.ToString());
            }
            for (int i = 1200; i < 2100; i += 200)
            {
                delays.Add(i.ToString());
            }
            return delays;
        }

        public static void PlayKeepAlive()
        {
            if (KeepAlive && !AudioMuted)
            {
                Debug.WriteLine("KeepAlive playing...");
                KeepAlivePlayer.MediaEnded -= KeepAliveEndedEventHandler;
                KeepAlivePlayer.MediaEnded += KeepAliveEndedEventHandler;
                string audioFile = Path.Combine(DirectoryMgr.AudioClipsDirectory, @"Sound Files\KeepAlive.mp3");
                if (File.Exists(audioFile))
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        KeepAlivePlayer.Open(new Uri(audioFile));
                        KeepAlivePlayer.Play();
                    }));
                }
            }
        }

        public static bool StartRecording(bool settingLevels = false)
        {
            if (waveInDevices > 0)
            {
                WaveSource = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(22050, 1),
                    DeviceNumber = DeviceNumber
                };
                if (!settingLevels)
                {
                    DateStamp = DateTime.Now.ToString("yyyy-MM-dd-HH_mm");
                    string AudioDirectory = Path.Combine(DirectoryMgr.RecordingsDirectory, SessionName);
                    if (!Directory.Exists(AudioDirectory))
                    {
                        Directory.CreateDirectory(AudioDirectory);
                    }

                    WaveAudioFile = Path.Combine(AudioDirectory, "Study_" + DateStamp + ".wav");
                    Mp3AudioFile = Path.Combine(AudioDirectory, "Study_" + DateStamp + ".mp3");
                    WaveFile = new WaveFileWriter(WaveAudioFile, WaveSource.WaveFormat);
                    Recording = true;
                    Logger.AddLogEntry(LogCategory.INFO, "Recording session to file " + WaveAudioFile);
                }
                WaveSource.DataAvailable -= OnDataAvailable;
                WaveSource.DataAvailable += new EventHandler<WaveInEventArgs>(OnDataAvailable);
                WaveSource.RecordingStopped -= OnRecordingStopped;
                WaveSource.RecordingStopped += new EventHandler<StoppedEventArgs>(OnRecordingStopped);
                try
                {
                    WaveSource.StartRecording();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Oops - something's gone awry..." + e.ToString());
                }

                return true;
            }
            return false;
        }

        public static void StopRecording()
        {
            WaveSource.StopRecording();
            WaveSource.Dispose();
        }

        public static void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (Recording && WaveFile != null)
            {
                WaveFile.Write(e.Buffer, 0, e.BytesRecorded);
                WaveFile.Flush();
            }
            else
            {
                float max = 0;
                // interpret as 16 bit audio
                for (int index = 0; index < e.BytesRecorded; index += 2)
                {
                    short sample = (short)((e.Buffer[index + 1] << 8) |
                                            e.Buffer[index + 0]);
                    // to floating point
                    var sample32 = sample / 32768f;
                    // absolute value
                    if (sample32 < 0)
                    {
                        sample32 = -sample32;
                    }
                    // is this the max value?
                    if (sample32 > max)
                    {
                        max = sample32;
                    }

                    CurrentInputLevel = max * 100;
                    EventSystem.Publish(new AudioUpdated() { RecordingLevel = CurrentInputLevel });
                }
            }
        }

        public static void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            Debug.WriteLine("WaveSourceRecordingStopped");
            WaveSource?.Dispose();
            WaveSource = null;
            WaveFile?.Dispose();
            WaveFile = null;
            if (Recording)
            {
                Recording = false;
                // Convert wav file to mp3
                WavToMP3(WaveAudioFile, Mp3AudioFile);
                if (File.Exists(WaveAudioFile) && File.Exists(Mp3AudioFile))
                {
                    File.Delete(WaveAudioFile);
                }
                Logger.AddLogEntry(LogCategory.INFO, "Recording ended. Audio converted to mp3 file " + Mp3AudioFile);
            }
            else
            {
                CurrentInputLevel = 0;
                EventSystem.Publish(new AudioUpdated() { RecordingLevel = CurrentInputLevel });
            }
        }

        public static void WavToMP3(string waveFileName, string mp3FileName, int bitRate = 128)
        {
            using (var reader = new AudioFileReader(waveFileName))
            using (var writer = new LameMP3FileWriter(mp3FileName, reader.WaveFormat, bitRate))
            {
                reader.CopyTo(writer);
            }
        }

        private static void MP3ToWav(string mp3file, string wavfile)
        {
            using (MediaFoundationReader reader = new MediaFoundationReader(mp3file))
            {
                using (WaveStream convertedStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(wavfile, convertedStream);
                }
            }
        }
    }
}