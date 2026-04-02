using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Teacher Dashboard
    ///
    /// Builds TeacherDashboardOverlay on GameCanvas:
    ///   - Full-screen dark overlay, always on top
    ///   - PasscodePanel: ●●● display + 3×4 numpad
    ///   - DataPanel: session table + Clear / Back / Close
    ///
    /// A hidden trigger button (bottom-right corner of GameCanvas) opens it.
    /// Safe to re-run — destroys and recreates.
    /// </summary>
    public static class TeacherDashboardBuilder
    {
        [MenuItem("ETEC510/Build Teacher Dashboard")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            DestroyIfExists(canvasGO, "TeacherDashboardOverlay");
            DestroyIfExists(canvasGO, "TeacherDashboardTrigger");

            // ── Overlay root ──────────────────────────────────────────────────
            var overlay = MakeRect(canvasGO.transform, "TeacherDashboardOverlay");
            SetStretch(overlay);
            overlay.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);
            overlay.gameObject.SetActive(false);

            var dashboard = overlay.gameObject.AddComponent<TeacherDashboard>();

            // ── Passcode panel ─────────────────────────────────────────────────
            var pcPanel = MakeRect(overlay, "PasscodePanel");
            SetCenter(pcPanel, new Vector2(320f, 460f));
            AddImage(pcPanel, new Color(0.07f, 0.07f, 0.14f, 1f));

            // X close button — top-right corner of passcode panel
            var pcCloseGO = MakeRect(pcPanel, "PasscodeCloseButton");
            PositionRT(pcCloseGO, new Vector2(0.82f, 0.90f), new Vector2(0.98f, 1.00f));
            pcCloseGO.gameObject.AddComponent<Image>().color = new Color(0.4f, 0.1f, 0.1f, 1f);
            var pcCloseBtn = pcCloseGO.gameObject.AddComponent<Button>();
            var pcCloseLbl = MakeTMP(pcCloseGO, "Label", "X", 22, FontStyles.Bold);
            PositionRT(pcCloseLbl, Vector2.zero, Vector2.one);

            // Title
            var title = MakeTMP(pcPanel, "Title", "TEACHER VIEW", 36, FontStyles.Bold);
            PositionRT(title, new Vector2(0.05f, 0.82f), new Vector2(0.82f, 0.97f));

            // Dots display
            var dotsGO = MakeTMP(pcPanel, "PasscodeDisplay", "○○○", 48, FontStyles.Bold);
            PositionRT(dotsGO, new Vector2(0.2f, 0.65f), new Vector2(0.8f, 0.82f));

            // Error label
            var errorGO = MakeTMP(pcPanel, "PasscodeError", "Incorrect code. Try again.", 20, FontStyles.Normal);
            errorGO.color = new Color(1f, 0.35f, 0.35f, 1f);
            PositionRT(errorGO.gameObject.GetComponent<RectTransform>(), new Vector2(0.05f, 0.57f), new Vector2(0.95f, 0.66f));
            errorGO.gameObject.SetActive(false);

            // ── Numpad (3×4 grid: 1-9, Clear, 0, Enter) ─────────────────────
            var numpad = MakeRect(pcPanel, "Numpad");
            PositionRT(numpad, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.57f));
            var grid = numpad.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize        = new Vector2(74, 66);
            grid.spacing         = new Vector2(8, 8);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding         = new RectOffset(4, 4, 4, 4);

            var digitBtns = new Button[10];

            // Row 0: 1 2 3
            // Row 1: 4 5 6
            // Row 2: 7 8 9
            // Row 3: CLR 0 ENTER
            int[] digitOrder = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            foreach (int d in digitOrder)
            {
                var btn = MakeNumpadButton(numpad, d.ToString(), d.ToString(), new Color(0.15f, 0.15f, 0.28f, 1f));
                digitBtns[d] = btn;
            }

            var clrBtn   = MakeNumpadButton(numpad, "ClearBtn",  "CLR",   new Color(0.3f, 0.12f, 0.12f, 1f));
            digitBtns[0] = MakeNumpadButton(numpad, "Digit0Btn", "0",     new Color(0.15f, 0.15f, 0.28f, 1f));
            var enterBtn = MakeNumpadButton(numpad, "EnterBtn",  "ENTER", new Color(0.1f, 0.35f, 0.15f, 1f));

            // ── Data panel ─────────────────────────────────────────────────────
            var dataPanel = MakeRect(overlay, "DataPanel");
            SetStretchWithPadding(dataPanel, 32f);
            AddImage(dataPanel, new Color(0.05f, 0.05f, 0.12f, 1f));
            dataPanel.gameObject.SetActive(false);

            // Data panel title
            var dTitle = MakeTMP(dataPanel, "DataTitle", "STUDENT DATA", 32, FontStyles.Bold);
            PositionRT(dTitle, new Vector2(0.02f, 0.93f), new Vector2(0.75f, 1f));

            // ── Stat boxes row ─────────────────────────────────────────────────
            // Four boxes: Sessions | Students | Completed | Hints Used
            var statsRow = MakeRect(dataPanel, "StatsRow");
            PositionRT(statsRow, new Vector2(0f, 0.84f), new Vector2(1f, 0.93f));

            var statColors = new Color[]
            {
                new Color(0.10f, 0.25f, 0.50f, 1f),  // Sessions  — blue
                new Color(0.10f, 0.35f, 0.20f, 1f),  // Students  — green
                new Color(0.35f, 0.20f, 0.05f, 1f),  // Completed — amber
                new Color(0.35f, 0.10f, 0.10f, 1f),  // Hints     — red
            };
            var statLabels  = new[] { "Sessions", "Students", "Completed", "Hints Used" };
            var statValueTMPs = new TextMeshProUGUI[4];

            float boxW = 1f / 4f;
            for (int b = 0; b < 4; b++)
            {
                float x0 = b * boxW + 0.005f;
                float x1 = x0 + boxW - 0.01f;
                var box = MakeRect(statsRow, $"StatBox{b}");
                PositionRT(box, new Vector2(x0, 0f), new Vector2(x1, 1f));
                box.gameObject.AddComponent<Image>().color = statColors[b];

                var lbl = MakeTMP(box, "Label", statLabels[b], 14, FontStyles.Normal);
                lbl.color = new Color(0.7f, 0.85f, 1f, 1f);
                PositionRT(lbl, new Vector2(0f, 0.55f), Vector2.one);

                var val = MakeTMP(box, "Value", "—", 28, FontStyles.Bold);
                PositionRT(val, Vector2.zero, new Vector2(1f, 0.6f));
                statValueTMPs[b] = val;
            }

            // Student dropdown
            var dropdownGO = MakeRect(dataPanel, "StudentDropdown");
            PositionRT(dropdownGO, new Vector2(0f, 0.77f), new Vector2(1f, 0.84f));
            var dropdownImg = dropdownGO.gameObject.AddComponent<Image>();
            dropdownImg.color = new Color(0.12f, 0.12f, 0.25f, 1f);
            var dropdown = dropdownGO.gameObject.AddComponent<TMP_Dropdown>();

            // Dropdown label
            var ddLabel = new GameObject("Label", typeof(RectTransform));
            ddLabel.transform.SetParent(dropdownGO, false);
            var ddLabelRT = (RectTransform)ddLabel.transform;
            ddLabelRT.anchorMin = new Vector2(0.02f, 0f); ddLabelRT.anchorMax = new Vector2(0.9f, 1f);
            ddLabelRT.offsetMin = ddLabelRT.offsetMax = Vector2.zero;
            var ddLabelTMP = ddLabel.AddComponent<TextMeshProUGUI>();
            ddLabelTMP.text      = "All Students";
            ddLabelTMP.fontSize  = 20;
            ddLabelTMP.color     = Color.white;
            ddLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;
            dropdown.captionText = ddLabelTMP;

            // Dropdown arrow
            var ddArrow = new GameObject("Arrow", typeof(RectTransform));
            ddArrow.transform.SetParent(dropdownGO, false);
            var ddArrowRT = (RectTransform)ddArrow.transform;
            ddArrowRT.anchorMin = new Vector2(0.9f, 0f); ddArrowRT.anchorMax = Vector2.one;
            ddArrowRT.offsetMin = ddArrowRT.offsetMax = Vector2.zero;
            var ddArrowTMP = ddArrow.AddComponent<TextMeshProUGUI>();
            ddArrowTMP.text      = "▼";
            ddArrowTMP.fontSize  = 18;
            ddArrowTMP.color     = Color.white;
            ddArrowTMP.alignment = TextAlignmentOptions.Center;

            // Dropdown template (required by TMP_Dropdown)
            var tmpl = MakeRect(dropdownGO, "Template");
            PositionRT(tmpl, new Vector2(0f, 0f), new Vector2(1f, 0f));
            tmpl.sizeDelta        = new Vector2(0f, 200f);
            tmpl.pivot            = new Vector2(0.5f, 1f);
            tmpl.anchoredPosition = Vector2.zero;
            tmpl.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.2f, 1f);
            var tmplScroll = tmpl.gameObject.AddComponent<ScrollRect>();
            tmplScroll.horizontal = false;

            var viewport = MakeRect(tmpl, "Viewport");
            SetStretch(viewport);
            viewport.gameObject.AddComponent<Image>();
            viewport.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            tmplScroll.viewport = viewport;

            var tmplContent = MakeRect(viewport, "Content");
            tmplContent.anchorMin = new Vector2(0f, 1f); tmplContent.anchorMax = Vector2.one;
            tmplContent.pivot = new Vector2(0.5f, 1f);
            tmplContent.sizeDelta = new Vector2(0f, 28f);
            tmplScroll.content = tmplContent;

            var item = MakeRect(tmplContent, "Item");
            item.anchorMin = new Vector2(0f, 0.5f); item.anchorMax = new Vector2(1f, 0.5f);
            item.sizeDelta = new Vector2(0f, 28f);
            item.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.3f, 1f);
            item.gameObject.AddComponent<Toggle>();

            var itemLabel = MakeRect(item, "Item Label");
            SetStretch(itemLabel);
            var itemLabelTMP = itemLabel.gameObject.AddComponent<TextMeshProUGUI>();
            itemLabelTMP.text      = "Option";
            itemLabelTMP.fontSize  = 18;
            itemLabelTMP.color     = Color.white;
            itemLabelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            dropdown.itemText     = itemLabelTMP;
            dropdown.template     = tmpl;
            tmpl.gameObject.SetActive(false);
            dropdown.AddOptions(new System.Collections.Generic.List<TMP_Dropdown.OptionData>
                { new TMP_Dropdown.OptionData("All Students") });

            // Data text (monospace scroll area)
            var scrollGO = MakeRect(dataPanel, "ScrollView");
            PositionRT(scrollGO, new Vector2(0f, 0.12f), new Vector2(1f, 0.77f));
            var scroll      = scrollGO.gameObject.AddComponent<ScrollRect>();
            var scrollBg    = scrollGO.gameObject.AddComponent<Image>();
            scrollBg.color  = new Color(0.02f, 0.02f, 0.06f, 1f);
            scroll.horizontal = false;

            var contentGO   = MakeRect(scrollGO, "Content");
            SetTopStretch(contentGO);
            var csf = contentGO.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content  = contentGO;

            var dataText    = contentGO.gameObject.AddComponent<TextMeshProUGUI>();
            dataText.fontSize    = 18;
            dataText.font        = GetMonoFont();
            dataText.color       = new Color(0.85f, 1f, 0.85f, 1f);
            dataText.textWrappingMode = TextWrappingModes.NoWrap;
            dataText.alignment   = TextAlignmentOptions.TopLeft;
            dataText.margin      = new Vector4(8, 8, 8, 8);

            // Bottom button row
            var btnRow = MakeRect(dataPanel, "ButtonRow");
            PositionRT(btnRow, new Vector2(0f, 0f), new Vector2(1f, 0.11f));

            var clearDataBtn = MakeLabelButton(btnRow, "ClearDataBtn", "Clear Data",
                new Vector2(0.02f, 0.1f), new Vector2(0.30f, 0.9f),
                new Color(0.55f, 0.1f, 0.1f, 1f));

            var backBtn = MakeLabelButton(btnRow, "BackBtn", "← Passcode",
                new Vector2(0.35f, 0.1f), new Vector2(0.63f, 0.9f),
                new Color(0.15f, 0.15f, 0.3f, 1f));

            var closeBtn = MakeLabelButton(btnRow, "CloseBtn", "✕ Close",
                new Vector2(0.68f, 0.1f), new Vector2(0.98f, 0.9f),
                new Color(0.18f, 0.45f, 0.18f, 1f));

            // ── Trigger button — top-right corner (standard button style) ────────
            var triggerGO = MakeRect(canvasGO.transform, "TeacherDashboardTrigger");
            var triggerRT = triggerGO;
            triggerRT.anchorMin        = new Vector2(1f, 1f);
            triggerRT.anchorMax        = new Vector2(1f, 1f);
            triggerRT.pivot            = new Vector2(1f, 1f);
            triggerRT.sizeDelta        = new Vector2(300f, 75f);
            triggerRT.anchoredPosition = new Vector2(-20f, -20f);

            // Own Canvas so it always renders on top regardless of sibling order
            var triggerCanvas             = triggerGO.gameObject.AddComponent<Canvas>();
            triggerCanvas.overrideSorting = true;
            triggerCanvas.sortingOrder    = 999;
            triggerGO.gameObject.AddComponent<GraphicRaycaster>();

            var triggerImg = triggerGO.gameObject.AddComponent<Image>();
            triggerImg.sprite                 = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            triggerImg.type                   = Image.Type.Sliced;
            triggerImg.pixelsPerUnitMultiplier = 0.1f;
            triggerImg.color                  = new Color(0.18f, 0.45f, 0.85f, 1f);
            triggerImg.raycastTarget          = true;
            var triggerBtn = triggerGO.gameObject.AddComponent<Button>();

            // Label
            var triggerLbl = new GameObject("Label", typeof(RectTransform));
            triggerLbl.transform.SetParent(triggerGO, false);
            var triggerLblRT = (RectTransform)triggerLbl.transform;
            triggerLblRT.anchorMin = Vector2.zero;
            triggerLblRT.anchorMax = Vector2.one;
            triggerLblRT.offsetMin = triggerLblRT.offsetMax = Vector2.zero;
            var triggerTMP = triggerLbl.AddComponent<TextMeshProUGUI>();
            triggerTMP.text      = "Teacher Code: 510";
            triggerTMP.fontSize  = 28;
            triggerTMP.fontStyle = FontStyles.Bold;
            triggerTMP.alignment = TextAlignmentOptions.Center;
            triggerTMP.color     = Color.white;

            AddShine(triggerGO);

            // Pulse + glow (standard-blue base → bright-blue glow)
            var pulse = triggerGO.gameObject.AddComponent<TeacherButtonPulse>();
            pulse.buttonImage = triggerImg;
            pulse.baseColour  = new Color(0.18f, 0.45f, 0.85f, 1f);
            pulse.glowColour  = new Color(0.35f, 0.65f, 1.00f, 1f);

            // Overlay stays on top of all panels
            overlay.SetAsLastSibling();

            // ── Wire TeacherDashboard component ────────────────────────────────
            var so = new SerializedObject(dashboard);
            var runnerAudio = runner.GetComponent<UnityEngine.AudioSource>();
            so.FindProperty("musicSource").objectReferenceValue      = runnerAudio;
            so.FindProperty("passcodePanel").objectReferenceValue    = pcPanel.gameObject;
            so.FindProperty("dataPanel").objectReferenceValue        = dataPanel.gameObject;
            so.FindProperty("passcodeDisplay").objectReferenceValue  = dotsGO;
            so.FindProperty("passcodeError").objectReferenceValue    = errorGO;
            so.FindProperty("studentDropdown").objectReferenceValue   = dropdown;
            so.FindProperty("dataText").objectReferenceValue          = dataText;
            so.FindProperty("statSessions").objectReferenceValue      = statValueTMPs[0];
            so.FindProperty("statStudents").objectReferenceValue      = statValueTMPs[1];
            so.FindProperty("statCompleted").objectReferenceValue     = statValueTMPs[2];
            so.FindProperty("statHints").objectReferenceValue         = statValueTMPs[3];
            so.FindProperty("clearDigitButton").objectReferenceValue = clrBtn;
            so.FindProperty("enterButton").objectReferenceValue      = enterBtn;
            so.FindProperty("backButton").objectReferenceValue       = backBtn.GetComponent<Button>();
            so.FindProperty("clearDataButton").objectReferenceValue  = clearDataBtn.GetComponent<Button>();
            so.FindProperty("closeButton").objectReferenceValue         = closeBtn.GetComponent<Button>();
            so.FindProperty("passcodeCloseButton").objectReferenceValue = pcCloseBtn;
            so.FindProperty("triggerButton").objectReferenceValue       = triggerBtn;

            // Wire digit buttons array
            var digitsProp = so.FindProperty("digitButtons");
            digitsProp.arraySize = 10;
            for (int i = 0; i < 10; i++)
                digitsProp.GetArrayElementAtIndex(i).objectReferenceValue = digitBtns[i];

            so.ApplyModifiedProperties();

            // Wire digit button OnClick callbacks
            for (int d = 0; d < 10; d++)
            {
                if (digitBtns[d] == null) continue;
                int captured = d;
                digitBtns[d].onClick.AddListener(() => dashboard.OnDigitPressed(captured));
            }
            clrBtn.onClick.AddListener(dashboard.OnClearDigit);
            enterBtn.onClick.AddListener(dashboard.OnEnter);
            backBtn.GetComponent<Button>().onClick.AddListener(dashboard.OnBack);
            clearDataBtn.GetComponent<Button>().onClick.AddListener(dashboard.OnClearData);
            closeBtn.GetComponent<Button>().onClick.AddListener(dashboard.OnClose);
            triggerBtn.onClick.AddListener(dashboard.Open);

            // ── Wire CaseRunner ────────────────────────────────────────────────
            var runnerSO = new SerializedObject(runner);
            runnerSO.FindProperty("teacherDashboard").objectReferenceValue       = dashboard;
            runnerSO.FindProperty("teacherDashboardTrigger").objectReferenceValue = triggerBtn;
            runnerSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Teacher Dashboard built. Press Ctrl+S to save.");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        static void DestroyIfExists(GameObject parent, string name)
        {
            var t = parent.transform.Find(name);
            if (t != null) Object.DestroyImmediate(t.gameObject);
        }

        static RectTransform MakeRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static void SetStretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void SetStretchWithPadding(RectTransform rt, float pad)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        static void SetCenter(RectTransform rt, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }

        static void SetTopStretch(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0, 1); rt.anchorMax = Vector2.one;
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void PositionRT(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min; rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static void PositionRT(Component c, Vector2 min, Vector2 max) =>
            PositionRT((RectTransform)c.transform, min, max);

        static void AddImage(RectTransform rt, Color color) =>
            rt.gameObject.AddComponent<Image>().color = color;

        static TextMeshProUGUI MakeTMP(RectTransform parent, string name, string text, float size, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text       = text;
            tmp.fontSize   = size;
            tmp.fontStyle  = style;
            tmp.alignment  = TextAlignmentOptions.Center;
            tmp.color      = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        static Button MakeNumpadButton(RectTransform parent, string name, string label, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite                  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type                    = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.1f;
            img.color = color;
            var btn = go.AddComponent<Button>();

            AddShine(go.transform);

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return btn;
        }

        static GameObject MakeLabelButton(RectTransform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite                  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type                    = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.1f;
            img.color = color;
            go.AddComponent<Button>();

            AddShine(go.transform);

            var lblGO = new GameObject("Label", typeof(RectTransform));
            lblGO.transform.SetParent(go.transform, false);
            var lblRT = (RectTransform)lblGO.transform;
            lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
            lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            return go;
        }

        static void AddShine(Transform parent)
        {
            var go = new GameObject("Shine", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.04f, 0.52f);
            rt.anchorMax = new Vector2(0.96f, 0.90f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite                  = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            img.type                    = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.1f;
            img.color                   = new Color(1f, 1f, 1f, 0.07f);
            img.raycastTarget           = false;
        }

        static TMP_FontAsset GetMonoFont()
        {
            var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return null;
        }
    }
}
