using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Design Level Complete Panel
    /// Rebuilds the LevelCompletePanel with a "Case Closed" celebration screen.
    /// Keeps the same child names so SceneWirer / CaseRunner references stay valid.
    /// Safe to re-run.
    ///
    /// Layout (top → bottom):
    ///   BgFill          — deep OLED black, full panel
    ///   ScanlineOverlay — subtle horizontal stripe texture feel (thin repeating rule)
    ///   HeaderBadge     — "CASE CLOSED" stamp, large cyan, bold, letter-spaced
    ///   HeaderGlow      — thin neon cyan rule under header
    ///   StatusLabel     — "INVESTIGATION COMPLETE" small-caps, muted
    ///   CompletionBodyText — body message
    ///   XPCard          — dark card containing XPText (gold)
    ///   RestartButton   — full-width cyan pill at bottom
    /// </summary>
    public static class LevelCompleteDesigner
    {
        // ── Palette ───────────────────────────────────────────────────────────
        static readonly Color BgDeep        = Hex("#050810");
        static readonly Color BgCard        = Hex("#0A1220");
        static readonly Color AccentCyan    = Hex("#00D4FF");
        static readonly Color AccentTeal    = Hex("#00A8C8");
        static readonly Color GoldBright    = Hex("#FFD700");
        static readonly Color GoldDim       = Hex("#B8913A");
        static readonly Color TextLight     = Hex("#D8EEF4");
        static readonly Color TextMuted     = Hex("#3A5A6A");
        static readonly Color BtnLabel      = Hex("#000D14");
        static readonly Color NeonRule      = new Color(0f, 0.83f, 1f, 0.55f);
        static readonly Color CardBorder    = new Color(0f, 0.83f, 1f, 0.20f);

        [MenuItem("ETEC510/Design Level Complete Panel")]
        public static void DesignPanel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var panelT = ct.Find("LevelCompletePanel");
            if (panelT == null) { Debug.LogError("ETEC510: LevelCompletePanel not found."); return; }
            var panel = panelT.gameObject;

            // Remove old background image component (replaced by BgFill child)
            var oldImg = panel.GetComponent<Image>();
            if (oldImg != null) Object.DestroyImmediate(oldImg);

            // ── Clear existing children (rebuild clean) ────────────────────────
            for (int i = panelT.childCount - 1; i >= 0; i--)
                Object.DestroyImmediate(panelT.GetChild(i).gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── BgFill ────────────────────────────────────────────────────────
            var bg = MakeRect("BgFill", panelT);
            Fill(bg); bg.AddComponent<Image>().color = BgDeep;

            // ── Top accent line ───────────────────────────────────────────────
            var topLine = MakeRect("TopAccent", panelT);
            Anch(topLine, 0f, 0.97f, 1f, 1f);
            topLine.AddComponent<Image>().color = AccentCyan;

            // ── CASE CLOSED header ────────────────────────────────────────────
            var header = MakeRect("HeaderBadge", panelT);
            Anch(header, 0.05f, 0.72f, 0.95f, 0.92f);
            var headerTMP = header.AddComponent<TextMeshProUGUI>();
            headerTMP.text             = "CASE CLOSED";
            headerTMP.alignment        = TextAlignmentOptions.Center;
            headerTMP.fontSize         = 64;
            headerTMP.fontStyle        = FontStyles.Bold;
            headerTMP.color            = AccentCyan;
            headerTMP.characterSpacing = 8f;
            ApplyMonoFont(headerTMP);

            // Shadow gives a glow-like effect
            var headerShadow = header.AddComponent<Shadow>();
            headerShadow.effectColor    = new Color(0f, 0.83f, 1f, 0.6f);
            headerShadow.effectDistance = new Vector2(0, -3);

            // ── Neon rule below header ────────────────────────────────────────
            var rule = MakeRect("HeaderGlow", panelT);
            Anch(rule, 0.05f, 0.705f, 0.95f, 0.716f);
            rule.AddComponent<Image>().color = NeonRule;

            // ── "INVESTIGATION COMPLETE" status label ─────────────────────────
            var status = MakeRect("StatusLabel", panelT);
            Anch(status, 0.05f, 0.65f, 0.95f, 0.71f);
            var statusTMP = status.AddComponent<TextMeshProUGUI>();
            statusTMP.text             = "INVESTIGATION COMPLETE";
            statusTMP.alignment        = TextAlignmentOptions.Center;
            statusTMP.fontSize         = 18;
            statusTMP.color            = TextMuted;
            statusTMP.characterSpacing = 6f;
            ApplyMonoFont(statusTMP);

            // ── Completion body text ──────────────────────────────────────────
            var body = MakeRect("CompletionBodyText", panelT);
            Anch(body, 0.06f, 0.42f, 0.94f, 0.65f);
            var bodyTMP = body.AddComponent<TextMeshProUGUI>();
            bodyTMP.text             = "You cracked the case.";
            bodyTMP.alignment        = TextAlignmentOptions.Center;
            bodyTMP.fontSize         = 26;
            bodyTMP.color            = TextLight;
            bodyTMP.textWrappingMode = TextWrappingModes.Normal;
            bodyTMP.lineSpacing      = 6f;
            ApplyMonoFont(bodyTMP);

            // ── XP card ───────────────────────────────────────────────────────
            var xpCard = MakeRect("XPCard", panelT);
            Anch(xpCard, 0.12f, 0.28f, 0.88f, 0.42f);
            var xpCardBorder = xpCard.AddComponent<Image>();
            xpCardBorder.color       = CardBorder;
            xpCardBorder.sprite      = rounded;
            xpCardBorder.type        = Image.Type.Sliced;
            xpCardBorder.pixelsPerUnitMultiplier = 0.20f;

            var xpCardFill = MakeRect("Fill", xpCard.transform);
            var xpFillRT = xpCardFill.GetComponent<RectTransform>();
            xpFillRT.anchorMin = Vector2.zero; xpFillRT.anchorMax = Vector2.one;
            xpFillRT.offsetMin = new Vector2(1.5f, 1.5f); xpFillRT.offsetMax = new Vector2(-1.5f, -1.5f);
            var xpFillImg = xpCardFill.AddComponent<Image>();
            xpFillImg.color       = BgCard;
            xpFillImg.sprite      = rounded;
            xpFillImg.type        = Image.Type.Sliced;
            xpFillImg.pixelsPerUnitMultiplier = 0.20f;

            var xpLabel = MakeRect("XPText", xpCard.transform);
            var xpLabelRT = xpLabel.GetComponent<RectTransform>();
            xpLabelRT.anchorMin = Vector2.zero; xpLabelRT.anchorMax = Vector2.one;
            xpLabelRT.offsetMin = Vector2.zero; xpLabelRT.offsetMax = Vector2.zero;
            var xpTMP = xpLabel.AddComponent<TextMeshProUGUI>();
            xpTMP.text             = "+50 XP  •  Total: 50 XP";
            xpTMP.alignment        = TextAlignmentOptions.Center;
            xpTMP.fontSize         = 28;
            xpTMP.color            = GoldBright;
            xpTMP.fontStyle        = FontStyles.Bold;
            xpTMP.characterSpacing = 3f;
            ApplyMonoFont(xpTMP);

            // CompletionImage — hidden behind BgFill, kept for CaseRunner wiring
            var compImg = MakeRect("CompletionImage", panelT);
            Anch(compImg, 0f, 0f, 1f, 1f);
            compImg.transform.SetAsFirstSibling(); // behind everything
            var compImgComp = compImg.AddComponent<Image>();
            compImgComp.color = Color.clear; // invisible; sprite set at runtime by CaseRunner

            // ── Restart button ────────────────────────────────────────────────
            var restart = MakeRect("RestartButton", panelT);
            Anch(restart, 0.10f, 0.08f, 0.90f, 0.22f);
            var restartImg = restart.AddComponent<Image>();
            restartImg.color       = AccentTeal;
            restartImg.sprite      = rounded;
            restartImg.type        = Image.Type.Sliced;
            restartImg.pixelsPerUnitMultiplier = 0.12f;
            restart.AddComponent<Button>();

            var rLabel = MakeRect("Label", restart.transform);
            var rLabelRT = rLabel.GetComponent<RectTransform>();
            rLabelRT.anchorMin = Vector2.zero; rLabelRT.anchorMax = Vector2.one;
            rLabelRT.offsetMin = Vector2.zero; rLabelRT.offsetMax = Vector2.zero;
            var rTMP = rLabel.AddComponent<TextMeshProUGUI>();
            rTMP.text             = "PLAY AGAIN";
            rTMP.alignment        = TextAlignmentOptions.Center;
            rTMP.fontSize         = 26;
            rTMP.color            = BtnLabel;
            rTMP.fontStyle        = FontStyles.Bold;
            rTMP.characterSpacing = 5f;
            ApplyMonoFont(rTMP);

            // ── Re-wire CaseRunner ────────────────────────────────────────────
            var runner = Object.FindFirstObjectByType<ETEC510.UI.CaseRunner>();
            if (runner != null)
            {
                var so = new SerializedObject(runner);
                so.FindProperty("completionBodyText").objectReferenceValue = bodyTMP;
                so.FindProperty("completionImage").objectReferenceValue    = compImgComp;
                so.FindProperty("xpText").objectReferenceValue             = xpTMP;
                so.FindProperty("restartButton").objectReferenceValue      = restart.GetComponent<Button>();
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(runner);
                Debug.Log("  CaseRunner re-wired.");
            }

            panel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Level Complete screen designed. Press Ctrl+S to save.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static GameObject MakeRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void Fill(GameObject go)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static void Anch(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(x0, y0); rt.anchorMax = new Vector2(x1, y1);
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }

        static void ApplyMonoFont(TMP_Text tmp)
        {
            string[] candidates = { "LiberationMono", "RobotoMono", "SourceCodePro",
                                    "Inconsolata", "Courier", "CourierNew" };
            foreach (var n in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {n}");
                if (guids.Length > 0)
                {
                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (f != null) { tmp.font = f; return; }
                }
            }
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
