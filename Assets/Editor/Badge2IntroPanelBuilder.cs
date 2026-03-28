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
    /// Menu: ETEC510 > Build Badge 2 Intro Panel
    ///
    /// Creates Badge2IntroPanel under GameCanvas — two-state panel:
    ///
    ///   VIDEO STATE (video playing):
    ///     - Fullscreen RawImage shows badge award video
    ///     - Skip button visible at bottom-right (stops video → content state)
    ///     - Background, Proceed button, Back button all hidden
    ///
    ///   CONTENT STATE (video ended or skipped):
    ///     - Background image (Badge2IntroBg) fills screen
    ///     - Invisible ProceedButton overlaid on "Proceed to Task 2" artwork → Badge2Panel
    ///     - Back button visible at bottom-left → CriticalDecisionPanel
    ///     - Skip button hidden
    ///
    /// Hierarchy:
    ///   Badge2IntroPanel  (Image black bg, VideoPlayer, inactive by default)
    ///     Badge2IntroVideoDisplay  (RawImage fullscreen)
    ///     Badge2IntroBg            (Image, fullscreen, hidden during video)
    ///     Badge2IntroSkipButton    (bottom-right, visible during video)
    ///       Label (TMP)
    ///     Badge2IntroProceedButton (invisible overlay on "Proceed to Task 2", hidden during video)
    ///     Badge2IntroBackButton    (bottom-left, hidden during video)
    ///       Label (TMP)
    ///
    /// Wires: badge2IntroPanel, badge2IntroVideoDisplay, badge2IntroVideoPlayer,
    ///        badge2IntroBackground, badge2IntroSkipButton,
    ///        badge2IntroProceedButton, badge2IntroBackButton on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class Badge2IntroPanelBuilder
    {
        [MenuItem("ETEC510/Build Badge 2 Intro Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("Badge2IntroPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("Badge2IntroPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just before Badge2Panel in the hierarchy
            var badge2Panel = canvasGO.transform.Find("Badge2Panel");
            if (badge2Panel != null)
                panelGO.transform.SetSiblingIndex(badge2Panel.GetSiblingIndex());

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
            var displayGO = new GameObject("Badge2IntroVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero;
            displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = Vector2.zero;
            displayRT.offsetMax = Vector2.zero;

            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── Background image (content state, hidden during video) ─────────
            var bgContentGO = new GameObject("Badge2IntroBg", typeof(RectTransform));
            bgContentGO.transform.SetParent(panelGO.transform, false);
            var bgContentRT = (RectTransform)bgContentGO.transform;
            bgContentRT.anchorMin = Vector2.zero;
            bgContentRT.anchorMax = Vector2.one;
            bgContentRT.offsetMin = Vector2.zero;
            bgContentRT.offsetMax = Vector2.zero;

            var bgContentImg = bgContentGO.AddComponent<Image>();
            bgContentImg.raycastTarget = true;

            // Try to load Badge2IntroBg sprite — add it to Assets/Images if available
            var bgGuids = AssetDatabase.FindAssets("Badge2Background t:Texture2D", new[] { "Assets/Images" });
            if (bgGuids.Length > 0)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(bgGuids[0]));
                if (sprite != null)
                {
                    bgContentImg.sprite         = sprite;
                    bgContentImg.type           = Image.Type.Simple;
                    bgContentImg.preserveAspect = false;
                }
            }
            else
            {
                bgContentImg.color = new Color(0.05f, 0.05f, 0.15f, 1f);
                Debug.LogWarning("ETEC510: 'Badge2Background' not found in Assets/Images — using solid colour.");
            }
            bgContentGO.SetActive(false); // hidden until video ends

            // ── Skip button (bottom-right, visible during video) ──────────────
            var skipGO = new GameObject("Badge2IntroSkipButton", typeof(RectTransform));
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

            // ── Invisible Proceed button (over "Proceed to Task 2" artwork) ───
            // Default anchors cover centre of screen — adjust in Inspector to match artwork.
            var proceedGO = new GameObject("Badge2IntroProceedButton", typeof(RectTransform));
            proceedGO.transform.SetParent(panelGO.transform, false);
            var proceedRT = (RectTransform)proceedGO.transform;
            proceedRT.anchorMin = new Vector2(0.30f, 0.10f);
            proceedRT.anchorMax = new Vector2(0.70f, 0.25f);
            proceedRT.offsetMin = Vector2.zero;
            proceedRT.offsetMax = Vector2.zero;

            var proceedImg = proceedGO.AddComponent<Image>();
            proceedImg.color         = Color.clear;
            proceedImg.raycastTarget = true;

            proceedGO.AddComponent<Button>();
            proceedGO.SetActive(false); // hidden until content state

            // ── Back button (bottom-left, visible in content state) ───────────
            var backGO = new GameObject("Badge2IntroBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.32f, 0.02f);
            backRT.anchorMax = new Vector2(0.68f, 0.12f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;

            var backImg = backGO.AddComponent<Image>();
            backImg.sprite                  = rounded;
            backImg.type                    = Image.Type.Sliced;
            backImg.pixelsPerUnitMultiplier = 0.10f;
            backImg.color                   = DetectiveStyleGuide.BtnNav;
            backImg.raycastTarget           = true;

            backGO.AddComponent<Button>();

            var backShadow = backGO.AddComponent<Shadow>();
            backShadow.effectColor    = new Color(0f, 0f, 0f, 0.5f);
            backShadow.effectDistance = new Vector2(2f, -2f);

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

            backGO.SetActive(false); // hidden until content state

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge2IntroPanel").objectReferenceValue          = panelGO;
            so.FindProperty("badge2IntroVideoDisplay").objectReferenceValue   = rawImg;
            so.FindProperty("badge2IntroVideoPlayer").objectReferenceValue    = vp;
            so.FindProperty("badge2IntroBackground").objectReferenceValue     = bgContentImg;
            so.FindProperty("badge2IntroSkipButton").objectReferenceValue     = skipGO.GetComponent<Button>();
            so.FindProperty("badge2IntroProceedButton").objectReferenceValue  = proceedGO.GetComponent<Button>();
            so.FindProperty("badge2IntroBackButton").objectReferenceValue     = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge2IntroPanel built.\n" +
                      "- Adjust Badge2IntroProceedButton anchors in Inspector to match 'Proceed to Task 2' artwork.\n" +
                      "- Set Badge2IntroVideoFile in CaseData Inspector.\n" +
                      "- Add Badge2IntroBg.png to Assets/Images and re-run if needed.\n" +
                      "Press Ctrl+S to save.");
        }
    }
}
