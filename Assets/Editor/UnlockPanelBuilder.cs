using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Unlock Panel
    /// Creates UnlockPanel between PasswordLockPanel and EvidenceDetailPanel.
    /// Plays the UnlockVideo (6) Pragmatic Decision) then advances to EvidenceDetail.
    ///
    /// Hierarchy:
    ///   UnlockPanel  (Image bg, VideoPlayer, inactive by default)
    ///     UnlockVideoDisplay  (RawImage, fullscreen)
    ///     UnlockSkipButton    (bottom-right)
    ///
    /// Wires unlockPanel, unlockVideoDisplay, unlockVideoPlayer, unlockSkipButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class UnlockPanelBuilder
    {
        [MenuItem("ETEC510/Build Unlock Panel")]
        public static void BuildPanel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // Remove old panel if it exists
            var existing = ct.Find("UnlockPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log("  Removed existing UnlockPanel.");
            }

            // Create panel after PasswordLockPanel
            var panelGO = new GameObject("UnlockPanel", typeof(RectTransform));
            panelGO.transform.SetParent(ct, false);

            var passwordPanel = ct.Find("PasswordLockPanel");
            if (passwordPanel != null)
                panelGO.transform.SetSiblingIndex(passwordPanel.GetSiblingIndex() + 1);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = DetectiveStyleGuide.BgDeep;

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            vp.renderMode      = VideoRenderMode.RenderTexture;

            panelGO.SetActive(false);

            // ── UnlockVideoDisplay ─────────────────────────────────────────────
            var displayGO = new GameObject("UnlockVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "UnlockVideoDisplay", saved);

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── UnlockSkipButton ───────────────────────────────────────────────
            var skipGO = new GameObject("UnlockSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.60f, 0.02f);
            skipRT.anchorMax = new Vector2(0.96f, 0.12f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "UnlockSkipButton", saved);

            skipGO.AddComponent<Image>().raycastTarget = true;
            skipGO.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(skipGO.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "Skip  >>";

            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Wire CaseRunner ────────────────────────────────────────────────
            var controllerT = ct.Find("CaseController");
            if (controllerT == null) { Debug.LogWarning("ETEC510: CaseController not found — wire fields manually."); }
            else
            {
                var runner = controllerT.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("unlockPanel").objectReferenceValue        = panelGO;
                    so.FindProperty("unlockVideoDisplay").objectReferenceValue = rawImg;
                    so.FindProperty("unlockVideoPlayer").objectReferenceValue  = vp;
                    so.FindProperty("unlockSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  CaseRunner fields wired: unlockPanel, unlockVideoDisplay, unlockVideoPlayer, unlockSkipButton.");
                }
            }

            EditorUtility.SetDirty(panelGO);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: UnlockPanel built. Ctrl+S to save.");
        }
    }
}
