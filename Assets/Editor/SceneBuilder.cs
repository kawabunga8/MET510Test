using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Scene Structure
    /// Creates the full Canvas + all 10 panels for the escape-room flow.
    /// Run once on an empty scene, then wire the CaseRunner Inspector fields.
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem("ETEC510/Build Scene Structure")]
        public static void BuildScene()
        {
            if (!EditorUtility.DisplayDialog(
                "Build Scene Structure",
                "This will create the full Canvas and all 10 panels in the current scene.\n\nContinue?",
                "Build", "Cancel"))
                return;

            // ── Canvas ────────────────────────────────────────────────────────
            var canvasGO = new GameObject("GameCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // ── CaseController ────────────────────────────────────────────────
            var controllerGO = new GameObject("CaseController");
            controllerGO.transform.SetParent(canvasGO.transform, false);

            // ── EventSystem ───────────────────────────────────────────────────
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // ── Build all panels ──────────────────────────────────────────────
            BuildIntroPanel(canvasGO.transform);
            BuildMissionBriefingPanel(canvasGO.transform);
            BuildEvidenceBoardPanel(canvasGO.transform);
            BuildSpotTheCluePanel(canvasGO.transform);
            BuildGutCheckPanel(canvasGO.transform);
            BuildFindTheMotivePanel(canvasGO.transform);
            BuildPasswordLockPanel(canvasGO.transform);
            BuildEvidenceDetailPanel(canvasGO.transform);
            BuildHintsFromChiefPanel(canvasGO.transform);
            BuildLevelCompletePanel(canvasGO.transform);

            // Select CaseController so designer can wire Inspector fields
            Selection.activeGameObject = controllerGO;

            Debug.Log("ETEC510: Scene structure built. Select CaseController and wire all CaseRunner Inspector fields.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // INTRO PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildIntroPanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "IntroPanel", new Color(0.05f, 0.05f, 0.1f));

            // Video surface (RawImage fills the panel)
            var videoDisplay = CreateChild<RawImage>(panel, "VideoBackground");
            SetStretch(videoDisplay.GetComponent<RectTransform>());
            videoDisplay.color = Color.white;

            // VideoPlayer on the panel itself
            panel.gameObject.AddComponent<VideoPlayer>();

            // Skip button — top-right
            var skipBtn = CreateButton(panel, "SkipButton", "Skip ▶");
            var skipRT = skipBtn.GetComponent<RectTransform>();
            skipRT.anchorMin = new Vector2(1, 1);
            skipRT.anchorMax = new Vector2(1, 1);
            skipRT.pivot     = new Vector2(1, 1);
            skipRT.anchoredPosition = new Vector2(-30, -30);
            skipRT.sizeDelta = new Vector2(140, 50);

            // Enter button — center-bottom, hidden by default
            var enterBtn = CreateButton(panel, "EnterButton", "Enter →");
            var enterRT = enterBtn.GetComponent<RectTransform>();
            enterRT.anchorMin = new Vector2(0.5f, 0);
            enterRT.anchorMax = new Vector2(0.5f, 0);
            enterRT.pivot     = new Vector2(0.5f, 0);
            enterRT.anchoredPosition = new Vector2(0, 60);
            enterRT.sizeDelta = new Vector2(220, 65);
            enterBtn.SetActive(false);

            Debug.Log("  ✓ IntroPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // MISSION BRIEFING PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildMissionBriefingPanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "MissionBriefingPanel", new Color(0.08f, 0.08f, 0.15f));
            panel.gameObject.SetActive(false);

            // Background image
            var bg = CreateChild<Image>(panel, "BriefingImage");
            SetStretch(bg.GetComponent<RectTransform>());
            bg.color = new Color(1, 1, 1, 0.15f);
            bg.preserveAspect = false;

            // Title
            var title = CreateTMPText(panel, "TitleText", "MISSION CRITICAL", 52, FontStyles.Bold);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.75f);
            titleRT.anchorMax = new Vector2(0.9f, 0.92f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // Body
            var body = CreateTMPText(panel, "BodyText", "Briefing text goes here...", 28, FontStyles.Normal);
            var bodyRT = body.GetComponent<RectTransform>();
            bodyRT.anchorMin = new Vector2(0.1f, 0.25f);
            bodyRT.anchorMax = new Vector2(0.9f, 0.72f);
            bodyRT.offsetMin = bodyRT.offsetMax = Vector2.zero;
            body.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;

            // Start button
            var startBtn = CreateButton(panel, "StartButton", "Start Investigation →");
            var startRT = startBtn.GetComponent<RectTransform>();
            startRT.anchorMin = new Vector2(0.5f, 0);
            startRT.anchorMax = new Vector2(0.5f, 0);
            startRT.pivot     = new Vector2(0.5f, 0);
            startRT.anchoredPosition = new Vector2(0, 50);
            startRT.sizeDelta = new Vector2(320, 65);

            Debug.Log("  ✓ MissionBriefingPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // EVIDENCE BOARD PANEL (hub)
        // ─────────────────────────────────────────────────────────────────────
        static void BuildEvidenceBoardPanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "EvidenceBoardPanel", new Color(0.1f, 0.07f, 0.04f));
            panel.gameObject.SetActive(false);

            var bg = CreateChild<Image>(panel, "EvidenceBoardImage");
            SetStretch(bg.GetComponent<RectTransform>());
            bg.color = new Color(1, 1, 1, 0.3f);

            // Title
            var title = CreateTMPText(panel, "TitleText", "EVIDENCE BOARD", 48, FontStyles.Bold);
            var titleRT = title.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.1f, 0.82f);
            titleRT.anchorMax = new Vector2(0.9f, 0.95f);
            titleRT.offsetMin = titleRT.offsetMax = Vector2.zero;

            // Collected code display  — "5  1  0"
            var codeRow = CreateEmptyRect(panel, "CodeDisplay");
            var codeRT = codeRow.GetComponent<RectTransform>();
            codeRT.anchorMin = new Vector2(0.3f, 0.70f);
            codeRT.anchorMax = new Vector2(0.7f, 0.82f);
            codeRT.offsetMin = codeRT.offsetMax = Vector2.zero;

            CreateDigitLabel(codeRow.transform, "Digit1Text", "?", new Vector2(0f, 0f), new Vector2(0.33f, 1f));
            CreateDigitLabel(codeRow.transform, "Digit2Text", "?", new Vector2(0.33f, 0f), new Vector2(0.66f, 1f));
            CreateDigitLabel(codeRow.transform, "Digit3Text", "?", new Vector2(0.66f, 0f), new Vector2(1f, 1f));

            // Three investigation path buttons
            BuildEvidenceBoardButton(panel, "SpotTheClueButton",    "🔍 Spot the Clue",    new Vector2(0.1f, 0.38f), new Vector2(0.38f, 0.65f));
            BuildEvidenceBoardButton(panel, "GutCheckButton",       "💬 Gut Check",        new Vector2(0.41f, 0.38f), new Vector2(0.59f, 0.65f));
            BuildEvidenceBoardButton(panel, "FindTheMotiveButton",  "🎯 Find the Motive",  new Vector2(0.62f, 0.38f), new Vector2(0.9f, 0.65f));

            // Enter password button (locked until all 3 done)
            var lockBtn = CreateButton(panel, "EnterPasswordButton", "🔒  Enter Code Room");
            var lockRT = lockBtn.GetComponent<RectTransform>();
            lockRT.anchorMin = new Vector2(0.35f, 0.12f);
            lockRT.anchorMax = new Vector2(0.65f, 0.28f);
            lockRT.offsetMin = lockRT.offsetMax = Vector2.zero;
            lockBtn.GetComponent<Button>().interactable = false;

            Debug.Log("  ✓ EvidenceBoardPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // SPOT THE CLUE PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildSpotTheCluePanel(Transform parent)
        {
            var panel = CreateInvestigationPanel(parent, "SpotTheCluePanel",
                "SPOT THE CLUE", "SpotPromptText", "SpotEvidenceImage");
            panel.SetActive(false);

            // Explanation text (hidden until confirmed)
            var explain = CreateTMPText(panel.transform, "SpotExplanationText",
                "Good catch! Here's why this is suspicious...", 26, FontStyles.Normal);
            SetAnchoredRect(explain.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.38f));
            explain.SetActive(false);

            CreateButton(panel.transform, "SpotConfirmButton", "✔  I Found It!",
                new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.17f));

            CreateButton(panel.transform, "SpotBackButton", "← Back to Board",
                new Vector2(0.01f, 0.01f), new Vector2(0.22f, 0.09f));

            Debug.Log("  ✓ SpotTheCluePanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // GUT CHECK PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildGutCheckPanel(Transform parent)
        {
            var panel = CreateInvestigationPanel(parent, "GutCheckPanel",
                "GUT CHECK", "GutPromptText", "GutEvidenceImage");
            panel.SetActive(false);

            // Option buttons
            CreateButton(panel.transform, "GutOptionButton0", "Option A",
                new Vector2(0.05f, 0.20f), new Vector2(0.48f, 0.33f));
            CreateButton(panel.transform, "GutOptionButton1", "Option B",
                new Vector2(0.52f, 0.20f), new Vector2(0.95f, 0.33f));

            // Feedback
            var feedback = CreateTMPText(panel.transform, "GutFeedbackText", "", 26, FontStyles.Normal);
            SetAnchoredRect(feedback.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.10f), new Vector2(0.95f, 0.19f));

            // Next / back
            var nextBtn = CreateButton(panel.transform, "GutNextButton", "Back to Board →",
                new Vector2(0.3f, 0.02f), new Vector2(0.7f, 0.10f));
            nextBtn.SetActive(false);

            CreateButton(panel.transform, "GutBackButton", "← Back to Board",
                new Vector2(0.01f, 0.01f), new Vector2(0.22f, 0.09f));

            Debug.Log("  ✓ GutCheckPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // FIND THE MOTIVE PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildFindTheMotivePanel(Transform parent)
        {
            var panel = CreateInvestigationPanel(parent, "FindTheMotivePanel",
                "FIND THE MOTIVE", "MotivePromptText", "MotiveEvidenceImage");
            panel.SetActive(false);

            var explain = CreateTMPText(panel.transform, "MotiveExplanationText",
                "Here's who benefits and why...", 26, FontStyles.Normal);
            SetAnchoredRect(explain.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.18f), new Vector2(0.95f, 0.38f));
            explain.SetActive(false);

            CreateButton(panel.transform, "MotiveConfirmButton", "✔  I Found Them!",
                new Vector2(0.3f, 0.06f), new Vector2(0.7f, 0.17f));

            CreateButton(panel.transform, "MotiveBackButton", "← Back to Board",
                new Vector2(0.01f, 0.01f), new Vector2(0.22f, 0.09f));

            Debug.Log("  ✓ FindTheMotivePanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // PASSWORD LOCK PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildPasswordLockPanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "PasswordLockPanel", new Color(0.05f, 0.05f, 0.12f));
            panel.gameObject.SetActive(false);

            var title = CreateTMPText(panel, "TitleText", "ENTER THE CODE", 52, FontStyles.Bold);
            SetAnchoredRect(title.GetComponent<RectTransform>(),
                new Vector2(0.2f, 0.72f), new Vector2(0.8f, 0.88f));

            // Input field
            var inputGO = new GameObject("PasswordInputField");
            inputGO.transform.SetParent(panel, false);
            var inputRT = inputGO.AddComponent<RectTransform>();
            SetAnchoredRect(inputRT, new Vector2(0.3f, 0.52f), new Vector2(0.7f, 0.66f));
            var inputImg = inputGO.AddComponent<Image>();
            inputImg.color = new Color(1, 1, 1, 0.15f);
            var inputField = inputGO.AddComponent<TMP_InputField>();

            // Input field text child
            var textChild = new GameObject("Text");
            textChild.transform.SetParent(inputGO.transform, false);
            var inputText = textChild.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 40;
            inputText.alignment = TextAlignmentOptions.Center;
            inputText.color = Color.white;
            var textRT = textChild.GetComponent<RectTransform>();
            SetStretch(textRT);
            inputField.textComponent = inputText;

            // Feedback
            var feedback = CreateTMPText(panel, "PasswordFeedbackText", "", 26, FontStyles.Normal);
            SetAnchoredRect(feedback.GetComponent<RectTransform>(),
                new Vector2(0.2f, 0.44f), new Vector2(0.8f, 0.52f));
            feedback.GetComponent<TMP_Text>().color = new Color(1f, 0.4f, 0.4f);

            CreateButton(panel, "PasswordSubmitButton", "Unlock →",
                new Vector2(0.35f, 0.30f), new Vector2(0.65f, 0.42f));

            CreateButton(panel, "PasswordBackButton", "← Back to Board",
                new Vector2(0.01f, 0.01f), new Vector2(0.22f, 0.09f));

            Debug.Log("  ✓ PasswordLockPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // EVIDENCE DETAIL (VERDICT) PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildEvidenceDetailPanel(Transform parent)
        {
            var panel = CreateInvestigationPanel(parent, "EvidenceDetailPanel",
                "THE EVIDENCE — YOUR VERDICT", "VerdictPromptText", "VerdictEvidenceImage");
            panel.SetActive(false);

            CreateButton(panel.transform, "VerdictOptionButton0", "REAL",
                new Vector2(0.05f, 0.06f), new Vector2(0.45f, 0.20f));
            CreateButton(panel.transform, "VerdictOptionButton1", "FAKE",
                new Vector2(0.55f, 0.06f), new Vector2(0.95f, 0.20f));

            CreateButton(panel.transform, "VerdictBackButton", "← Back to Board",
                new Vector2(0.01f, 0.01f), new Vector2(0.22f, 0.09f));

            Debug.Log("  ✓ EvidenceDetailPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // HINTS FROM CHIEF PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildHintsFromChiefPanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "HintsFromChiefPanel", new Color(0.08f, 0.06f, 0.02f));
            panel.gameObject.SetActive(false);

            var title = CreateTMPText(panel, "TitleText", "HINT FROM THE CHIEF", 46, FontStyles.Bold);
            SetAnchoredRect(title.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.82f), new Vector2(0.9f, 0.95f));

            // Chief silhouette image — right side
            var chiefImg = CreateChild<Image>(panel, "ChiefImage");
            SetAnchoredRect(chiefImg.GetComponent<RectTransform>(),
                new Vector2(0.65f, 0.15f), new Vector2(0.95f, 0.80f));
            chiefImg.preserveAspect = true;

            // Hint text — left side
            var hint = CreateTMPText(panel, "HintBodyText", "Here's a clue to help you...", 30, FontStyles.Normal);
            SetAnchoredRect(hint.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.35f), new Vector2(0.60f, 0.78f));
            hint.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;

            CreateButton(panel, "HintTryAgainButton", "Try Again →",
                new Vector2(0.05f, 0.12f), new Vector2(0.35f, 0.28f));

            Debug.Log("  ✓ HintsFromChiefPanel");
        }

        // ─────────────────────────────────────────────────────────────────────
        // LEVEL COMPLETE PANEL
        // ─────────────────────────────────────────────────────────────────────
        static void BuildLevelCompletePanel(Transform parent)
        {
            var panel = CreateFullscreenPanel(parent, "LevelCompletePanel", new Color(0.02f, 0.15f, 0.05f));
            panel.gameObject.SetActive(false);

            var title = CreateTMPText(panel, "TitleText", "LEVEL 1 COMPLETE!", 64, FontStyles.Bold);
            SetAnchoredRect(title.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.92f));
            title.GetComponent<TMP_Text>().color = new Color(1f, 0.85f, 0.1f);

            var img = CreateChild<Image>(panel, "CompletionImage");
            SetAnchoredRect(img.GetComponent<RectTransform>(),
                new Vector2(0.35f, 0.38f), new Vector2(0.65f, 0.70f));
            img.preserveAspect = true;

            var body = CreateTMPText(panel, "CompletionBodyText",
                "You successfully cracked the case!", 30, FontStyles.Normal);
            SetAnchoredRect(body.GetComponent<RectTransform>(),
                new Vector2(0.1f, 0.24f), new Vector2(0.9f, 0.37f));

            var xp = CreateTMPText(panel, "XPText", "You earned 100 XP!", 36, FontStyles.Bold);
            SetAnchoredRect(xp.GetComponent<RectTransform>(),
                new Vector2(0.2f, 0.10f), new Vector2(0.8f, 0.22f));
            xp.GetComponent<TMP_Text>().color = new Color(1f, 0.85f, 0.1f);

            Debug.Log("  ✓ LevelCompletePanel");
        }

        // ═════════════════════════════════════════════════════════════════════
        // HELPERS
        // ═════════════════════════════════════════════════════════════════════

        static Transform CreateFullscreenPanel(Transform parent, string name, Color bgColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            SetStretch(rt);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            return go.transform;
        }

        static GameObject CreateInvestigationPanel(Transform parent, string panelName,
            string titleStr, string promptName, string imageName)
        {
            var panel = CreateFullscreenPanel(parent, panelName, new Color(0.06f, 0.06f, 0.12f));

            // Header
            var title = CreateTMPText(panel, "TitleText", titleStr, 44, FontStyles.Bold);
            SetAnchoredRect(title.GetComponent<RectTransform>(),
                new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.98f));

            // Evidence image — left half
            var img = CreateChild<Image>(panel, imageName);
            SetAnchoredRect(img.GetComponent<RectTransform>(),
                new Vector2(0.02f, 0.38f), new Vector2(0.48f, 0.86f));
            img.preserveAspect = true;

            // Prompt text — right half
            var prompt = CreateTMPText(panel, promptName, "Prompt text...", 28, FontStyles.Normal);
            SetAnchoredRect(prompt.GetComponent<RectTransform>(),
                new Vector2(0.52f, 0.38f), new Vector2(0.98f, 0.86f));
            prompt.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;

            return panel.gameObject;
        }

        static void BuildEvidenceBoardButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var btn = CreateButton(parent, name, label);
            SetAnchoredRect(btn.GetComponent<RectTransform>(), anchorMin, anchorMax);
        }

        static T CreateChild<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go.AddComponent<T>();
        }

        static GameObject CreateButton(Transform parent, string name, string label,
            Vector2 anchorMin = default, Vector2 anchorMax = default)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();

            if (anchorMin != default || anchorMax != default)
                SetAnchoredRect(rt, anchorMin, anchorMax);

            var img = go.AddComponent<Image>();
            img.sprite                  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type                    = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.1f;
            img.color = new Color(0.18f, 0.45f, 0.85f);
            go.AddComponent<Button>();

            // Shine overlay — top portion, white at low alpha
            var shineGO = new GameObject("Shine");
            shineGO.transform.SetParent(go.transform, false);
            var shineRT = shineGO.AddComponent<RectTransform>();
            shineRT.anchorMin = new Vector2(0.04f, 0.52f);
            shineRT.anchorMax = new Vector2(0.96f, 0.90f);
            shineRT.offsetMin = shineRT.offsetMax = Vector2.zero;
            var shineImg = shineGO.AddComponent<Image>();
            shineImg.sprite                  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            shineImg.type                    = Image.Type.Sliced;
            shineImg.pixelsPerUnitMultiplier = 0.1f;
            shineImg.color                   = new Color(1f, 1f, 1f, 0.07f);
            shineImg.raycastTarget           = false;

            // Label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = labelGO.AddComponent<RectTransform>();
            SetStretch(labelRT);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return go;
        }

        static GameObject CreateEmptyRect(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        static GameObject CreateTMPText(Transform parent, string name, string text,
            float fontSize, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return go;
        }

        static void CreateDigitLabel(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = CreateTMPText(parent, name, text, 64, FontStyles.Bold);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<TMP_Text>().color = new Color(1f, 0.85f, 0.1f);
        }

        static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void SetAnchoredRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }
    }
}
