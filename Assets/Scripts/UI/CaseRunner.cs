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

        // ── Tool Sub-Panels ───────────────────────────────────────────────────
        [Header("Tool Sub-Panels")]
        public GameObject accountProfilePanel;
        public Button     accountProfileBackButton;
        public GameObject commentsSectionPanel;
        public Button     commentsSectionBackButton;
        public GameObject metaDataPanel;
        public Button     metaDataBackButton;

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
        public Button      evidenceBoardBackButton;  // returns to Mission Briefing

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
        private bool        _boardIntroPlayed;
        private GameObject  _imagePopupReturnPanel;

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

            // Mission Briefing
            if (briefingStartButton)  briefingStartButton.onClick.AddListener(OnMissionStartPressed);
            if (briefingSkipButton)   briefingSkipButton.onClick.AddListener(SkipBriefingVideo);
            if (briefingRepeatButton) briefingRepeatButton.onClick.AddListener(RepeatBriefingVideo);

            // Mission Start
            if (missionStartSkipButton) missionStartSkipButton.onClick.AddListener(SkipMissionStartVideo);

            // Evidence Board
            if (evidenceBoardBackButton) evidenceBoardBackButton.onClick.AddListener(OnMissionStartPressed);
            if (spotTheClueButton)   spotTheClueButton.onClick.AddListener(ShowSpotTheClue);
            if (gutCheckButton)      gutCheckButton.onClick.AddListener(ShowGutCheck);
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
            if (accountProfileBackButton)  accountProfileBackButton.onClick.AddListener(ShowSpotTheClue);
            if (commentsSectionBackButton) commentsSectionBackButton.onClick.AddListener(ShowGutCheck);
            if (metaDataBackButton)        metaDataBackButton.onClick.AddListener(ShowFindTheMotive);

            // Image Popup
            if (imagePopupBackButton) imagePopupBackButton.onClick.AddListener(HideImagePopup);

            // Password Lock
            if (passwordSubmitButton) passwordSubmitButton.onClick.AddListener(OnPasswordSubmit);
            if (passwordBackButton)   passwordBackButton.onClick.AddListener(ShowEvidenceBoard);
            if (unlockSkipButton)     unlockSkipButton.onClick.AddListener(SkipUnlockVideo);
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
            AddClick(spotBackButton); AddClick(spotEvidenceButton); AddClick(spotAccountProfileButton);
            AddClick(gutBackButton); AddClick(gutEvidenceButton); AddClick(gutCommentsSectionButton);
            AddClick(motiveBackButton); AddClick(motiveEvidenceButton); AddClick(motiveMetaDataButton);
            AddClick(imagePopupBackButton);
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
                spotTheCluePanel, gutCheckPanel, findTheMotivePanel,
                passwordLockPanel, unlockPanel, evidenceDetailPanel,
                hintsFromChiefPanel, levelCompletePanel,
                accountProfilePanel, commentsSectionPanel, metaDataPanel
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
            if (introAudioSource != null) introAudioSource.Stop();

            ShowPanel(missionBriefingPanel);
            SetBriefingContentVisible(false);
            PlayBriefingVideo();
        }

        private void SetBriefingContentVisible(bool visible)
        {
            if (briefingTitleText)   briefingTitleText.gameObject.SetActive(visible);
            if (briefingBodyText)    briefingBodyText.gameObject.SetActive(visible);
            if (briefingImage)       briefingImage.gameObject.SetActive(visible);
            if (briefingStartButton) briefingStartButton.gameObject.SetActive(visible);
            if (briefingRepeatButton) briefingRepeatButton.gameObject.SetActive(visible);
            // Video overlay visible when content is hidden
            if (briefingVideoDisplay) { briefingVideoDisplay.color = visible ? Color.clear : Color.white; briefingVideoDisplay.raycastTarget = !visible; }
            if (briefingSkipButton)   briefingSkipButton.gameObject.SetActive(!visible);
        }

        private void SkipBriefingVideo()
        {
            if (briefingVideoPlayer != null)
            {
                briefingVideoPlayer.loopPointReached -= OnBriefingVideoFinished;
                briefingVideoPlayer.Stop();
            }
            ShowBriefingContent();
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

            // Remove stale listeners before re-adding (safe if not subscribed)
            briefingVideoPlayer.errorReceived    -= OnBriefingVideoError;
            briefingVideoPlayer.loopPointReached -= OnBriefingVideoFinished;
            briefingVideoPlayer.prepareCompleted -= OnBriefingVideoPrepared;

            briefingVideoPlayer.isLooping       = false;
            briefingVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct; // play video audio
            briefingVideoPlayer.errorReceived    += OnBriefingVideoError;
            briefingVideoPlayer.loopPointReached += OnBriefingVideoFinished;
            briefingVideoPlayer.prepareCompleted += OnBriefingVideoPrepared;

            if (url != null) { briefingVideoPlayer.source = VideoSource.Url; briefingVideoPlayer.url = url; }
            else             { briefingVideoPlayer.source = VideoSource.VideoClip; briefingVideoPlayer.clip = caseData.BriefingVideo; }

            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(DuckAudio(mainAudioSource, 0.15f, 0.5f));
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
            if (briefingVideoDisplay != null) briefingVideoDisplay.color = Color.clear;
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            ShowBriefingContent();
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
            ShowEvidenceBoard();
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

            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
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
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            _boardIntroPlayed = true;
            ShowEvidenceBoard();
        }

        private void OnMissionStartVideoError(VideoPlayer vp, string msg)
        {
            Debug.LogError($"[CaseRunner] Mission start video error: {msg}");
            vp.errorReceived -= OnMissionStartVideoError;
            if (missionStartVideoDisplay != null) missionStartVideoDisplay.gameObject.SetActive(false);
            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(UnduckAudio(mainAudioSource, 1f, 0.5f));
            _boardIntroPlayed = true;
            ShowEvidenceBoard();
        }

        public void ShowEvidenceBoard()
        {
            if (introAudioSource != null) introAudioSource.Stop();

            if (mainAudioSource != null && caseData.MainMusic != null && !mainAudioSource.isPlaying)
            {
                mainAudioSource.clip   = caseData.MainMusic;
                mainAudioSource.loop   = true;
                mainAudioSource.volume = _boardIntroPlayed ? 1f : 0f; // silent until video finishes on first visit
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
                if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
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

            if (mainAudioSource != null && mainAudioSource.isPlaying)
                StartCoroutine(FadeOutAudio(mainAudioSource, 1.2f));

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
            bool hasSource = url != null || (!IsWebGL && caseData.HintVideo != null);
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

        // On WebGL VideoClip assets cannot be used — URL is required.
        private static bool IsWebGL =>
            Application.platform == RuntimePlatform.WebGLPlayer;

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
                PlayUnlockVideo();
            else
                PopupController.Show("Wrong Code",
                    "That code is incorrect.\nReview your evidence clues and try again.",
                    "Try Again");
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

            if (AudioListener.volume > 0f && mainAudioSource != null && mainAudioSource.isPlaying)
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
