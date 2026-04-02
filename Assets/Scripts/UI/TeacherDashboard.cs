using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ETEC510.Runtime;

namespace ETEC510.UI
{
    public class TeacherDashboard : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject passcodePanel;
        public GameObject dataPanel;

        [Header("Passcode")]
        public TMP_Text passcodeDisplay;
        public TMP_Text passcodeError;

        [Header("Stat Boxes")]
        public TMP_Text statSessions;
        public TMP_Text statStudents;
        public TMP_Text statCompleted;
        public TMP_Text statHints;

        [Header("Data")]
        public TMP_Dropdown studentDropdown;
        public TMP_Text     dataText;

        [Header("Buttons — numpad (assign 0-9 in order)")]
        public Button[] digitButtons;
        public Button   clearDigitButton;
        public Button   enterButton;

        [Header("Trigger")]
        public Button triggerButton;

        [Header("Audio")]
        public AudioSource musicSource;
        private float _preDuckVolume = 1f;
        private const float DuckLevel = 0.15f;

        [Header("Buttons — data panel")]
        public Button backButton;
        public Button clearDataButton;
        public Button closeButton;
        public Button passcodeCloseButton;

        public System.Action OnClosedCallback;

        private const string Passcode = "510";
        private string _input = "";

        // Colour palette (TMP rich text)
        private const string ColHeader  = "#A8C8FF";
        private const string ColValue   = "#E8FFE8";
        private const string ColDim     = "#667788";
        private const string ColYes     = "#66FF99";
        private const string ColNo      = "#FF7766";
        private const string ColSection = "#FFD966";

        private void Awake()
        {
            if (digitButtons != null)
                for (int i = 0; i < digitButtons.Length; i++)
                {
                    if (digitButtons[i] == null) continue;
                    int captured = i;
                    digitButtons[i].onClick.AddListener(() => OnDigitPressed(captured));
                }
            if (clearDigitButton)    clearDigitButton.onClick.AddListener(OnClearDigit);
            if (enterButton)         enterButton.onClick.AddListener(OnEnter);
            if (backButton)          backButton.onClick.AddListener(OnBack);
            if (clearDataButton)     clearDataButton.onClick.AddListener(OnClearData);
            if (closeButton)         closeButton.onClick.AddListener(OnClose);
            if (passcodeCloseButton) passcodeCloseButton.onClick.AddListener(OnClose);
            if (studentDropdown)     studentDropdown.onValueChanged.AddListener(_ => RefreshData());
        }

        public void Open()
        {
            if (triggerButton != null) triggerButton.gameObject.SetActive(false);
            if (musicSource != null)
            {
                _preDuckVolume     = musicSource.volume;
                musicSource.volume = DuckLevel;
            }
            gameObject.SetActive(true);
            ShowDataPanel();
        }

        // ── Passcode ────────────────────────────────────────────────────────────

        private void ShowPasscodePanel()
        {
            if (passcodePanel) passcodePanel.SetActive(true);
            if (dataPanel)     dataPanel.SetActive(false);
            _input = "";
            UpdateDisplay();
            if (passcodeError) passcodeError.gameObject.SetActive(false);
        }

        public void OnDigitPressed(int digit)
        {
            if (_input.Length >= 3) return;
            _input += digit.ToString();
            UpdateDisplay();
        }

        public void OnClearDigit()
        {
            if (_input.Length > 0) _input = _input.Substring(0, _input.Length - 1);
            UpdateDisplay();
        }

        public void OnEnter()
        {
            if (_input == Passcode)
                ShowDataPanel();
            else
            {
                if (passcodeError)
                {
                    passcodeError.text = "Incorrect code. Try again.";
                    passcodeError.gameObject.SetActive(true);
                }
                _input = "";
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (passcodeDisplay == null) return;
            var sb = new StringBuilder();
            for (int i = 0; i < 3; i++) sb.Append(i < _input.Length ? "●" : "○");
            passcodeDisplay.text = sb.ToString();
        }

        // ── Data panel ──────────────────────────────────────────────────────────

        private void ShowDataPanel()
        {
            if (passcodePanel) passcodePanel.SetActive(false);
            if (dataPanel)     dataPanel.SetActive(true);
            PopulateDropdown();
            RefreshData();
        }

        private void PopulateDropdown()
        {
            if (studentDropdown == null) return;
            studentDropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData> { new TMP_Dropdown.OptionData("All Students") };
            foreach (var name in GetUniqueStudents())
                options.Add(new TMP_Dropdown.OptionData(name));
            studentDropdown.AddOptions(options);
            studentDropdown.value = 0;
        }

        private List<string> GetUniqueStudents()
        {
            var seen  = new HashSet<string>();
            var names = new List<string>();
            foreach (var s in DataTracker.GetAll())
            {
                var name = StudentName(s);
                if (seen.Add(name)) names.Add(name);
            }
            return names;
        }

        private void RefreshData()
        {
            var all = DataTracker.GetAll();

            // ── Filter ──────────────────────────────────────────────────────────
            string filter = null;
            if (studentDropdown != null && studentDropdown.value > 0)
                filter = studentDropdown.options[studentDropdown.value].text;
            var sessions = filter == null ? all
                : all.FindAll(s => StudentName(s) == filter);

            // ── Stat boxes ──────────────────────────────────────────────────────
            int completed = sessions.FindAll(s => s.caseCompleted).Count;
            int hintCount = sessions.FindAll(s => s.hintUsed).Count;
            if (statSessions)  statSessions.text  = sessions.Count.ToString();
            if (statStudents)  statStudents.text  = filter != null ? "1" : GetUniqueStudents().Count.ToString();
            if (statCompleted) statCompleted.text = $"{completed}/{sessions.Count}";
            if (statHints)     statHints.text     = sessions.Count > 0
                ? $"{Mathf.RoundToInt(100f * hintCount / sessions.Count)}%"
                : "—";

            if (dataText == null) return;
            if (sessions.Count == 0) { dataText.text = "\n  No session data recorded yet."; return; }

            var sb = new StringBuilder();

            // ── Attempts table ──────────────────────────────────────────────────
            sb.AppendLine(C($"  {"#",-3} {"Student",-20} {"Class",-12} {"Spot",-6} {"Gut",-6} {"Motive",-8} {"Verdict",-9} {"Hint",-6} {"Done"}", ColHeader));
            sb.AppendLine(C("  " + new string('─', 76), ColDim));

            for (int i = 0; i < sessions.Count; i++)
            {
                var s   = sessions[i];
                var row = new StringBuilder("  ");
                row.Append(C($"{i + 1,-3} ", ColDim));
                row.Append(C($"{Truncate(StudentName(s), 20),-20} ", ColValue));
                row.Append(C($"{Truncate(s.className ?? "", 12),-12} ", ColDim));
                row.Append(Num(s.spotAttempts, 6));
                row.Append(Num(s.gutCheckAttempts, 6));
                row.Append(Num(s.motiveAttempts, 8));
                row.Append(Num(s.verdictAttempts, 9));
                row.Append(s.hintUsed
                    ? C($"{"Y",-6}", ColNo)
                    : C($"{"N",-6}", ColDim));
                row.Append(s.caseCompleted
                    ? C("Y", ColYes)
                    : C("N", ColNo));
                sb.AppendLine(row.ToString());
            }

            // ── Time per screen ─────────────────────────────────────────────────
            sb.AppendLine();
            sb.AppendLine(C("  TIME PER SCREEN", ColSection));
            sb.AppendLine(C("  " + new string('─', 50), ColDim));

            for (int i = 0; i < sessions.Count; i++)
            {
                var s = sessions[i];
                if (s.panelNames == null || s.panelNames.Count == 0) continue;

                sb.AppendLine();
                sb.AppendLine(C($"  {StudentName(s)}  •  Session {i + 1}  •  {s.timestamp}", ColHeader));
                sb.AppendLine(C($"  {"Screen",-34} {"Time",8}", ColDim));

                var indices = new List<int>();
                for (int j = 0; j < s.panelNames.Count; j++) indices.Add(j);
                indices.Sort((a, b) => s.panelSeconds[b].CompareTo(s.panelSeconds[a]));

                foreach (int j in indices)
                {
                    var secs    = s.panelSeconds[j];
                    var timeStr = secs >= 60f
                        ? $"{Mathf.FloorToInt(secs / 60f)}m {Mathf.FloorToInt(secs % 60f)}s"
                        : $"{secs:F1}s";
                    // Highlight screens where student spent the most time
                    var nameCol = j == indices[0] ? ColYes : ColValue;
                    sb.AppendLine(
                        C($"  {FriendlyName(s.panelNames[j]),-34}", nameCol) +
                        C($"{timeStr,8}", ColSection));
                }
            }

            dataText.text = sb.ToString();
        }

        public void OnClearData()
        {
            DataTracker.ClearAll();
            PopulateDropdown();
            RefreshData();
        }

        public void OnBack()  => ShowPasscodePanel();

        public void OnClose()
        {
            if (musicSource != null) musicSource.volume = _preDuckVolume;
            gameObject.SetActive(false);
            if (triggerButton != null) triggerButton.gameObject.SetActive(true);
            OnClosedCallback?.Invoke();
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static string StudentName(SessionRecord s) =>
            string.IsNullOrWhiteSpace(s.studentName) ? "Anonymous" : s.studentName;

        private static string C(string text, string hex) => $"<color={hex}>{text}</color>";

        private static string Num(int n, int width) =>
            C(n.ToString().PadRight(width), n == 0 ? ColDim : ColValue);

        private static string FriendlyName(string panelName)
        {
            var s = panelName;
            if (s.EndsWith("Panel")) s = s.Substring(0, s.Length - 5);
            var result = new StringBuilder();
            foreach (char c in s)
            {
                if (char.IsUpper(c) && result.Length > 0) result.Append(' ');
                result.Append(c);
            }
            return result.ToString();
        }

        private static string Truncate(string s, int max) =>
            s.Length <= max ? s : s.Substring(0, max - 1) + "…";
    }
}
