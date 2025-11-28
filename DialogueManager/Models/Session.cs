/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DialogueManager.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        public string SessionName { get; set; }

        public List<AudioClip> SessionAudioClips { get { return AudioClipsMgr.GetAudioClips(SessionAudioClipsList); } }

        public List<string> SessionAudioClipsList1 { get; set; } = new List<string>();

        public List<int> SessionAudioClipsList { get; set; } = new List<int>();

        public bool IsRuleset { get; set; }

        public Ruleset Ruleset { get; set; }

        private bool castDisplayEnabled;
        public bool CastDisplayEnabled
        {
            get { return castDisplayEnabled; }
            set
            {
                castDisplayEnabled = value;
                EventSystem.Publish(new EnableCastDisplayChanged() { SessionName = this.SessionName, CastEnable = castDisplayEnabled });
            }
        }

        public double SpeedRatio { get; set; } = 1.0;

        public bool KeepAlive { get; set; } = true;

        public bool AddAudioClip(string label, int position = -1)
        {
            int clipId = AudioClipsMgr.GetAudioClip(label).ClipId;
            if (!SessionAudioClipsList.Any(x => x == clipId))
            {
                if (position == -1)
                {
                    SessionAudioClipsList.Add(clipId);
                }
                else
                {
                    SessionAudioClipsList.Insert(position, clipId);
                }
            }
            return false;
        }
    }
}
