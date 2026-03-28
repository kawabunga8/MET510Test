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
    ///   - Skip button (styled, bottom-right) → Badge2IntroPanel
    ///   - Back button (styled, bottom-left)  → Badge1Panel
    ///
    /// Hierarchy:
    ///   CritDecAwardPanel  (Image black bg, VideoPlayer, inactive by default)
    ///     CritDecAwardVideoDisplay  (RawImage fullscreen)
    ///     CritDecAwardSkipButton    (bottom-right, styled Primary)
    ///       Label / BgFill / Shine
    ///     CritDecAwardBackButton    (bottom-left, styled Nav)
    ///       Label / BgFill / Shine
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
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

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
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

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
            DetectiveStyleGuide.ApplySavedRect(displayRT, "CritDecAwardVideoDisplay", saved);

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── Skip button (styled, bottom-right) → Badge2IntroPanel ────────
            var skipGO = new GameObject("CritDecAwardSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f);
            skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "CritDecAwardSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLabelGO = new GameObject("Label", typeof(RectTransform));
            skipLabelGO.transform.SetParent(skipGO.transform, false);
            var skipLabelRT = (RectTransform)skipLabelGO.transform;
            skipLabelRT.anchorMin = Vector2.zero; skipLabelRT.anchorMax = Vector2.one;
            skipLabelRT.offsetMin = Vector2.zero; skipLabelRT.offsetMax = Vector2.zero;
            skipLabelGO.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Back button (styled, bottom-left) → Badge1Panel ──────────────
            var backGO = new GameObject("CritDecAwardBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.03f, 0.02f);
            backRT.anchorMax = new Vector2(0.21f, 0.13f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "CritDecAwardBackButton", saved);
            backGO.AddComponent<Image>();
            backGO.AddComponent<Button>();
            var backLabelGO = new GameObject("Label", typeof(RectTransform));
            backLabelGO.transform.SetParent(backGO.transform, false);
            var backLabelRT = (RectTransform)backLabelGO.transform;
            backLabelRT.anchorMin = Vector2.zero; backLabelRT.anchorMax = Vector2.one;
            backLabelRT.offsetMin = Vector2.zero; backLabelRT.offsetMax = Vector2.zero;
            backLabelGO.AddComponent<TextMeshProUGUI>().text = "Back";
            DetectiveStyleGuide.StyleButton(backGO, DetectiveStyleGuide.ButtonRole.Nav);

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
