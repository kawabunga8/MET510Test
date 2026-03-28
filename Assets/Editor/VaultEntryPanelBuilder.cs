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
    /// Menu: ETEC510 > Build Vault Entry Panel
    ///
    /// Creates VaultEntryPanel under GameCanvas — plays VaultEntry.mp4 after
    /// the correct password is entered, before EvidenceDetailPanel.
    ///
    /// Hierarchy:
    ///   VaultEntryPanel  (black bg, VideoPlayer, inactive by default)
    ///     VaultEntryVideoDisplay  (RawImage fullscreen)
    ///     VaultEntrySkipButton    (bottom-right, Primary style, 1s delay)
    ///
    /// Wires: vaultEntryPanel, vaultEntryVideoDisplay, vaultEntryVideoPlayer,
    ///        vaultEntrySkipButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class VaultEntryPanelBuilder
    {
        [MenuItem("ETEC510/Build Vault Entry Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("VaultEntryPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("VaultEntryPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just before EvidenceDetailPanel
            var evidenceDetail = canvasGO.transform.Find("EvidenceDetailPanel");
            int targetIndex = evidenceDetail != null
                ? evidenceDetail.GetSiblingIndex()
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
            vp.renderMode      = VideoRenderMode.RenderTexture;
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;

            panelGO.SetActive(false);

            // ── Video display (fullscreen RawImage) ───────────────────────────
            var displayGO = new GameObject("VaultEntryVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "VaultEntryVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── Skip button (bottom-right, Primary style) ─────────────────────
            var skipGO = new GameObject("VaultEntrySkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f);
            skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "VaultEntrySkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLabelGO = new GameObject("Label", typeof(RectTransform));
            skipLabelGO.transform.SetParent(skipGO.transform, false);
            var skipLabelRT = (RectTransform)skipLabelGO.transform;
            skipLabelRT.anchorMin = Vector2.zero; skipLabelRT.anchorMax = Vector2.one;
            skipLabelRT.offsetMin = Vector2.zero; skipLabelRT.offsetMax = Vector2.zero;
            skipLabelGO.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("vaultEntryPanel").objectReferenceValue        = panelGO;
            so.FindProperty("vaultEntryVideoDisplay").objectReferenceValue = rawImg;
            so.FindProperty("vaultEntryVideoPlayer").objectReferenceValue  = vp;
            so.FindProperty("vaultEntrySkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: VaultEntryPanel built.\n" +
                      "Set VaultEntryVideoFile = 'VaultEntry.mp4' in Case01 Inspector.\n" +
                      "Press Ctrl+S to save.");
        }
    }
}
