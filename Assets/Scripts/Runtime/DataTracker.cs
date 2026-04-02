using System.Collections.Generic;
using UnityEngine;

namespace ETEC510.Runtime
{
    [System.Serializable]
    public class SessionRecord
    {
        public string       timestamp;
        public string       caseId;
        public string       studentName;
        public string       className;
        public int          spotAttempts;
        public int          gutCheckAttempts;
        public int          motiveAttempts;
        public int          verdictAttempts;
        public bool         hintUsed;
        public bool         caseCompleted;
        // Panel time tracking — parallel lists (JsonUtility can't serialize Dictionary)
        public List<string> panelNames   = new List<string>();
        public List<float>  panelSeconds = new List<float>();
    }

    public static class DataTracker
    {
        private const string Key         = "etec510_sessions";
        private const int    MaxSessions = 100;

        private static SessionRecord _current;

        [System.Serializable]
        private class SessionList { public List<SessionRecord> sessions = new List<SessionRecord>(); }

        public static void StartSession(string caseId)
        {
            _current = new SessionRecord
            {
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                caseId    = caseId
            };
        }

        public static void SetStudentInfo(string name, string className)
        {
            if (_current == null) return;
            _current.studentName = string.IsNullOrWhiteSpace(name) ? "Anonymous" : name.Trim();
            _current.className   = string.IsNullOrWhiteSpace(className) ? "" : className.Trim();
        }

        public static void LogPanelTime(string panelName, float seconds)
        {
            if (_current == null || string.IsNullOrEmpty(panelName) || seconds < 0.5f) return;
            int idx = _current.panelNames.IndexOf(panelName);
            if (idx >= 0)
                _current.panelSeconds[idx] += seconds;
            else
            {
                _current.panelNames.Add(panelName);
                _current.panelSeconds.Add(seconds);
            }
        }

        public static void LogSpotAttempt()    { if (_current != null) _current.spotAttempts++; }
        public static void LogGutAttempt()     { if (_current != null) _current.gutCheckAttempts++; }
        public static void LogMotiveAttempt()  { if (_current != null) _current.motiveAttempts++; }
        public static void LogVerdictAttempt() { if (_current != null) _current.verdictAttempts++; }
        public static void LogHintUsed()       { if (_current != null) _current.hintUsed = true; }

        public static void LogCaseCompleted()
        {
            if (_current == null) return;
            _current.caseCompleted = true;
            Save();
        }

        // Persist current session (e.g. on app quit without completion)
        public static void Save()
        {
            if (_current == null) return;
            var all = LoadAll();
            all.Add(_current);
            if (all.Count > MaxSessions) all.RemoveAt(0);
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(new SessionList { sessions = all }));
            PlayerPrefs.Save();
        }

        public static List<SessionRecord> GetAll() => LoadAll();

        public static void ClearAll()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        private static List<SessionRecord> LoadAll()
        {
            var json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return new List<SessionRecord>();
            return JsonUtility.FromJson<SessionList>(json)?.sessions ?? new List<SessionRecord>();
        }
    }
}
