using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// MET510 Digital Detective Toolkit — Visual Style Guide
    ///
    /// Single source of truth for all colours, typography, and UI helper methods
    /// used by every editor builder script.
    ///
    /// COLOUR PALETTE
    /// ──────────────────────────────────────────────────────────────────────────
    ///  Background:
    ///    BgDeep       #0D0D12   near-black — scene background
    ///    BgPanel      #14120E   dark sepia — panel fills
    ///    BgOverlay    #000000@72%  semi-opaque overlay strip
    ///
    ///  Accent:
    ///    Gold         #B8913A   aged gold — headings, badge text, borders
    ///    GoldDim      #7A5E22   muted gold — secondary accent
    ///
    ///  Text:
    ///    Parchment    #EBE0B8   warm off-white — titles
    ///    TextLight    #E8E8E8   cool white — body / question text
    ///    TextMuted    #9A9080   greyed — hints, disabled state
    ///
    ///  Buttons (background colour):
    ///    BtnPrimary   #2B2010   dark amber — main action (Start, Submit)
    ///    BtnConfirm   #1B3016   dark forest green — affirmative choice
    ///    BtnAlternate #1A1016   dark plum — negative / alternate choice
    ///    BtnNav       #12141E   dark slate-blue — Back / Return navigation
    ///    BtnDanger    #2E0F0F   dark crimson — Restart / destructive
    ///    BtnWarn      #2A1A08   dark amber-brown — Try Again
    ///
    ///  Buttons (label colour):
    ///    LabelGold    #D4AF5A   bright gold — on dark amber/primary buttons
    ///    LabelGreen   #8FD48A   light green — on confirm buttons
    ///    LabelLight   #E8E8E8   cool white — on nav / warn buttons
    ///    LabelCrimson #E88A8A   light red — on danger buttons
    ///
    /// BUTTON ROLES
    /// ──────────────────────────────────────────────────────────────────────────
    ///  Primary   — main story-advance action (Start Investigation, Submit)
    ///  Confirm   — affirmative choice (Sound On, Yes)
    ///  Alternate — negative / secondary choice (Sound Off, No)
    ///  Choice    — multiple-choice answers (OptionButton0/1, GutCheck, Verdict)
    ///  Nav       — panel navigation (Back to Evidence, Return to Evidence)
    ///  Danger    — destructive / reset (Restart)
    ///  Warn      — retry prompt (Try Again)
    /// </summary>
    public static class DetectiveStyleGuide
    {
        // ── Colours ───────────────────────────────────────────────────────────

        public static readonly Color BgDeep        = Hex("#0D0D12");
        public static readonly Color BgPanel       = Hex("#14120E");
        public static readonly Color BgOverlay     = new Color(0f, 0f, 0f, 0.72f);

        public static readonly Color Gold          = Hex("#B8913A");
        public static readonly Color GoldDim       = Hex("#7A5E22");
        public static readonly Color GoldLabel     = Hex("#D4AF5A");

        public static readonly Color Parchment     = Hex("#EBE0B8");
        public static readonly Color TextLight     = Hex("#E8E8E8");
        public static readonly Color TextMuted     = Hex("#9A9080");

        // Button backgrounds
        public static readonly Color BtnPrimary    = Hex("#1A6BC8");
        public static readonly Color BtnConfirm    = Hex("#1B3016");
        public static readonly Color BtnAlternate  = Hex("#1A1016");
        public static readonly Color BtnChoice     = Hex("#1A6BC8");
        public static readonly Color BtnNav        = Hex("#1A6BC8");
        public static readonly Color BtnDanger     = Hex("#2E0F0F");
        public static readonly Color BtnWarn       = Hex("#2A1A08");
        public static readonly Color BtnTool       = Hex("#1A3A30"); // dark teal — investigation tool buttons

        // Button label colours
        public static readonly Color LabelGold     = Hex("#D4AF5A");
        public static readonly Color LabelGreen    = Hex("#8FD48A");
        public static readonly Color LabelLight    = Hex("#E8E8E8");
        public static readonly Color LabelCrimson  = Hex("#E88A8A");
        public static readonly Color LabelAmber    = Hex("#E8C07A");
        public static readonly Color LabelTeal     = Hex("#7ADCC8"); // cyan-teal — on tool buttons

        // ── Button roles ──────────────────────────────────────────────────────

        public enum ButtonRole { Primary, Confirm, Alternate, Choice, Nav, Danger, Warn, Tool }

        /// <summary>Returns (bgColor, labelColor) for the given role.</summary>
        public static (Color bg, Color label) ButtonColors(ButtonRole role) => role switch
        {
            ButtonRole.Primary   => (BtnPrimary,   LabelLight),
            ButtonRole.Confirm   => (BtnConfirm,   LabelGreen),
            ButtonRole.Alternate => (BtnAlternate, LabelLight),
            ButtonRole.Choice    => (BtnChoice,    LabelLight),
            ButtonRole.Nav       => (BtnNav,       LabelLight),
            ButtonRole.Danger    => (BtnDanger,    LabelCrimson),
            ButtonRole.Warn      => (BtnWarn,      LabelAmber),
            ButtonRole.Tool      => (BtnTool,      LabelTeal),
            _                    => (BtnNav,       LabelLight),
        };

        // ── Typography ────────────────────────────────────────────────────────

        public enum TextRole { Badge, Title, Question, Body, ButtonLabel, Muted }

        public static void ApplyTextStyle(TextMeshProUGUI t, TextRole role)
        {
            switch (role)
            {
                case TextRole.Badge:
                    t.fontSize         = 22f;
                    t.color            = Gold;
                    t.fontStyle        = FontStyles.Bold | FontStyles.UpperCase;
                    t.characterSpacing = 6f;
                    t.alignment        = TextAlignmentOptions.Center;
                    break;
                case TextRole.Title:
                    t.fontSize  = 56f;
                    t.color     = Parchment;
                    t.fontStyle = FontStyles.Bold | FontStyles.Italic;
                    t.alignment = TextAlignmentOptions.Center;
                    break;
                case TextRole.Question:
                    t.fontSize  = 46f;
                    t.color     = TextLight;
                    t.fontStyle = FontStyles.Normal;
                    t.alignment = TextAlignmentOptions.Center;
                    break;
                case TextRole.Body:
                    t.fontSize  = 28f;
                    t.color     = TextLight;
                    t.fontStyle = FontStyles.Normal;
                    t.alignment = TextAlignmentOptions.Left;
                    break;
                case TextRole.ButtonLabel:
                    t.fontSize         = 32f;
                    t.fontStyle        = FontStyles.Bold;
                    t.characterSpacing = 2f;
                    t.alignment        = TextAlignmentOptions.Center;
                    break;
                case TextRole.Muted:
                    t.fontSize  = 24f;
                    t.color     = TextMuted;
                    t.fontStyle = FontStyles.Normal;
                    t.alignment = TextAlignmentOptions.Center;
                    break;
            }
        }

        // ── Button helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Fully styles a Button GameObject:
        ///   - Rounded corners via Unity's built-in sliced UISprite
        ///   - Gold border rim (outer Image = GoldDim, inner BgFill = role bg colour)
        ///   - Subtle white highlight sheen on top half
        ///   - Drop shadow for lift
        ///   - Label text styled as ButtonLabel with correct role colour
        ///
        /// Expected child hierarchy (created if missing):
        ///   ButtonGO  (Image:gold border + Button + Shadow)
        ///     BgFill  (Image:dark bg, inset 3 px)
        ///     Shine   (Image:white @ 6%, top 45 %)
        ///     Label   (TextMeshProUGUI)
        /// </summary>
        public static void StyleButton(GameObject go, ButtonRole role)
        {
            if (go == null) return;
            var (bg, labelCol) = ButtonColors(role);

            var rounded = UnityEditor.AssetDatabase
                .GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Outer ring = gold border ──────────────────────────────────────
            var outerImg = go.GetComponent<Image>();
            if (outerImg != null)
            {
                if (rounded != null) { outerImg.sprite = rounded; outerImg.type = Image.Type.Sliced; outerImg.pixelsPerUnitMultiplier = 0.10f; }
                outerImg.color        = GoldDim;
                outerImg.raycastTarget = true;
            }

            // ── BgFill — dark bg, inset 3 px (leaves gold rim visible) ────────
            var bgFillT = go.transform.Find("BgFill");
            if (bgFillT == null)
            {
                var bgGO = new GameObject("BgFill", typeof(RectTransform));
                bgGO.transform.SetParent(go.transform, false);
                bgFillT = bgGO.transform;
            }
            bgFillT.SetSiblingIndex(0);
            var brt = (RectTransform)bgFillT;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(3, 3); brt.offsetMax = new Vector2(-3, -3);
            var bgFillImg = bgFillT.GetComponent<Image>() ?? bgFillT.gameObject.AddComponent<Image>();
            if (rounded != null) { bgFillImg.sprite = rounded; bgFillImg.type = Image.Type.Sliced; bgFillImg.pixelsPerUnitMultiplier = 0.1f; }
            bgFillImg.color         = new Color(bg.r, bg.g, bg.b, 0.6f);
            bgFillImg.raycastTarget = false;

            // ── Shine — white sheen on upper half (simulates surface catch-light) ─
            var shineT = go.transform.Find("Shine");
            if (shineT == null)
            {
                var shGO = new GameObject("Shine", typeof(RectTransform));
                shGO.transform.SetParent(go.transform, false);
                shineT = shGO.transform;
            }
            shineT.SetSiblingIndex(1);
            var srt = (RectTransform)shineT;
            srt.anchorMin = new Vector2(0.04f, 0.52f); srt.anchorMax = new Vector2(0.96f, 0.90f);
            srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;
            var shineImg = shineT.GetComponent<Image>() ?? shineT.gameObject.AddComponent<Image>();
            if (rounded != null) { shineImg.sprite = rounded; shineImg.type = Image.Type.Sliced; shineImg.pixelsPerUnitMultiplier = 0.1f; }
            shineImg.color         = new Color(1f, 1f, 1f, 0.07f);
            shineImg.raycastTarget = false;

            // ── Drop shadow ───────────────────────────────────────────────────
            var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0f, 0f, 0f, 0.65f);
            shadow.effectDistance = new Vector2(3f, -3f);

            // ── Label — move to front, apply text style ───────────────────────
            var labelT = go.transform.Find("Label");
            if (labelT != null)
            {
                labelT.SetAsLastSibling();
                var tmp = labelT.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { ApplyTextStyle(tmp, TextRole.ButtonLabel); tmp.color = labelCol; }
            }
            else
            {
                // Fallback: any TMP_Text child
                var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) { ApplyTextStyle(tmp, TextRole.ButtonLabel); tmp.color = labelCol; }
            }
        }

        /// <summary>
        /// Infers a ButtonRole from a GameObject name using naming conventions.
        ///   *BackButton / *ReturnButton  → Nav
        ///   RestartButton                → Danger
        ///   *TryAgainButton              → Warn
        ///   *OnButton / StartButton / SubmitButton → Primary/Confirm
        ///   *OffButton                   → Alternate
        ///   *OptionButton* / *VerdictOption* / GutOption* → Choice
        ///   (fallback)                   → Primary
        /// </summary>
        public static ButtonRole InferRole(string name)
        {
            var n = name.ToLowerInvariant();
            if (n.Contains("back")    || n.Contains("return")) return ButtonRole.Nav;
            if (n.Contains("restart"))                          return ButtonRole.Danger;
            if (n.Contains("tryagain"))                         return ButtonRole.Warn;
            if (n.Contains("off")     || n.Contains("silence")) return ButtonRole.Alternate;
            if (n.Contains("option")  || n.Contains("verdict") ||
                n.Contains("gutopt")  || n.Contains("choice"))  return ButtonRole.Choice;
            if (n.EndsWith("onbutton") || n.Contains("soundon")) return ButtonRole.Confirm;
            if (n.Contains("start")   || n.Contains("submit")  ||
                n.Contains("enter"))                             return ButtonRole.Primary;
            return ButtonRole.Primary;
        }

        // ── Utility ───────────────────────────────────────────────────────────

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }

        // ── Rect persistence helpers ──────────────────────────────────────────

        /// <summary>
        /// Snapshots the RectTransform of <paramref name="root"/> (key "") and
        /// all direct children (key = child.name) so they can be restored after
        /// the panel is destroyed and recreated.
        /// </summary>
        public static Dictionary<string, (Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)>
            SaveChildRects(Transform root)
        {
            var d = new Dictionary<string, (Vector2, Vector2, Vector2, Vector2)>();
            if (root == null) return d;
            var rt = root.GetComponent<RectTransform>();
            if (rt != null) d[""] = (rt.anchorMin, rt.anchorMax, rt.offsetMin, rt.offsetMax);
            foreach (Transform child in root)
            {
                var crt = child.GetComponent<RectTransform>();
                if (crt != null) d[child.name] = (crt.anchorMin, crt.anchorMax, crt.offsetMin, crt.offsetMax);
            }
            return d;
        }

        /// <summary>
        /// Applies a previously saved rect to <paramref name="rt"/> if <paramref name="key"/>
        /// exists in <paramref name="saved"/>. No-op when the key is absent.
        /// </summary>
        public static void ApplySavedRect(
            RectTransform rt, string key,
            Dictionary<string, (Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)> saved)
        {
            if (rt == null || saved == null || !saved.TryGetValue(key, out var v)) return;
            rt.anchorMin = v.anchorMin;
            rt.anchorMax = v.anchorMax;
            rt.offsetMin = v.offsetMin;
            rt.offsetMax = v.offsetMax;
        }
    }
}
