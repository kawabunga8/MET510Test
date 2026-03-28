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
    /// Menu: ETEC510 > Build Badge 1 Panel
    ///
    /// Creates Badge1Panel under GameCanvas:
    ///   - Full-screen background image (Badge1Background.png)
    ///   - "Repeat Video" button — bottom-left, replays Badge 1 Intro video
    ///   - Three invisible overlay buttons positioned over the baked-in tool
    ///     button graphics on the right side of the background image:
    ///       • Analyze Account    → SpotTheClue
    ///       • Evaluate Comments  → GutCheck
    ///       • Extract Meta Data  → FindTheMotive
    ///
    /// Invisible buttons use a transparent Image so raycasts still register.
    /// Adjust their RectTransform anchors in the Inspector to align exactly
    /// with the background artwork.
    ///
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class Badge1PanelBuilder
    {
        [MenuItem("ETEC510/Build Badge 1 Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // ── Snapshot current button anchors before destroying ──────────────
            var existing = canvasGO.transform.Find("Badge1Panel");
            var ancRepeat   = ReadAnchors(existing, "RepeatVideoButton",      new Vector2(0.02f,0.08f), new Vector2(0.28f,0.19f));
            var ancMsg      = ReadAnchors(existing, "IncomingMessageText",    new Vector2(0.02f,0.22f), new Vector2(0.59f,0.42f));
            var ancCrit     = ReadAnchors(existing, "CriticalDecisionButton", new Vector2(0.27f,0.08f), new Vector2(0.54f,0.19f));
            var ancAnalyze  = ReadAnchors(existing, "AnalyzeAccountButton",   new Vector2(0.66f,0.62f), new Vector2(1.00f,0.95f));
            var ancComments = ReadAnchors(existing, "EvaluateCommentsButton", new Vector2(0.66f,0.28f), new Vector2(1.00f,0.61f));
            var ancMeta     = ReadAnchors(existing, "ExtractMetaDataButton",  new Vector2(0.66f,0.00f), new Vector2(1.00f,0.27f));
            var ancViral    = ReadAnchors(existing, "ViralImageButton",       new Vector2(0.00f,0.45f), new Vector2(0.32f,1.00f));

            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var font = FindFont();

            // ── Root panel — full stretch ──────────────────────────────────────
            var panelRT = NewStretch("Badge1Panel", canvasGO.transform);
            panelRT.gameObject.SetActive(false);

            // ── Background image — cover fill ──────────────────────────────────
            var bgImg = panelRT.gameObject.AddComponent<Image>();
            bgImg.raycastTarget = true;

            var bgSprite = FindSprite("Badge1Background");
            if (bgSprite != null)
            {
                bgImg.sprite = bgSprite;
                bgImg.type   = Image.Type.Simple;
                bgImg.preserveAspect = false; // stretch to fill
            }
            else
            {
                bgImg.color = new Color(0.06f, 0.06f, 0.10f, 1f);
                Debug.LogWarning("ETEC510: Badge1Background sprite not found — using solid colour.");
            }

            // ── Repeat Video button — inside the dark blue box, bottom-left ───
            var repeatGO = NewChild("RepeatVideoButton", panelRT);
            var repeatRT = (RectTransform)repeatGO.transform;
            repeatRT.anchorMin = ancRepeat.min;
            repeatRT.anchorMax = ancRepeat.max;
            repeatRT.offsetMin = Vector2.zero;
            repeatRT.offsetMax = Vector2.zero;
            repeatGO.AddComponent<Image>();
            repeatGO.AddComponent<Button>();
            AddLabel(repeatGO, "Repeat Video", font);
            DetectiveStyleGuide.StyleButton(repeatGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Incoming Messages text box — upper portion of the dark blue box ─
            var msgGO = NewChild("IncomingMessageText", panelRT);
            var msgRT = (RectTransform)msgGO.transform;
            msgRT.anchorMin = ancMsg.min;
            msgRT.anchorMax = ancMsg.max;
            msgRT.offsetMin = Vector2.zero;
            msgRT.offsetMax = Vector2.zero;
            var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text          = "";
            msgTMP.alignment     = TextAlignmentOptions.TopLeft;
            msgTMP.fontSize      = 36f;
            msgTMP.color         = new Color(0.55f, 0.85f, 1f, 1f); // light blue to match HUD style
            msgTMP.fontStyle     = FontStyles.Bold;
            msgTMP.raycastTarget = false;
            if (font != null) msgTMP.font = font;

            // ── Critical Decision button — inside the dark blue box, bottom-right
            var critGO = NewChild("CriticalDecisionButton", panelRT);
            var critRT = (RectTransform)critGO.transform;
            critRT.anchorMin = ancCrit.min;
            critRT.anchorMax = ancCrit.max;
            critRT.offsetMin = Vector2.zero;
            critRT.offsetMax = Vector2.zero;
            critGO.AddComponent<Image>();
            critGO.AddComponent<Button>();
            AddLabel(critGO, "Make Decision", font);
            DetectiveStyleGuide.StyleButton(critGO, DetectiveStyleGuide.ButtonRole.Primary);

            // ── Invisible overlay buttons on the right-side tool graphics ──────
            // Positions calibrated to the Badge1Background artwork.
            // Adjust anchors in the Inspector if artwork changes.

            // Analyze Account — top tool button
            var analyzeGO = NewInvisibleButton("AnalyzeAccountButton", panelRT,
                ancAnalyze.min, ancAnalyze.max);

            // Evaluate Comments — middle tool button
            var commentsGO = NewInvisibleButton("EvaluateCommentsButton", panelRT,
                ancComments.min, ancComments.max);

            // Extract Meta Data — bottom tool button
            var metaGO = NewInvisibleButton("ExtractMetaDataButton", panelRT,
                ancMeta.min, ancMeta.max);

            // ── Viral Image button — invisible overlay, top-left corner ──────────
            var viralGO = NewInvisibleButton("ViralImageButton", panelRT,
                ancViral.min, ancViral.max);

            // ── Position panel just below MissionStartPanel ────────────────────
            var missionStart = canvasGO.transform.Find("MissionStartPanel");
            int targetIndex = missionStart != null
                ? missionStart.GetSiblingIndex() + 1
                : canvasGO.transform.childCount;
            panelRT.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner ────────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge1Panel").objectReferenceValue                = panelRT.gameObject;
            so.FindProperty("badge1BackgroundImage").objectReferenceValue      = bgImg;
            so.FindProperty("badge1IncomingMessageText").objectReferenceValue       = msgTMP;
            so.FindProperty("badge1RepeatButton").objectReferenceValue             = repeatGO.GetComponent<Button>();
            so.FindProperty("badge1CriticalDecisionButton").objectReferenceValue  = critGO.GetComponent<Button>();
            so.FindProperty("badge1AnalyzeAccountButton").objectReferenceValue    = analyzeGO.GetComponent<Button>();
            so.FindProperty("badge1EvaluateCommentsButton").objectReferenceValue = commentsGO.GetComponent<Button>();
            so.FindProperty("badge1ExtractMetaDataButton").objectReferenceValue  = metaGO.GetComponent<Button>();
            so.FindProperty("badge1ViralImageButton").objectReferenceValue       = viralGO.GetComponent<Button>();

            // Auto-assign Viral_Image sprite if present in Assets/Images
            var viralGuids = AssetDatabase.FindAssets("Viral_Image t:Texture2D", new[] { "Assets/Images" });
            if (viralGuids.Length > 0)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(viralGuids[0]));
                if (sprite != null) so.FindProperty("viralImageSprite").objectReferenceValue = sprite;
            }
            else Debug.LogWarning("ETEC510: 'Viral_Image.png' not found in Assets/Images — assign viralImageSprite manually.");

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge1Panel built. Adjust invisible button anchors in Inspector if needed. Press Ctrl+S to save.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>Reads anchor min/max from a named child RectTransform, or returns defaults.</summary>
        static (Vector2 min, Vector2 max) ReadAnchors(Transform parent, string childName,
            Vector2 defaultMin, Vector2 defaultMax)
        {
            if (parent == null) return (defaultMin, defaultMax);
            var child = parent.Find(childName);
            if (child == null) return (defaultMin, defaultMax);
            var rt = child as RectTransform ?? child.GetComponent<RectTransform>();
            if (rt == null) return (defaultMin, defaultMax);
            return (rt.anchorMin, rt.anchorMax);
        }

        /// <summary>Creates a button with a fully transparent Image — invisible but clickable.</summary>
        static GameObject NewInvisibleButton(string name, RectTransform parent,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = NewChild(name, parent);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.color         = Color.clear; // invisible
            img.raycastTarget = true;        // still catches clicks

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
