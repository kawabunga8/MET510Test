using ETEC510.Cases;
using ETEC510.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETEC510.UI
{
    public class CaseRunner : MonoBehaviour
    {
        [Header("Case")]
        public CaseData caseData;

        [Header("UI")]
        public TMP_Text titleText;
        public TMP_Text briefingText;
        public TMP_Text questionText;
        public TMP_Text feedbackText;
        public TMP_Text progressText;

        [Tooltip("Buttons used to answer the current question. Button label uses TMP_Text from children.")]
        public Button[] optionButtons;

        public Button nextButton;
        public TMP_Text nextButtonLabel;

        private CaseSession _session;
        private bool _showingFeedback;

        private void Start()
        {
            if (caseData == null)
            {
                Debug.LogError("CaseRunner: No CaseData assigned.");
                SetAllOptionButtonsInteractable(false);
                if (nextButton != null) nextButton.interactable = false;
                return;
            }

            _session = new CaseSession(caseData);

            if (titleText != null) titleText.text = caseData.Title;
            if (briefingText != null) briefingText.text = caseData.BriefingText;

            if (feedbackText != null) feedbackText.text = "";

            WireButtons();
            ShowCurrentQuestion();
        }

        private void WireButtons()
        {
            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    var idx = i;
                    if (optionButtons[i] == null) continue;
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnSelectOption(idx));
                }
            }

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(OnNext);
                nextButton.interactable = false;
            }
        }

        private void ShowCurrentQuestion()
        {
            _showingFeedback = false;

            var q = _session.CurrentQuestion();
            if (q == null)
            {
                ShowCompletion();
                return;
            }

            if (questionText != null) questionText.text = q.Prompt;
            if (feedbackText != null) feedbackText.text = "";

            // Fill option button labels
            for (int i = 0; i < optionButtons.Length; i++)
            {
                var btn = optionButtons[i];
                if (btn == null) continue;

                var hasOption = q.Options != null && i < q.Options.Count;
                btn.gameObject.SetActive(hasOption);
                btn.interactable = true;

                if (hasOption)
                {
                    var label = btn.GetComponentInChildren<TMP_Text>();
                    if (label != null) label.text = q.Options[i];
                }
            }

            if (nextButton != null) nextButton.interactable = false;
            if (nextButtonLabel != null) nextButtonLabel.text = "Next";

            UpdateProgressText();
        }

        private void OnSelectOption(int selectedIndex)
        {
            if (_showingFeedback) return;

            var result = _session.Answer(selectedIndex);
            _showingFeedback = true;

            if (feedbackText != null)
            {
                feedbackText.text = result.feedback;
            }

            SetAllOptionButtonsInteractable(false);

            if (nextButton != null) nextButton.interactable = true;
            if (nextButtonLabel != null) nextButtonLabel.text = _session.IsComplete ? "Finish" : "Next";

            UpdateProgressText();
        }

        private void OnNext()
        {
            if (_session.IsComplete)
            {
                _session.CompleteCase();
                ShowCompletion();
                return;
            }

            ShowCurrentQuestion();
        }

        private void ShowCompletion()
        {
            SetAllOptionButtonsInteractable(false);

            if (questionText != null) questionText.text = "Case complete.";
            if (feedbackText != null)
            {
                var xp = caseData != null ? caseData.XpForCompletion : 0;
                feedbackText.text = $"Nice work. You earned {xp} XP.";
            }

            if (nextButton != null) nextButton.interactable = false;
            if (nextButtonLabel != null) nextButtonLabel.text = "Done";

            UpdateProgressText(final: true);
        }

        private void UpdateProgressText(bool final = false)
        {
            if (progressText == null || caseData == null) return;

            var total = caseData.Questions?.Count ?? 0;
            var answered = Mathf.Min(_session.CurrentQuestionIndex, total);
            var xp = ProgressStore.GetXp();

            progressText.text = final
                ? $"Answered {answered}/{total} • Total XP: {xp}"
                : $"Question {answered + 1}/{total} • Total XP: {xp}";
        }

        private void SetAllOptionButtonsInteractable(bool isInteractable)
        {
            if (optionButtons == null) return;
            foreach (var btn in optionButtons)
            {
                if (btn == null) continue;
                btn.interactable = isInteractable;
            }
        }
    }
}
