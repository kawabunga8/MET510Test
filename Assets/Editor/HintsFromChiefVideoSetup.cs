using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Setup Hints Video Display
    ///
    /// Sets up HintsFromChiefPanel for two-stage video playback:
    ///
    ///   Stage 1 — Panel video (HintFromChief.mp4):
    ///     HintVideoDisplay (RawImage, fullscreen, direct child of panel)
    ///     VideoPlayer on HintsFromChiefPanel → hintVideoPlayer
    ///
    ///   Stage 2 — Overlay video (HintsVideo.mp4):
    ///     HintVideoDisplay (RawImage, fullscreen, first child of HintOverlay)
    ///     HintsVideo (VideoPlayer, child of HintOverlay) → hintsOverlayVideoPlayer
    ///
    /// Also rebuilds HintReturnButton inside HintOverlay.
    /// Sets HintOverlay Image to transparent.
    /// Safe to re-run.
    /// </summary>
    public static class HintsFromChiefVideoSetup
    {
        [MenuItem("ETEC510/Setup Hints Video Display")]
        public static void Setup()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var hintsPanelT = canvasGO.transform.Find("HintsFromChiefPanel");
            if (hintsPanelT == null) { Debug.LogError("ETEC510: HintsFromChiefPanel not found."); return; }

            var overlayT = hintsPanelT.Find("HintOverlay");
            if (overlayT == null) { Debug.LogError("ETEC510: HintOverlay not found."); return; }

            // ── Stage 1: Panel-level video (HintFromChief.mp4) ───────────────

            // VideoPlayer on the panel itself
            var panelVP = hintsPanelT.GetComponent<VideoPlayer>();
            if (panelVP == null) panelVP = hintsPanelT.gameObject.AddComponent<VideoPlayer>();
            panelVP.renderMode      = VideoRenderMode.RenderTexture;
            panelVP.playOnAwake     = false;
            panelVP.isLooping       = false;
            panelVP.audioOutputMode = VideoAudioOutputMode.Direct;

            // Panel-level RawImage (fullscreen, below HintOverlay in draw order)
            var existingPanelDisplay = hintsPanelT.Find("HintVideoDisplay");
            if (existingPanelDisplay != null) { Object.DestroyImmediate(existingPanelDisplay.gameObject); }

            var panelDisplayGO = new GameObject("HintVideoDisplay", typeof(RectTransform));
            panelDisplayGO.transform.SetParent(hintsPanelT, false);
            panelDisplayGO.transform.SetSiblingIndex(overlayT.GetSiblingIndex()); // just before overlay
            var panelDisplayRT = (RectTransform)panelDisplayGO.transform;
            panelDisplayRT.anchorMin = Vector2.zero;
            panelDisplayRT.anchorMax = Vector2.one;
            panelDisplayRT.offsetMin = Vector2.zero;
            panelDisplayRT.offsetMax = Vector2.zero;
            var panelRawImg = panelDisplayGO.AddComponent<RawImage>();
            panelRawImg.color         = Color.clear;
            panelRawImg.raycastTarget = false;

            // ── Stage 2: Overlay video (HintsVideo.mp4) ──────────────────────

            // Make HintOverlay Image transparent
            var overlayImg = overlayT.GetComponent<Image>();
            if (overlayImg != null)
            {
                var c = overlayImg.color;
                overlayImg.color = new Color(c.r, c.g, c.b, 0f);
                EditorUtility.SetDirty(overlayImg);
                Debug.Log("  HintOverlay Image set to transparent.");
            }

            // Ensure HintsVideo VideoPlayer exists inside HintOverlay
            var hintsVideoT = overlayT.Find("HintsVideo");
            VideoPlayer overlayVP;
            if (hintsVideoT == null)
            {
                var go = new GameObject("HintsVideo");
                go.transform.SetParent(overlayT, false);
                overlayVP = go.AddComponent<VideoPlayer>();
                Debug.Log("  Created HintsVideo GO with VideoPlayer.");
            }
            else
            {
                overlayVP = hintsVideoT.GetComponent<VideoPlayer>();
                if (overlayVP == null) overlayVP = hintsVideoT.gameObject.AddComponent<VideoPlayer>();
            }
            overlayVP.renderMode      = VideoRenderMode.RenderTexture;
            overlayVP.playOnAwake     = false;
            overlayVP.isLooping       = false;
            overlayVP.audioOutputMode = VideoAudioOutputMode.Direct;

            // Overlay RawImage (fullscreen within HintOverlay, first child)
            var existingOverlayDisplay = overlayT.Find("HintVideoDisplay");
            if (existingOverlayDisplay != null) { Object.DestroyImmediate(existingOverlayDisplay.gameObject); }

            var overlayDisplayGO = new GameObject("HintVideoDisplay", typeof(RectTransform));
            overlayDisplayGO.transform.SetParent(overlayT, false);
            overlayDisplayGO.transform.SetAsFirstSibling();
            var overlayDisplayRT = (RectTransform)overlayDisplayGO.transform;
            overlayDisplayRT.anchorMin = Vector2.zero;
            overlayDisplayRT.anchorMax = Vector2.one;
            overlayDisplayRT.offsetMin = Vector2.zero;
            overlayDisplayRT.offsetMax = Vector2.zero;
            var overlayRawImg = overlayDisplayGO.AddComponent<RawImage>();
            overlayRawImg.color         = Color.clear;
            overlayRawImg.raycastTarget = false;

            // ── Rebuild HintReturnButton ──────────────────────────────────────
            var oldReturn = overlayT.Find("HintReturnButton");
            if (oldReturn != null) { Object.DestroyImmediate(oldReturn.gameObject); }

            var returnGO = new GameObject("HintReturnButton", typeof(RectTransform));
            returnGO.transform.SetParent(overlayT, false);
            var returnRT = (RectTransform)returnGO.transform;
            returnRT.anchorMin = new Vector2(0.03f, 0.02f);
            returnRT.anchorMax = new Vector2(0.35f, 0.12f);
            returnRT.offsetMin = Vector2.zero;
            returnRT.offsetMax = Vector2.zero;
            returnGO.AddComponent<Image>();
            returnGO.AddComponent<Button>();
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(returnGO.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;
            labelGO.AddComponent<TextMeshProUGUI>().text = "Return to Evidence";
            DetectiveStyleGuide.StyleButton(returnGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("hintVideoDisplay").objectReferenceValue          = panelRawImg;
            so.FindProperty("hintVideoPlayer").objectReferenceValue           = panelVP;
            so.FindProperty("hintsOverlayVideoDisplay").objectReferenceValue  = overlayRawImg;
            so.FindProperty("hintsOverlayVideoPlayer").objectReferenceValue   = overlayVP;
            so.FindProperty("hintOverlay").objectReferenceValue               = overlayT.gameObject;
            so.FindProperty("hintReturnButton").objectReferenceValue          = returnGO.GetComponent<Button>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Hints video setup complete.\n" +
                      "  Panel: hintVideoPlayer + hintVideoDisplay wired (HintFromChief.mp4).\n" +
                      "  Overlay: hintsOverlayVideoPlayer + hintsOverlayVideoDisplay wired.\n" +
                      "  Set HintsOverlayVideoFile = 'HintsVideo.mp4' in Case01 Inspector.\n" +
                      "  Press Ctrl+S to save.");
        }
    }
}
