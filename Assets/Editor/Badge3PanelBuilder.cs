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
    /// Menu: ETEC510 > Build Badge 3 Intro Panel
    ///       ETEC510 > Build Badge 3 Award Panel
    ///       ETEC510 > Build Conclusion Panel
    ///
    /// All three are video panels that follow the same structure as
    /// Badge2IntroPanel and DispositionalAwardPanel. Safe to re-run.
    /// </summary>
    public static class Badge3PanelBuilder
    {
        // ── Badge 3 Intro ─────────────────────────────────────────────────────

        [MenuItem("ETEC510/Build Badge 3 Intro Panel")]
        public static void BuildBadge3Intro()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("Badge3IntroPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("Badge3IntroPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just before Badge3AwardPanel (or after DispositionalAwardPanel)
            var anchor = canvasGO.transform.Find("Badge3AwardPanel") ??
                         canvasGO.transform.Find("DispositionalAwardPanel");
            if (anchor != null)
                panelGO.transform.SetSiblingIndex(anchor.GetSiblingIndex());

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            panelGO.AddComponent<Image>().color = Color.black;

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            panelGO.SetActive(false);

            // ── Video display ─────────────────────────────────────────────────
            var displayGO = new GameObject("Badge3IntroVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero; displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "Badge3IntroVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color = Color.clear; rawImg.raycastTarget = false;

            // ── Background image (content state) ─────────────────────────────
            var bgGO = new GameObject("Badge3IntroBg", typeof(RectTransform));
            bgGO.transform.SetParent(panelGO.transform, false);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(bgRT, "Badge3IntroBg", saved);
            var bgImg = bgGO.AddComponent<Image>();
            var bgGuids = AssetDatabase.FindAssets("Badge3Background t:Texture2D", new[] { "Assets/Images" });
            if (bgGuids.Length > 0)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(bgGuids[0]));
                if (sp != null) { bgImg.sprite = sp; bgImg.type = Image.Type.Simple; bgImg.preserveAspect = false; }
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.05f, 0.15f, 1f);
                Debug.LogWarning("ETEC510: 'Badge3Background' not found in Assets/Images — using solid colour.");
            }
            bgGO.SetActive(false);

            // ── Skip button — bottom-right, 300×75, BgFill + Shine ───────────
            var skipGO = new GameObject("Badge3IntroSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = skipRT.anchorMax = skipRT.pivot = new Vector2(1f, 0f);
            skipRT.sizeDelta        = new Vector2(300f, 75f);
            skipRT.anchoredPosition = new Vector2(-20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(skipRT, "Badge3IntroSkipButton", saved);
            skipGO.AddComponent<Button>();

            var skipBgFill = new GameObject("BgFill", typeof(RectTransform));
            skipBgFill.transform.SetParent(skipGO.transform, false);
            var skipBgRT = (RectTransform)skipBgFill.transform;
            skipBgRT.anchorMin = Vector2.zero; skipBgRT.anchorMax = Vector2.one;
            skipBgRT.offsetMin = skipBgRT.offsetMax = Vector2.zero;
            var skipBgImg = skipBgFill.AddComponent<Image>();
            skipBgImg.sprite                  = rounded;
            skipBgImg.type                    = Image.Type.Sliced;
            skipBgImg.pixelsPerUnitMultiplier = 0.1f;
            skipBgImg.color                   = new Color(0.18f, 0.45f, 0.85f, 1f);

            var skipShine = new GameObject("Shine", typeof(RectTransform));
            skipShine.transform.SetParent(skipGO.transform, false);
            var skipShineRT = (RectTransform)skipShine.transform;
            skipShineRT.anchorMin = new Vector2(0.04f, 0.52f); skipShineRT.anchorMax = new Vector2(0.96f, 0.90f);
            skipShineRT.offsetMin = skipShineRT.offsetMax = Vector2.zero;
            var skipShineImg = skipShine.AddComponent<Image>();
            skipShineImg.sprite                  = rounded;
            skipShineImg.type                    = Image.Type.Sliced;
            skipShineImg.pixelsPerUnitMultiplier = 0.1f;
            skipShineImg.color                   = new Color(1f, 1f, 1f, 0.07f);
            skipShineImg.raycastTarget           = false;

            var skipLbl = new GameObject("Label", typeof(RectTransform));
            skipLbl.transform.SetParent(skipGO.transform, false);
            var skipLblRT = (RectTransform)skipLbl.transform;
            skipLblRT.anchorMin = Vector2.zero; skipLblRT.anchorMax = Vector2.one;
            skipLblRT.offsetMin = skipLblRT.offsetMax = Vector2.zero;
            var skipTMP = skipLbl.AddComponent<TextMeshProUGUI>();
            skipTMP.text = "Skip"; skipTMP.fontSize = 28f; skipTMP.fontStyle = FontStyles.Bold;
            skipTMP.color = Color.white; skipTMP.alignment = TextAlignmentOptions.Center;

            // ── Proceed button (invisible overlay, hidden during video) ───────
            var proceedGO = new GameObject("Badge3IntroProceedButton", typeof(RectTransform));
            proceedGO.transform.SetParent(panelGO.transform, false);
            var proceedRT = (RectTransform)proceedGO.transform;
            proceedRT.anchorMin = new Vector2(0.30f, 0.10f); proceedRT.anchorMax = new Vector2(0.70f, 0.25f);
            proceedRT.offsetMin = proceedRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(proceedRT, "Badge3IntroProceedButton", saved);
            var proceedImg = proceedGO.AddComponent<Image>();
            proceedImg.color = Color.clear; proceedImg.raycastTarget = true;
            proceedGO.AddComponent<Button>();
            proceedGO.SetActive(false);

            // ── Back button — bottom-left, text-only Label ────────────────────
            var backGO = new GameObject("Badge3IntroBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = backRT.anchorMax = backRT.pivot = new Vector2(0f, 0f);
            backRT.sizeDelta        = new Vector2(200f, 75f);
            backRT.anchoredPosition = new Vector2(20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(backRT, "Badge3IntroBackButton", saved);
            backGO.AddComponent<Button>();
            var backLbl = new GameObject("Label", typeof(RectTransform));
            backLbl.transform.SetParent(backGO.transform, false);
            var backLblRT = (RectTransform)backLbl.transform;
            backLblRT.anchorMin = Vector2.zero; backLblRT.anchorMax = Vector2.one;
            backLblRT.offsetMin = backLblRT.offsetMax = Vector2.zero;
            var backTMP = backLbl.AddComponent<TextMeshProUGUI>();
            backTMP.text = "Back"; backTMP.fontSize = 28f; backTMP.fontStyle = FontStyles.Bold;
            backTMP.color = DetectiveStyleGuide.LabelLight; backTMP.alignment = TextAlignmentOptions.Center;
            backGO.SetActive(false);

            // ── Content back button — pill-shaped, bottom-left, shown in content state ──
            var contentBackGO = new GameObject("Badge3IntroContentBackButton", typeof(RectTransform));
            contentBackGO.transform.SetParent(panelGO.transform, false);
            var contentBackRT = (RectTransform)contentBackGO.transform;
            contentBackRT.anchorMin = contentBackRT.anchorMax = contentBackRT.pivot = new Vector2(0f, 0f);
            contentBackRT.sizeDelta        = new Vector2(300f, 75f);
            contentBackRT.anchoredPosition = new Vector2(20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(contentBackRT, "Badge3IntroContentBackButton", saved);
            contentBackGO.AddComponent<Button>();

            var contentBgFill = new GameObject("BgFill", typeof(RectTransform));
            contentBgFill.transform.SetParent(contentBackGO.transform, false);
            var contentBgRT = (RectTransform)contentBgFill.transform;
            contentBgRT.anchorMin = Vector2.zero; contentBgRT.anchorMax = Vector2.one;
            contentBgRT.offsetMin = contentBgRT.offsetMax = Vector2.zero;
            var contentBgImg = contentBgFill.AddComponent<Image>();
            contentBgImg.sprite                  = rounded;
            contentBgImg.type                    = Image.Type.Sliced;
            contentBgImg.pixelsPerUnitMultiplier = 0.05f;   // very low → pill shape
            contentBgImg.color                   = new Color(0.18f, 0.45f, 0.85f, 1f);

            var contentShine = new GameObject("Shine", typeof(RectTransform));
            contentShine.transform.SetParent(contentBackGO.transform, false);
            var contentShineRT = (RectTransform)contentShine.transform;
            contentShineRT.anchorMin = new Vector2(0.04f, 0.52f); contentShineRT.anchorMax = new Vector2(0.96f, 0.90f);
            contentShineRT.offsetMin = contentShineRT.offsetMax = Vector2.zero;
            var contentShineImg = contentShine.AddComponent<Image>();
            contentShineImg.sprite                  = rounded;
            contentShineImg.type                    = Image.Type.Sliced;
            contentShineImg.pixelsPerUnitMultiplier = 0.05f;
            contentShineImg.color                   = new Color(1f, 1f, 1f, 0.07f);
            contentShineImg.raycastTarget           = false;

            var contentLbl = new GameObject("Label", typeof(RectTransform));
            contentLbl.transform.SetParent(contentBackGO.transform, false);
            var contentLblRT = (RectTransform)contentLbl.transform;
            contentLblRT.anchorMin = Vector2.zero; contentLblRT.anchorMax = Vector2.one;
            contentLblRT.offsetMin = contentLblRT.offsetMax = Vector2.zero;
            var contentTMP = contentLbl.AddComponent<TextMeshProUGUI>();
            contentTMP.text = "Back"; contentTMP.fontSize = 28f; contentTMP.fontStyle = FontStyles.Bold;
            contentTMP.color = Color.white; contentTMP.alignment = TextAlignmentOptions.Center;

            contentBackGO.SetActive(false);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge3IntroPanel").objectReferenceValue               = panelGO;
            so.FindProperty("badge3IntroVideoDisplay").objectReferenceValue        = rawImg;
            so.FindProperty("badge3IntroVideoPlayer").objectReferenceValue         = vp;
            so.FindProperty("badge3IntroBackground").objectReferenceValue          = bgImg;
            so.FindProperty("badge3IntroSkipButton").objectReferenceValue          = skipGO.GetComponent<Button>();
            so.FindProperty("badge3IntroProceedButton").objectReferenceValue       = proceedGO.GetComponent<Button>();
            so.FindProperty("badge3IntroBackButton").objectReferenceValue          = backGO.GetComponent<Button>();
            so.FindProperty("badge3IntroContentBackButton").objectReferenceValue   = contentBackGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge3IntroPanel built.\n" +
                      "- Adjust Badge3IntroProceedButton anchors to match artwork.\n" +
                      "- Add Badge3Background.png to Assets/Images and re-run if needed.\n" +
                      "Press Ctrl+S to save.");
        }

        // ── Badge 3 Decision ──────────────────────────────────────────────────

        [MenuItem("ETEC510/Build Badge 3 Decision Panel")]
        public static void BuildBadge3Decision()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("Badge3DecisionPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("Badge3DecisionPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            var anchor = canvasGO.transform.Find("Badge3AwardPanel") ??
                         canvasGO.transform.Find("Badge3IntroPanel");
            int idx = anchor != null ? anchor.GetSiblingIndex() + 1 : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(idx);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            panelGO.AddComponent<Image>().color = Color.black;

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake = false; vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            panelGO.SetActive(false);

            // ── Video display ─────────────────────────────────────────────────
            var displayGO = new GameObject("Badge3DecisionVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero; displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "Badge3DecisionVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color = Color.clear; rawImg.raycastTarget = false;

            // ── Skip button — bottom-right, 300×75, BgFill + Shine ───────────
            var skipGO = new GameObject("Badge3DecisionSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = skipRT.anchorMax = skipRT.pivot = new Vector2(1f, 0f);
            skipRT.sizeDelta        = new Vector2(300f, 75f);
            skipRT.anchoredPosition = new Vector2(-20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(skipRT, "Badge3DecisionSkipButton", saved);
            skipGO.AddComponent<Button>();

            var skipBgFill = new GameObject("BgFill", typeof(RectTransform));
            skipBgFill.transform.SetParent(skipGO.transform, false);
            var skipBgRT = (RectTransform)skipBgFill.transform;
            skipBgRT.anchorMin = Vector2.zero; skipBgRT.anchorMax = Vector2.one;
            skipBgRT.offsetMin = skipBgRT.offsetMax = Vector2.zero;
            var skipBgImg = skipBgFill.AddComponent<Image>();
            skipBgImg.sprite                  = rounded;
            skipBgImg.type                    = Image.Type.Sliced;
            skipBgImg.pixelsPerUnitMultiplier = 0.1f;
            skipBgImg.color                   = new Color(0.18f, 0.45f, 0.85f, 1f);

            var skipShine = new GameObject("Shine", typeof(RectTransform));
            skipShine.transform.SetParent(skipGO.transform, false);
            var skipShineRT = (RectTransform)skipShine.transform;
            skipShineRT.anchorMin = new Vector2(0.04f, 0.52f); skipShineRT.anchorMax = new Vector2(0.96f, 0.90f);
            skipShineRT.offsetMin = skipShineRT.offsetMax = Vector2.zero;
            var skipShineImg = skipShine.AddComponent<Image>();
            skipShineImg.sprite                  = rounded;
            skipShineImg.type                    = Image.Type.Sliced;
            skipShineImg.pixelsPerUnitMultiplier = 0.1f;
            skipShineImg.color                   = new Color(1f, 1f, 1f, 0.07f);
            skipShineImg.raycastTarget           = false;

            var skipLbl = new GameObject("Label", typeof(RectTransform));
            skipLbl.transform.SetParent(skipGO.transform, false);
            var skipLblRT = (RectTransform)skipLbl.transform;
            skipLblRT.anchorMin = Vector2.zero; skipLblRT.anchorMax = Vector2.one;
            skipLblRT.offsetMin = skipLblRT.offsetMax = Vector2.zero;
            var skipTMP = skipLbl.AddComponent<TextMeshProUGUI>();
            skipTMP.text = "Skip"; skipTMP.fontSize = 28f; skipTMP.fontStyle = FontStyles.Bold;
            skipTMP.color = Color.white; skipTMP.alignment = TextAlignmentOptions.Center;

            // ── Background image (content state) ─────────────────────────────
            var bgGO = new GameObject("Badge3DecisionBg", typeof(RectTransform));
            bgGO.transform.SetParent(panelGO.transform, false);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(bgRT, "Badge3DecisionBg", saved);
            var bgImg = bgGO.AddComponent<Image>();
            var bgGuids = AssetDatabase.FindAssets("Badge3DecisionBG t:Texture2D", new[] { "Assets/Images" });
            if (bgGuids.Length > 0)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(bgGuids[0]));
                if (sp != null) { bgImg.sprite = sp; bgImg.type = Image.Type.Simple; bgImg.preserveAspect = false; }
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.05f, 0.15f, 1f);
                Debug.LogWarning("ETEC510: 'Badge3DecisionBG' not found in Assets/Images — using solid colour.");
            }
            bgGO.SetActive(false);

            // ── Invisible overlays — decision choices ─────────────────────────
            Button MakeOverlay(string name, float yMin, float yMax)
            {
                var go = new GameObject(name, typeof(RectTransform));
                go.transform.SetParent(panelGO.transform, false);
                var rt = (RectTransform)go.transform;
                rt.anchorMin = new Vector2(0.02f, yMin);
                rt.anchorMax = new Vector2(0.98f, yMax);
                rt.offsetMin = rt.offsetMax = Vector2.zero;
                DetectiveStyleGuide.ApplySavedRect(rt, name, saved);
                var img = go.AddComponent<Image>();
                img.color = Color.clear;
                go.AddComponent<Button>();
                go.SetActive(false);
                return go.GetComponent<Button>();
            }

            var reportBtn      = MakeOverlay("ReportPostButton",     0.60f, 0.75f);
            var commentBtn     = MakeOverlay("CommentOnPostButton",  0.40f, 0.55f);
            var directMsgBtn   = MakeOverlay("DirectMessageButton",  0.20f, 0.35f);

            // ── Back button — bottom-left, 300×75, Nav style ─────────────────
            var backGO = new GameObject("Badge3DecisionBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = backRT.anchorMax = backRT.pivot = new Vector2(0f, 0f);
            backRT.sizeDelta        = new Vector2(300f, 75f);
            backRT.anchoredPosition = new Vector2(20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(backRT, "Badge3DecisionBackButton", saved);
            backGO.AddComponent<Image>();
            backGO.AddComponent<Button>();
            var backLblGO = new GameObject("Label", typeof(RectTransform));
            backLblGO.transform.SetParent(backGO.transform, false);
            var backLblRT = (RectTransform)backLblGO.transform;
            backLblRT.anchorMin = Vector2.zero; backLblRT.anchorMax = Vector2.one;
            backLblRT.offsetMin = backLblRT.offsetMax = Vector2.zero;
            backLblGO.AddComponent<TextMeshProUGUI>().text = "Back";
            DetectiveStyleGuide.StyleButton(backGO, DetectiveStyleGuide.ButtonRole.Nav);
            backGO.SetActive(false);

            // ── Hover text area ───────────────────────────────────────────────
            var msgGO = new GameObject("Badge3DecisionMessageText", typeof(RectTransform));
            msgGO.transform.SetParent(panelGO.transform, false);
            var msgRT = (RectTransform)msgGO.transform;
            msgRT.anchorMin = new Vector2(0.02f, 0.04f);
            msgRT.anchorMax = new Vector2(0.98f, 0.14f);
            msgRT.offsetMin = msgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(msgRT, "Badge3DecisionMessageText", saved);
            var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text      = "";
            msgTMP.fontSize  = 28f;
            msgTMP.fontStyle = FontStyles.Bold;
            msgTMP.color     = DetectiveStyleGuide.LabelLight;
            msgTMP.alignment = TextAlignmentOptions.Center;
            msgGO.SetActive(false);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge3DecisionPanel").objectReferenceValue         = panelGO;
            so.FindProperty("badge3DecisionVideoDisplay").objectReferenceValue  = rawImg;
            so.FindProperty("badge3DecisionVideoPlayer").objectReferenceValue   = vp;
            so.FindProperty("badge3DecisionSkipButton").objectReferenceValue    = skipGO.GetComponent<Button>();
            so.FindProperty("badge3DecisionBackground").objectReferenceValue    = bgImg;
            so.FindProperty("reportPostButton").objectReferenceValue           = reportBtn;
            so.FindProperty("commentOnPostButton").objectReferenceValue        = commentBtn;
            so.FindProperty("directMessageButton").objectReferenceValue        = directMsgBtn;
            so.FindProperty("badge3DecisionMessageText").objectReferenceValue  = msgTMP;
            so.FindProperty("badge3DecisionBackButton").objectReferenceValue   = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge3DecisionPanel built.\n" +
                      "- Adjust ReportPostButton, CommentOnPostButton, DirectMessageButton anchors to match artwork.\n" +
                      "- Add Badge3DecisionBG.png to Assets/Images and re-run if needed.\n" +
                      "Press Ctrl+S to save.");
        }

        // ── Badge 3 Decision False ────────────────────────────────────────────

        [MenuItem("ETEC510/Build Badge 3 Decision False Panel")]
        public static void BuildBadge3DecisionFalse()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("Badge3DecisionFalsePanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel — sibling after Badge3DecisionPanel ────────────────
            var panelGO = new GameObject("Badge3DecisionFalsePanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var anchor = canvasGO.transform.Find("Badge3DecisionPanel");
            int idx = anchor != null ? anchor.GetSiblingIndex() + 1 : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(idx);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            panelGO.AddComponent<Image>().color = Color.black;
            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake = false; vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            panelGO.SetActive(false);

            // ── Video display ─────────────────────────────────────────────────
            var displayGO = new GameObject("Badge3DecisionFalseVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero; displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "Badge3DecisionFalseVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color = Color.clear; rawImg.raycastTarget = false;

            // ── Skip button — bottom-right, 300×75, BgFill + Shine ───────────
            var skipGO = new GameObject("Badge3DecisionFalseSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = skipRT.anchorMax = skipRT.pivot = new Vector2(1f, 0f);
            skipRT.sizeDelta = new Vector2(300f, 75f);
            skipRT.anchoredPosition = new Vector2(-20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(skipRT, "Badge3DecisionFalseSkipButton", saved);
            skipGO.AddComponent<Button>();
            var sBg = new GameObject("BgFill", typeof(RectTransform));
            sBg.transform.SetParent(skipGO.transform, false);
            var sBgRT = (RectTransform)sBg.transform;
            sBgRT.anchorMin = Vector2.zero; sBgRT.anchorMax = Vector2.one;
            sBgRT.offsetMin = sBgRT.offsetMax = Vector2.zero;
            var sBgImg = sBg.AddComponent<Image>();
            sBgImg.sprite = rounded; sBgImg.type = Image.Type.Sliced;
            sBgImg.pixelsPerUnitMultiplier = 0.1f;
            sBgImg.color = new Color(0.18f, 0.45f, 0.85f, 1f);
            var sShine = new GameObject("Shine", typeof(RectTransform));
            sShine.transform.SetParent(skipGO.transform, false);
            var sShineRT = (RectTransform)sShine.transform;
            sShineRT.anchorMin = new Vector2(0.04f, 0.52f); sShineRT.anchorMax = new Vector2(0.96f, 0.90f);
            sShineRT.offsetMin = sShineRT.offsetMax = Vector2.zero;
            var sShineImg = sShine.AddComponent<Image>();
            sShineImg.sprite = rounded; sShineImg.type = Image.Type.Sliced;
            sShineImg.pixelsPerUnitMultiplier = 0.1f;
            sShineImg.color = new Color(1f, 1f, 1f, 0.07f);
            sShineImg.raycastTarget = false;
            var sLbl = new GameObject("Label", typeof(RectTransform));
            sLbl.transform.SetParent(skipGO.transform, false);
            var sLblRT = (RectTransform)sLbl.transform;
            sLblRT.anchorMin = Vector2.zero; sLblRT.anchorMax = Vector2.one;
            sLblRT.offsetMin = sLblRT.offsetMax = Vector2.zero;
            var sTMP = sLbl.AddComponent<TextMeshProUGUI>();
            sTMP.text = "Skip"; sTMP.fontSize = 28f; sTMP.fontStyle = FontStyles.Bold;
            sTMP.color = Color.white; sTMP.alignment = TextAlignmentOptions.Center;

            // ── Back button — bottom-left, 300×75, Nav style ──────────────────
            var backGO = new GameObject("Badge3DecisionFalseBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = backRT.anchorMax = backRT.pivot = new Vector2(0f, 0f);
            backRT.sizeDelta = new Vector2(300f, 75f);
            backRT.anchoredPosition = new Vector2(20f, 20f);
            DetectiveStyleGuide.ApplySavedRect(backRT, "Badge3DecisionFalseBackButton", saved);
            backGO.AddComponent<Image>();
            backGO.AddComponent<Button>();
            var bLbl = new GameObject("Label", typeof(RectTransform));
            bLbl.transform.SetParent(backGO.transform, false);
            var bLblRT = (RectTransform)bLbl.transform;
            bLblRT.anchorMin = Vector2.zero; bLblRT.anchorMax = Vector2.one;
            bLblRT.offsetMin = bLblRT.offsetMax = Vector2.zero;
            bLbl.AddComponent<TextMeshProUGUI>().text = "Try Again";
            DetectiveStyleGuide.StyleButton(backGO, DetectiveStyleGuide.ButtonRole.Nav);
            backGO.SetActive(false);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge3DecisionFalsePanel").objectReferenceValue         = panelGO;
            so.FindProperty("badge3DecisionFalseVideoDisplay").objectReferenceValue  = rawImg;
            so.FindProperty("badge3DecisionFalseVideoPlayer").objectReferenceValue   = vp;
            so.FindProperty("badge3DecisionFalseSkipButton").objectReferenceValue    = skipGO.GetComponent<Button>();
            so.FindProperty("badge3DecisionFalseBackButton").objectReferenceValue    = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge3DecisionFalsePanel built. Press Ctrl+S to save.");
        }

        // ── Badge 3 Award ─────────────────────────────────────────────────────

        [MenuItem("ETEC510/Build Badge 3 Award Panel")]
        public static void BuildBadge3Award()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("Badge3AwardPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("Badge3AwardPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            var anchor = canvasGO.transform.Find("ConclusionPanel") ??
                         canvasGO.transform.Find("DispositionalAwardPanel");
            int idx = anchor != null ? anchor.GetSiblingIndex() : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(idx);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            panelGO.AddComponent<Image>().color = Color.black;

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake = false; vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            panelGO.SetActive(false);

            // ── Video display ─────────────────────────────────────────────────
            var displayGO = new GameObject("Badge3AwardVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero; displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "Badge3AwardVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color = Color.clear; rawImg.raycastTarget = false;

            // ── Skip button (bottom-right) ────────────────────────────────────
            var skipGO = new GameObject("Badge3AwardSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f); skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "Badge3AwardSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLblGO = new GameObject("Label", typeof(RectTransform));
            skipLblGO.transform.SetParent(skipGO.transform, false);
            var skipLblRT = (RectTransform)skipLblGO.transform;
            skipLblRT.anchorMin = Vector2.zero; skipLblRT.anchorMax = Vector2.one;
            skipLblRT.offsetMin = skipLblRT.offsetMax = Vector2.zero;
            skipLblGO.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Case Closed button — invisible overlay, hidden until video ends ─
            var ccGO = new GameObject("CaseClosedButton", typeof(RectTransform));
            ccGO.transform.SetParent(panelGO.transform, false);
            var ccRT = (RectTransform)ccGO.transform;
            ccRT.anchorMin = Vector2.zero; ccRT.anchorMax = Vector2.one;
            ccRT.offsetMin = ccRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(ccRT, "CaseClosedButton", saved);
            var ccImg = ccGO.AddComponent<Image>();
            ccImg.color = Color.clear;
            ccGO.AddComponent<Button>();
            ccGO.SetActive(false);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge3AwardPanel").objectReferenceValue         = panelGO;
            so.FindProperty("badge3AwardVideoDisplay").objectReferenceValue  = rawImg;
            so.FindProperty("badge3AwardVideoPlayer").objectReferenceValue   = vp;
            so.FindProperty("badge3AwardSkipButton").objectReferenceValue    = skipGO.GetComponent<Button>();
            so.FindProperty("caseClosedButton").objectReferenceValue         = ccGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge3AwardPanel built. Press Ctrl+S to save.");
        }

        // ── Conclusion ────────────────────────────────────────────────────────

        [MenuItem("ETEC510/Build Conclusion Panel")]
        public static void BuildConclusion()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("ConclusionPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("ConclusionPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            // Position just before LevelCompletePanel
            var levelComplete = canvasGO.transform.Find("LevelCompletePanel");
            int idx = levelComplete != null ? levelComplete.GetSiblingIndex() : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(idx);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero; panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            panelGO.AddComponent<Image>().color = Color.black;

            var vp = panelGO.AddComponent<VideoPlayer>();
            vp.playOnAwake = false; vp.isLooping = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;
            panelGO.SetActive(false);

            // ── Video display ─────────────────────────────────────────────────
            var displayGO = new GameObject("ConclusionVideoDisplay", typeof(RectTransform));
            displayGO.transform.SetParent(panelGO.transform, false);
            var displayRT = (RectTransform)displayGO.transform;
            displayRT.anchorMin = Vector2.zero; displayRT.anchorMax = Vector2.one;
            displayRT.offsetMin = displayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(displayRT, "ConclusionVideoDisplay", saved);
            var rawImg = displayGO.AddComponent<RawImage>();
            rawImg.color = Color.clear; rawImg.raycastTarget = false;

            // ── Skip button (bottom-right) ────────────────────────────────────
            var skipGO = new GameObject("ConclusionSkipButton", typeof(RectTransform));
            skipGO.transform.SetParent(panelGO.transform, false);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.79f, 0.02f); skipRT.anchorMax = new Vector2(0.97f, 0.13f);
            skipRT.offsetMin = skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "ConclusionSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            var skipLblGO = new GameObject("Label", typeof(RectTransform));
            skipLblGO.transform.SetParent(skipGO.transform, false);
            var skipLblRT = (RectTransform)skipLblGO.transform;
            skipLblRT.anchorMin = Vector2.zero; skipLblRT.anchorMax = Vector2.one;
            skipLblRT.offsetMin = skipLblRT.offsetMax = Vector2.zero;
            skipLblGO.AddComponent<TextMeshProUGUI>().text = "Skip";
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("conclusionPanel").objectReferenceValue        = panelGO;
            so.FindProperty("conclusionVideoDisplay").objectReferenceValue = rawImg;
            so.FindProperty("conclusionVideoPlayer").objectReferenceValue  = vp;
            so.FindProperty("conclusionSkipButton").objectReferenceValue   = skipGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: ConclusionPanel built. Press Ctrl+S to save.");
        }
    }
}
