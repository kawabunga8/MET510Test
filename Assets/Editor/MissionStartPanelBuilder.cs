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
    /// Menu: ETEC510 > Build Mission Start Panel
    /// Creates MissionStartPanel between MissionBriefingPanel and EvidenceBoardPanel.
    ///
    /// Hierarchy:
    ///   MissionStartPanel  (Image bg, VideoPlayer, inactive by default)
    ///     MissionStartVideoDisplay  (RawImage, fullscreen)
    ///     MissionStartSkipButton    (bottom-right, Skip to Evidence Board)
    ///       Label  (TMP)
    ///
    /// Wires missionStartPanel, missionStartVideoDisplay, missionStartVideoPlayer,
    /// and missionStartSkipButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class MissionStartPanelBuilder
    {
        [MenuItem("ETEC510/Build Mission Start Panel")]
        public static void BuildPanel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // ── Remove old panel if it exists ────────────────────────────────
            var existing = ct.Find("MissionStartPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log("  Removed existing MissionStartPanel.");
            }

            // ── Create panel ─────────────────────────────────────────────────
            var panelGO = new GameObject("MissionStartPanel", typeof(RectTransform));
            panelGO.transform.SetParent(ct, false);

            // Position it right after MissionBriefingPanel in the hierarchy
            var briefingPanel = ct.Find("MissionBriefingPanel");
            if (briefingPanel != null)
                panelGO.transform.SetSiblingIndex(briefingPanel.GetSiblingIndex() + 1);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            // Dark background image
            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = DetectiveStyleGuide.BgDeep;

            // VideoPlayer on the panel root
            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            vp.renderMode      = VideoRenderMode.RenderTexture; // driven by script at runtime

            // Panel starts inactive
            panelGO.SetActive(false);

            // ── MissionStartVideoDisplay (fullscreen RawImage) ────────────────
            var displayGO = new GameObject("MissionStartVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "MissionStartVideoDisplay", saved);

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;   // hidden until texture is ready
            rawImg.raycastTarget = false;

            // ── MissionStartSkipButton (bottom-right) ─────────────────────────
            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            var skipGO = new GameObject("MissionStartSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin        = new Vector2(0.60f, 0.02f);
            skipRT.anchorMax        = new Vector2(0.96f, 0.12f);
            skipRT.offsetMin = Vector2.zero;
            skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "MissionStartSkipButton", saved);

            var skipImg = skipGO.AddComponent<Image>();
            skipImg.sprite                  = rounded;
            skipImg.type                    = Image.Type.Sliced;
            skipImg.pixelsPerUnitMultiplier = 0.10f;
            skipImg.color                   = DetectiveStyleGuide.BtnNav;
            skipImg.raycastTarget           = true;

            skipGO.AddComponent<Button>();

            var shadow = skipGO.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0f, 0f, 0f, 0.5f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // Label
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(skipGO.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "Skip to Evidence Board  >>";
            tmp.fontSize  = 28f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color     = DetectiveStyleGuide.LabelLight;
            tmp.alignment = TextAlignmentOptions.Center;

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var controllerT = ct.Find("CaseController");
            if (controllerT == null) { Debug.LogWarning("ETEC510: CaseController not found — wire fields manually."); }
            else
            {
                var runner = controllerT.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("missionStartPanel").objectReferenceValue        = panelGO;
                    so.FindProperty("missionStartVideoDisplay").objectReferenceValue = rawImg;
                    so.FindProperty("missionStartVideoPlayer").objectReferenceValue  = vp;
                    so.FindProperty("missionStartSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  CaseRunner fields wired: missionStartPanel, missionStartVideoDisplay, missionStartVideoPlayer, missionStartSkipButton.");
                }
            }

            EditorUtility.SetDirty(panelGO);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: MissionStartPanel built. Run ETEC510 > Style All Buttons, then Ctrl+S to save.");
        }
    }
}
