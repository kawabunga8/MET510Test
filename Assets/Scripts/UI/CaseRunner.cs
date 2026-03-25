using ETEC510.Cases;
using ETEC510.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

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
        public AudioSource mainAudioSource;  // looping background music during investigation

        // ── Sound Toggle ──────────────────────────────────────────────────────
        [Header("Sound Toggle")]
        public GameObject soundTogglePanel;
        public Button     soundOnButton;
        public Button     soundOffButton;

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
        public RawImage introVideoDisplay;   // RawImage that shows the video
        public VideoPlayer introVideoPlayer; // VideoPlayer component on the intro panel
        public AudioSource introAudioSource; // AudioSource for intro music
        public Button introEnterButton;      // shown after video ends
        public Button introSkipButton;       // lets player skip the video early

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
        public Button   enterPasswordButton;
        public TMP_Text boardWarningText;      // popup warning shown when room is locked
        public TMP_Text digit1Text;            // shows "?" until Spot the Clue done
        public TMP_Text digit2Text;            // shows "?" until Gut Check done
        public TMP_Text digit3Text;            // shows "?" until Find the Motive done

        // ── Spot the Clue ─────────────────────────────────────────────────────
        [Header("Spot the Clue")]
        public TMP_Text spotPromptText;
        public Image    spotEvidenceImage;
        public Button[] spotOptionButtons;     // 2 answer buttons
        public TMP_Text spotFeedbackText;
        public Button   spotBackButton;

        // ── Gut Check ─────────────────────────────────────────────────────────
        [Header("Gut Check")]
        public TMP_Text gutPromptText;
        public Image    gutEvidenceImage;
        public Button[] gutOptionButtons;      // 2 buttons
        public TMP_Text gutFeedbackText;
        public Button   gutNextButton;
        public Button   gutBackButton;

        // ── Find the Motive ───────────────────────────────────────────────────
        [Header("Find the Motive")]
        public TMP_Text motivePromptText;
        public Image    motiveEvidenceImage;
        public Button[] motiveOptionButtons;   // 2 answer buttons
        public TMP_Text motiveFeedbackText;
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
        public RawImage    hintVideoDisplay;
        public VideoPlayer hintVideoPlayer;
        public GameObject  hintOverlay;        // shown only after video ends
        public TMP_Text hintBodyText;
        public Image    hintImage;
        public Button   hintTryAgainButton;    // returns to Evidence Detail (verdict)
        public Button   hintReturnButton;      // returns to Evidence Board

        // ── Level Complete ────────────────────────────────────────────────────
        [Header("Level Complete")]
        public TMP_Text   completionBodyText;
        public Image      completionImage;
        public TMP_Text   xpText;
        public Button     restartButton;
        public AudioClip  completionMusic;   // "Shadows in the File"

        // ── SFX ───────────────────────────────────────────────────────────────
        [Header("SFX")]
        public AudioClip  clickSound;    // assign any short UI click clip
        public AudioSource sfxSource;   // separate non-looping source for one-shots

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

            // Self-initialize audio sources if not wired in Inspector.
            // Use explicit null checks — Unity's fake-null breaks the ?? operator.
            if (mainAudioSource == null)
            {
                mainAudioSource = GetComponent<AudioSource>();
                if (mainAudioSource == null) mainAudioSource = gameObject.AddComponent<AudioSource>();
            }
            mainAudioSource.playOnAwake = false;
            mainAudioSource.loop        = true;

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake  = false;
                sfxSource.loop         = false;
                sfxSource.spatialBlend = 0f;   // 2D — must be 0 for UI sounds
                sfxSource.volume       = 1f;
            }

            if (introAudioSource == null && introPanel != null)
            {
                introAudioSource = introPanel.GetComponent<AudioSource>();
                if (introAudioSource == null) introAudioSource = introPanel.AddComponent<AudioSource>();
            }
            if (introAudioSource != null)
                introAudioSource.playOnAwake = false;

            // Restore saved sound preference (default: on)
            AudioListener.volume = PlayerPrefs.GetInt("etec510_sound_enabled", 1) == 1 ? 1f : 0f;

            _session = new CaseSession(caseData);
            WireButtons();

            if (soundTogglePanel != null)
                ShowPanel(soundTogglePanel);
            else
                ShowIntroPanel();
        }

        private void ShowIntroPanel()
        {
            ShowPanel(introPanel);
            PlayIntroVideo();
        }

        private void OnSoundChoice(bool soundOn)
        {
            AudioListener.volume = soundOn ? 1f : 0f;
            PlayerPrefs.SetInt("etec510_sound_enabled", soundOn ? 1 : 0);
            PlayerPrefs.Save();

            if (!soundOn)
            {
                if (mainAudioSource  != null) mainAudioSource.Stop();
                if (introAudioSource != null) introAudioSource.Stop();
            }

            ShowIntroPanel();
        }

        private void PlayIntroVideo()
        {
            if (introVideoPlayer == null)
            {
                Debug.LogWarning("[CaseRunner] No video player — skipping to Enter.");
                SetIntroButtonsVisible(enterOnly: true);
                return;
            }

            var url = BuildVideoUrl(caseData.IntroVideoFile);
            bool hasSource = url != null || caseData.IntroVideo != null;
            if (!hasSource)
            {
                Debug.LogWarning("[CaseRunner] No video source — skipping to Enter.");
                SetIntroButtonsVisible(enterOnly: true);
                return;
            }

            SetIntroButtonsVisible(enterOnly: false);
            introVideoPlayer.audioOutputMode  = VideoAudioOutputMode.None;
            introVideoPlayer.isLooping        = false;
            introVideoPlayer.errorReceived    += OnVideoError;
            introVideoPlayer.loopPointReached += OnIntroVideoFinished;
            introVideoPlayer.prepareCompleted += OnIntroPrepared;

            if (url != null)
            {
                introVideoPlayer.source = VideoSource.Url;
                introVideoPlayer.url    = url;
            }
            else
            {
                introVideoPlayer.source = VideoSource.VideoClip;
                introVideoPlayer.clip   = caseData.IntroVideo;
            }
            introVideoPlayer.Prepare();
            Debug.Log($"[CaseRunner] Intro video Prepare() — source={introVideoPlayer.source}");
        }

        private void OnIntroPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnIntroPrepared;

            // VideoPlayer already targets IntroRT (set in scene) — just play.
            // Do NOT reassign targetTexture; doing so breaks the RawImage connection.
            vp.Play();

            if (introAudioSource != null && caseData.IntroMusic != null)
            {
                introAudioSource.clip = caseData.IntroMusic;
                introAudioSource.Play();
                Debug.Log($"[CaseRunner] Intro music — clip={caseData.IntroMusic.name}, isPlaying={introAudioSource.isPlaying}, volume={introAudioSource.volume}, spatialBlend={introAudioSource.spatialBlend}, mute={introAudioSource.mute}");
            }
            else
            {
                Debug.LogWarning($"[CaseRunner] Intro music skipped — audioSrc={(introAudioSource != null ? "ok" : "NULL")}, clip={(caseData.IntroMusic != null ? caseData.IntroMusic.name : "NULL")}");
            }
            Debug.Log($"[CaseRunner] OnIntroPrepared — {vp.width}x{vp.height}, isPlaying={vp.isPlaying}");
        }

        private void OnVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] VideoPlayer error: {msg}");
            vp.errorReceived -= OnVideoError;
            SetIntroButtonsVisible(enterOnly: true);
        }

        private void RestartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            // Only clean up dynamically created RTs (hint video).
            // IntroRT is a scene asset — do NOT destroy it.
            if (hintVideoPlayer != null && hintVideoPlayer.targetTexture != null)
            {
                hintVideoPlayer.targetTexture.Release();
                Destroy(hintVideoPlayer.targetTexture);
            }
        }

        private void OnIntroVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnIntroVideoFinished;
            if (introAudioSource != null) introAudioSource.Stop();
            SetIntroButtonsVisible(enterOnly: true);
        }

        private void OnIntroSkip()
        {
            if (introVideoPlayer != null)
            {
                introVideoPlayer.loopPointReached -= OnIntroVideoFinished;
                introVideoPlayer.Stop();
            }
            if (introAudioSource != null) StartCoroutine(FadeOutAudio(introAudioSource, 0.8f));
            SetIntroButtonsVisible(enterOnly: true);
        }

        private System.Collections.IEnumerator FadeOutAudio(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }
            source.Stop();
            source.volume = startVolume;
        }

        private void SetIntroButtonsVisible(bool enterOnly)
        {
            if (introEnterButton != null) introEnterButton.gameObject.SetActive(enterOnly);
            if (introSkipButton  != null) introSkipButton.gameObject.SetActive(!enterOnly);
        }

        // ── Button wiring ─────────────────────────────────────────────────────

        private void WireButtons()
        {
            // Sound Toggle
            if (soundOnButton)  soundOnButton.onClick.AddListener(() => OnSoundChoice(true));
            if (soundOffButton) soundOffButton.onClick.AddListener(() => OnSoundChoice(false));

            // Intro
            if (introEnterButton) introEnterButton.onClick.AddListener(ShowMissionBriefing);
            if (introSkipButton)  introSkipButton.onClick.AddListener(OnIntroSkip);

            // Mission Briefing
            if (briefingStartButton) briefingStartButton.onClick.AddListener(ShowEvidenceBoard);

            // Evidence Board
            if (spotTheClueButton)   spotTheClueButton.onClick.AddListener(ShowSpotTheClue);
            if (gutCheckButton)      gutCheckButton.onClick.AddListener(ShowGutCheck);
            if (findTheMotiveButton) findTheMotiveButton.onClick.AddListener(ShowFindTheMotive);
            if (enterPasswordButton) enterPasswordButton.onClick.AddListener(OnEnterPasswordPressed);

            // Spot the Clue
            WireIndexedButtons(spotOptionButtons, OnSpotAnswer);
            if (spotBackButton) spotBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Gut Check
            WireIndexedButtons(gutOptionButtons, OnGutCheckAnswer);
            if (gutBackButton) gutBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Find the Motive
            WireIndexedButtons(motiveOptionButtons, OnMotiveAnswer);
            if (motiveBackButton) motiveBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Password Lock
            if (passwordSubmitButton) passwordSubmitButton.onClick.AddListener(OnPasswordSubmit);
            if (passwordBackButton)   passwordBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (passwordInputField)   passwordInputField.onValueChanged.AddListener(_ => PlayClick());

            // Evidence Detail
            WireIndexedButtons(verdictOptionButtons, OnVerdictAnswer);
            if (verdictBackButton) verdictBackButton.onClick.AddListener(ShowEvidenceBoard);

            // Hints from Chief
            if (hintTryAgainButton) hintTryAgainButton.onClick.AddListener(ShowEvidenceDetail);
            if (hintReturnButton)   hintReturnButton.onClick.AddListener(ShowEvidenceBoard);

            // Level Complete
            if (restartButton) restartButton.onClick.AddListener(RestartGame);

            // ── Click sounds on every button ──────────────────────────────────
            AddClick(soundOnButton); AddClick(soundOffButton);
            AddClick(introEnterButton); AddClick(introSkipButton);
            AddClick(briefingStartButton);
            AddClick(spotTheClueButton); AddClick(gutCheckButton);
            AddClick(findTheMotiveButton); AddClick(enterPasswordButton);
            AddClick(spotBackButton); AddClick(gutBackButton);
            AddClick(motiveBackButton);
            AddClick(passwordSubmitButton); AddClick(passwordBackButton);
            AddClick(verdictBackButton);
            AddClick(hintTryAgainButton); AddClick(hintReturnButton);
            AddClick(restartButton);
            if (spotOptionButtons  != null) foreach (var b in spotOptionButtons)   AddClick(b);
            if (gutOptionButtons   != null) foreach (var b in gutOptionButtons)    AddClick(b);
            if (motiveOptionButtons!= null) foreach (var b in motiveOptionButtons) AddClick(b);
            if (verdictOptionButtons!= null) foreach (var b in verdictOptionButtons) AddClick(b);
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
                soundTogglePanel, introPanel, missionBriefingPanel, evidenceBoardPanel,
                spotTheCluePanel, gutCheckPanel, findTheMotivePanel,
                passwordLockPanel, evidenceDetailPanel,
                hintsFromChiefPanel, levelCompletePanel
            };
            foreach (var p in all)
                if (p != null) p.SetActive(p == target);
        }

        // Ensures an Image fills its panel without stretching (zoom-to-fill / cover).
        private static void EnsureCoverFit(Image img)
        {
            if (img == null) return;
            var fitter = img.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = img.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        public void ShowMissionBriefing()
        {
            if (briefingTitleText) briefingTitleText.text = caseData.Title;
            if (briefingBodyText)  briefingBodyText.text  = caseData.BriefingText;
            if (briefingImage && caseData.BriefingImage)
                briefingImage.sprite = caseData.BriefingImage;
            EnsureCoverFit(briefingImage);
            ShowPanel(missionBriefingPanel);
        }

        public void ShowEvidenceBoard()
        {
            if (mainAudioSource != null && caseData.MainMusic != null && !mainAudioSource.isPlaying)
            {
                mainAudioSource.clip = caseData.MainMusic;
                mainAudioSource.loop = true;
                mainAudioSource.Play();
            }

            if (evidenceBoardImage && caseData.EvidenceBoardImage)
                evidenceBoardImage.sprite = caseData.EvidenceBoardImage;
            EnsureCoverFit(evidenceBoardImage);

            if (digit1Text) digit1Text.text = _session.SpotTheClueCompleted   ? caseData.SpotTheClue.CodeDigit   : "?";
            if (digit2Text) digit2Text.text = _session.GutCheckCompleted       ? caseData.GutCheck.CodeDigit       : "?";
            if (digit3Text) digit3Text.text = _session.FindTheMotiveCompleted  ? caseData.FindTheMotive.CodeDigit  : "?";

            if (boardWarningText) boardWarningText.gameObject.SetActive(false);

            if (enterPasswordButton)
            {
                enterPasswordButton.onClick.RemoveAllListeners();
                enterPasswordButton.onClick.AddListener(OnEnterPasswordPressed);
            }

            ShowPanel(evidenceBoardPanel);
        }

        private void ShowSpotTheClue()
        {
            var step = caseData.SpotTheClue;
            if (spotPromptText) spotPromptText.text = step.Prompt;
            if (spotEvidenceImage && step.EvidenceImage)
                spotEvidenceImage.sprite = step.EvidenceImage;
            if (spotFeedbackText) spotFeedbackText.text = "";

            var done = _session.SpotTheClueCompleted;
            for (int i = 0; i < spotOptionButtons.Length; i++)
            {
                var btn = spotOptionButtons[i];
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
            ShowPanel(gutCheckPanel);
        }

        private void ShowFindTheMotive()
        {
            var step = caseData.FindTheMotive;
            if (motivePromptText) motivePromptText.text = step.Prompt;
            if (motiveEvidenceImage && step.EvidenceImage)
                motiveEvidenceImage.sprite = step.EvidenceImage;
            if (motiveFeedbackText) motiveFeedbackText.text = "";

            var done = _session.FindTheMotiveCompleted;
            for (int i = 0; i < motiveOptionButtons.Length; i++)
            {
                var btn = motiveOptionButtons[i];
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
            ShowPanel(findTheMotivePanel);
        }

        private void OnEnterPasswordPressed()
        {
            if (_session.AllCluesFound)
                ShowPasswordLock();
            else
                PopupController.ShowToast(
                    "This room is password-protected.\nCollect all three clues first.", 2.5f);
        }

        private void ShowPasswordLock()
        {
            Debug.Log($"[CaseRunner] ShowPasswordLock — panel={(passwordLockPanel != null ? passwordLockPanel.name : "NULL")}");
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

            // Auto-discover overlay if Inspector field was never wired
            if (hintOverlay == null && hintsFromChiefPanel != null)
            {
                var t = hintsFromChiefPanel.transform.Find("HintOverlay");
                if (t != null) hintOverlay = t.gameObject;
            }

            if (completionMusic != null && mainAudioSource != null)
                StartCoroutine(CrossfadeMusic(mainAudioSource, completionMusic, 1.2f));

            SetHintContentVisible(false);
            ShowPanel(hintsFromChiefPanel);
            PlayHintVideo();
        }

        // Shows or hides hint content. Uses the overlay GO when available;
        // falls back to controlling hintBodyText + hintTryAgainButton directly.
        private void SetHintContentVisible(bool visible)
        {
            if (hintOverlay != null)
            {
                hintOverlay.SetActive(visible);
            }
            else
            {
                if (hintBodyText)      hintBodyText.gameObject.SetActive(visible);
                if (hintTryAgainButton) hintTryAgainButton.gameObject.SetActive(visible);
            }
        }

        private void PlayHintVideo()
        {
            if (hintVideoPlayer == null)
            {
                SetHintContentVisible(true);
                return;
            }

            var url = BuildVideoUrl(caseData.HintVideoFile);
            bool hasSource = url != null || caseData.HintVideo != null;
            if (!hasSource)
            {
                SetHintContentVisible(true);
                return;
            }

            hintVideoPlayer.isLooping        = false;
            hintVideoPlayer.errorReceived    += OnHintVideoError;
            hintVideoPlayer.loopPointReached += OnHintVideoFinished;
            hintVideoPlayer.prepareCompleted += OnHintVideoPrepared;

            if (url != null)
            {
                hintVideoPlayer.source = VideoSource.Url;
                hintVideoPlayer.url    = url;
            }
            else
            {
                hintVideoPlayer.source = VideoSource.VideoClip;
                hintVideoPlayer.clip   = caseData.HintVideo;
            }
            hintVideoPlayer.Prepare();
        }

        // Returns a StreamingAssets URL for the given filename, or null if not provided.
        // On WebGL, Application.streamingAssetsPath is already an http:// URL.
        private static string BuildVideoUrl(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            return System.IO.Path.Combine(
                Application.streamingAssetsPath, "Video", filename).Replace("\\", "/");
        }

        private void OnHintVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnHintVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (hintVideoDisplay != null)
                hintVideoDisplay.texture = rt;
            vp.Play();
        }

        private void OnHintVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnHintVideoFinished;
            SetHintContentVisible(true);
        }

        private void OnHintVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Hint video error: {msg}");
            vp.errorReceived -= OnHintVideoError;
            SetHintContentVisible(true);
        }

        private void ShowLevelComplete()
        {
            _session.CompleteCase();
            if (completionBodyText) completionBodyText.text = caseData.CompletionText;
            if (completionImage && caseData.CompletionImage)
                completionImage.sprite = caseData.CompletionImage;
            EnsureCoverFit(completionImage);
            if (xpText)
                xpText.text = $"You earned {caseData.XpForCompletion} XP!  •  Total XP: {ProgressStore.GetXp()}";

            if (completionMusic != null && mainAudioSource != null)
                StartCoroutine(CrossfadeMusic(mainAudioSource, completionMusic, 1.2f));

            ShowPanel(levelCompletePanel);
        }

        private System.Collections.IEnumerator CrossfadeMusic(AudioSource source, AudioClip newClip, float duration)
        {
            // Already playing this clip — don't restart, just continue
            if (source.clip == newClip && source.isPlaying)
                yield break;

            // Fade out current track
            float start = source.volume;
            float half = duration * 0.5f;
            for (float t = 0; t < half; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(start, 0f, t / half);
                yield return null;
            }
            source.Stop();
            source.clip   = newClip;
            source.loop   = true;
            source.volume = 0f;
            source.Play();
            // Fade in new track
            for (float t = 0; t < half; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(0f, start, t / half);
                yield return null;
            }
            source.volume = start;
        }

        // ── Action handlers ───────────────────────────────────────────────────

        private void OnSpotAnswer(int selectedIndex)
        {
            if (_session.SpotTheClueCompleted) return;
            var step = caseData.SpotTheClue;
            bool correct = selectedIndex == step.CorrectIndex;
            if (correct) { _session.CompleteSpotTheClue(); LockButtons(spotOptionButtons); }
            PopupController.Show(
                correct ? "Clue Found!" : "Look Again...",
                correct ? step.FeedbackCorrect : step.FeedbackIncorrect,
                "Got it");
        }

        private void OnGutCheckAnswer(int selectedIndex)
        {
            if (_session.GutCheckCompleted) return;
            var result = _session.AnswerGutCheck(selectedIndex);
            if (result.isCorrect) LockButtons(gutOptionButtons);
            PopupController.Show(
                result.isCorrect ? "Good Instinct!" : "Think Again...",
                result.feedback,
                "Got it");
        }

        private void OnMotiveAnswer(int selectedIndex)
        {
            if (_session.FindTheMotiveCompleted) return;
            var step = caseData.FindTheMotive;
            bool correct = selectedIndex == step.CorrectIndex;
            if (correct) { _session.CompleteMotive(); LockButtons(motiveOptionButtons); }
            PopupController.Show(
                correct ? "Motive Identified!" : "Look Deeper...",
                correct ? step.FeedbackCorrect : step.FeedbackIncorrect,
                "Got it");
        }

        private void PlayClick()
        {
            if (sfxSource != null && clickSound != null)
                sfxSource.PlayOneShot(clickSound);
        }

        private void AddClick(Button btn)
        {
            if (btn != null) btn.onClick.AddListener(PlayClick);
        }

        private static void LockButtons(Button[] buttons)
        {
            foreach (var btn in buttons) if (btn) btn.interactable = false;
        }

        private void OnPasswordSubmit()
        {
            var input = passwordInputField != null ? passwordInputField.text : "";
            if (_session.ValidatePassword(input))
                ShowEvidenceDetail();
            else
                PopupController.Show("Wrong Code",
                    "That code is incorrect.\nReview your evidence clues and try again.",
                    "Try Again");
        }

        private void OnVerdictAnswer(int selectedIndex)
        {
            var result = _session.SubmitVerdict(selectedIndex);
            var step   = caseData.Verdict;
            if (result.isCorrect)
                ShowLevelComplete();
            else
                PopupController.Show("Not Quite...",
                    step.FeedbackIncorrect, "Get a Hint", ShowHintsFromChief);
        }
    }
}
