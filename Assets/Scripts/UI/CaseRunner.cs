using ETEC510.Cases;
using ETEC510.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

        // ── Badge 1 Panel ─────────────────────────────────────────────────────
        [Header("Badge 1 Panel")]
        public GameObject badge1Panel;
        public Image      badge1BackgroundImage;
        public Button     badge1RepeatButton;               // replays Badge 1 Intro video
        public Button     badge1AnalyzeAccountButton;     // invisible overlay → AnalyzeAccountPanel
        public Button     badge1EvaluateCommentsButton;   // invisible overlay → EvaluateCommentsPanel
        public Button     badge1ExtractMetaDataButton;    // invisible overlay → ExtractMetaDataPanel
        public Button     badge1CriticalDecisionButton;    // → CriticalDecisionPointPanel
        public TMP_Text   badge1IncomingMessageText;      // Incoming Messages text box
        public Button     badge1ViralImageButton;         // invisible overlay top-left → viral image popup
        public Sprite     viralImageSprite;               // Viral_Image.png

        // ── Tool Panels (from Badge 1) ─────────────────────────────────────────
        [Header("Tool Panels")]
        public GameObject analyzeAccountPanel;
        public Button     analyzeAccountBackButton;   // invisible, over Return button in background
        public GameObject evaluateCommentsPanel;
        public Button     evaluateCommentsBackButton;
        public GameObject extractMetaDataPanel;
        public Button     extractMetaDataBackButton;

        // ── Tool Sub-Panels ───────────────────────────────────────────────────
        [Header("Tool Sub-Panels")]
        public GameObject accountProfilePanel;
        public Button     accountProfileBackButton;
        public GameObject commentsSectionPanel;
        public Button     commentsSectionBackButton;
        public GameObject metaDataPanel;
        public Button     metaDataBackButton;

        // ── Critical Decision Point ───────────────────────────────────────────
        [Header("Critical Decision Point")]
        public GameObject  criticalDecisionPanel;
        public RawImage    criticalDecisionVideoDisplay;
        public VideoPlayer criticalDecisionVideoPlayer;
        public Button      criticalDecisionSkipButton;
        public Image       criticalDecisionBackground;   // shown after video ends
        public Button      criticalDecisionSelect1Button; // invisible overlay — Account
        public Button      criticalDecisionSelect2Button; // invisible overlay — Comments
        public Button      criticalDecisionSelect3Button; // invisible overlay — MetaData
        public Button      criticalDecisionBackButton;    // visible, returns to Badge1Panel
        public Button      criticalDecisionNextButton;    // visible, skips to Badge2Panel
        public TMP_Text    criticalDecisionMessageText;   // hover text display

        // ── Badge 2 Panel ─────────────────────────────────────────────────────
        [Header("Badge 2 Panel")]
        public GameObject  badge2IntroPanel;
        public RawImage    badge2IntroVideoDisplay;
        public VideoPlayer badge2IntroVideoPlayer;
        public Image       badge2IntroBackground;      // shown after video ends
        public Button      badge2IntroSkipButton;      // visible during video → stops video, shows background
        public Button      badge2IntroProceedButton;   // invisible, over "Proceed to Task 2" → Badge2Panel
        public Button      badge2IntroBackButton;      // visible → CriticalDecisionPanel
        public GameObject  badge2Panel;
        public Image       badge2BackgroundImage;
        public TMP_Text    badge2IncomingMessageText;
        public Button      badge2RepeatButton;
        public Button      badge2AnalyticsBoardButton;
        public Button      badge2CommentFeedButton;
        public Button      badge2ContinueButton;
        public Button      badge2ViralImageButton;         // invisible overlay → viral image popup

        // ── Badge 2 Tool Panels ───────────────────────────────────────────────
        [Header("Badge 2 Tool Panels")]
        public GameObject badge2AnalyticsPanel;
        public Button     badge2AnalyticsBackButton;
        public GameObject badge2CommentFeedPanel;
        public Button     badge2CommentFeedBackButton;

        // ── Critical Decision Try Again ───────────────────────────────────────
        [Header("Critical Decision Try Again")]
        public GameObject critDecTryAgainPanel;
        public Button     critDecTryAgainRetryButton;   // invisible, over artwork "Try Again"

        // ── Critical Decision Award ───────────────────────────────────────────
        [Header("Critical Decision Award")]
        public GameObject  critDecAwardPanel;
        public RawImage    critDecAwardVideoDisplay;
        public VideoPlayer critDecAwardVideoPlayer;
        public Button      critDecAwardSkipButton;   // invisible overlay → Badge2IntroPanel

        // ── Badge Achieved ────────────────────────────────────────────────────
        [Header("Badge Achieved")]
        public GameObject  badgeAchievedPanel;
        public RawImage    badgeAchievedVideoDisplay;
        public VideoPlayer badgeAchievedVideoPlayer;
        public Button      badgeAchievedSkipButton;
        public Button      badgeAchievedBackButton;

        // ── Dispositional Award ───────────────────────────────────────────────
        [Header("Dispositional Award")]
        public GameObject  dispositionalAwardPanel;
        public RawImage    dispositionalAwardVideoDisplay;
        public VideoPlayer dispositionalAwardVideoPlayer;
        public Button      dispositionalAwardSkipButton;

        // Digit replay buttons on Evidence Board (clicking an earned digit replays its badge video)
        public Button digit1Button;
        public Button digit2Button;
        public Button digit3Button;

        // ── Intro ─────────────────────────────────────────────────────────────
        [Header("Intro")]
        public RawImage introVideoDisplay;   // RawImage that shows the video
        public VideoPlayer introVideoPlayer; // VideoPlayer component on the intro panel
        public AudioSource introAudioSource; // AudioSource for intro music
        public Button introEnterButton;      // shown after video ends
        public Button introSkipButton;       // lets player skip the video early

        // ── Mission Briefing ──────────────────────────────────────────────────
        [Header("Mission Briefing")]
        public RawImage    briefingVideoDisplay;  // RawImage for briefing video
        public VideoPlayer briefingVideoPlayer;   // VideoPlayer on the briefing panel
        public Button      briefingSkipButton;       // click video area to skip
        public Button      briefingRepeatButton;    // replays the video
        public TMP_Text briefingTitleText;
        public TMP_Text briefingBodyText;
        public Image    briefingImage;
        public Button   briefingStartButton;   // "Start Investigation"

        // ── Mission Start ─────────────────────────────────────────────────────
        [Header("Mission Start")]
        public GameObject  missionStartPanel;
        public RawImage    missionStartVideoDisplay;
        public VideoPlayer missionStartVideoPlayer;
        public Button      missionStartSkipButton;

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
        public RawImage    boardIntroVideoDisplay;
        public VideoPlayer boardIntroVideoPlayer;
        public Button      evidenceBoardBackButton;   // → Badge1Panel
        public Button      evidenceBoardBadge2Button; // → Badge2Panel

        // ── Spot the Clue ─────────────────────────────────────────────────────
        [Header("Spot the Clue")]
        public RawImage    spotVideoDisplay;
        public VideoPlayer spotVideoPlayer;
        public TMP_Text spotPromptText;
        public Image    spotEvidenceImage;
        public Button   spotEvidenceButton;           // click to zoom image
        public Button   spotAccountProfileButton;     // opens Account Profile sub-panel
        public Button[] spotOptionButtons;            // 2 answer buttons
        public TMP_Text spotFeedbackText;
        public Button   spotBackButton;

        // ── Image Popup ───────────────────────────────────────────────────────
        [Header("Image Popup")]
        public GameObject imagePopupPanel;
        public Image      imagePopupImage;
        public Button     imagePopupBackButton;

        // ── Gut Check ─────────────────────────────────────────────────────────
        [Header("Gut Check")]
        public TMP_Text gutPromptText;
        public Image    gutEvidenceImage;
        public Button   gutEvidenceButton;            // click to zoom image
        public Button   gutCommentsSectionButton;     // opens Comments Section sub-panel
        public Button[] gutOptionButtons;             // 2 buttons
        public TMP_Text gutFeedbackText;
        public Button   gutNextButton;
        public Button   gutBackButton;

        // ── Find the Motive ───────────────────────────────────────────────────
        [Header("Find the Motive")]
        public TMP_Text motivePromptText;
        public Image    motiveEvidenceImage;
        public Button   motiveEvidenceButton;         // click to zoom image
        public Button   motiveMetaDataButton;         // opens MetaData sub-panel
        public Button[] motiveOptionButtons;          // 2 answer buttons
        public TMP_Text motiveFeedbackText;
        public Button   motiveBackButton;

        // ── Password Lock ─────────────────────────────────────────────────────
        [Header("Password Lock")]
        public TMP_InputField passwordInputField;
        public TMP_Text       passwordFeedbackText;
        public Button         passwordSubmitButton;
        public Button         passwordBackButton;

        // ── Unlock ────────────────────────────────────────────────────────────
        [Header("Unlock")]
        public GameObject  unlockPanel;
        public RawImage    unlockVideoDisplay;
        public VideoPlayer unlockVideoPlayer;
        public Button      unlockSkipButton;

        [Header("Vault Entry")]
        public GameObject  vaultEntryPanel;
        public RawImage    vaultEntryVideoDisplay;
        public VideoPlayer vaultEntryVideoPlayer;
        public Button      vaultEntrySkipButton;

        // ── Evidence Detail (Verdict) ─────────────────────────────────────────
        [Header("Evidence Detail")]
        public TMP_Text verdictPromptText;
        public Image    verdictEvidenceImage;
        public Button[] verdictOptionButtons;  // 2 buttons: Real / Fake
        public Button   verdictBackButton;

        // ── Hints from Chief ──────────────────────────────────────────────────
        [Header("Hints from Chief")]
        public RawImage    hintVideoDisplay;           // RawImage on the main panel (HintFromChief.mp4)
        public VideoPlayer hintVideoPlayer;            // VideoPlayer on HintsFromChiefPanel (HintFromChief.mp4)
        public RawImage    hintsOverlayVideoDisplay;   // RawImage inside HintOverlay (HintsVideo.mp4)
        public VideoPlayer hintsOverlayVideoPlayer;    // VideoPlayer inside HintOverlay (HintsVideo.mp4)
        public GameObject  hintOverlay;
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
        private CaseSession   _session;
        private bool          _boardIntroPlayed;
        private bool          _critDecAwardButtonsWired;
        private bool          _badge2IntroButtonsWired;
        private bool          _badge2PanelButtonsWired;
        private GameObject    _imagePopupReturnPanel;
        private System.Action _badgeReturnAction;
        private System.Action _badgeBackAction;

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
            bool hasSource = url != null || (!IsWebGL && caseData.IntroVideo != null);
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
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (introVideoDisplay != null) introVideoDisplay.texture = rt;
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
            if (hintsOverlayVideoPlayer != null && hintsOverlayVideoPlayer.targetTexture != null)
            {
                hintsOverlayVideoPlayer.targetTexture.Release();
                Destroy(hintsOverlayVideoPlayer.targetTexture);
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

        private System.Collections.IEnumerator ShowAfterDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null) go.SetActive(true);
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

        // Fades music volume down to duckLevel (e.g. 0.15) over duration seconds.
        private System.Collections.IEnumerator DuckAudio(AudioSource source, float duckLevel, float duration)
        {
            if (source == null || !source.isPlaying) yield break;
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, duckLevel, elapsed / duration);
                yield return null;
            }
            source.volume = duckLevel;
        }

        // Fades music volume back up to targetVolume (typically 1) over duration seconds.
        private System.Collections.IEnumerator UnduckAudio(AudioSource source, float targetVolume, float duration)
        {
            if (source == null) yield break;
            float startVolume = source.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
                yield return null;
            }
            source.volume = targetVolume;
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

            // Badge 1 Panel
            if (badge1RepeatButton)           badge1RepeatButton.onClick.AddListener(OnMissionStartPressed);
            if (badge1AnalyzeAccountButton)   badge1AnalyzeAccountButton.onClick.AddListener(() => ShowPanel(analyzeAccountPanel));
            if (badge1EvaluateCommentsButton) badge1EvaluateCommentsButton.onClick.AddListener(() => ShowPanel(evaluateCommentsPanel));
            if (badge1ExtractMetaDataButton)      badge1ExtractMetaDataButton.onClick.AddListener(() => ShowPanel(extractMetaDataPanel));
            if (badge1CriticalDecisionButton)     badge1CriticalDecisionButton.onClick.AddListener(ShowCriticalDecisionPanel);
            if (badge1ViralImageButton)           badge1ViralImageButton.onClick.AddListener(() => ShowImagePopup(viralImageSprite, badge1Panel));

            // Badge 1 hover text
            AddHoverText(badge1AnalyzeAccountButton,   "Analyze Account");
            AddHoverText(badge1EvaluateCommentsButton, "Evaluate Comments");
            AddHoverText(badge1ExtractMetaDataButton,  "Extract Meta Data");
            AddHoverText(badge1CriticalDecisionButton, "Make Your Decision");
            AddHoverText(badge1RepeatButton,           "Replay Video");
            AddHoverText(badge1ViralImageButton,       "View Viral Image");

            // Critical Decision hover text
            AddHoverText(criticalDecisionSelect1Button, "Analyze Account",   criticalDecisionMessageText);
            AddHoverText(criticalDecisionSelect2Button, "Evaluate Comments", criticalDecisionMessageText);
            AddHoverText(criticalDecisionSelect3Button, "Extract Meta Data", criticalDecisionMessageText);
            AddHoverText(criticalDecisionNextButton,    "Skip to Next",      criticalDecisionMessageText);

            // Critical Decision Point
            if (criticalDecisionSkipButton)   criticalDecisionSkipButton.onClick.AddListener(SkipCriticalDecisionVideo);
            if (criticalDecisionBackButton)   criticalDecisionBackButton.onClick.AddListener(ShowBadge1Panel);
            if (criticalDecisionNextButton)   criticalDecisionNextButton.onClick.AddListener(PlayCritDecAwardVideo);
            // Select buttons — answer choices for the critical decision
            if (criticalDecisionSelect1Button) criticalDecisionSelect1Button.onClick.AddListener(() => OnCriticalDecisionSelect(0));
            if (criticalDecisionSelect2Button) criticalDecisionSelect2Button.onClick.AddListener(() => OnCriticalDecisionSelect(1));
            if (criticalDecisionSelect3Button) criticalDecisionSelect3Button.onClick.AddListener(() => OnCriticalDecisionSelect(2));

            // Badge 2 Intro
            if (badge2IntroSkipButton)    badge2IntroSkipButton.onClick.AddListener(SkipBadge2IntroVideo);
            if (badge2IntroProceedButton) badge2IntroProceedButton.onClick.AddListener(ShowBadge2Panel);
            if (badge2IntroBackButton)    badge2IntroBackButton.onClick.AddListener(PlayCritDecAwardVideo);
            if (badge2IntroSkipButton != null || badge2IntroProceedButton != null || badge2IntroBackButton != null)
                _badge2IntroButtonsWired = true;

            // Badge 2 Panel
            if (badge2RepeatButton)          badge2RepeatButton.onClick.AddListener(PlayBadge2IntroVideo);
            if (badge2AnalyticsBoardButton)  badge2AnalyticsBoardButton.onClick.AddListener(ShowBadge2AnalyticsPanel);
            if (badge2CommentFeedButton)     badge2CommentFeedButton.onClick.AddListener(ShowBadge2CommentFeedPanel);
            if (badge2ContinueButton)        badge2ContinueButton.onClick.AddListener(OnBadge2Continue);
            if (badge2AnalyticsBoardButton != null || badge2CommentFeedButton != null) _badge2PanelButtonsWired = true;

            // Badge 2 hover text
            AddHoverText(badge2AnalyticsBoardButton, "Analytics Board",  badge2IncomingMessageText);
            AddHoverText(badge2CommentFeedButton,    "Comments Feed",     badge2IncomingMessageText);
            AddHoverText(badge2RepeatButton,         "Replay Video",      badge2IncomingMessageText);
            AddHoverText(badge2ContinueButton,       "Dispositional Badge Award", badge2IncomingMessageText);

            // Badge 2 Tool Panels
            if (badge2AnalyticsBackButton)   badge2AnalyticsBackButton.onClick.AddListener(ShowBadge2Panel);
            if (badge2CommentFeedBackButton)  badge2CommentFeedBackButton.onClick.AddListener(ShowBadge2Panel);

            // Tool Panels
            if (analyzeAccountBackButton)   analyzeAccountBackButton.onClick.AddListener(ShowBadge1Panel);
            if (evaluateCommentsBackButton) evaluateCommentsBackButton.onClick.AddListener(ShowBadge1Panel);
            if (extractMetaDataBackButton)  extractMetaDataBackButton.onClick.AddListener(ShowBadge1Panel);

            // Mission Briefing
            if (briefingStartButton)  briefingStartButton.onClick.AddListener(OnMissionStartPressed);
            if (briefingSkipButton)   briefingSkipButton.onClick.AddListener(SkipBriefingVideo);
            if (briefingRepeatButton) briefingRepeatButton.onClick.AddListener(RepeatBriefingVideo);

            // Mission Start
            if (missionStartSkipButton) missionStartSkipButton.onClick.AddListener(SkipMissionStartVideo);

            // Evidence Board
            if (evidenceBoardBackButton)   evidenceBoardBackButton.onClick.AddListener(ShowBadge1Panel);
            if (evidenceBoardBadge2Button) evidenceBoardBadge2Button.onClick.AddListener(ShowBadge2Panel);
            if (spotTheClueButton)   spotTheClueButton.onClick.AddListener(OnSpotTheClueButton);
            if (gutCheckButton)      gutCheckButton.onClick.AddListener(OnGutCheckButton);
            if (findTheMotiveButton) findTheMotiveButton.onClick.AddListener(ShowFindTheMotive);
            if (enterPasswordButton) enterPasswordButton.onClick.AddListener(OnEnterPasswordPressed);

            // Spot the Clue
            WireIndexedButtons(spotOptionButtons, OnSpotAnswer);
            if (spotBackButton)            spotBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (spotEvidenceButton)        spotEvidenceButton.onClick.AddListener(
                () => ShowImagePopup(spotEvidenceImage != null ? spotEvidenceImage.sprite : null, spotTheCluePanel));
            if (spotAccountProfileButton)  spotAccountProfileButton.onClick.AddListener(
                () => ShowPanel(accountProfilePanel));

            // Gut Check
            WireIndexedButtons(gutOptionButtons, OnGutCheckAnswer);
            if (gutBackButton)             gutBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (gutEvidenceButton)         gutEvidenceButton.onClick.AddListener(
                () => ShowImagePopup(gutEvidenceImage != null ? gutEvidenceImage.sprite : null, gutCheckPanel));
            if (gutCommentsSectionButton)  gutCommentsSectionButton.onClick.AddListener(
                () => ShowPanel(commentsSectionPanel));

            // Find the Motive
            WireIndexedButtons(motiveOptionButtons, OnMotiveAnswer);
            if (motiveBackButton)          motiveBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (motiveEvidenceButton)      motiveEvidenceButton.onClick.AddListener(
                () => ShowImagePopup(motiveEvidenceImage != null ? motiveEvidenceImage.sprite : null, findTheMotivePanel));
            if (motiveMetaDataButton)      motiveMetaDataButton.onClick.AddListener(
                () => ShowPanel(metaDataPanel));

            // Tool Sub-Panels
            if (accountProfileBackButton)  accountProfileBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (commentsSectionBackButton) commentsSectionBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (metaDataBackButton)        metaDataBackButton.onClick.AddListener(ShowFindTheMotive);

            // Critical Decision Try Again
            if (critDecTryAgainRetryButton) critDecTryAgainRetryButton.onClick.AddListener(ShowCriticalDecisionBackground);

            // Critical Decision Award (Pragmatic)
            if (critDecAwardSkipButton) critDecAwardSkipButton.onClick.AddListener(SkipCritDecAwardVideo);
            if (critDecAwardSkipButton != null) _critDecAwardButtonsWired = true;

            // Badge Achieved
            if (badgeAchievedSkipButton) badgeAchievedSkipButton.onClick.AddListener(SkipBadgeVideo);
            if (badgeAchievedBackButton) badgeAchievedBackButton.onClick.AddListener(OnBadgeAchievedBack);
            if (digit1Button) digit1Button.onClick.AddListener(() =>
            {
                if (_session.SpotTheClueCompleted)
                    ShowBadgeAchieved(caseData.SpotTheClueBadgeVideoFile, ShowEvidenceBoard);
            });
            if (digit2Button) digit2Button.onClick.AddListener(() =>
            {
                if (_session.GutCheckCompleted)
                    ShowBadgeAchieved(caseData.GutCheckBadgeVideoFile, ShowEvidenceBoard);
            });
            if (digit3Button) digit3Button.onClick.AddListener(() =>
            {
                if (_session.FindTheMotiveCompleted)
                    ShowBadgeAchieved(caseData.FindTheMotiveBadgeVideoFile, ShowEvidenceBoard);
            });

            // Image Popup
            if (imagePopupBackButton) imagePopupBackButton.onClick.AddListener(HideImagePopup);

            // Password Lock
            if (passwordSubmitButton) passwordSubmitButton.onClick.AddListener(OnPasswordSubmit);
            if (passwordBackButton)   passwordBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (unlockSkipButton)     unlockSkipButton.onClick.AddListener(SkipUnlockVideo);
            if (vaultEntrySkipButton) vaultEntrySkipButton.onClick.AddListener(SkipVaultEntryVideo);
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
            AddClick(badge2IntroSkipButton); AddClick(badge2IntroProceedButton); AddClick(badge2IntroBackButton);
            AddClick(badge2RepeatButton); AddClick(badge2AnalyticsBoardButton); AddClick(badge2CommentFeedButton); AddClick(badge2ContinueButton); AddClick(badge2ViralImageButton);
            AddClick(badge2AnalyticsBackButton); AddClick(badge2CommentFeedBackButton);
            AddClick(badge1RepeatButton); AddClick(badge1AnalyzeAccountButton);
            AddClick(badge1EvaluateCommentsButton); AddClick(badge1ExtractMetaDataButton);
            AddClick(badge1CriticalDecisionButton); AddClick(badge1ViralImageButton);
            AddClick(analyzeAccountBackButton); AddClick(evaluateCommentsBackButton); AddClick(extractMetaDataBackButton);
            AddClick(criticalDecisionSkipButton); AddClick(criticalDecisionBackButton); AddClick(criticalDecisionNextButton);
            AddClick(criticalDecisionSelect1Button); AddClick(criticalDecisionSelect2Button); AddClick(criticalDecisionSelect3Button);
            AddClick(introEnterButton); AddClick(introSkipButton);
            AddClick(briefingStartButton);
            AddClick(spotTheClueButton); AddClick(gutCheckButton);
            AddClick(findTheMotiveButton); AddClick(enterPasswordButton);
            AddClick(spotBackButton); AddClick(spotEvidenceButton); AddClick(spotAccountProfileButton);
            AddClick(gutBackButton); AddClick(gutEvidenceButton); AddClick(gutCommentsSectionButton);
            AddClick(motiveBackButton); AddClick(motiveEvidenceButton); AddClick(motiveMetaDataButton);
            AddClick(imagePopupBackButton);
            AddClick(critDecTryAgainRetryButton);
            AddClick(critDecAwardSkipButton);
            AddClick(badgeAchievedSkipButton); AddClick(badgeAchievedBackButton); AddClick(digit1Button); AddClick(digit2Button); AddClick(digit3Button);
            AddClick(accountProfileBackButton); AddClick(commentsSectionBackButton); AddClick(metaDataBackButton);
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
                soundTogglePanel, introPanel, missionBriefingPanel, missionStartPanel, evidenceBoardPanel,
                badge1Panel, criticalDecisionPanel, critDecTryAgainPanel, critDecAwardPanel, badge2IntroPanel, badge2Panel,
                badge2AnalyticsPanel, badge2CommentFeedPanel,
                analyzeAccountPanel, evaluateCommentsPanel, extractMetaDataPanel,
                spotTheCluePanel, gutCheckPanel, findTheMotivePanel,
                passwordLockPanel, unlockPanel, vaultEntryPanel, evidenceDetailPanel,
                hintsFromChiefPanel, levelCompletePanel,
                accountProfilePanel, commentsSectionPanel, metaDataPanel,
                badgeAchievedPanel,
                dispositionalAwardPanel
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
            // Populate fields but keep hidden until loop video starts
            if (briefingTitleText) briefingTitleText.text = caseData.Title;
            if (briefingBodyText)  briefingBodyText.text  = caseData.BriefingText;
            if (briefingImage && caseData.BriefingImage) briefingImage.sprite = caseData.BriefingImage;
            EnsureCoverFit(briefingImage);
            SetBriefingContentVisible(false);

            if (introAudioSource != null) introAudioSource.Stop();
            ShowPanel(missionBriefingPanel);
            PlayBriefingVideo();
        }

        private void SetBriefingContentVisible(bool visible)
        {
            if (briefingTitleText)   briefingTitleText.gameObject.SetActive(visible);
            if (briefingBodyText)    briefingBodyText.gameObject.SetActive(visible);
            if (briefingImage)       briefingImage.gameObject.SetActive(visible);
            if (briefingStartButton) briefingStartButton.gameObject.SetActive(visible);
            if (briefingRepeatButton) briefingRepeatButton.gameObject.SetActive(visible);
            // Video loops behind content — always visible, never blocks input
            if (briefingVideoDisplay) { briefingVideoDisplay.color = Color.white; briefingVideoDisplay.raycastTarget = false; }
        }

        private void SkipBriefingVideo()
        {
            if (briefingSkipButton != null) briefingSkipButton.gameObject.SetActive(false);
            if (briefingVideoPlayer != null)
            {
                briefingVideoPlayer.loopPointReached -= OnBriefingVideoFinished;
                briefingVideoPlayer.Stop();
            }
            SetBriefingContentVisible(true);
            StartMainMusic();
            // Jump straight to the loop video
            var loopUrl = BuildVideoUrl(caseData.BriefingLoopVideoFile);
            if (loopUrl != null && briefingVideoPlayer != null)
            {
                briefingVideoPlayer.errorReceived    -= OnBriefingVideoError;
                briefingVideoPlayer.prepareCompleted -= OnBriefingVideoPrepared;
                briefingVideoPlayer.isLooping       = true;
                briefingVideoPlayer.source          = VideoSource.Url;
                briefingVideoPlayer.url             = loopUrl;
                briefingVideoPlayer.errorReceived    += OnBriefingVideoError;
                briefingVideoPlayer.prepareCompleted += OnBriefingVideoPrepared;
                briefingVideoPlayer.gameObject.SetActive(true);
                briefingVideoPlayer.Prepare();
            }
        }

        private void RepeatBriefingVideo()
        {
            SetBriefingContentVisible(false);
            PlayBriefingVideo();
        }

        private void ShowBriefingContent()
        {
            if (briefingTitleText) briefingTitleText.text = caseData.Title;
            if (briefingBodyText)  briefingBodyText.text  = caseData.BriefingText;
            if (briefingImage && caseData.BriefingImage)
                briefingImage.sprite = caseData.BriefingImage;
            EnsureCoverFit(briefingImage);
            SetBriefingContentVisible(true);
        }

        private void PlayBriefingVideo()
        {
            if (briefingVideoPlayer == null) { ShowBriefingContent(); return; }

            var url = BuildVideoUrl(caseData.BriefingVideoFile);
            bool hasSource = url != null || (!IsWebGL && caseData.BriefingVideo != null);
            if (!hasSource) { ShowBriefingContent(); return; }

            if (briefingVideoDisplay != null) briefingVideoDisplay.color = Color.clear;

            // Show skip button as label-only (transparent background)
            if (briefingSkipButton != null)
            {
                briefingSkipButton.gameObject.SetActive(true);
                var img = briefingSkipButton.GetComponent<UnityEngine.UI.Image>();
                if (img != null) img.color = Color.clear;
            }

            // Remove stale listeners before re-adding (safe if not subscribed)
            briefingVideoPlayer.errorReceived    -= OnBriefingVideoError;
            briefingVideoPlayer.loopPointReached -= OnBriefingVideoFinished;
            briefingVideoPlayer.prepareCompleted -= OnBriefingVideoPrepared;

            briefingVideoPlayer.isLooping       = false;
            briefingVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            briefingVideoPlayer.errorReceived    += OnBriefingVideoError;
            briefingVideoPlayer.loopPointReached += OnBriefingVideoFinished;
            briefingVideoPlayer.prepareCompleted += OnBriefingVideoPrepared;

            if (url != null) { briefingVideoPlayer.source = VideoSource.Url; briefingVideoPlayer.url = url; }
            else             { briefingVideoPlayer.source = VideoSource.VideoClip; briefingVideoPlayer.clip = caseData.BriefingVideo; }

            briefingVideoPlayer.gameObject.SetActive(true);
            briefingVideoPlayer.Prepare();
        }

        private void OnBriefingVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnBriefingVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (briefingVideoDisplay != null)
            {
                briefingVideoDisplay.texture = rt;
                briefingVideoDisplay.color = Color.white;
            }
            vp.Play();
        }

        private void OnBriefingVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnBriefingVideoFinished;
            if (briefingSkipButton != null) briefingSkipButton.gameObject.SetActive(false);
            SetBriefingContentVisible(true);
            StartMainMusic();
            // Switch to loop video if configured
            var loopUrl = BuildVideoUrl(caseData.BriefingLoopVideoFile);
            if (loopUrl != null)
            {
                vp.errorReceived    -= OnBriefingVideoError;
                vp.prepareCompleted -= OnBriefingVideoPrepared;
                vp.isLooping       = true;
                vp.source          = VideoSource.Url;
                vp.url             = loopUrl;
                vp.errorReceived    += OnBriefingVideoError;
                vp.prepareCompleted += OnBriefingVideoPrepared;
                vp.gameObject.SetActive(true);
                vp.Prepare();
            }
            else
            {
                if (briefingVideoDisplay != null) briefingVideoDisplay.color = Color.clear;
            }
        }

        private void OnBriefingVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Briefing video error: {msg}");
            vp.errorReceived -= OnBriefingVideoError;
            if (briefingVideoDisplay != null) briefingVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowBriefingContent();
        }

        private void SkipMissionStartVideo()
        {
            if (missionStartVideoPlayer != null)
            {
                missionStartVideoPlayer.loopPointReached -= OnMissionStartVideoFinished;
                missionStartVideoPlayer.Stop();
            }
            if (missionStartVideoDisplay != null) missionStartVideoDisplay.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            _boardIntroPlayed = true;
            ShowBadge1Panel();
        }

        private void OnMissionStartPressed()
        {
            var url = BuildVideoUrl(caseData.MissionStartVideoFile);
            bool hasSource = url != null || (!IsWebGL && caseData.MissionStartVideo != null);
            if (!hasSource) { _boardIntroPlayed = true; ShowEvidenceBoard(); return; }

            ShowPanel(missionStartPanel);

            if (missionStartVideoDisplay != null)
            {
                missionStartVideoDisplay.gameObject.SetActive(true);
                missionStartVideoDisplay.color        = Color.clear;
                missionStartVideoDisplay.raycastTarget = false;
            }

            if (missionStartVideoPlayer == null) { ShowEvidenceBoard(); return; }

            missionStartVideoPlayer.errorReceived    -= OnMissionStartVideoError;
            missionStartVideoPlayer.loopPointReached -= OnMissionStartVideoFinished;
            missionStartVideoPlayer.prepareCompleted -= OnMissionStartVideoPrepared;

            missionStartVideoPlayer.isLooping       = false;
            missionStartVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            missionStartVideoPlayer.errorReceived    += OnMissionStartVideoError;
            missionStartVideoPlayer.loopPointReached += OnMissionStartVideoFinished;
            missionStartVideoPlayer.prepareCompleted += OnMissionStartVideoPrepared;

            if (url != null) { missionStartVideoPlayer.source = VideoSource.Url; missionStartVideoPlayer.url = url; }
            else             { missionStartVideoPlayer.source = VideoSource.VideoClip; missionStartVideoPlayer.clip = caseData.MissionStartVideo; }

            if (IsNarration(caseData.MissionStartVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            missionStartVideoPlayer.Prepare();
        }

        private void OnMissionStartVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnMissionStartVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (missionStartVideoDisplay != null)
            {
                missionStartVideoDisplay.texture       = rt;
                missionStartVideoDisplay.color         = Color.white;
                missionStartVideoDisplay.raycastTarget = false;
            }
            vp.Play();
        }

        private void OnMissionStartVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnMissionStartVideoFinished;
            if (missionStartVideoDisplay != null) missionStartVideoDisplay.gameObject.SetActive(false);
            _boardIntroPlayed = true;
            StartMainMusic();
            ShowBadge1Panel();
        }

        private void OnMissionStartVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Mission start video error: {msg}");
            vp.errorReceived -= OnMissionStartVideoError;
            if (missionStartVideoDisplay != null) missionStartVideoDisplay.gameObject.SetActive(false);
            _boardIntroPlayed = true;
            StartMainMusic();
            ShowBadge1Panel();
        }

        private void ShowBadge1Panel()
        {
            Debug.Log($"[CaseRunner] ShowBadge1Panel called. badge1Panel={badge1Panel?.name ?? "NULL"}\n{System.Environment.StackTrace}");
            if (badge1Panel != null)
                ShowPanel(badge1Panel);
            else
                ShowEvidenceBoard(); // fallback if panel not built yet
        }

        private void OnBadge2Continue()
        {
            PlayDispositionalAwardVideo(onComplete: ShowEvidenceBoard, onBack: ShowBadge2Panel);
        }

        private void ShowBadge2Panel()
        {
            if (badge2Panel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null) { var t = canvas.transform.Find("Badge2Panel"); if (t != null) badge2Panel = t.gameObject; }
            }
            if (badge2Panel == null) { ShowEvidenceBoard(); return; }

            // Resolve text box if needed
            if (badge2IncomingMessageText == null)
                badge2IncomingMessageText = badge2Panel.transform.Find("IncomingMessageText")?.GetComponent<TMP_Text>();

            // Resolve button refs via fallback if null
            if (badge2AnalyticsBoardButton == null)
                badge2AnalyticsBoardButton = badge2Panel.transform.Find("AnalyticsBoardButton")?.GetComponent<Button>();
            if (badge2CommentFeedButton == null)
                badge2CommentFeedButton = badge2Panel.transform.Find("CommentFeedButton")?.GetComponent<Button>();
            if (badge2RepeatButton == null)
                badge2RepeatButton = badge2Panel.transform.Find("Badge2RepeatButton")?.GetComponent<Button>();
            if (badge2ContinueButton == null)
                badge2ContinueButton = badge2Panel.transform.Find("Badge2ContinueButton")?.GetComponent<Button>();
            if (badge2ViralImageButton == null)
                badge2ViralImageButton = badge2Panel.transform.Find("Badge2ViralImageButton")?.GetComponent<Button>();

            // Wire each button using remove+add to avoid duplicates
            WireOnce(badge2AnalyticsBoardButton, ShowBadge2AnalyticsPanel);
            WireOnce(badge2CommentFeedButton,    ShowBadge2CommentFeedPanel);
            WireOnce(badge2RepeatButton,         PlayBadge2IntroVideo);
            WireOnce(badge2ContinueButton,       OnBadge2Continue);
            if (badge2ViralImageButton != null)
                WireOnce(badge2ViralImageButton, () => ShowImagePopup(viralImageSprite, badge2Panel));
            if (!_badge2PanelButtonsWired)
            {
                AddHoverText(badge2AnalyticsBoardButton, "Analytics Board",        badge2IncomingMessageText);
                AddHoverText(badge2CommentFeedButton,    "Comments Feed",           badge2IncomingMessageText);
                AddHoverText(badge2RepeatButton,         "Replay Video",            badge2IncomingMessageText);
                AddHoverText(badge2ContinueButton,       "Dispositional Badge Award", badge2IncomingMessageText);
                AddHoverText(badge2ViralImageButton,     "View Viral Image",        badge2IncomingMessageText);
                _badge2PanelButtonsWired = true;
            }

            ShowPanel(badge2Panel);
        }

        private void ShowBadge2AnalyticsPanel()
        {
            if (badge2AnalyticsPanel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null)
                {
                    var t = canvas.transform.Find("Badge2AnalyticsPanel");
                    if (t != null) badge2AnalyticsPanel = t.gameObject;
                }
            }
            if (badge2AnalyticsPanel != null)
                ShowPanel(badge2AnalyticsPanel);
            else if (badge2IncomingMessageText != null)
                badge2IncomingMessageText.text = "Analytics Board not available.";
        }

        private void ShowBadge2CommentFeedPanel()
        {
            if (badge2CommentFeedPanel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null)
                {
                    var t = canvas.transform.Find("Badge2CommentFeedPanel");
                    if (t != null) badge2CommentFeedPanel = t.gameObject;
                }
            }
            if (badge2CommentFeedPanel != null)
                ShowPanel(badge2CommentFeedPanel);
            else if (badge2IncomingMessageText != null)
                badge2IncomingMessageText.text = "Comment Feed not available.";
        }

        private void PlayBadge2IntroVideo()
        {
            // Runtime lookup fallback
            if (badge2IntroPanel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null)
                {
                    var t = canvas.transform.Find("Badge2IntroPanel");
                    if (t != null) badge2IntroPanel = t.gameObject;
                }
            }
            if (badge2IntroVideoPlayer == null && badge2IntroPanel != null)
                badge2IntroVideoPlayer = badge2IntroPanel.GetComponent<VideoPlayer>();
            if (badge2IntroSkipButton == null && badge2IntroPanel != null)
                badge2IntroSkipButton = badge2IntroPanel.transform.Find("Badge2IntroSkipButton")?.GetComponent<Button>();
            if (badge2IntroProceedButton == null && badge2IntroPanel != null)
                badge2IntroProceedButton = badge2IntroPanel.transform.Find("Badge2IntroProceedButton")?.GetComponent<Button>();
            if (badge2IntroBackButton == null && badge2IntroPanel != null)
                badge2IntroBackButton = badge2IntroPanel.transform.Find("Badge2IntroBackButton")?.GetComponent<Button>();
            if (badge2IntroBackground == null && badge2IntroPanel != null)
                badge2IntroBackground = badge2IntroPanel.transform.Find("Badge2IntroBg")?.GetComponent<Image>();

            // Wire listeners if resolved via fallback
            if (!_badge2IntroButtonsWired)
            {
                if (badge2IntroSkipButton    != null) badge2IntroSkipButton.onClick.AddListener(SkipBadge2IntroVideo);
                if (badge2IntroProceedButton != null) badge2IntroProceedButton.onClick.AddListener(ShowBadge2Panel);
                if (badge2IntroBackButton    != null) badge2IntroBackButton.onClick.AddListener(PlayCritDecAwardVideo);
                if (badge2IntroSkipButton != null || badge2IntroProceedButton != null || badge2IntroBackButton != null)
                    _badge2IntroButtonsWired = true;
            }

            var url = BuildVideoUrl(caseData.Badge2IntroVideoFile);
            bool hasSource = url != null || (!IsWebGL && false);
            if (badge2IntroPanel == null || badge2IntroVideoPlayer == null || !hasSource)
            {
                ShowBadge2Panel(); return;
            }

            ShowPanel(badge2IntroPanel);

            // Video state: show skip + back, hide background/proceed
            if (badge2IntroBackground    != null) badge2IntroBackground.gameObject.SetActive(false);
            if (badge2IntroSkipButton    != null) badge2IntroSkipButton.gameObject.SetActive(true);
            if (badge2IntroProceedButton != null) badge2IntroProceedButton.gameObject.SetActive(false);
            if (badge2IntroBackButton    != null) badge2IntroBackButton.gameObject.SetActive(true);

            if (badge2IntroVideoDisplay != null)
            {
                badge2IntroVideoDisplay.gameObject.SetActive(true);
                badge2IntroVideoDisplay.color        = Color.clear;
                badge2IntroVideoDisplay.raycastTarget = false;
            }

            badge2IntroVideoPlayer.errorReceived    -= OnBadge2IntroVideoError;
            badge2IntroVideoPlayer.loopPointReached -= OnBadge2IntroVideoFinished;
            badge2IntroVideoPlayer.prepareCompleted -= OnBadge2IntroVideoPrepared;
            badge2IntroVideoPlayer.isLooping        = false;
            badge2IntroVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            badge2IntroVideoPlayer.errorReceived    += OnBadge2IntroVideoError;
            badge2IntroVideoPlayer.loopPointReached += OnBadge2IntroVideoFinished;
            badge2IntroVideoPlayer.prepareCompleted += OnBadge2IntroVideoPrepared;
            badge2IntroVideoPlayer.source = VideoSource.Url;
            badge2IntroVideoPlayer.url    = url;

            if (IsNarration(caseData.Badge2IntroVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            badge2IntroVideoPlayer.Prepare();
        }

        private void SkipBadge2IntroVideo()
        {
            if (badge2IntroVideoPlayer != null)
            {
                badge2IntroVideoPlayer.loopPointReached -= OnBadge2IntroVideoFinished;
                badge2IntroVideoPlayer.Stop();
            }
            ShowBadge2IntroContent();
        }

        private void ShowBadge2IntroContent()
        {
            // Hide video state elements before transitioning to Badge2Panel
            if (badge2IntroVideoDisplay != null) badge2IntroVideoDisplay.gameObject.SetActive(false);
            if (badge2IntroSkipButton   != null) badge2IntroSkipButton.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowBadge2Panel();
        }

        private void OnBadge2IntroVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnBadge2IntroVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (badge2IntroVideoDisplay != null)
            {
                badge2IntroVideoDisplay.texture = rt;
                badge2IntroVideoDisplay.color   = Color.white;
            }
            vp.Play();
        }

        private void OnBadge2IntroVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnBadge2IntroVideoFinished;
            ShowBadge2IntroContent();
        }

        private void OnBadge2IntroVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Badge 2 intro video error: {msg}");
            vp.errorReceived -= OnBadge2IntroVideoError;
            ShowBadge2IntroContent();
        }

        private void OnCriticalDecisionSelect(int idx)
        {
            var step = caseData != null ? caseData.CriticalDecision : null;
            Debug.Log($"[CaseRunner] CritDecSelect idx={idx} step={step != null} correctIdx={step?.CorrectIndex}");
            if (step == null) return;
            PlayClick();
            if (idx == step.CorrectIndex)
            {
                PlayCritDecAwardVideo();
            }
            else
            {
                ShowCritDecTryAgainPanel();
            }
        }

        private void ShowCritDecTryAgainPanel()
        {
            if (critDecTryAgainPanel != null)
                ShowPanel(critDecTryAgainPanel);
            else
                ShowCriticalDecisionBackground(); // fallback
        }

        // ── Critical Decision Award ───────────────────────────────────────────

        private void PlayCritDecAwardVideo()
        {
            var _diagVideoFile = caseData?.CriticalDecision?.BadgeVideoFile;
            Debug.Log($"[CaseRunner] PlayCritDecAwardVideo — panel={critDecAwardPanel?.name ?? "NULL"} player={critDecAwardVideoPlayer?.name ?? "NULL"} badgeVideoFile={_diagVideoFile ?? "NULL"}");

            // Runtime lookup fallback — resolves null serialized refs caused by scene-save timing
            if (critDecAwardPanel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null)
                {
                    var t = canvas.transform.Find("CritDecAwardPanel");
                    if (t != null) critDecAwardPanel = t.gameObject;
                }
            }
            if (critDecAwardVideoPlayer == null && critDecAwardPanel != null)
                critDecAwardVideoPlayer = critDecAwardPanel.GetComponent<VideoPlayer>();
            if (critDecAwardSkipButton == null && critDecAwardPanel != null)
                critDecAwardSkipButton = critDecAwardPanel.transform.Find("CritDecAwardSkipButton")?.GetComponent<Button>();
            if (critDecAwardVideoDisplay == null && critDecAwardPanel != null)
                critDecAwardVideoDisplay = critDecAwardPanel.transform.Find("CritDecAwardVideoDisplay")?.GetComponent<RawImage>();
            if (critDecAwardVideoDisplay == null && critDecAwardPanel != null)
                critDecAwardVideoDisplay = EnsureVideoDisplay(critDecAwardPanel, "CritDecAwardVideoDisplay");

            // Wire listeners — use WireOnce for back button so it is always correctly wired
            // regardless of whether _critDecAwardButtonsWired was set early by the skip button.
            if (critDecAwardSkipButton != null && !_critDecAwardButtonsWired)
            {
                critDecAwardSkipButton.onClick.AddListener(SkipCritDecAwardVideo);
                AddClick(critDecAwardSkipButton);
            }
            _critDecAwardButtonsWired = true;

            var step = caseData != null ? caseData.CriticalDecision : null;
            var videoFile = step?.BadgeVideoFile;
            var url = BuildVideoUrl(videoFile);

            if (critDecAwardPanel == null || critDecAwardVideoPlayer == null || url == null)
            {
                Debug.LogWarning($"[CaseRunner] CritDecAward fallback — panel={critDecAwardPanel != null} player={critDecAwardVideoPlayer != null} url={url}");
                PlayBadge2IntroVideo(); return;
            }

            ShowPanel(critDecAwardPanel);

            if (critDecAwardSkipButton != null) critDecAwardSkipButton.gameObject.SetActive(true);

            if (critDecAwardVideoDisplay != null)
            {
                critDecAwardVideoDisplay.gameObject.SetActive(true);
                critDecAwardVideoDisplay.color        = Color.clear;
                critDecAwardVideoDisplay.raycastTarget = false;
            }

            critDecAwardVideoPlayer.errorReceived    -= OnCritDecAwardVideoError;
            critDecAwardVideoPlayer.loopPointReached -= OnCritDecAwardVideoFinished;
            critDecAwardVideoPlayer.prepareCompleted -= OnCritDecAwardVideoPrepared;
            critDecAwardVideoPlayer.isLooping        = false;
            critDecAwardVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            critDecAwardVideoPlayer.errorReceived    += OnCritDecAwardVideoError;
            critDecAwardVideoPlayer.loopPointReached += OnCritDecAwardVideoFinished;
            critDecAwardVideoPlayer.prepareCompleted += OnCritDecAwardVideoPrepared;
            critDecAwardVideoPlayer.source = VideoSource.Url;
            critDecAwardVideoPlayer.url    = url;

            if (IsNarration(videoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            critDecAwardVideoPlayer.Prepare();
        }

        private void SkipCritDecAwardVideo()
        {
            if (critDecAwardVideoPlayer != null)
            {
                critDecAwardVideoPlayer.loopPointReached -= OnCritDecAwardVideoFinished;
                critDecAwardVideoPlayer.Stop();
            }
            ShowCritDecAwardContent();
        }

        private void ShowCritDecAwardContent()
        {
            _critDecAwardContentShown = true;
            if (critDecAwardVideoDisplay != null) critDecAwardVideoDisplay.color = Color.clear;
            if (critDecAwardSkipButton != null) critDecAwardSkipButton.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            PlayBadge2IntroVideo();

        }

        private bool _critDecAwardContentShown;

        private void OnCritDecAwardVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnCritDecAwardVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (critDecAwardVideoDisplay != null)
            {
                critDecAwardVideoDisplay.texture = rt;
                critDecAwardVideoDisplay.color   = Color.white;
            }
            _critDecAwardContentShown = false;
            vp.Play();
            StartCoroutine(WaitForCritDecAwardVideoEnd(vp));
        }

        private System.Collections.IEnumerator WaitForCritDecAwardVideoEnd(VideoPlayer vp)
        {
            // Wait until playing starts, then wait until it stops
            yield return new WaitUntil(() => vp.isPlaying);
            yield return new WaitUntil(() => !vp.isPlaying || !vp.gameObject.activeInHierarchy);
            if (!_critDecAwardContentShown)
                ShowCritDecAwardContent();
        }

        private void OnCritDecAwardVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnCritDecAwardVideoFinished;
            if (!_critDecAwardContentShown)
                ShowCritDecAwardContent();
        }

        private void OnCritDecAwardVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] CritDec award video error: {msg}");
            vp.errorReceived -= OnCritDecAwardVideoError;
            ShowCritDecAwardContent();
        }

        private void ShowCriticalDecisionBackground()
        {
            ShowPanel(criticalDecisionPanel);
            if (criticalDecisionVideoDisplay != null) criticalDecisionVideoDisplay.gameObject.SetActive(false);
            if (criticalDecisionBackground   != null) criticalDecisionBackground.gameObject.SetActive(true);
            SetCriticalDecisionButtonsVisible(true);
        }

        public void ShowCriticalDecisionPanel()
        {
            Debug.Log($"[CaseRunner] ShowCriticalDecisionPanel — panel={criticalDecisionPanel?.name ?? "NULL"} videoFile={caseData?.CriticalDecisionVideoFile ?? "NULL"} videoPlayer={criticalDecisionVideoPlayer?.name ?? "NULL"}");
            ShowPanel(criticalDecisionPanel);

            if (criticalDecisionBackground != null) criticalDecisionBackground.gameObject.SetActive(false);
            SetCriticalDecisionButtonsVisible(false);

            if (criticalDecisionVideoDisplay != null)
            {
                criticalDecisionVideoDisplay.gameObject.SetActive(true);
                criticalDecisionVideoDisplay.color        = Color.clear;
                criticalDecisionVideoDisplay.raycastTarget = false;
            }

            var url = BuildVideoUrl(caseData.CriticalDecisionVideoFile);
            bool hasSource = url != null || (!IsWebGL && false); // WebGL URL only
            if (criticalDecisionVideoPlayer == null || !hasSource)
            {
                ShowCriticalDecisionContent(); return;
            }

            criticalDecisionVideoPlayer.errorReceived    -= OnCriticalDecisionVideoError;
            criticalDecisionVideoPlayer.loopPointReached -= OnCriticalDecisionVideoFinished;
            criticalDecisionVideoPlayer.prepareCompleted -= OnCriticalDecisionVideoPrepared;
            criticalDecisionVideoPlayer.isLooping        = false;
            criticalDecisionVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            criticalDecisionVideoPlayer.errorReceived    += OnCriticalDecisionVideoError;
            criticalDecisionVideoPlayer.loopPointReached += OnCriticalDecisionVideoFinished;
            criticalDecisionVideoPlayer.prepareCompleted += OnCriticalDecisionVideoPrepared;
            criticalDecisionVideoPlayer.source = VideoSource.Url;
            criticalDecisionVideoPlayer.url    = url;

            if (IsNarration(caseData.CriticalDecisionVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            criticalDecisionVideoPlayer.Prepare();
        }

        private void ShowCriticalDecisionContent()
        {
            if (criticalDecisionVideoDisplay != null) criticalDecisionVideoDisplay.gameObject.SetActive(false);
            if (criticalDecisionBackground   != null) criticalDecisionBackground.gameObject.SetActive(true);
            SetCriticalDecisionButtonsVisible(true);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
        }

        private void SetCriticalDecisionButtonsVisible(bool visible)
        {
            if (criticalDecisionSkipButton)    criticalDecisionSkipButton.gameObject.SetActive(!visible);
            if (criticalDecisionBackButton)    criticalDecisionBackButton.gameObject.SetActive(visible);
            if (criticalDecisionNextButton)    criticalDecisionNextButton.gameObject.SetActive(visible);
        }

        private void SkipCriticalDecisionVideo()
        {
            if (criticalDecisionVideoPlayer != null)
            {
                criticalDecisionVideoPlayer.loopPointReached -= OnCriticalDecisionVideoFinished;
                criticalDecisionVideoPlayer.Stop();
            }
            ShowCriticalDecisionContent();
        }

        private void OnCriticalDecisionVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnCriticalDecisionVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (criticalDecisionVideoDisplay != null)
            {
                criticalDecisionVideoDisplay.texture = rt;
                criticalDecisionVideoDisplay.color   = Color.white;
            }
            vp.Play();
        }

        private void OnCriticalDecisionVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnCriticalDecisionVideoFinished;
            ShowCriticalDecisionContent();
        }

        private void OnCriticalDecisionVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Critical Decision video error: {msg}");
            vp.errorReceived -= OnCriticalDecisionVideoError;
            ShowCriticalDecisionContent();
        }

        public void ShowEvidenceBoard()
        {
            if (introAudioSource != null) introAudioSource.Stop();

            if (evidenceBoardImage && caseData.EvidenceBoardImage)
                evidenceBoardImage.sprite = caseData.EvidenceBoardImage;
            EnsureCoverFit(evidenceBoardImage);

            if (digit1Text) digit1Text.text = _session.SpotTheClueCompleted   ? caseData.SpotTheClue.CodeDigit   : "?";
            if (digit2Text) digit2Text.text = _session.GutCheckCompleted       ? caseData.GutCheck.CodeDigit       : "?";
            if (digit3Text) digit3Text.text = _session.FindTheMotiveCompleted  ? caseData.FindTheMotive.CodeDigit  : "?";

            SetButtonLabelVisible(spotTheClueButton,  !_session.SpotTheClueCompleted);
            SetButtonLabelVisible(gutCheckButton,     !_session.GutCheckCompleted);
            SetButtonLabelVisible(findTheMotiveButton,!_session.FindTheMotiveCompleted);

            if (boardWarningText) boardWarningText.gameObject.SetActive(false);

            if (enterPasswordButton)
            {
                enterPasswordButton.onClick.RemoveAllListeners();
                enterPasswordButton.onClick.AddListener(OnEnterPasswordPressed);
            }

            ShowPanel(evidenceBoardPanel);

            // Play intro video once on the first visit to the board
            if (!_boardIntroPlayed)
                PlayBoardIntroVideo();
        }

        private void StartMainMusic()
        {
            if (mainAudioSource == null || caseData.MainMusic == null) return;
            if (!mainAudioSource.isPlaying)
            {
                mainAudioSource.clip   = caseData.MainMusic;
                mainAudioSource.loop   = true;
                mainAudioSource.volume = 0f;
                mainAudioSource.Play();
            }
            StartCoroutine(UnduckAudio(mainAudioSource, 1f, 1.5f));
        }

        private void PlayBoardIntroVideo()
        {
            if (boardIntroVideoPlayer == null)
            {
                if (boardIntroVideoDisplay != null) boardIntroVideoDisplay.gameObject.SetActive(false);
                _boardIntroPlayed = true; StartMainMusic(); return;
            }

            var url = BuildVideoUrl(caseData.EvidenceBoardIntroVideoFile);
            bool hasSource = url != null || (!IsWebGL && caseData.EvidenceBoardIntroVideo != null);
            if (!hasSource)
            {
                if (boardIntroVideoDisplay != null) boardIntroVideoDisplay.gameObject.SetActive(false);
                _boardIntroPlayed = true; StartMainMusic(); return;
            }

            _boardIntroPlayed = true;
            if (boardIntroVideoDisplay != null) { boardIntroVideoDisplay.gameObject.SetActive(true); boardIntroVideoDisplay.color = Color.clear; boardIntroVideoDisplay.raycastTarget = false; }

            boardIntroVideoPlayer.errorReceived    -= OnBoardIntroVideoError;
            boardIntroVideoPlayer.loopPointReached -= OnBoardIntroVideoFinished;
            boardIntroVideoPlayer.prepareCompleted -= OnBoardIntroVideoPrepared;

            boardIntroVideoPlayer.isLooping       = false;
            boardIntroVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            boardIntroVideoPlayer.errorReceived    += OnBoardIntroVideoError;
            boardIntroVideoPlayer.loopPointReached += OnBoardIntroVideoFinished;
            boardIntroVideoPlayer.prepareCompleted += OnBoardIntroVideoPrepared;

            if (url != null) { boardIntroVideoPlayer.source = VideoSource.Url; boardIntroVideoPlayer.url = url; }
            else             { boardIntroVideoPlayer.source = VideoSource.VideoClip; boardIntroVideoPlayer.clip = caseData.EvidenceBoardIntroVideo; }

            boardIntroVideoPlayer.Prepare();
        }

        private void OnBoardIntroVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnBoardIntroVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (boardIntroVideoDisplay != null)
            {
                boardIntroVideoDisplay.texture = rt;
                boardIntroVideoDisplay.color = Color.white; boardIntroVideoDisplay.raycastTarget = false;
            }
            vp.Play();
        }

        private void OnBoardIntroVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnBoardIntroVideoFinished;
            if (boardIntroVideoDisplay != null) boardIntroVideoDisplay.gameObject.SetActive(false);
            StartMainMusic();
        }

        private void OnBoardIntroVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Board intro video error: {msg}");
            vp.errorReceived -= OnBoardIntroVideoError;
            if (boardIntroVideoDisplay != null) boardIntroVideoDisplay.gameObject.SetActive(false);
            StartMainMusic();
        }

        private void OnSpotTheClueButton()
        {
            if (!_session.SpotTheClueCompleted)
                _session.CompleteSpotTheClue();
            ShowBadgeAchieved(caseData.SpotTheClueBadgeVideoFile, ShowEvidenceBoard);
        }

        private void OnGutCheckButton()
        {
            if (!_session.GutCheckCompleted)
                _session.CompleteGutCheck();
            ShowBadgeAchieved(caseData.GutCheckBadgeVideoFile, ShowEvidenceBoard);
        }

        private void ShowSpotTheClue()
        {
            ShowPanel(spotTheCluePanel);
            SetSpotContentVisible(false);

            var url = BuildVideoUrl(caseData.SpotTheClueVideoFile);
            bool hasVideo = url != null || (!IsWebGL && caseData.SpotTheClueVideo != null);

            if (spotVideoPlayer != null && hasVideo)
            {
                if (spotVideoDisplay != null) spotVideoDisplay.color = Color.clear;
                spotVideoPlayer.errorReceived    -= OnSpotVideoError;
                spotVideoPlayer.loopPointReached -= OnSpotVideoFinished;
                spotVideoPlayer.prepareCompleted -= OnSpotVideoPrepared;
                spotVideoPlayer.isLooping        = false;
                spotVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
                spotVideoPlayer.errorReceived    += OnSpotVideoError;
                spotVideoPlayer.loopPointReached += OnSpotVideoFinished;
                spotVideoPlayer.prepareCompleted += OnSpotVideoPrepared;

                if (url != null) { spotVideoPlayer.source = VideoSource.Url; spotVideoPlayer.url = url; }
                else             { spotVideoPlayer.source = VideoSource.VideoClip; spotVideoPlayer.clip = caseData.SpotTheClueVideo; }
                if (IsNarration(caseData.SpotTheClueVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                    StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
                spotVideoPlayer.Prepare();
            }
            else
            {
                ShowSpotContent();
            }
        }

        private void SetSpotContentVisible(bool visible)
        {
            if (spotPromptText)   spotPromptText.gameObject.SetActive(visible);
            if (spotEvidenceImage) spotEvidenceImage.gameObject.SetActive(visible);
            if (spotFeedbackText) spotFeedbackText.gameObject.SetActive(visible);
            if (spotBackButton)   spotBackButton.gameObject.SetActive(visible);
            if (spotVideoDisplay) { spotVideoDisplay.color = visible ? Color.clear : Color.white; spotVideoDisplay.raycastTarget = !visible; }
            foreach (var b in spotOptionButtons) if (b) b.gameObject.SetActive(visible);
        }

        private void ShowSpotContent()
        {
            var step = caseData.SpotTheClue;
            if (spotPromptText) spotPromptText.text = step.Prompt;
            if (spotEvidenceImage && step.EvidenceImage) spotEvidenceImage.sprite = step.EvidenceImage;
            if (spotFeedbackText) spotFeedbackText.text = "";
            var done = _session.SpotTheClueCompleted;
            for (int i = 0; i < spotOptionButtons.Length; i++)
            {
                var btn = spotOptionButtons[i];
                if (btn == null) continue;
                bool hasOption = i < step.Options.Length;
                btn.gameObject.SetActive(hasOption);
                btn.interactable = !done;
                if (hasOption) { var label = btn.GetComponentInChildren<TMP_Text>(); if (label) label.text = step.Options[i]; }
            }
            SetSpotContentVisible(true);
        }

        private void OnSpotVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnSpotVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (spotVideoDisplay != null)
            {
                spotVideoDisplay.texture = rt;
                spotVideoDisplay.color = Color.white;
            }
            vp.Play();
        }

        private void OnSpotVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnSpotVideoFinished;
            if (spotVideoDisplay != null) spotVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowSpotContent();
        }

        private void OnSpotVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Spot video error: {msg}");
            vp.errorReceived -= OnSpotVideoError;
            if (spotVideoDisplay != null) spotVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowSpotContent();
        }

        private void ShowImagePopup(Sprite sprite, GameObject returnPanel)
        {
            _imagePopupReturnPanel = returnPanel;
            if (imagePopupImage != null && sprite != null) imagePopupImage.sprite = sprite;
            if (imagePopupPanel != null) imagePopupPanel.SetActive(true);
        }

        private void HideImagePopup()
        {
            if (imagePopupPanel != null) imagePopupPanel.SetActive(false);
        }

        // ── Badge Achieved ─────────────────────────────────────────────────────

        private void ShowBadgeAchieved(string videoFile, System.Action onComplete, System.Action onBack = null)
        {
            _badgeReturnAction = onComplete;
            _badgeBackAction   = onBack ?? ShowBadge1Panel;
            ShowPanel(badgeAchievedPanel);

            if (badgeAchievedVideoDisplay != null)
            {
                badgeAchievedVideoDisplay.gameObject.SetActive(true);
                badgeAchievedVideoDisplay.color        = Color.clear;
                badgeAchievedVideoDisplay.raycastTarget = false;
            }

            if (badgeAchievedVideoPlayer == null || string.IsNullOrEmpty(videoFile))
            {
                OnBadgeVideoFinished(null); return;
            }

            var url = BuildVideoUrl(videoFile);
            if (url == null) { OnBadgeVideoFinished(null); return; }

            badgeAchievedVideoPlayer.errorReceived    -= OnBadgeVideoError;
            badgeAchievedVideoPlayer.loopPointReached -= OnBadgeVideoFinished;
            badgeAchievedVideoPlayer.prepareCompleted -= OnBadgeVideoPrepared;
            badgeAchievedVideoPlayer.isLooping        = false;
            badgeAchievedVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            badgeAchievedVideoPlayer.errorReceived    += OnBadgeVideoError;
            badgeAchievedVideoPlayer.loopPointReached += OnBadgeVideoFinished;
            badgeAchievedVideoPlayer.prepareCompleted += OnBadgeVideoPrepared;
            badgeAchievedVideoPlayer.source        = VideoSource.Url;
            badgeAchievedVideoPlayer.url           = url;
            badgeAchievedVideoPlayer.playbackSpeed = GetPlaybackSpeed(videoFile);

            if (IsNarration(videoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            badgeAchievedVideoPlayer.Prepare();
        }

        private void SkipBadgeVideo()
        {
            if (badgeAchievedVideoPlayer != null)
            {
                badgeAchievedVideoPlayer.loopPointReached -= OnBadgeVideoFinished;
                badgeAchievedVideoPlayer.Stop();
            }
            if (badgeAchievedVideoDisplay != null) badgeAchievedVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            var action = _badgeReturnAction;
            _badgeReturnAction = null;
            _badgeBackAction   = null;
            action?.Invoke();
        }

        private void OnBadgeAchievedBack()
        {
            if (badgeAchievedVideoPlayer != null)
            {
                badgeAchievedVideoPlayer.loopPointReached -= OnBadgeVideoFinished;
                badgeAchievedVideoPlayer.Stop();
            }
            if (badgeAchievedVideoDisplay != null) badgeAchievedVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            var action = _badgeBackAction;
            _badgeReturnAction = null;
            _badgeBackAction   = null;
            action?.Invoke();
        }

        private void OnBadgeVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnBadgeVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (badgeAchievedVideoDisplay != null)
            {
                badgeAchievedVideoDisplay.texture = rt;
                badgeAchievedVideoDisplay.color   = Color.white;
            }
            vp.Play();
        }

        private void OnBadgeVideoFinished(VideoPlayer vp)
        {
            if (vp != null) vp.loopPointReached -= OnBadgeVideoFinished;
            if (badgeAchievedVideoDisplay != null) badgeAchievedVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            var action = _badgeReturnAction;
            _badgeReturnAction = null;
            action?.Invoke();
        }

        private void OnBadgeVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Badge video error: {msg}");
            vp.errorReceived -= OnBadgeVideoError;
            if (badgeAchievedVideoDisplay != null) badgeAchievedVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            var action = _badgeReturnAction;
            _badgeReturnAction = null;
            action?.Invoke();
        }

        // ── Dispositional Award ───────────────────────────────────────────────

        private bool _dispositionalAwardButtonsWired;
        private bool _dispositionalAwardContentShown;
        private System.Action _dispositionalAwardReturnAction;

        public void PlayDispositionalAwardVideo(System.Action onComplete = null, System.Action onBack = null)
        {
            _dispositionalAwardReturnAction = onComplete;

            // Runtime fallback
            if (dispositionalAwardPanel == null)
            {
                var canvas = GameObject.Find("GameCanvas");
                if (canvas != null)
                {
                    var t = canvas.transform.Find("DispositionalAwardPanel");
                    if (t != null) dispositionalAwardPanel = t.gameObject;
                }
            }
            if (dispositionalAwardVideoPlayer == null && dispositionalAwardPanel != null)
                dispositionalAwardVideoPlayer = dispositionalAwardPanel.GetComponent<VideoPlayer>();
            if (dispositionalAwardSkipButton == null && dispositionalAwardPanel != null)
                dispositionalAwardSkipButton = dispositionalAwardPanel.transform.Find("DispositionalAwardSkipButton")?.GetComponent<Button>();
            if (dispositionalAwardVideoDisplay == null && dispositionalAwardPanel != null)
                dispositionalAwardVideoDisplay = dispositionalAwardPanel.transform.Find("DispositionalAwardVideoDisplay")?.GetComponent<RawImage>();

            if (!_dispositionalAwardButtonsWired)
            {
                if (dispositionalAwardSkipButton != null)
                {
                    dispositionalAwardSkipButton.onClick.AddListener(SkipDispositionalAwardVideo);
                    AddClick(dispositionalAwardSkipButton);
                }
                _dispositionalAwardButtonsWired = true;
            }

            var url = BuildVideoUrl(caseData?.DispositionalAwardVideoFile);
            if (dispositionalAwardPanel == null || dispositionalAwardVideoPlayer == null || url == null)
            {
                Debug.LogWarning($"[CaseRunner] DispositionalAward fallback — panel={dispositionalAwardPanel != null} player={dispositionalAwardVideoPlayer != null} url={url}");
                _dispositionalAwardReturnAction?.Invoke();
                _dispositionalAwardReturnAction = null;
                return;
            }

            ShowPanel(dispositionalAwardPanel);

            if (dispositionalAwardSkipButton != null) dispositionalAwardSkipButton.gameObject.SetActive(true);

            if (dispositionalAwardVideoDisplay != null)
            {
                dispositionalAwardVideoDisplay.gameObject.SetActive(true);
                dispositionalAwardVideoDisplay.color        = Color.clear;
                dispositionalAwardVideoDisplay.raycastTarget = false;
            }

            dispositionalAwardVideoPlayer.errorReceived    -= OnDispositionalAwardVideoError;
            dispositionalAwardVideoPlayer.loopPointReached -= OnDispositionalAwardVideoFinished;
            dispositionalAwardVideoPlayer.prepareCompleted -= OnDispositionalAwardVideoPrepared;
            dispositionalAwardVideoPlayer.isLooping        = false;
            dispositionalAwardVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            dispositionalAwardVideoPlayer.errorReceived    += OnDispositionalAwardVideoError;
            dispositionalAwardVideoPlayer.loopPointReached += OnDispositionalAwardVideoFinished;
            dispositionalAwardVideoPlayer.prepareCompleted += OnDispositionalAwardVideoPrepared;
            dispositionalAwardVideoPlayer.source = VideoSource.Url;
            dispositionalAwardVideoPlayer.url    = url;

            _dispositionalAwardContentShown = false;
            dispositionalAwardVideoPlayer.Prepare();
        }

        private void StopDispositionalAwardVideo()
        {
            if (dispositionalAwardVideoPlayer != null)
            {
                dispositionalAwardVideoPlayer.loopPointReached -= OnDispositionalAwardVideoFinished;
                dispositionalAwardVideoPlayer.Stop();
            }
            if (dispositionalAwardVideoDisplay != null) dispositionalAwardVideoDisplay.color = Color.clear;
        }

        private void SkipDispositionalAwardVideo()
        {
            StopDispositionalAwardVideo();
            ShowDispositionalAwardContent();
        }

        private void ShowDispositionalAwardContent()
        {
            if (_dispositionalAwardContentShown) return;
            _dispositionalAwardContentShown = true;
            if (dispositionalAwardVideoDisplay != null) dispositionalAwardVideoDisplay.color = Color.clear;
            if (dispositionalAwardSkipButton != null) dispositionalAwardSkipButton.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            var action = _dispositionalAwardReturnAction;
            _dispositionalAwardReturnAction = null;
            action?.Invoke();
        }

        private void OnDispositionalAwardVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnDispositionalAwardVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (dispositionalAwardVideoDisplay != null)
            {
                dispositionalAwardVideoDisplay.texture = rt;
                dispositionalAwardVideoDisplay.color   = Color.white;
            }
            vp.Play();
        }

        private void OnDispositionalAwardVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnDispositionalAwardVideoFinished;
            ShowDispositionalAwardContent();
        }

        private void OnDispositionalAwardVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Dispositional award video error: {msg}");
            vp.errorReceived -= OnDispositionalAwardVideoError;
            ShowDispositionalAwardContent();
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

            if (hintOverlay == null && hintsFromChiefPanel != null)
            {
                var t = hintsFromChiefPanel.transform.Find("HintOverlay");
                if (t != null) hintOverlay = t.gameObject;
            }

            StartMainMusic();
            ShowPanel(hintsFromChiefPanel);

            // Both videos start simultaneously.
            // Overlay video controls when the Return button appears.
            SetHintContentVisible(false);
            PlayHintPanelVideo();
            PlayHintsOverlayVideo();
        }

        // ── Panel video (HintFromChief.mp4) ──────────────────────────────────

        private void PlayHintPanelVideo()
        {
            if (hintVideoDisplay != null) { hintVideoDisplay.color = Color.clear; hintVideoDisplay.texture = null; }
            if (hintVideoPlayer == null) return;

            var url = BuildVideoUrl(caseData.HintVideoFile);
            bool hasSource = url != null || (!IsWebGL && caseData.HintVideo != null);
            if (!hasSource) return;

            hintVideoPlayer.isLooping        = false;
            hintVideoPlayer.errorReceived    += OnHintPanelVideoError;
            hintVideoPlayer.prepareCompleted += OnHintPanelVideoPrepared;

            if (url != null) { hintVideoPlayer.source = VideoSource.Url; hintVideoPlayer.url = url; }
            else             { hintVideoPlayer.source = VideoSource.VideoClip; hintVideoPlayer.clip = caseData.HintVideo; }
            hintVideoPlayer.Prepare();
        }

        private void OnHintPanelVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnHintPanelVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (hintVideoDisplay != null) { hintVideoDisplay.texture = rt; hintVideoDisplay.color = Color.white; }
            vp.Play();
        }

        private void OnHintPanelVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Hint panel video error: {msg}");
            vp.errorReceived -= OnHintPanelVideoError;
        }

        // ── Overlay video (HintsVideo.mp4) ───────────────────────────────────

        private void PlayHintsOverlayVideo()
        {
            if (hintsOverlayVideoDisplay != null) { hintsOverlayVideoDisplay.color = Color.clear; hintsOverlayVideoDisplay.texture = null; }

            if (hintsOverlayVideoPlayer == null) { SetHintContentVisible(true); return; }

            var url = BuildVideoUrl(caseData.HintsOverlayVideoFile);
            if (url == null) { SetHintContentVisible(true); return; }

            hintsOverlayVideoPlayer.isLooping        = false;
            hintsOverlayVideoPlayer.errorReceived    += OnHintsOverlayVideoError;
            hintsOverlayVideoPlayer.loopPointReached += OnHintsOverlayVideoFinished;
            hintsOverlayVideoPlayer.prepareCompleted += OnHintsOverlayVideoPrepared;
            hintsOverlayVideoPlayer.source        = VideoSource.Url;
            hintsOverlayVideoPlayer.url           = url;
            hintsOverlayVideoPlayer.playbackSpeed = GetPlaybackSpeed(caseData.HintsOverlayVideoFile);
            hintsOverlayVideoPlayer.Prepare();
        }

        private void OnHintsOverlayVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnHintsOverlayVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (hintsOverlayVideoDisplay != null) { hintsOverlayVideoDisplay.texture = rt; hintsOverlayVideoDisplay.color = Color.white; }
            vp.Play();
        }

        private void OnHintsOverlayVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnHintsOverlayVideoFinished;
            SetHintContentVisible(true);
        }

        private void OnHintsOverlayVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Hints overlay video error: {msg}");
            vp.errorReceived -= OnHintsOverlayVideoError;
            SetHintContentVisible(true);
        }

        // ── Overlay content visibility ────────────────────────────────────────

        // visible=false: show overlay video display, hide buttons/text
        // visible=true:  hide overlay video display, show buttons/text
        private void SetHintContentVisible(bool visible)
        {
            if (hintOverlay != null)
            {
                if (!hintOverlay.activeSelf) hintOverlay.SetActive(true);
                for (int i = 0; i < hintOverlay.transform.childCount; i++)
                {
                    var child = hintOverlay.transform.GetChild(i);
                    if (child.name == "HintVideoDisplay")
                        child.gameObject.SetActive(!visible);
                    else if (child.name == "HintsVideo")
                        child.gameObject.SetActive(true); // VideoPlayer GO must never be disabled
                    else
                        child.gameObject.SetActive(visible);
                }
            }
            else
            {
                if (hintBodyText)       hintBodyText.gameObject.SetActive(visible);
                if (hintTryAgainButton) hintTryAgainButton.gameObject.SetActive(visible);
            }
        }

        // Returns a StreamingAssets URL for the given filename, or null if not provided.
        // On WebGL, Application.streamingAssetsPath is already an http:// URL.
        private static float GetPlaybackSpeed(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return 1f;
            var f = filename.Trim();
            if (f.Equals("VaultEntry.mp4",                     System.StringComparison.OrdinalIgnoreCase)) return 3f;
            if (f.Equals("HintsVideo.mp4",                     System.StringComparison.OrdinalIgnoreCase)) return 3f;
            if (f.Equals("BadgeCodeReveal-Dispositional.mp4",  System.StringComparison.OrdinalIgnoreCase)) return 3f;
            if (f.Equals("BadgeCodeReveal-Pragmatic.mp4",      System.StringComparison.OrdinalIgnoreCase)) return 3f;
            return 1f;
        }

        private static string BuildVideoUrl(string filename)
        {
            if (string.IsNullOrEmpty(filename)) return null;
            return System.IO.Path.Combine(
                Application.streamingAssetsPath, "Video", filename).Replace("\\", "/");
        }

        /// <summary>
        /// Returns or creates a full-screen RawImage child named <paramref name="name"/> on <paramref name="panel"/>.
        /// Used as a fallback when the Inspector reference is missing from the scene.
        /// </summary>
        private static RawImage EnsureVideoDisplay(GameObject panel, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(panel.transform, false);
            go.transform.SetAsFirstSibling();
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;
            var raw = go.AddComponent<RawImage>();
            raw.color          = Color.clear;
            raw.raycastTarget  = false;
            return raw;
        }

        // On WebGL VideoClip assets cannot be used — URL is required.
        private static bool IsWebGL =>
            Application.platform == RuntimePlatform.WebGLPlayer;

        // Returns true only if the video filename is listed in NarrationVideoFiles.
        // Music is ducked only for narration; SFX-only or silent videos leave music untouched.
        private bool IsNarration(string filename) =>
            !string.IsNullOrEmpty(filename) &&
            caseData?.NarrationVideoFiles != null &&
            System.Array.IndexOf(caseData.NarrationVideoFiles, filename) >= 0;

        private void ShowLevelComplete()
        {
            _session.CompleteCase();
            if (completionBodyText) completionBodyText.text = caseData.CompletionText;
            if (completionImage && caseData.CompletionImage)
                completionImage.sprite = caseData.CompletionImage;
            EnsureCoverFit(completionImage);
            if (xpText)
                xpText.text = $"You earned {caseData.XpForCompletion} XP!  •  Total XP: {ProgressStore.GetXp()}";

            if (mainAudioSource != null)
            {
                if (completionMusic != null)
                    StartCoroutine(CrossfadeMusic(mainAudioSource, completionMusic, 1.2f));
                else
                    StartCoroutine(FadeOutAudio(mainAudioSource, 1.2f));
            }

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
            if (correct)
            {
                _session.CompleteSpotTheClue();
                LockButtons(spotOptionButtons);
                var feedback = step.FeedbackCorrect;
                ShowBadgeAchieved(caseData.SpotTheClueBadgeVideoFile, () =>
                {
                    ShowPanel(spotTheCluePanel);
                    PopupController.Show("Clue Found!", feedback, "Got it");
                });
            }
            else
            {
                PopupController.Show("Look Again...", step.FeedbackIncorrect, "Got it", isCorrect: false);
            }
        }

        private void OnGutCheckAnswer(int selectedIndex)
        {
            if (_session.GutCheckCompleted) return;
            var result = _session.AnswerGutCheck(selectedIndex);
            if (result.isCorrect)
            {
                LockButtons(gutOptionButtons);
                var feedback = result.feedback;
                ShowBadgeAchieved(caseData.GutCheckBadgeVideoFile, () =>
                {
                    ShowPanel(gutCheckPanel);
                    PopupController.Show("Good Instinct!", feedback, "Got it");
                });
            }
            else
            {
                PopupController.Show("Think Again...", result.feedback, "Got it", isCorrect: false);
            }
        }

        private void OnMotiveAnswer(int selectedIndex)
        {
            if (_session.FindTheMotiveCompleted) return;
            var step = caseData.FindTheMotive;
            bool correct = selectedIndex == step.CorrectIndex;
            if (correct)
            {
                _session.CompleteMotive();
                LockButtons(motiveOptionButtons);
                var feedback = step.FeedbackCorrect;
                ShowBadgeAchieved(caseData.FindTheMotiveBadgeVideoFile, () =>
                {
                    ShowPanel(findTheMotivePanel);
                    PopupController.Show("Motive Identified!", feedback, "Got it");
                });
            }
            else
            {
                PopupController.Show("Look Deeper...", step.FeedbackIncorrect, "Got it", isCorrect: false);
            }
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

        private void WireOnce(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null) return;
            btn.onClick.RemoveListener(action);
            btn.onClick.AddListener(action);
        }

        private void SetButtonLabelVisible(Button btn, bool visible)
        {
            if (btn == null) return;
            var labelT = btn.transform.Find("Label");
            if (labelT != null) labelT.gameObject.SetActive(visible);
        }

        private void AddHoverText(Button btn, string message)
            => AddHoverText(btn, message, badge1IncomingMessageText);

        private void AddHoverText(Button btn, string message, TMP_Text target)
        {
            if (btn == null || target == null) return;
            var trigger = btn.gameObject.GetComponent<EventTrigger>()
                          ?? btn.gameObject.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => target.text = message);
            trigger.triggers.Add(enter);

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => target.text = "");
            trigger.triggers.Add(exit);
        }

        private static void LockButtons(Button[] buttons)
        {
            foreach (var btn in buttons) if (btn) btn.interactable = false;
        }

        private void OnPasswordSubmit()
        {
            var input = passwordInputField != null ? passwordInputField.text : "";
            if (_session.ValidatePassword(input))
                PlayVaultEntryVideo();
            else
                PopupController.Show("Wrong Code",
                    "That code is incorrect.\nReview your evidence clues and try again.",
                    "Try Again");
        }

        // ── Vault Entry Video ─────────────────────────────────────────────────

        private void PlayVaultEntryVideo()
        {
            var url = BuildVideoUrl(caseData.VaultEntryVideoFile);
            if (vaultEntryPanel == null || vaultEntryVideoPlayer == null || url == null)
            {
                PlayUnlockVideo(); return; // fallback to old unlock panel
            }

            if (vaultEntryVideoDisplay == null)
                vaultEntryVideoDisplay = vaultEntryPanel.transform.Find("VaultEntryVideoDisplay")?.GetComponent<RawImage>();
            if (vaultEntryVideoDisplay == null)
                vaultEntryVideoDisplay = EnsureVideoDisplay(vaultEntryPanel, "VaultEntryVideoDisplay");

            ShowPanel(vaultEntryPanel);

            if (vaultEntryVideoDisplay != null)
            {
                vaultEntryVideoDisplay.gameObject.SetActive(true);
                vaultEntryVideoDisplay.color        = Color.clear;
                vaultEntryVideoDisplay.raycastTarget = false;
            }
            if (vaultEntrySkipButton != null)
            {
                vaultEntrySkipButton.gameObject.SetActive(false);
                StartCoroutine(ShowAfterDelay(vaultEntrySkipButton.gameObject, 1f));
            }

            vaultEntryVideoPlayer.errorReceived    -= OnVaultEntryVideoError;
            vaultEntryVideoPlayer.loopPointReached -= OnVaultEntryVideoFinished;
            vaultEntryVideoPlayer.prepareCompleted -= OnVaultEntryVideoPrepared;
            vaultEntryVideoPlayer.isLooping        = false;
            vaultEntryVideoPlayer.audioOutputMode  = VideoAudioOutputMode.Direct;
            vaultEntryVideoPlayer.errorReceived    += OnVaultEntryVideoError;
            vaultEntryVideoPlayer.loopPointReached += OnVaultEntryVideoFinished;
            vaultEntryVideoPlayer.prepareCompleted += OnVaultEntryVideoPrepared;
            vaultEntryVideoPlayer.source        = VideoSource.Url;
            vaultEntryVideoPlayer.url           = url;
            vaultEntryVideoPlayer.playbackSpeed = GetPlaybackSpeed(caseData.VaultEntryVideoFile);

            if (IsNarration(caseData.VaultEntryVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            vaultEntryVideoPlayer.Prepare();
        }

        private void SkipVaultEntryVideo()
        {
            if (vaultEntryVideoPlayer != null)
            {
                vaultEntryVideoPlayer.loopPointReached -= OnVaultEntryVideoFinished;
                vaultEntryVideoPlayer.Stop();
            }
            if (vaultEntryVideoDisplay != null) vaultEntryVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowEvidenceDetail();
        }

        private void OnVaultEntryVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnVaultEntryVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (vaultEntryVideoDisplay != null) { vaultEntryVideoDisplay.texture = rt; vaultEntryVideoDisplay.color = Color.white; }
            vp.Play();
        }

        private void OnVaultEntryVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnVaultEntryVideoFinished;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowEvidenceDetail();
        }

        private void OnVaultEntryVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] VaultEntry video error: {msg}");
            vp.errorReceived -= OnVaultEntryVideoError;
            ShowEvidenceDetail();
        }

        private void SkipUnlockVideo()
        {
            if (unlockVideoPlayer != null)
            {
                unlockVideoPlayer.loopPointReached -= OnUnlockVideoFinished;
                unlockVideoPlayer.Stop();
            }
            if (unlockVideoDisplay != null) unlockVideoDisplay.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowEvidenceDetail();
        }

        private void PlayUnlockVideo()
        {
            if (unlockVideoPlayer == null) { ShowEvidenceDetail(); return; }

            var url = BuildVideoUrl(caseData.UnlockVideoFile);
            bool hasSource = url != null || (!IsWebGL && caseData.UnlockVideo != null);
            if (!hasSource) { ShowEvidenceDetail(); return; }

            ShowPanel(unlockPanel);

            if (unlockVideoDisplay != null)
            {
                unlockVideoDisplay.gameObject.SetActive(true);
                unlockVideoDisplay.color        = Color.clear;
                unlockVideoDisplay.raycastTarget = false;
            }

            unlockVideoPlayer.errorReceived    -= OnUnlockVideoError;
            unlockVideoPlayer.loopPointReached -= OnUnlockVideoFinished;
            unlockVideoPlayer.prepareCompleted -= OnUnlockVideoPrepared;

            unlockVideoPlayer.isLooping       = false;
            unlockVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            unlockVideoPlayer.errorReceived    += OnUnlockVideoError;
            unlockVideoPlayer.loopPointReached += OnUnlockVideoFinished;
            unlockVideoPlayer.prepareCompleted += OnUnlockVideoPrepared;

            if (url != null) { unlockVideoPlayer.source = VideoSource.Url; unlockVideoPlayer.url = url; }
            else             { unlockVideoPlayer.source = VideoSource.VideoClip; unlockVideoPlayer.clip = caseData.UnlockVideo; }

            if (IsNarration(caseData.UnlockVideoFile) && AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
            unlockVideoPlayer.Prepare();
        }

        private void OnUnlockVideoPrepared(VideoPlayer vp)
        {
            vp.prepareCompleted -= OnUnlockVideoPrepared;
            var rt = new RenderTexture((int)vp.width, (int)vp.height, 0, RenderTextureFormat.ARGB32);
            rt.Create();
            vp.targetTexture = rt;
            if (unlockVideoDisplay != null)
            {
                unlockVideoDisplay.texture = rt;
                unlockVideoDisplay.color = Color.white; unlockVideoDisplay.raycastTarget = false;
            }
            vp.Play();
        }

        private void OnUnlockVideoFinished(VideoPlayer vp)
        {
            vp.loopPointReached -= OnUnlockVideoFinished;
            if (unlockVideoDisplay != null) unlockVideoDisplay.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowEvidenceDetail();
        }

        private void OnUnlockVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Unlock video error: {msg}");
            vp.errorReceived -= OnUnlockVideoError;
            if (unlockVideoDisplay != null) unlockVideoDisplay.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowEvidenceDetail();
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
