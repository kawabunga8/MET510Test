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
    /// Menu: ETEC510 > Build Badge 2 Comment Feed Panel
    ///
    /// Creates Badge2CommentFeedPanel under GameCanvas:
    ///   - Full-screen background: 11) Badge 2 Comment Feed.png
    ///   - Visible Back button (bottom-left) → Badge2Panel
    ///
    /// Safe to re-run.
    /// </summary>
    public static class Badge2CommentFeedPanelBuilder
    {
        [MenuItem("ETEC510/Build Badge 2 Comment Feed Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var existing = canvasGO.transform.Find("Badge2CommentFeedPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("Badge2CommentFeedPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);

            var analyticsPanel = canvasGO.transform.Find("Badge2AnalyticsPanel");
            var badge2Panel    = canvasGO.transform.Find("Badge2Panel");
            int targetIndex = analyticsPanel != null
                ? analyticsPanel.GetSiblingIndex() + 1
                : (badge2Panel != null ? badge2Panel.GetSiblingIndex() + 1 : canvasGO.transform.childCount);
            panelGO.transform.SetSiblingIndex(targetIndex);

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(panelRT, "", saved);
            panelGO.SetActive(false);

            // ── Background image ───────────────────────────────────────────────
            var bgImg = panelGO.AddComponent<Image>();
            bgImg.raycastTarget = true;

            var guids = AssetDatabase.FindAssets("11_Badge2CommentFeed t:Texture2D", new[] { "Assets/Images" });
            if (guids.Length > 0)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (sprite != null)
                {
                    bgImg.sprite         = sprite;
                    bgImg.type           = Image.Type.Simple;
                    bgImg.preserveAspect = false;
                }
            }
            else
            {
                bgImg.color = new Color(0.05f, 0.1f, 0.2f, 1f);
                Debug.LogWarning("ETEC510: '11_Badge2CommentFeed.png' not found in Assets/Images.");
            }

            // ── Back button (visible) ─────────────────────────────────────────
            var backGO = new GameObject("Badge2CommentFeedBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.03f, 0.02f);
            backRT.anchorMax = new Vector2(0.21f, 0.12f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "Badge2CommentFeedBackButton", saved);

            var backImg = backGO.AddComponent<Image>();
            backImg.sprite                  = rounded;
            backImg.type                    = Image.Type.Sliced;
            backImg.pixelsPerUnitMultiplier = 0.10f;
            backImg.color                   = DetectiveStyleGuide.BtnNav;
            backImg.raycastTarget           = true;

            backGO.AddComponent<Button>();

            var backShadow = backGO.AddComponent<Shadow>();
            backShadow.effectColor    = new Color(0f, 0f, 0f, 0.5f);
            backShadow.effectDistance = new Vector2(2f, -2f);

            var backLabel = new GameObject("Label", typeof(RectTransform));
            backLabel.transform.SetParent(backGO.transform, false);
            var backLabelRT = (RectTransform)backLabel.transform;
            backLabelRT.anchorMin = Vector2.zero; backLabelRT.anchorMax = Vector2.one;
            backLabelRT.offsetMin = Vector2.zero; backLabelRT.offsetMax = Vector2.zero;
            var backTMP = backLabel.AddComponent<TextMeshProUGUI>();
            backTMP.text      = "Back";
            backTMP.fontSize  = 28f;
            backTMP.fontStyle = FontStyles.Bold;
            backTMP.color     = DetectiveStyleGuide.LabelLight;
            backTMP.alignment = TextAlignmentOptions.Center;

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge2CommentFeedPanel").objectReferenceValue      = panelGO;
            so.FindProperty("badge2CommentFeedBackButton").objectReferenceValue = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge2CommentFeedPanel built. Press Ctrl+S to save.");
        }
    }
}
