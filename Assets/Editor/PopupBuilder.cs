using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Popup Panel
    /// Creates a high-tech mobile-screen popup overlay wired to PopupController.
    /// Safe to re-run (destroys and recreates).
    ///
    /// Visual style: dark OLED phone screen, cyan/teal accent, monospace font.
    ///
    ///   PopupRoot    (full-screen frosted blocker)
    ///     PopupCard  (centred phone-screen card)
    ///       PhoneFrame      (rounded dark shell, accent border glow)
    ///       StatusBar       (top bar: signal dots + "DETECTIVE OS")
    ///       StatusRule      (thin cyan divider under status bar)
    ///       TitleText       (bold cyan, monospace, all-caps feel)
    ///       BodyText        (light grey, monospace, word-wrapped)
    ///       ConfirmButton   (bottom centre — solid cyan/teal pill)
    ///       CancelButton    (bottom left   — ghost outline, hidden by default)
    ///       DismissButton   (top-right X   — transparent, accent text)
    /// </summary>
    public static class PopupBuilder
    {
        // ── Tech palette ──────────────────────────────────────────────────────
        static readonly Color ScreenBg      = Hex("#0A0E14");   // deep OLED black
        static readonly Color FrameBorder   = Hex("#1B2A3A");   // dark navy frame
        static readonly Color AccentCyan    = Hex("#00D4FF");   // neon cyan — primary accent
        static readonly Color AccentTeal    = Hex("#00A8C8");   // teal — button bg
        static readonly Color AccentDim     = Hex("#005F7A");   // dim teal — cancel outline
        static readonly Color StatusBg      = Hex("#0D1620");   // slightly lighter than screen
        static readonly Color TextPrimary   = Hex("#00D4FF");   // cyan — titles
        static readonly Color TextBody      = Hex("#B0C8D8");   // pale blue-grey — body
        static readonly Color TextMuted     = Hex("#4A6A7A");   // muted — status bar
        static readonly Color BtnLabel      = Hex("#000D14");   // near-black on bright btn
        static readonly Color RuleGlow      = new Color(0f, 0.83f, 1f, 0.35f);
        static readonly Color BlockerColor  = new Color(0f, 0.04f, 0.08f, 0.82f);

        // ── Font ──────────────────────────────────────────────────────────────
        static TMP_FontAsset FindMonoFont()
        {
            string[] candidates = { "LiberationMono", "RobotoMono", "SourceCodePro",
                                    "Inconsolata", "Courier", "CourierNew", "Typewriter" };
            foreach (var name in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {name}");
                if (guids.Length > 0)
                {
                    var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (font != null) { Debug.Log($"  Mono font: {font.name}"); return font; }
                }
            }
            Debug.LogWarning("  No mono font found — assign one manually in the Inspector.");
            return null;
        }

        [MenuItem("ETEC510/Build Popup Panel")]
        public static void BuildPopup()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var existing = ct.Find("PopupRoot");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var monoFont = FindMonoFont();

            // ── Root & frosted blocker ─────────────────────────────────────────
            var root = NewStretch("PopupRoot", ct);
            root.SetAsLastSibling();

            var blockerImg = NewStretch("Blocker", root.transform).gameObject.AddComponent<Image>();
            blockerImg.color = BlockerColor;
            blockerImg.raycastTarget = true;

            // ── Phone card ────────────────────────────────────────────────────
            // Portrait shape: x 0.18-0.82, y 0.10-0.90
            var cardGO = new GameObject("PopupCard", typeof(RectTransform));
            cardGO.transform.SetParent(root.transform, false);
            Anch(cardGO, 0.18f, 0.10f, 0.82f, 0.90f);

            // Outer frame (accent border glow — navy)
            var frameImg = cardGO.AddComponent<Image>();
            frameImg.color = FrameBorder;
            frameImg.raycastTarget = false;

            // Screen fill (OLED black, inset 2px)
            var screen = Child("ScreenFill", cardGO.transform);
            var screenRT = screen.GetComponent<RectTransform>();
            screenRT.anchorMin = Vector2.zero; screenRT.anchorMax = Vector2.one;
            screenRT.offsetMin = new Vector2(2, 2); screenRT.offsetMax = new Vector2(-2, -2);
            var screenImg = screen.AddComponent<Image>();
            screenImg.color = ScreenBg;
            screenImg.raycastTarget = false;

            // Cyan border highlight (top edge only — 2px strip at very top)
            var topGlow = Child("TopGlow", cardGO.transform);
            Anch(topGlow, 0f, 0.975f, 1f, 1f);
            var topGlowImg = topGlow.AddComponent<Image>();
            topGlowImg.color = AccentCyan;
            topGlowImg.raycastTarget = false;

            // ── Status bar ────────────────────────────────────────────────────
            var statusBar = Child("StatusBar", cardGO.transform);
            Anch(statusBar, 0f, 0.86f, 1f, 0.975f);
            var statusBarImg = statusBar.AddComponent<Image>();
            statusBarImg.color = StatusBg;
            statusBarImg.raycastTarget = false;

            // "DETECTIVE OS" label centred in status bar
            var statusLabel = Child("StatusLabel", statusBar.transform);
            var slRT = statusLabel.GetComponent<RectTransform>();
            slRT.anchorMin = Vector2.zero; slRT.anchorMax = Vector2.one;
            slRT.offsetMin = Vector2.zero; slRT.offsetMax = Vector2.zero;
            var slTMP = statusLabel.AddComponent<TextMeshProUGUI>();
            slTMP.text            = "DETECTIVE OS  v2.1";
            slTMP.alignment       = TextAlignmentOptions.Center;
            slTMP.fontSize        = 16;
            slTMP.color           = TextMuted;
            slTMP.characterSpacing = 4f;
            if (monoFont != null) slTMP.font = monoFont;

            // Thin cyan rule below status bar
            var rule = Child("StatusRule", cardGO.transform);
            Anch(rule, 0.02f, 0.855f, 0.98f, 0.865f);
            var ruleImg = rule.AddComponent<Image>();
            ruleImg.color = RuleGlow;
            ruleImg.raycastTarget = false;

            // ── Title text ────────────────────────────────────────────────────
            var titleGO = Child("TitleText", cardGO.transform);
            Anch(titleGO, 0.04f, 0.70f, 0.84f, 0.855f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text           = "ALERT";
            titleTMP.alignment      = TextAlignmentOptions.MidlineLeft;
            titleTMP.fontSize       = 30;
            titleTMP.color          = TextPrimary;
            titleTMP.fontStyle      = FontStyles.Bold;
            titleTMP.characterSpacing = 6f;
            if (monoFont != null) titleTMP.font = monoFont;

            // Subtle horizontal rule below title
            var titleRule = Child("TitleRule", cardGO.transform);
            Anch(titleRule, 0.04f, 0.685f, 0.96f, 0.692f);
            var titleRuleImg = titleRule.AddComponent<Image>();
            titleRuleImg.color = new Color(0f, 0.83f, 1f, 0.18f);
            titleRuleImg.raycastTarget = false;

            // ── Body text ─────────────────────────────────────────────────────
            var bodyGO = Child("BodyText", cardGO.transform);
            Anch(bodyGO, 0.05f, 0.26f, 0.95f, 0.68f);
            var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyTMP.text             = "Body";
            bodyTMP.alignment        = TextAlignmentOptions.TopLeft;
            bodyTMP.fontSize         = 24;
            bodyTMP.color            = TextBody;
            bodyTMP.textWrappingMode = TextWrappingModes.Normal;
            bodyTMP.lineSpacing      = 8f;
            if (monoFont != null) bodyTMP.font = monoFont;

            // ── Confirm button — bottom centre, cyan pill ─────────────────────
            var confirmGO = MakeTechButton(cardGO.transform, "ConfirmButton",
                0.15f, 0.07f, 0.85f, 0.22f, "CONFIRM", monoFont, AccentTeal, BtnLabel);

            // ── Cancel button — bottom left, ghost outline, hidden ─────────────
            var cancelGO = MakeTechButton(cardGO.transform, "CancelButton",
                0.04f, 0.07f, 0.20f, 0.22f, "BACK", monoFont, AccentDim, TextBody);
            cancelGO.SetActive(false);

            // ── Dismiss X — top-right, transparent ────────────────────────────
            var dismissGO = Child("DismissButton", cardGO.transform);
            Anch(dismissGO, 0.85f, 0.865f, 0.98f, 0.975f);
            var dismissImg = dismissGO.AddComponent<Image>();
            dismissImg.color = Color.clear;
            dismissGO.AddComponent<Button>();

            var dLabel = Child("Label", dismissGO.transform);
            var dLabelRT = dLabel.GetComponent<RectTransform>();
            dLabelRT.anchorMin = Vector2.zero; dLabelRT.anchorMax = Vector2.one;
            dLabelRT.offsetMin = Vector2.zero; dLabelRT.offsetMax = Vector2.zero;
            var dTMP = dLabel.AddComponent<TextMeshProUGUI>();
            dTMP.text      = "X";
            dTMP.alignment = TextAlignmentOptions.Center;
            dTMP.fontSize  = 22;
            dTMP.color     = AccentCyan;
            dTMP.fontStyle = FontStyles.Bold;
            if (monoFont != null) dTMP.font = monoFont;

            // ── Wire PopupController ──────────────────────────────────────────
            var hostT = ct.Find("PopupController");
            if (hostT == null)
            {
                var pcGO = new GameObject("PopupController");
                pcGO.transform.SetParent(ct, false);
                hostT = pcGO.transform;
            }
            var ctrl = hostT.GetComponent<ETEC510.UI.PopupController>()
                    ?? hostT.gameObject.AddComponent<ETEC510.UI.PopupController>();

            var so = new SerializedObject(ctrl);
            so.FindProperty("popupRoot").objectReferenceValue     = root.gameObject;
            so.FindProperty("popupCard").objectReferenceValue     = cardGO;
            so.FindProperty("titleText").objectReferenceValue     = titleTMP;
            so.FindProperty("bodyText").objectReferenceValue      = bodyTMP;
            so.FindProperty("confirmButton").objectReferenceValue  = confirmGO.GetComponent<Button>();
            so.FindProperty("confirmLabel").objectReferenceValue   =
                confirmGO.transform.Find("Label").GetComponent<TMP_Text>();
            so.FindProperty("cancelButton").objectReferenceValue   = cancelGO.GetComponent<Button>();
            so.FindProperty("cancelLabel").objectReferenceValue    =
                cancelGO.transform.Find("Label").GetComponent<TMP_Text>();
            so.FindProperty("dismissButton").objectReferenceValue  = dismissGO.GetComponent<Button>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            Debug.Log("  PopupController wired.");

            root.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Tech popup built. Press Ctrl+S to save.");
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

        static GameObject Child(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Anch(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static GameObject MakeTechButton(Transform parent, string name,
            float x0, float y0, float x1, float y1,
            string label, TMP_FontAsset font, Color bgColor, Color labelColor)
        {
            var go = Child(name, parent);
            Anch(go, x0, y0, x1, y1);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            go.AddComponent<Button>();

            var lGO = Child("Label", go.transform);
            var lRT = lGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;

            var tmp = lGO.AddComponent<TextMeshProUGUI>();
            tmp.text             = label;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.fontSize         = 22;
            tmp.color            = labelColor;
            tmp.fontStyle        = FontStyles.Bold;
            tmp.characterSpacing = 5f;
            if (font != null) tmp.font = font;

            return go;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
