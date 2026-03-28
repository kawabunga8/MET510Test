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
    /// Menu: ETEC510 > Build Dispositional Award Panel
    ///
    /// Creates DispositionalAwardPanel under GameCanvas — plays the Dispositional
    /// badge award video (DispositionalAwardVideoFile from CaseData).
    ///
    ///   VIDEO STATE (video playing):
    ///     - Fullscreen RawImage shows badge award video
    ///     - Skip button visible (text-only, bottom-right)
    ///     - Back button hidden
    ///
    /// Hierarchy:
    ///   DispositionalAwardPanel  (Image black bg, VideoPlayer, inactive by default)
    ///     DispositionalAwardVideoDisplay  (RawImage fullscreen)
    ///     DispositionalAwardSkipButton    (bottom-right, text-only)
    ///       Label (TMP)
    ///     DispositionalAwardBackButton    (bottom-left, text-only, starts hidden)
    ///       Label (TMP)
    ///
    /// Wires: dispositionalAwardPanel, dispositionalAwardVideoDisplay,
    ///        dispositionalAwardVideoPlayer, dispositionalAwardSkipButton,
    ///        dispositionalAwardBackButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class DispositionalAwardPanelBuilder
    {
        [MenuItem("ETEC510/Build Dispositional Award Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("DispositionalAwardPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("DispositionalAwardPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just after badgeAchievedPanel (or at end)
            var badgePanel = canvasGO.transform.Find("BadgeAchievedPanel");
            int targetIndex = badgePanel != null
                ? badgePanel.GetSiblingIndex() + 1
                : canvasGO.transform.childCount;
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
            var displayGO = new GameObject("DispositionalAwardVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "DispositionalAwardVideoDisplay", saved);

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── Skip button (styled, bottom-right) → Evidence Board ───────────
            var skipGO = new GameObject("DispositionalAwardSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f);
            skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "DispositionalAwardSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLabelGO = new GameObject("Label", typeof(RectTransform));
            skipLabelGO.transform.SetParent(skipGO.transform, false);
            var skipLabelRT = (RectTransform)skipLabelGO.transform;
            skipLabelRT.anchorMin = Vector2.zero; skipLabelRT.anchorMax = Vector2.one;
            skipLabelRT.offsetMin = Vector2.zero; skipLabelRT.offsetMax = Vector2.zero;
            skipLabelGO.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Back button (styled, bottom-left) → Badge2Panel ───────────────
            var backGO = new GameObject("DispositionalAwardBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.03f, 0.02f);
            backRT.anchorMax = new Vector2(0.21f, 0.13f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "DispositionalAwardBackButton", saved);
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
            so.FindProperty("dispositionalAwardPanel").objectReferenceValue        = panelGO;
            so.FindProperty("dispositionalAwardVideoDisplay").objectReferenceValue = rawImg;
            so.FindProperty("dispositionalAwardVideoPlayer").objectReferenceValue  = vp;
            so.FindProperty("dispositionalAwardSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
            so.FindProperty("dispositionalAwardBackButton").objectReferenceValue   = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: DispositionalAwardPanel built.\n" +
                      "- Set DispositionalAwardVideoFile in CaseData Inspector (e.g. '12) Badge 2 Award.mp4').\n" +
                      "Press Ctrl+S to save.");
        }
    }
}
