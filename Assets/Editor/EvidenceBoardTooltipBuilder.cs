using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Evidence Board Tooltips
    /// Adds a '?' button beside each path button on the EvidenceBoardPanel.
    /// Hovering shows a tooltip panel explaining what each path is about.
    /// Also creates the TooltipController panel on GameCanvas.
    /// Safe to re-run.
    /// </summary>
    public static class EvidenceBoardTooltipBuilder
    {
        // ── Palette (matches tech popup style) ────────────────────────────────
        static readonly Color TooltipBg     = new Color(0.04f, 0.08f, 0.13f, 0.96f);
        static readonly Color TooltipBorder = new Color(0f, 0.83f, 1f, 0.55f);
        static readonly Color TooltipText   = new Color(0.67f, 0.77f, 0.83f, 1f);
        static readonly Color QuestionBg    = new Color(0f, 0.55f, 0.72f, 0.25f);
        static readonly Color QuestionLabel = new Color(0f, 0.83f, 1f, 1f);

        static readonly (string path, string tip)[] PathTips =
        {
            ("EvidenceBoardPanel/SpotTheClueButton",
             "SPOT THE CLUE\nIdentify visual anomalies and inconsistencies hidden in the evidence."),

            ("EvidenceBoardPanel/GutCheckButton",
             "GUT CHECK\nTrust your instincts — does the story add up? Look for what feels off."),

            ("EvidenceBoardPanel/FindTheMotiveButton",
             "FIND THE MOTIVE\nWho benefits from this situation? Follow the incentives."),

            ("EvidenceBoardPanel/EnterPasswordButton",
             "UNLOCK VERDICT\nEnter the access code to submit your final case verdict."),
        };

        [MenuItem("ETEC510/Build Evidence Board Tooltips")]
        public static void BuildTooltips()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // ── Build / rebuild tooltip panel ─────────────────────────────────
            var existingPanel = ct.Find("TooltipPanel");
            var savedTooltip = DetectiveStyleGuide.SaveChildRects(existingPanel);
            if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

            var tooltipPanel = BuildTooltipPanel(ct, out var tooltipTMP, savedTooltip);

            // ── Create / update TooltipController ─────────────────────────────
            var tcHost = ct.Find("TooltipController");
            if (tcHost == null)
            {
                var go = new GameObject("TooltipController");
                go.transform.SetParent(ct, false);
                tcHost = go.transform;
            }
            var ctrl = tcHost.GetComponent<ETEC510.UI.TooltipController>()
                    ?? tcHost.gameObject.AddComponent<ETEC510.UI.TooltipController>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("tooltipPanel").objectReferenceValue = tooltipPanel;
            so.FindProperty("tooltipText").objectReferenceValue  = tooltipTMP;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);

            // ── Add '?' triggers to each path button ──────────────────────────
            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            foreach (var (path, tip) in PathTips)
            {
                var btnT = ct.Find(path);
                if (btnT == null) { Debug.LogWarning($"ETEC510: not found — {path}"); continue; }

                // Remove old '?' if present
                var oldQ = btnT.Find("TooltipQ");
                if (oldQ != null) Object.DestroyImmediate(oldQ.gameObject);

                // Create '?' badge anchored to top-right corner of the button
                var qGO = new GameObject("TooltipQ", typeof(RectTransform));
                qGO.transform.SetParent(btnT, false);

                var qRT = (RectTransform)qGO.transform;
                // Small badge, top-right corner, partially outside button
                qRT.anchorMin = new Vector2(1f, 1f);
                qRT.anchorMax = new Vector2(1f, 1f);
                qRT.pivot     = new Vector2(0.5f, 0.5f);
                qRT.sizeDelta = new Vector2(32, 32);
                qRT.anchoredPosition = new Vector2(-8, -8);

                // Circular bg
                var qImg = qGO.AddComponent<Image>();
                qImg.color  = QuestionBg;
                qImg.sprite = rounded;
                qImg.type   = Image.Type.Sliced;
                qImg.pixelsPerUnitMultiplier = 0.08f;

                qGO.AddComponent<Button>(); // makes it raycast-target

                // '?' label
                var lGO = new GameObject("Label", typeof(RectTransform));
                lGO.transform.SetParent(qGO.transform, false);
                var lRT = (RectTransform)lGO.transform;
                lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
                lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;

                var lTMP = lGO.AddComponent<TextMeshProUGUI>();
                lTMP.text      = "?";
                lTMP.alignment = TextAlignmentOptions.Center;
                lTMP.fontSize  = 18;
                lTMP.color     = QuestionLabel;
                lTMP.fontStyle = FontStyles.Bold;

                // TooltipTrigger — use reflection to avoid editor-assembly compile dependency
                var triggerType = typeof(ETEC510.UI.TooltipController).Assembly
                    .GetType("ETEC510.UI.TooltipTrigger");
                if (triggerType != null)
                {
                    var trigger = qGO.AddComponent(triggerType);
                    var soTrigger = new SerializedObject(trigger);
                    soTrigger.FindProperty("tooltipText").stringValue = tip;
                    soTrigger.ApplyModifiedProperties();
                }
                else
                {
                    Debug.LogWarning("  TooltipTrigger type not found — add it manually.");
                }

                EditorUtility.SetDirty(qGO);
                Debug.Log($"  '?' added to {path}");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Evidence board tooltips built. Press Ctrl+S to save.");
        }

        static RectTransform BuildTooltipPanel(Transform ct, out TMP_Text textOut,
            System.Collections.Generic.Dictionary<string, (Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)> saved = null)
        {
            var panel = new GameObject("TooltipPanel", typeof(RectTransform));
            panel.transform.SetParent(ct, false);

            var panelRT = (RectTransform)panel.transform;
            // Pivot bottom-centre so it sits above whatever it's anchored to
            panelRT.pivot     = new Vector2(0.5f, 0f);
            panelRT.anchorMin = new Vector2(0f, 0f);
            panelRT.anchorMax = new Vector2(0f, 0f);
            panelRT.sizeDelta = new Vector2(320, 90);
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);

            // Border layer
            var borderImg = panel.AddComponent<Image>();
            borderImg.color       = TooltipBorder;
            borderImg.sprite      = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            borderImg.type        = Image.Type.Sliced;
            borderImg.pixelsPerUnitMultiplier = 0.18f;
            borderImg.raycastTarget = false;

            // Background fill (inset 2px)
            var bg = new GameObject("Bg", typeof(RectTransform));
            bg.transform.SetParent(panel.transform, false);
            var bgRT = (RectTransform)bg.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = new Vector2(2, 2); bgRT.offsetMax = new Vector2(-2, -2);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color       = TooltipBg;
            bgImg.sprite      = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImg.type        = Image.Type.Sliced;
            bgImg.pixelsPerUnitMultiplier = 0.18f;
            bgImg.raycastTarget = false;

            // Text
            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(panel.transform, false);
            var textRT = (RectTransform)textGO.transform;
            textRT.anchorMin = Vector2.zero; textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 8); textRT.offsetMax = new Vector2(-10, -8);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text             = "";
            tmp.fontSize         = 18;
            tmp.color            = TooltipText;
            tmp.alignment        = TextAlignmentOptions.MidlineLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.lineSpacing      = 4f;

            // Try to find a mono font
            var guids = AssetDatabase.FindAssets("t:TMP_FontAsset LiberationMono");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:TMP_FontAsset RobotoMono");
            if (guids.Length > 0)
            {
                var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
                if (f != null) tmp.font = f;
            }

            panel.SetActive(false);
            textOut = tmp;
            return panelRT;
        }
    }
}
