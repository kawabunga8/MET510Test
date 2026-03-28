using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Badge Achieved Panel
    ///
    /// Creates a full-screen BadgeAchievedPanel under GameCanvas:
    ///   - Dark overlay background
    ///   - RawImage for badge video playback (+ VideoPlayer)
    ///   - "Skip →" button at bottom right
    ///
    /// Wires CaseRunner fields:
    ///   badgeAchievedPanel, badgeAchievedVideoDisplay,
    ///   badgeAchievedVideoPlayer, badgeAchievedSkipButton
    ///
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class BadgeAchievedPanelBuilder
    {
        [MenuItem("ETEC510/Build Badge Achieved Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("BadgeAchievedPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel — full stretch ──────────────────────────────────────
            var panelRT = NewStretch("BadgeAchievedPanel", canvasGO.transform);
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);
            panelRT.gameObject.SetActive(false);

            var bg = panelRT.gameObject.AddComponent<Image>();
            bg.color         = new Color(0f, 0f, 0f, 0.92f);
            bg.raycastTarget = true;

            // ── Video RawImage — fills the panel ──────────────────────────────
            var vidGO = NewChild("BadgeVideoDisplay", panelRT);
            var vidRT = (RectTransform)vidGO.transform;
            vidRT.anchorMin = Vector2.zero; vidRT.anchorMax = Vector2.one;
            vidRT.offsetMin = Vector2.zero; vidRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(vidRT, "BadgeVideoDisplay", saved);
            var rawImg = vidGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear; // hidden until video plays
            rawImg.raycastTarget = false;

            // ── VideoPlayer on the same GO ─────────────────────────────────────
            var vp = vidGO.AddComponent<VideoPlayer>();
            vp.playOnAwake      = false;
            vp.isLooping        = false;
            vp.audioOutputMode  = VideoAudioOutputMode.Direct;
            vp.renderMode       = VideoRenderMode.RenderTexture; // set at runtime

            // ── Skip button — bottom-right ─────────────────────────────────────
            var skipGO = NewChild("BadgeSkipButton", panelRT);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f);
            skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "BadgeSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLabel = NewChild("Label", skipGO.transform);
            var skipLabelRT = (RectTransform)skipLabel.transform;
            skipLabelRT.anchorMin = Vector2.zero; skipLabelRT.anchorMax = Vector2.one;
            skipLabelRT.offsetMin = Vector2.zero; skipLabelRT.offsetMax = Vector2.zero;
            skipLabel.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);


            // ── Position just below ImagePopupPanel (or at top of canvas) ─────
            var imagePopup = canvasGO.transform.Find("ImagePopupPanel");
            int targetIndex = imagePopup != null ? imagePopup.GetSiblingIndex() : canvasGO.transform.childCount;
            panelRT.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner via SerializedObject ───────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badgeAchievedPanel").objectReferenceValue        = panelRT.gameObject;
            so.FindProperty("badgeAchievedVideoDisplay").objectReferenceValue = rawImg;
            so.FindProperty("badgeAchievedVideoPlayer").objectReferenceValue  = vp;
            so.FindProperty("badgeAchievedSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: BadgeAchievedPanel built. Press Ctrl+S to save.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static RectTransform NewStretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        static GameObject NewChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static TMP_FontAsset FindFont()
        {
            string[] candidates = { "LiberationSans", "Roboto", "Arial", "OpenSans" };
            foreach (var n in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {n}");
                if (guids.Length > 0)
                {
                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (f != null) return f;
                }
            }
            return null;
        }
    }
}
