using ETEC510.Cases;
using ETEC510.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETEC510.UI
{
    /// <summary>
    /// Panel-based controller for the escape-room case flow.
    /// Assign all panel GameObjects and UI references in the Inspector.
    ///
    /// Flow:
    ///   Intro → Mission Briefing → Evidence Board (hub)
    ///     ├─ Spot the Clue   → reveals digit 1
    ///     ├─ Gut Check       → reveals digit 2
    ///     └─ Find the Motive → reveals digit 3
    ///   Evidence Board → Password Lock ("510") → Evidence Detail
    ///     ├─ Correct verdict → Level Complete
    ///     └─ Wrong verdict   → Hints from Chief → Evidence Detail
    ///
    /// Every non-hub panel has a "Back to Evidence Board" button.
    /// </summary>
    public class CaseRunner : MonoBehaviour
    {
        [Header("Case")]
        public CaseData caseData;

        // ── Panels ────────────────────────────────────────────────────────────
        [Header("Panels")]
        public GameObject introPanel;
        public GameObject missionBriefingPanel;
        public GameObject evidenceBoardPanel;
        public GameObject spotTheCluePanel;
        public GameObject gutCheckPanel;
        public GameObject findTheMotivePanel;
        public GameObject passwordLockPanel;
        public GameObject evidenceDetailPanel;
        public GameObject hintsFromChiefPanel;
        public GameObject levelCompletePanel;

        // ── Intro ─────────────────────────────────────────────────────────────
        [Header("Intro")]
        public Button introEnterButton;

        // ── Mission Briefing ──────────────────────────────────────────────────
        [Header("Mission Briefing")]
        public TMP_Text briefingTitleText;
        public TMP_Text briefingBodyText;
        public Image    briefingImage;
        public Button   briefingStartButton;   // "Start Investigation"

        // ── Evidence Board ────────────────────────────────────────────────────
        [Header("Evidence Board")]
        public Image    evidenceBoardImage;
        public Button   spotTheClueButton;
        public Button   gutCheckButton;
        public Button   findTheMotiveButton;
        public Button   enterPasswordButton;   // enabled when all 3 clues found
        public TMP_Text digit1Text;            // shows "?" until Spot the Clue done
        public TMP_Text digit2Text;            // shows "?" until Gut Check done
        public TMP_Text digit3Text;            // shows "?" until Find the Motive done

        // ── Spot the Clue ─────────────────────────────────────────────────────
        [Header("Spot the Clue")]
        public TMP_Text spotPromptText;
        public Image    spotEvidenceImage;
        public TMP_Text spotExplanationText;   // hidden until confirmed
        public Button   spotConfirmButton;     // "I found it!"
        public Button   spotBackButton;

        // ── Gut Check ─────────────────────────────────────────────────────────
        [Header("Gut Check")]
        public TMP_Text gutPromptText;
        public Image    gutEvidenceImage;
        public Button[] gutOptionButtons;      // 2 buttons
        public TMP_Text gutFeedbackText;
        public Button   gutNextButton;         // "Back to Board" after answering
        public Button   gutBackButton;

        // ── Find the Motive ───────────────────────────────────────────────────
        [Header("Find the Motive")]
        public TMP_Text motivePromptText;
        public Image    motiveEvidenceImage;
        public TMP_Text motiveExplanationText; // hidden until confirmed
        public Button   motiveConfirmButton;   // "I found them!"
        public Button   motiveBackButton;

        // ── Password Lock ─────────────────────────────────────────────────────
        [Header("Password Lock")]
        public TMP_InputField passwordInputField;
        public TMP_Text       passwordFeedbackText;
        public Button         passwordSubmitButton;
        public Button         passwordBackButton;

        // ── Evidence Detail (Verdict) ─────────────────────────────────────────
        [Header("Evidence Detail")]
        public TMP_Text verdictPromptText;
        public Image    verdictEvidenceImage;
        public Button[] verdictOptionButtons;  // 2 buttons: Real / Fake
        public Button   verdictBackButton;

        // ── Hints from Chief ──────────────────────────────────────────────────
        [Header("Hints from Chief")]
        public TMP_Text hintBodyText;
        public Image    hintImage;
        public Button   hintTryAgainButton;    // returns to Evidence Detail

        // ── Level Complete ────────────────────────────────────────────────────
        [Header("Level Complete")]
        public TMP_Text completionBodyText;
        public Image    completionImage;
        public TMP_Text xpText;

        // ── Private ───────────────────────────────────────────────────────────
        private CaseSession _session;

        // ═════════════════════════════════════════════════════════════════════

        private void Start()
        {
            if (caseData == null)
            {
                Debug.LogError("CaseRunner: No CaseData assigned.");
                return;
            }

            _session = new CaseSession(caseData);
            WireButtons();
            ShowPanel(introPanel);
        }

        // ── Button wiring ─────────────────────────────────────────────────────

        private void WireButtons()
        {
            // Intro
            if (introEnterButton)    introEnterButton.onClick.AddListener(ShowMissionBriefing);

            // Mission Briefing
            if (briefingStartButton) briefingStartButton.onClick.AddListener(ShowEvidenceBoard);

            // Evidence Board
            if (spotTheClueButton)   spotTheClueButton.onClick.AddListener(ShowSpotTheClue);
            if (gutCheckButton)      gutCheckButton.onClick.AddListener(ShowGutCheck);
            if (findTheMotiveButton) findTheMotiveButton.onClick.AddListener(ShowFindTheMotive);
            if (enterPasswordButton) enterPasswordButton.onClick.AddListener(ShowPasswordLock);

            // Spot the Clue
            if (spotConfirmButton) spotConfirmButton.onClick.AddListener(OnSpotClueConfirm);
            if (spotBackButton)    spotBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Gut Check
            WireIndexedButtons(gutOptionButtons, OnGutCheckAnswer);
            if (gutNextButton) gutNextButton.onClick.AddListener(ShowEvidenceBoard);
            if (gutBackButton) gutBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Find the Motive
            if (motiveConfirmButton) motiveConfirmButton.onClick.AddListener(OnMotiveConfirm);
            if (motiveBackButton)    motiveBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Password Lock
            if (passwordSubmitButton) passwordSubmitButton.onClick.AddListener(OnPasswordSubmit);
            if (passwordBackButton)   passwordBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Evidence Detail
            WireIndexedButtons(verdictOptionButtons, OnVerdictAnswer);
            if (verdictBackButton) verdictBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Hints from Chief
            if (hintTryAgainButton) hintTryAgainButton.onClick.AddListener(ShowEvidenceDetail);
        }

        private void WireIndexedButtons(Button[] buttons, UnityEngine.Events.UnityAction<int> handler)
        {
            if (buttons == null) return;
            for (int i = 0; i < buttons.Length; i++)
            {
                var idx = i;
                if (buttons[i] != null)
                    buttons[i].onClick.AddListener(() => handler(idx));
            }
        }

        // ── Panel switching ───────────────────────────────────────────────────

        private void ShowPanel(GameObject target)
        {
            GameObject[] all = {
                introPanel, missionBriefingPanel, evidenceBoardPanel,
                spotTheCluePanel, gutCheckPanel, findTheMotivePanel,
                passwordLockPanel, evidenceDetailPanel,
                hintsFromChiefPanel, levelCompletePanel
            };
            foreach (var p in all)
                if (p != null) p.SetActive(p == target);
        }

        public void ShowMissionBriefing()
        {
            if (briefingTitleText) briefingTitleText.text = caseData.Title;
            if (briefingBodyText)  briefingBodyText.text  = caseData.BriefingText;
            if (briefingImage && caseData.BriefingImage)
                briefingImage.sprite = caseData.BriefingImage;
            ShowPanel(missionBriefingPanel);
        }

        public void ShowEvidenceBoard()
        {
            if (evidenceBoardImage && caseData.EvidenceBoardImage)
                evidenceBoardImage.sprite = caseData.EvidenceBoardImage;

            if (digit1Text) digit1Text.text = _session.SpotTheClueCompleted   ? caseData.SpotTheClue.CodeDigit   : "?";
            if (digit2Text) digit2Text.text = _session.GutCheckCompleted       ? caseData.GutCheck.CodeDigit       : "?";
            if (digit3Text) digit3Text.text = _session.FindTheMotiveCompleted  ? caseData.FindTheMotive.CodeDigit  : "?";

            if (enterPasswordButton) enterPasswordButton.interactable = _session.AllCluesFound;

            ShowPanel(evidenceBoardPanel);
        }

        private void ShowSpotTheClue()
        {
            var step = caseData.SpotTheClue;
            if (spotPromptText)    spotPromptText.text = step.Prompt;
            if (spotEvidenceImage && step.EvidenceImage)
                spotEvidenceImage.sprite = step.EvidenceImage;

            var done = _session.SpotTheClueCompleted;
            if (spotExplanationText)
            {
                spotExplanationText.text = step.ExplanationText;
                spotExplanationText.gameObject.SetActive(done);
            }
            if (spotConfirmButton) spotConfirmButton.interactable = !done;
            ShowPanel(spotTheCluePanel);
        }

        private void ShowGutCheck()
        {
            var step = caseData.GutCheck;
            if (gutPromptText)   gutPromptText.text   = step.Prompt;
            if (gutFeedbackText) gutFeedbackText.text = "";
            if (gutEvidenceImage && step.EvidenceImage)
                gutEvidenceImage.sprite = step.EvidenceImage;

            var done = _session.GutCheckCompleted;
            for (int i = 0; i < gutOptionButtons.Length; i++)
            {
                var btn = gutOptionButtons[i];
                if (btn == null) continue;
                bool hasOption = i < step.Options.Length;
                btn.gameObject.SetActive(hasOption);
                btn.interactable = !done;
                if (hasOption)
                {
                    var label = btn.GetComponentInChildren<TMP_Text>();
                    if (label) label.text = step.Options[i];
                }
            }
            if (gutNextButton) gutNextButton.gameObject.SetActive(done);
            ShowPanel(gutCheckPanel);
        }

        private void ShowFindTheMotive()
        {
            var step = caseData.FindTheMotive;
            if (motivePromptText) motivePromptText.text = step.Prompt;
            if (motiveEvidenceImage && step.EvidenceImage)
                motiveEvidenceImage.sprite = step.EvidenceImage;

            var done = _session.FindTheMotiveCompleted;
            if (motiveExplanationText)
            {
                motiveExplanationText.text = step.ExplanationText;
                motiveExplanationText.gameObject.SetActive(done);
            }
            if (motiveConfirmButton) motiveConfirmButton.interactable = !done;
            ShowPanel(findTheMotivePanel);
        }

        private void ShowPasswordLock()
        {
            if (passwordInputField)    passwordInputField.text = "";
            if (passwordFeedbackText)  passwordFeedbackText.text = "";
            ShowPanel(passwordLockPanel);
        }

        private void ShowEvidenceDetail()
        {
            var step = caseData.Verdict;
            if (verdictPromptText) verdictPromptText.text = step.Prompt;
            if (verdictEvidenceImage && step.EvidenceImage)
                verdictEvidenceImage.sprite = step.EvidenceImage;

            for (int i = 0; i < verdictOptionButtons.Length; i++)
            {
                var btn = verdictOptionButtons[i];
                if (btn == null) continue;
                bool hasOption = i < step.Options.Length;
                btn.gameObject.SetActive(hasOption);
                btn.interactable = true;
                if (hasOption)
                {
                    var label = btn.GetComponentInChildren<TMP_Text>();
                    if (label) label.text = step.Options[i];
                }
            }
            ShowPanel(evidenceDetailPanel);
        }

        private void ShowHintsFromChief()
        {
            if (hintBodyText) hintBodyText.text = caseData.HintText;
            if (hintImage && caseData.HintImage) hintImage.sprite = caseData.HintImage;
            ShowPanel(hintsFromChiefPanel);
        }

        private void ShowLevelComplete()
        {
            _session.CompleteCase();
            if (completionBodyText) completionBodyText.text = caseData.CompletionText;
            if (completionImage && caseData.CompletionImage)
                completionImage.sprite = caseData.CompletionImage;
            if (xpText)
                xpText.text = $"You earned {caseData.XpForCompletion} XP!  •  Total XP: {ProgressStore.GetXp()}";
            ShowPanel(levelCompletePanel);
        }

        // ── Action handlers ───────────────────────────────────────────────────

        private void OnSpotClueConfirm()
        {
            _session.CompleteSpotTheClue();
            if (spotExplanationText)
            {
                spotExplanationText.text = caseData.SpotTheClue.ExplanationText;
                spotExplanationText.gameObject.SetActive(true);
            }
            if (spotConfirmButton) spotConfirmButton.interactable = false;
        }

        private void OnGutCheckAnswer(int selectedIndex)
        {
            if (_session.GutCheckCompleted) return;
            var result = _session.AnswerGutCheck(selectedIndex);
            if (gutFeedbackText) gutFeedbackText.text = result.feedback;
            foreach (var btn in gutOptionButtons) if (btn) btn.interactable = false;
            if (gutNextButton) gutNextButton.gameObject.SetActive(true);
        }

        private void OnMotiveConfirm()
        {
            _session.CompleteMotive();
            if (motiveExplanationText)
            {
                motiveExplanationText.text = caseData.FindTheMotive.ExplanationText;
                motiveExplanationText.gameObject.SetActive(true);
            }
            if (motiveConfirmButton) motiveConfirmButton.interactable = false;
        }

        private void OnPasswordSubmit()
        {
            var input = passwordInputField != null ? passwordInputField.text : "";
            if (_session.ValidatePassword(input))
            {
                ShowEvidenceDetail();
            }
            else
            {
                if (passwordFeedbackText)
                    passwordFeedbackText.text = "Incorrect code. Review your evidence clues!";
            }
        }

        private void OnVerdictAnswer(int selectedIndex)
        {
            var result = _session.SubmitVerdict(selectedIndex);
            if (result.isCorrect)
                ShowLevelComplete();
            else
                ShowHintsFromChief();
        }
    }
}
