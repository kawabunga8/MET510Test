using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using TMPro;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Critical Decision Panel
    ///
    /// Creates CriticalDecisionPointPanel under GameCanvas:
    ///   - RawImage + VideoPlayer for the Critical Decision video
    ///   - "Skip →" button shown during video
    ///   - CriticalDecisionPointBackground image, revealed after video ends
    ///   - Three invisible overlay Buttons on the three "Select" labels in the artwork:
    ///       Select 1 (Account)   → SpotTheClue
    ///       Select 2 (Comments)  → GutCheck
    ///       Select 3 (MetaData)  → FindTheMotive
    ///   - Visible "Back" button (Nav style) → Badge 1 Panel
    ///
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class CriticalDecisionPanelBuilder
    {
        [MenuItem("ETEC510/Build Critical Decision Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("CriticalDecisionPointPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var font = FindFont();

            // ── Root panel ────────────────────────────────────────────────────
            var panelRT = NewStretch("CriticalDecisionPointPanel", canvasGO.transform);
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);
            panelRT.gameObject.SetActive(false);

            // Dark base so there's no flash between video and background
            var baseImg = panelRT.gameObject.AddComponent<Image>();
            baseImg.color         = new Color(0f, 0f, 0f, 1f);
            baseImg.raycastTarget = true;

            // ── Video display (RawImage, full stretch) ────────────────────────
            var vidGO = NewChild("CritDecVideoDisplay", panelRT);
            var vidRT = (RectTransform)vidGO.transform;
            vidRT.anchorMin = Vector2.zero; vidRT.anchorMax = Vector2.one;
            vidRT.offsetMin = Vector2.zero; vidRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(vidRT, "CritDecVideoDisplay", saved);
            var rawImg = vidGO.AddComponent<RawImage>();
            rawImg.color         = Color.clear;
            rawImg.raycastTarget = false;

            // ── VideoPlayer ───────────────────────────────────────────────────
            var vp = vidGO.AddComponent<VideoPlayer>();
            vp.playOnAwake     = false;
            vp.isLooping       = false;
            vp.audioOutputMode = VideoAudioOutputMode.Direct;

            // ── Skip button (shown during video, hidden after) ────────────────
            var skipGO = NewChild("CritDecSkipButton", panelRT);
            var skipRT = (RectTransform)skipGO.transform;
            skipRT.anchorMin = new Vector2(0.72f, 0.03f);
            skipRT.anchorMax = new Vector2(0.98f, 0.11f);
            skipRT.offsetMin = Vector2.zero; skipRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(skipRT, "CritDecSkipButton", saved);
            skipGO.AddComponent<Image>();
            skipGO.AddComponent<Button>();
            AddLabel(skipGO, "Skip", font);
            DetectiveStyleGuide.StyleButton(skipGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Background image (hidden during video, revealed after) ────────
            var bgGO = NewChild("CritDecBackground", panelRT);
            var bgRT = (RectTransform)bgGO.transform;
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(bgRT, "CritDecBackground", saved);
            var bgImg = bgGO.AddComponent<Image>();
            bgGO.SetActive(false); // hidden until video ends

            var bgSprite = FindSprite("CriticalDecisionPointBackground");
            if (bgSprite != null)
            {
                bgImg.sprite         = bgSprite;
                bgImg.type           = Image.Type.Simple;
                bgImg.preserveAspect = false;
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.08f, 0.15f, 1f);
                Debug.LogWarning("ETEC510: CriticalDecisionPointBackground not found — using solid colour.");
            }
            bgImg.raycastTarget = false;

            // ── Invisible Select buttons ──────────────────────────────────────
            // Positions calibrated to the CriticalDecisionPointBackground artwork.
            // Three "Select" pills sit in a row roughly y: 32–43% from bottom.

            // Select 1 — Account (left)
            var sel1 = NewInvisibleButton("SelectAccountButton", panelRT,
                new Vector2(0.05f, 0.32f), new Vector2(0.32f, 0.45f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)sel1.transform, "SelectAccountButton", saved);

            // Select 2 — Comments (centre)
            var sel2 = NewInvisibleButton("SelectCommentsButton", panelRT,
                new Vector2(0.37f, 0.32f), new Vector2(0.63f, 0.45f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)sel2.transform, "SelectCommentsButton", saved);

            // Select 3 — MetaData (right)
            var sel3 = NewInvisibleButton("SelectMetaDataButton", panelRT,
                new Vector2(0.68f, 0.32f), new Vector2(0.95f, 0.45f));
            DetectiveStyleGuide.ApplySavedRect((RectTransform)sel3.transform, "SelectMetaDataButton", saved);

            // ── Incoming Messages text box ────────────────────────────────────
            var msgGO = NewChild("CritDecMessageText", panelRT);
            var msgRT = (RectTransform)msgGO.transform;
            msgRT.anchorMin = new Vector2(0.02f, 0.22f);
            msgRT.anchorMax = new Vector2(0.59f, 0.42f);
            msgRT.offsetMin = Vector2.zero;
            msgRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(msgRT, "CritDecMessageText", saved);
            var msgTMP = msgGO.AddComponent<TextMeshProUGUI>();
            msgTMP.text          = "";
            msgTMP.alignment     = TextAlignmentOptions.TopLeft;
            msgTMP.fontSize      = 36f;
            msgTMP.color         = new Color(0.55f, 0.85f, 1f, 1f);
            msgTMP.fontStyle     = FontStyles.Bold;
            msgTMP.raycastTarget = false;
            if (font != null) msgTMP.font = font;

            // ── Visible Back button ───────────────────────────────────────────
            var backGO = NewChild("CritDecBackButton", panelRT);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.02f, 0.03f);
            backRT.anchorMax = new Vector2(0.28f, 0.12f);
            backRT.offsetMin = Vector2.zero; backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "CritDecBackButton", saved);
            backGO.AddComponent<Image>();
            backGO.AddComponent<Button>();
            AddLabel(backGO, "Back", font);
            DetectiveStyleGuide.StyleButton(backGO, DetectiveStyleGuide.ButtonRole.Nav);
            backGO.SetActive(false); // hidden during video, shown after

            // ── Visible Skip to Next button ───────────────────────────────────
            var nextGO = NewChild("CritDecNextButton", panelRT);
            var nextRT = (RectTransform)nextGO.transform;
            nextRT.anchorMin = new Vector2(0.60f, 0.03f);
            nextRT.anchorMax = new Vector2(0.98f, 0.12f);
            nextRT.offsetMin = Vector2.zero; nextRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(nextRT, "CritDecNextButton", saved);
            nextGO.AddComponent<Image>();
            nextGO.AddComponent<Button>();
            AddLabel(nextGO, "Skip to Next", font);
            DetectiveStyleGuide.StyleButton(nextGO, DetectiveStyleGuide.ButtonRole.Primary);
            nextGO.SetActive(false); // hidden during video, shown after

            // ── Sibling order — just after Badge1Panel ────────────────────────
            var badge1 = canvasGO.transform.Find("Badge1Panel");
            int targetIndex = badge1 != null ? badge1.GetSiblingIndex() + 1 : canvasGO.transform.childCount;
            panelRT.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("criticalDecisionPanel").objectReferenceValue         = panelRT.gameObject;
            so.FindProperty("criticalDecisionVideoDisplay").objectReferenceValue  = rawImg;
            so.FindProperty("criticalDecisionVideoPlayer").objectReferenceValue   = vp;
            so.FindProperty("criticalDecisionSkipButton").objectReferenceValue    = skipGO.GetComponent<Button>();
            so.FindProperty("criticalDecisionBackground").objectReferenceValue    = bgImg;
            so.FindProperty("criticalDecisionSelect1Button").objectReferenceValue = sel1.GetComponent<Button>();
            so.FindProperty("criticalDecisionSelect2Button").objectReferenceValue = sel2.GetComponent<Button>();
            so.FindProperty("criticalDecisionSelect3Button").objectReferenceValue = sel3.GetComponent<Button>();
            so.FindProperty("criticalDecisionBackButton").objectReferenceValue    = backGO.GetComponent<Button>();
            so.FindProperty("criticalDecisionNextButton").objectReferenceValue    = nextGO.GetComponent<Button>();
            so.FindProperty("criticalDecisionMessageText").objectReferenceValue  = msgTMP;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: CriticalDecisionPointPanel built. Assign CriticalDecisionVideoFile in CaseData Inspector. Press Ctrl+S to save.");
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
