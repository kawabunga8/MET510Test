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
    /// Menu: ETEC510 > Build CritDec Award Panel
    ///
    /// Creates CritDecAwardPanel under GameCanvas — plays the Critical Decision
    /// badge award video (CriticalDecision.BadgeVideoFile from CaseData).
    ///
    ///   - Invisible skip button (fullscreen overlay) → Badge2IntroPanel
    ///   - Visible Back button (bottom-left) → CriticalDecisionBackground
    ///
    /// Hierarchy:
    ///   CritDecAwardPanel  (Image black bg, VideoPlayer, inactive by default)
    ///     CritDecAwardVideoDisplay  (RawImage fullscreen)
    ///     CritDecAwardSkipButton    (fullscreen invisible overlay)
    ///     CritDecAwardBackButton    (bottom-left, visible)
    ///       Label (TMP)
    ///
    /// Wires: critDecAwardPanel, critDecAwardVideoDisplay, critDecAwardVideoPlayer,
    ///        critDecAwardSkipButton, critDecAwardBackButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class CritDecAwardPanelBuilder
    {
        [MenuItem("ETEC510/Build CritDec Award Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("CritDecAwardPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("CritDecAwardPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just before CritDecTryAgainPanel (or after CriticalDecisionPanel)
            var critDecTryAgain = canvasGO.transform.Find("CritDecTryAgainPanel");
            var critDecPanel    = canvasGO.transform.Find("CriticalDecisionPointPanel");
            int targetIndex = critDecTryAgain != null
                ? critDecTryAgain.GetSiblingIndex()
                : (critDecPanel != null ? critDecPanel.GetSiblingIndex() + 1 : canvasGO.transform.childCount);
            panelGO.transform.SetSiblingIndex(targetIndex);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 1f);

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;

            panelGO.SetActive(false);

            // ── Video display (fullscreen RawImage) ───────────────────────────
            var displayGO = new GameObject("CritDecAwardVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── Skip button (text-only, bottom-right) ────────────────────────
            var skipGO = new GameObject("CritDecAwardSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.60f, 0.02f);
            skipRT.anchorMax = new Vector2(0.96f, 0.12f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;

            var skipImg = skipGO.AddComponent<Image>();
            skipImg.color         = Color.clear;
            skipImg.raycastTarget = true;

            skipGO.AddComponent<Button>();

            var skipLabel = new GameObject("Label", typeof(RectTransform));
            skipLabel.transform.SetParent(skipGO.transform, false);
            var skipLabelRT = (RectTransform)skipLabel.transform;
            skipLabelRT.anchorMin = Vector2.zero; skipLabelRT.anchorMax = Vector2.one;
            skipLabelRT.offsetMin = Vector2.zero; skipLabelRT.offsetMax = Vector2.zero;
            var skipTMP = skipLabel.AddComponent<TextMeshProUGUI>();
            skipTMP.text      = "Skip";
            skipTMP.fontSize  = 28f;
            skipTMP.fontStyle = FontStyles.Bold;
            skipTMP.color     = DetectiveStyleGuide.LabelLight;
            skipTMP.alignment = TextAlignmentOptions.Center;

            // ── Back button (bottom-left, visible) ────────────────────────────
            var backGO = new GameObject("CritDecAwardBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.03f, 0.02f);
            backRT.anchorMax = new Vector2(0.36f, 0.12f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;

            var backImg = backGO.AddComponent<Image>();
            backImg.color         = Color.clear;
            backImg.raycastTarget = true;

            backGO.AddComponent<Button>();

            var backLabel = new GameObject("Label", typeof(RectTransform));
            backLabel.transform.SetParent(backGO.transform, false);
            var backLabelRT = (RectTransform)backLabel.transform;
            backLabelRT.anchorMin = Vector2.zero; backLabelRT.anchorMax = Vector2.one;
            backLabelRT.offsetMin = Vector2.zero; backLabelRT.offsetMax = Vector2.zero;
            var backTMP = backLabel.AddComponent<TextMeshProUGUI>();
            backTMP.text      = "Back";
            backTMP.fontSize  = 28f;
            backTMP.fontStyle = FontStyles.Bold;
            backTMP.color     = DetectiveStyleGuide.LabelLight;
            backTMP.alignment = TextAlignmentOptions.Center;
            backGO.SetActive(false); // hidden during video, shown after

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("critDecAwardPanel").objectReferenceValue        = panelGO;
            so.FindProperty("critDecAwardVideoDisplay").objectReferenceValue = rawImg;
            so.FindProperty("critDecAwardVideoPlayer").objectReferenceValue  = vp;
            so.FindProperty("critDecAwardSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
            so.FindProperty("critDecAwardBackButton").objectReferenceValue   = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: CritDecAwardPanel built. Press Ctrl+S to save.");
        }
    }
}
