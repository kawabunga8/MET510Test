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
    /// Menu: ETEC510 > Build Tool Sub-Panels
    ///
    /// Creates three sub-panels accessible from the investigation panels:
    ///   SpotTheCluePanel    → "Account Profile"    button → AccountProfilePanel
    ///   GutCheckPanel       → "Comments Section"   button → CommentsSectionPanel
    ///   FindTheMotivePanel  → "MetaData"            button → MetaDataPanel
    ///
    /// Each sub-panel gets a Back button (Nav style) that returns to its source panel.
    /// Safe to re-run (destroys and recreates panels and buttons).
    /// </summary>
    public static class ToolSubPanelBuilder
    {
        [MenuItem("ETEC510/Build Tool Sub-Panels")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var so      = new SerializedObject(runner);
            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var font    = FindFont();

            BuildSubPanel(canvasGO.transform, so, rounded, font,
                "AccountProfilePanel", "ACCOUNT PROFILE", "SpotTheCluePanel",
                "spotAccountProfileButton", "accountProfilePanel", "accountProfileBackButton");

            BuildSubPanel(canvasGO.transform, so, rounded, font,
                "CommentsSectionPanel", "COMMENTS SECTION", "GutCheckPanel",
                "gutCommentsSectionButton", "commentsSectionPanel", "commentsSectionBackButton");

            BuildSubPanel(canvasGO.transform, so, rounded, font,
                "MetaDataPanel", "META DATA", "FindTheMotivePanel",
                "motiveMetaDataButton", "metaDataPanel", "metaDataBackButton");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Tool sub-panels built. Press Ctrl+S to save.");
        }

        // ── Sub-panel ─────────────────────────────────────────────────────────

        static void BuildSubPanel(Transform canvasT, SerializedObject so, Sprite rounded, TMP_FontAsset font,
            string panelName, string title, string sourcePanelName,
            string gotoField, string panelField, string backField)
        {
            // Remove existing
            var existing = canvasT.Find(panelName);
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Root — full stretch, sits above normal panels (set last sibling below image popup)
            var panel = NewStretch(panelName, canvasT);
            DetectiveStyleGuide.ApplySavedRect(panel, "", saved);
            panel.gameObject.SetActive(false);

            // Background image using same sprite as other panels
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color         = DetectiveStyleGuide.BgPanel;
            bg.raycastTarget = true;

            // Gold border rule at top
            var topRule = NewChild("TopRule", panel);
            var trRT = (RectTransform)topRule.transform;
            trRT.anchorMin = new Vector2(0f, 0.955f); trRT.anchorMax = new Vector2(1f, 0.965f);
            trRT.offsetMin = Vector2.zero; trRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(trRT, "TopRule", saved);
            var trImg = topRule.AddComponent<Image>();
            trImg.color = DetectiveStyleGuide.Gold;
            trImg.raycastTarget = false;

            // Title text
            var titleGO = NewChild("TitleText", panel);
            var titleRT = (RectTransform)titleGO.transform;
            titleRT.anchorMin = new Vector2(0.05f, 0.87f); titleRT.anchorMax = new Vector2(0.95f, 0.97f);
            titleRT.offsetMin = Vector2.zero; titleRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(titleRT, "TitleText", saved);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text             = title;
            titleTMP.alignment        = TextAlignmentOptions.Center;
            titleTMP.fontSize         = 52f;
            titleTMP.color            = DetectiveStyleGuide.Parchment;
            titleTMP.fontStyle        = FontStyles.Bold;
            titleTMP.characterSpacing = 5f;
            if (font != null) titleTMP.font = font;

            // Gold rule below title
            var titleRule = NewChild("TitleRule", panel);
            var titlerRT = (RectTransform)titleRule.transform;
            titlerRT.anchorMin = new Vector2(0.05f, 0.855f); titlerRT.anchorMax = new Vector2(0.95f, 0.862f);
            titlerRT.offsetMin = Vector2.zero; titlerRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(titlerRT, "TitleRule", saved);
            var titlerImg = titleRule.AddComponent<Image>();
            titlerImg.color = new Color(DetectiveStyleGuide.Gold.r, DetectiveStyleGuide.Gold.g,
                                        DetectiveStyleGuide.Gold.b, 0.4f);
            titlerImg.raycastTarget = false;

            // Content area placeholder
            var contentGO = NewChild("ContentArea", panel);
            var contentRT = (RectTransform)contentGO.transform;
            contentRT.anchorMin = new Vector2(0.05f, 0.15f); contentRT.anchorMax = new Vector2(0.95f, 0.84f);
            contentRT.offsetMin = Vector2.zero; contentRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(contentRT, "ContentArea", saved);
            var contentImg = contentGO.AddComponent<Image>();
            contentImg.color         = new Color(0f, 0f, 0f, 0.25f);
            contentImg.sprite        = rounded;
            contentImg.type          = Image.Type.Sliced;
            contentImg.pixelsPerUnitMultiplier = 0.08f;
            contentImg.raycastTarget = false;

            // Back button — Nav style, bottom-centre
            var backGO = NewChild("BackButton", panel);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.25f, 0.03f); backRT.anchorMax = new Vector2(0.75f, 0.12f);
            backRT.offsetMin = Vector2.zero; backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "BackButton", saved);
            AddLabelledButton(backGO, "Back", font);
            DetectiveStyleGuide.StyleButton(backGO, DetectiveStyleGuide.ButtonRole.Nav);

            // Wire panel + back button into CaseRunner
            so.FindProperty(panelField).objectReferenceValue      = panel.gameObject;
            so.FindProperty(backField).objectReferenceValue       = backGO.GetComponent<Button>();

            // Position just above the normal panels (below ImagePopupPanel)
            var imagePopup = canvasT.Find("ImagePopupPanel");
            int targetIndex = imagePopup != null ? imagePopup.GetSiblingIndex() : canvasT.childCount;
            panel.transform.SetSiblingIndex(targetIndex);

            // Add goto button to the source panel
            var sourcePanel = FindInactive(canvasT, sourcePanelName);
            if (sourcePanel == null)
            {
                Debug.LogWarning($"ETEC510: {sourcePanelName} not found — skipping goto button.");
            }
            else
            {
                var oldBtn = sourcePanel.transform.Find(panelName + "Button");
                if (oldBtn != null) Object.DestroyImmediate(oldBtn.gameObject);

                var btnGO = NewChild(panelName + "Button", sourcePanel.transform);
                var btnRT = (RectTransform)btnGO.transform;
                btnRT.anchorMin = new Vector2(0.55f, 0.02f); btnRT.anchorMax = new Vector2(0.95f, 0.10f);
                btnRT.offsetMin = Vector2.zero; btnRT.offsetMax = Vector2.zero;

                // Derive a human label from the panel name (e.g. "AccountProfilePanel" → "Account Profile")
                var buttonLabel = panelName.Replace("Panel", "");
                // Insert spaces before capital letters
                var labelBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < buttonLabel.Length; i++)
                {
                    if (i > 0 && char.IsUpper(buttonLabel[i])) labelBuilder.Append(' ');
                    labelBuilder.Append(buttonLabel[i]);
                }

                AddLabelledButton(btnGO, labelBuilder.ToString(), font);
                DetectiveStyleGuide.StyleButton(btnGO, DetectiveStyleGuide.ButtonRole.Primary);

                so.FindProperty(gotoField).objectReferenceValue = btnGO.GetComponent<Button>();
                Debug.Log($"ETEC510: Added goto button to {sourcePanelName}.");
            }

            Debug.Log($"ETEC510: Built {panelName}.");
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

        static GameObject NewChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static void AddLabelledButton(GameObject go, string label, TMP_FontAsset font)
        {
            go.AddComponent<Image>(); // styled by DetectiveStyleGuide
            go.AddComponent<Button>();

            var labelGO = NewChild("Label", go.transform);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 32f;
            tmp.fontStyle = FontStyles.Bold;
            if (font != null) tmp.font = font;
        }

        // Finds a named GO including inactive ones
        static GameObject FindInactive(Transform root, string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name) return t.gameObject;
            return null;
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
