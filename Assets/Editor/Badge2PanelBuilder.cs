using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Badge 2 Panel
    ///
    /// Creates Badge2Panel under GameCanvas:
    ///   - Full-screen background image (10) Badge 2 Analytics Board.png)
    ///   - "Repeat Video" button — bottom-left
    ///   - "Continue" button — bottom-right (→ Level Complete)
    ///   - Incoming Messages text box — upper-left dark area
    ///   - Two invisible overlay buttons over the tool graphics:
    ///       • Analytics Board  (top)
    ///       • Comment Feed     (bottom)
    ///
    /// Adjust invisible button anchors in the Inspector to match artwork.
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class Badge2PanelBuilder
    {
        [MenuItem("ETEC510/Build Badge 2 Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("Badge2Panel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var font = FindFont();

            // ── Root panel — full stretch ──────────────────────────────────────
            var panelRT = NewStretch("Badge2Panel", canvasGO.transform);
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);
            panelRT.gameObject.SetActive(false);

            // ── Background image ───────────────────────────────────────────────
            var bgImg = panelRT.gameObject.AddComponent<Image>();
            bgImg.raycastTarget = true;

            var bgSprite = FindSprite("Badge2Background");
            if (bgSprite != null)
            {
                bgImg.sprite         = bgSprite;
                bgImg.type           = Image.Type.Simple;
                bgImg.preserveAspect = false;
            }
            else
            {
                bgImg.color = new Color(0.06f, 0.06f, 0.10f, 1f);
                Debug.LogWarning("ETEC510: Badge2Background sprite not found — using solid colour.");
            }

            // ── Incoming Messages text box ─────────────────────────────────────
            var msgGO = NewChild("IncomingMessageText", panelRT);
            var msgRT = (RectTransform)msgGO.transform;
            msgRT.anchorMin = new Vector2(0.02f, 0.55f);
            msgRT.anchorMax = new Vector2(0.57f, 0.68f);
            msgRT.offsetMin = Vector2.zero;
            msgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(msgRT, "IncomingMessageText", saved);
            var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text          = "";
            msgTMP.alignment     = TextAlignmentOptions.TopLeft;
            msgTMP.fontSize      = 36f;
            msgTMP.color         = new Color(0.55f, 0.85f, 1f, 1f);
            msgTMP.fontStyle     = FontStyles.Bold;
            msgTMP.raycastTarget = false;
            if (font != null) msgTMP.font = font;

            // ── Repeat Video button ────────────────────────────────────────────
            var repeatGO = NewChild("Badge2RepeatButton", panelRT);
            var repeatRT = (RectTransform)repeatGO.transform;
            repeatRT.anchorMin = new Vector2(0.29f, 0.02f);
            repeatRT.anchorMax = new Vector2(0.50f, 0.16f);
            repeatRT.offsetMin = Vector2.zero;
            repeatRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(repeatRT, "Badge2RepeatButton", saved);
            repeatGO.AddComponent<Image>();
            repeatGO.AddComponent<Button>();
            AddLabel(repeatGO, "Repeat Video", font);
            DetectiveStyleGuide.StyleButton(repeatGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Continue button ────────────────────────────────────────────────
            var contGO = NewChild("Badge2ContinueButton", panelRT);
            var contRT = (RectTransform)contGO.transform;
            contRT.anchorMin = new Vector2(0.51f, 0.02f);
            contRT.anchorMax = new Vector2(0.59f, 0.16f);
            contRT.offsetMin = Vector2.zero;
            contRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(contRT, "Badge2ContinueButton", saved);
            contGO.AddComponent<Image>();
            contGO.AddComponent<Button>();
            AddLabel(contGO, "Evidence Board", font);
            DetectiveStyleGuide.StyleButton(contGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Invisible tool buttons ─────────────────────────────────────────
            // Positions are approximate — adjust anchors in Inspector to match artwork.

            // Analytics Board — bottom-right button
            var analyticsGO = NewInvisibleButton("AnalyticsBoardButton", panelRT,
                new Vector2(0.60f, 0.02f), new Vector2(0.98f, 0.18f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)analyticsGO.transform, "AnalyticsBoardButton", saved);

            // Comment Feed — bottom-left artwork button
            var commentGO = NewInvisibleButton("CommentFeedButton", panelRT,
                new Vector2(0.02f, 0.02f), new Vector2(0.27f, 0.18f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)commentGO.transform, "CommentFeedButton", saved);

            // Viral Image — invisible overlay over the viral image in the scene
            var viralGO = NewInvisibleButton("Badge2ViralImageButton", panelRT,
                new Vector2(0.02f, 0.20f), new Vector2(0.55f, 0.54f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)viralGO.transform, "Badge2ViralImageButton", saved);

            // ── Sibling order — just after CriticalDecisionPointPanel ──────────
            var critPanel = canvasGO.transform.Find("CriticalDecisionPointPanel");
            int targetIndex = critPanel != null
                ? critPanel.GetSiblingIndex() + 1
                : canvasGO.transform.childCount;
            panelRT.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner ────────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge2Panel").objectReferenceValue              = panelRT.gameObject;
            so.FindProperty("badge2BackgroundImage").objectReferenceValue    = bgImg;
            so.FindProperty("badge2IncomingMessageText").objectReferenceValue = msgTMP;
            so.FindProperty("badge2RepeatButton").objectReferenceValue       = repeatGO.GetComponent<Button>();
            so.FindProperty("badge2ContinueButton").objectReferenceValue     = contGO.GetComponent<Button>();
            so.FindProperty("badge2AnalyticsBoardButton").objectReferenceValue = analyticsGO.GetComponent<Button>();
            so.FindProperty("badge2CommentFeedButton").objectReferenceValue  = commentGO.GetComponent<Button>();
            so.FindProperty("badge2ViralImageButton").objectReferenceValue   = viralGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge2Panel built. Adjust invisible button anchors in Inspector. Press Ctrl+S to save.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static GameObject NewInvisibleButton(string name, RectTransform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = NewChild(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color         = Color.clear;
            img.raycastTarget = true;
            go.AddComponent<Button>();
            return go;
        }

        static void AddLabel(GameObject go, string text, TMP_FontAsset font)
        {
            var labelGO = NewChild("Label", go.transform);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 28f;
            tmp.fontStyle = FontStyles.Bold;
            if (font != null) tmp.font = font;
        }

        static RectTransform NewStretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        static GameObject NewChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static Sprite FindSprite(string assetName)
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:Texture2D", new[] { "Assets/Images" });
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        static TMP_FontAsset FindFont()
        {
            string[] candidates = { "LiberationSans", "Roboto", "Arial", "OpenSans" };
            foreach (var n in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {n}");
                if (guids.Length > 0)
                {
                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                        AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (f != null) return f;
                }
            }
            return null;
        }
    }
}
