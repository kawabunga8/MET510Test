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
    /// Creates a high-tech mobile-device popup overlay wired to PopupController.
    /// Safe to re-run (destroys and recreates).
    ///
    /// Visual style: phone frame with rounded corners, OLED screen, cyan accent.
    ///
    ///   PopupRoot
    ///     Blocker          (dark frosted overlay)
    ///     PopupCard        (phone frame — rounded, dark bezel)
    ///       ScreenFill     (OLED black screen, inset)
    ///       TopGlow        (thin cyan top edge)
    ///       Notch          (camera cutout pill)
    ///       StatusBar      (DETECTIVE OS label)
    ///       StatusRule     (cyan separator)
    ///       TitleText      (cyan, bold mono)
    ///       TitleRule      (subtle cyan rule)
    ///       BodyText       (pale blue-grey, mono, wrapped)
    ///       ConfirmButton  (cyan pill, bottom centre)
    ///       CancelButton   (dim ghost, bottom left, hidden)
    ///       HomeBar        (thin pill at very bottom — home indicator)
    ///       DismissButton  (X top-right, cyan)
    /// </summary>
    public static class PopupBuilder
    {
        // ── Tech palette ──────────────────────────────────────────────────────
        static readonly Color ScreenBg     = Hex("#080C12");
        static readonly Color BezelColor   = Hex("#111820");
        static readonly Color AccentCyan   = Hex("#00D4FF");
        static readonly Color AccentTeal   = Hex("#00A8C8");
        static readonly Color AccentDim    = Hex("#004F66");
        static readonly Color StatusBg     = Hex("#0C1520");
        static readonly Color TextPrimary  = Hex("#00D4FF");
        static readonly Color TextBody     = Hex("#A8C4D4");
        static readonly Color TextMuted    = Hex("#3A5A6A");
        static readonly Color BtnLabel     = Hex("#000D14");
        static readonly Color RuleGlow     = new Color(0f, 0.83f, 1f, 0.30f);
        static readonly Color BlockerColor = new Color(0f, 0.03f, 0.07f, 0.88f);
        static readonly Color NotchColor   = Hex("#050810");
        static readonly Color HomeBar      = new Color(0f, 0.83f, 1f, 0.45f);

        // ── Font ──────────────────────────────────────────────────────────────
        static TMP_FontAsset FindMonoFont()
        {
            string[] candidates = { "LiberationMono", "RobotoMono", "SourceCodePro",
                                    "Inconsolata", "Courier", "CourierNew", "Typewriter" };
            foreach (var n in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {n}");
                if (guids.Length > 0)
                {
                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (f != null) { Debug.Log($"  Mono font: {f.name}"); return f; }
                }
            }
            Debug.LogWarning("  No mono font found — assign manually in Inspector.");
            return null;
        }

        // Built-in rounded-rect sliced sprite
        static Sprite RoundedSprite() =>
            AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

        [MenuItem("ETEC510/Build Popup Panel")]
        public static void BuildPopup()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var existing = ct.Find("PopupRoot");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var mono = FindMonoFont();
            var rounded = RoundedSprite();

            // ── Root & frosted blocker ─────────────────────────────────────────
            var root = NewStretch("PopupRoot", ct);
            root.SetAsLastSibling();
            DetectiveStyleGuide.ApplySavedRect(root, "", saved);

            var blockerRT = NewStretch("Blocker", root.transform);
            DetectiveStyleGuide.ApplySavedRect(blockerRT, "Blocker", saved);
            var blockerImg = blockerRT.gameObject.AddComponent<Image>();
            blockerImg.color = BlockerColor;
            blockerImg.raycastTarget = true;

            // ── Phone bezel (outer card) — rounded corners ─────────────────────
            // Portrait phone proportions: x 0.25-0.75, y 0.06-0.94
            var cardGO = new GameObject("PopupCard", typeof(RectTransform));
            cardGO.transform.SetParent(root.transform, false);
            Anch(cardGO, 0.25f, 0.06f, 0.75f, 0.94f);
            DetectiveStyleGuide.ApplySavedRect((RectTransform)cardGO.transform, "PopupCard", saved);

            var bezelImg = cardGO.AddComponent<Image>();
            bezelImg.color       = BezelColor;
            bezelImg.sprite      = rounded;
            bezelImg.type        = Image.Type.Sliced;
            bezelImg.pixelsPerUnitMultiplier = 0.08f;   // very low = very round corners
            bezelImg.raycastTarget = false;

            // Thin cyan accent ring on top half of bezel edge
            var accentRing = Child("AccentRing", cardGO.transform);
            var arRT = accentRing.GetComponent<RectTransform>();
            arRT.anchorMin = Vector2.zero; arRT.anchorMax = Vector2.one;
            arRT.offsetMin = new Vector2(1, 1); arRT.offsetMax = new Vector2(-1, -1);
            var arImg = accentRing.AddComponent<Image>();
            arImg.color       = new Color(0f, 0.83f, 1f, 0.12f);
            arImg.sprite      = rounded;
            arImg.type        = Image.Type.Sliced;
            arImg.pixelsPerUnitMultiplier = 0.08f;
            arImg.raycastTarget = false;

            // OLED screen fill — inset 6px, same rounded corners
            var screen = Child("ScreenFill", cardGO.transform);
            var screenRT = screen.GetComponent<RectTransform>();
            screenRT.anchorMin = Vector2.zero; screenRT.anchorMax = Vector2.one;
            screenRT.offsetMin = new Vector2(6, 6); screenRT.offsetMax = new Vector2(-6, -6);
            var screenImg = screen.AddComponent<Image>();
            screenImg.color       = ScreenBg;
            screenImg.sprite      = rounded;
            screenImg.type        = Image.Type.Sliced;
            screenImg.pixelsPerUnitMultiplier = 0.08f;
            screenImg.raycastTarget = false;

            // ── Notch (pill camera cutout centred at top) ──────────────────────
            var notch = Child("Notch", cardGO.transform);
            Anch(notch, 0.35f, 0.915f, 0.65f, 0.955f);
            var notchImg = notch.AddComponent<Image>();
            notchImg.color       = NotchColor;
            notchImg.sprite      = rounded;
            notchImg.type        = Image.Type.Sliced;
            notchImg.pixelsPerUnitMultiplier = 0.15f;
            notchImg.raycastTarget = false;

            // ── Status bar ────────────────────────────────────────────────────
            var statusBar = Child("StatusBar", cardGO.transform);
            Anch(statusBar, 0.03f, 0.855f, 0.97f, 0.912f);
            var statusBarImg = statusBar.AddComponent<Image>();
            statusBarImg.color = Color.clear;
            statusBarImg.raycastTarget = false;

            var slGO = Child("StatusLabel", statusBar.transform);
            var slRT = slGO.GetComponent<RectTransform>();
            slRT.anchorMin = Vector2.zero; slRT.anchorMax = Vector2.one;
            slRT.offsetMin = Vector2.zero; slRT.offsetMax = Vector2.zero;
            var slTMP = slGO.AddComponent<TextMeshProUGUI>();
            slTMP.text             = "DETECTIVE OS  v2.1";
            slTMP.alignment        = TextAlignmentOptions.Center;
            slTMP.fontSize         = 14;
            slTMP.color            = TextMuted;
            slTMP.characterSpacing = 4f;
            if (mono != null) slTMP.font = mono;

            // Cyan rule under status bar
            var statusRule = Child("StatusRule", cardGO.transform);
            Anch(statusRule, 0.05f, 0.848f, 0.95f, 0.856f);
            var srImg = statusRule.AddComponent<Image>();
            srImg.color = RuleGlow;
            srImg.raycastTarget = false;

            // ── Title text ────────────────────────────────────────────────────
            var titleGO = Child("TitleText", cardGO.transform);
            Anch(titleGO, 0.06f, 0.72f, 0.86f, 0.845f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text             = "ALERT";
            titleTMP.alignment        = TextAlignmentOptions.MidlineLeft;
            titleTMP.fontSize         = 28;
            titleTMP.color            = TextPrimary;
            titleTMP.fontStyle        = FontStyles.Bold;
            titleTMP.characterSpacing = 5f;
            if (mono != null) titleTMP.font = mono;

            // Subtle rule below title
            var titleRule = Child("TitleRule", cardGO.transform);
            Anch(titleRule, 0.06f, 0.714f, 0.94f, 0.720f);
            var trImg = titleRule.AddComponent<Image>();
            trImg.color = new Color(0f, 0.83f, 1f, 0.15f);
            trImg.raycastTarget = false;

            // ── Body text ─────────────────────────────────────────────────────
            var bodyGO = Child("BodyText", cardGO.transform);
            Anch(bodyGO, 0.06f, 0.30f, 0.94f, 0.710f);
            var bodyTMP = bodyGO.AddComponent<TextMeshProUGUI>();
            bodyTMP.text             = "Body";
            bodyTMP.alignment        = TextAlignmentOptions.TopLeft;
            bodyTMP.fontSize         = 22;
            bodyTMP.color            = TextBody;
            bodyTMP.textWrappingMode = TextWrappingModes.Normal;
            bodyTMP.lineSpacing      = 8f;
            if (mono != null) bodyTMP.font = mono;

            // ── Confirm button — cyan pill, bottom centre ──────────────────────
            var confirmGO = MakeTechButton(cardGO.transform, "ConfirmButton",
                0.10f, 0.10f, 0.90f, 0.24f, "CONFIRM", mono,
                AccentTeal, BtnLabel, rounded);

            // ── Cancel button — ghost, bottom left, hidden ─────────────────────
            var cancelGO = MakeTechButton(cardGO.transform, "CancelButton",
                0.05f, 0.10f, 0.38f, 0.24f, "BACK", mono,
                AccentDim, TextBody, rounded);
            cancelGO.SetActive(false);

            // ── Home indicator pill at very bottom ─────────────────────────────
            var home = Child("HomeBar", cardGO.transform);
            Anch(home, 0.30f, 0.025f, 0.70f, 0.048f);
            var homeImg = home.AddComponent<Image>();
            homeImg.color       = HomeBar;
            homeImg.sprite      = rounded;
            homeImg.type        = Image.Type.Sliced;
            homeImg.pixelsPerUnitMultiplier = 0.1f;
            homeImg.raycastTarget = false;

            // ── Dismiss X — top-right of screen ───────────────────────────────
            var dismissGO = Child("DismissButton", cardGO.transform);
            Anch(dismissGO, 0.84f, 0.855f, 0.97f, 0.955f);
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
            dTMP.fontSize  = 20;
            dTMP.color     = AccentCyan;
            dTMP.fontStyle = FontStyles.Bold;
            if (mono != null) dTMP.font = mono;

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
            so.FindProperty("popupRoot").objectReferenceValue    = root.gameObject;
            so.FindProperty("popupCard").objectReferenceValue    = cardGO;
            so.FindProperty("titleText").objectReferenceValue    = titleTMP;
            so.FindProperty("bodyText").objectReferenceValue     = bodyTMP;
            so.FindProperty("confirmButton").objectReferenceValue = confirmGO.GetComponent<Button>();
            so.FindProperty("confirmLabel").objectReferenceValue  =
                confirmGO.transform.Find("Label").GetComponent<TMP_Text>();
            so.FindProperty("cancelButton").objectReferenceValue  = cancelGO.GetComponent<Button>();
            so.FindProperty("cancelLabel").objectReferenceValue   =
                cancelGO.transform.Find("Label").GetComponent<TMP_Text>();
            so.FindProperty("dismissButton").objectReferenceValue = dismissGO.GetComponent<Button>();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            Debug.Log("  PopupController wired.");

            root.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Mobile-device popup built. Press Ctrl+S to save.");
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
            string label, TMP_FontAsset font, Color bgColor, Color labelColor, Sprite rounded)
        {
            var go = Child(name, parent);
            Anch(go, x0, y0, x1, y1);

            var img = go.AddComponent<Image>();
            img.color       = bgColor;
            img.sprite      = rounded;
            img.type        = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.12f;   // pill-shaped buttons

            go.AddComponent<Button>();

            var lGO = Child("Label", go.transform);
            var lRT = lGO.GetComponent<RectTransform>();
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;

            var tmp = lGO.AddComponent<TextMeshProUGUI>();
            tmp.text             = label;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.fontSize         = 20;
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
