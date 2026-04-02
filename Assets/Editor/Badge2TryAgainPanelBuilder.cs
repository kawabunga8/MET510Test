using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Badge2 Try Again Panel
    ///
    /// Creates Badge2TryAgainPanel under GameCanvas:
    ///   - Full-screen background: 13) Badge 2 Try Again.png
    ///   - Invisible back button → returns to Badge2DecisionPanel
    ///
    /// Adjust the invisible button anchors in the Inspector to match artwork.
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class Badge2TryAgainPanelBuilder
    {
        [MenuItem("ETEC510/Build Badge2 Try Again Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("Badge2TryAgainPanel");
            var saved = DetectiveStyleGuide.SaveChildRects(existing);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel — full stretch ──────────────────────────────────────
            var panelGO = new GameObject("Badge2TryAgainPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
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

            var guids = AssetDatabase.FindAssets("13) Badge 2 Try Again t:Texture2D", new[] { "Assets/Images" });
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
                bgImg.color = new Color(0.04f, 0.08f, 0.2f, 1f);
                Debug.LogWarning("ETEC510: '13) Badge 2 Try Again.png' not found — using solid colour.");
            }

            // ── Invisible back button — overlaid on artwork ────────────────────
            // Default anchors cover the back/return area; adjust in Inspector.
            var backGO = new GameObject("Badge2TryAgainBackButton", typeof(RectTransform));
            backGO.transform.SetParent(panelGO.transform, false);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.30f, 0.10f);
            backRT.anchorMax = new Vector2(0.70f, 0.25f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(backRT, "Badge2TryAgainBackButton", saved);

            var backImg = backGO.AddComponent<Image>();
            backImg.color         = Color.clear;
            backImg.raycastTarget = true;

            backGO.AddComponent<Button>();

            // ── Sibling order — just after Badge2DecisionPanel ─────────────────
            var decisionPanel = canvasGO.transform.Find("Badge2DecisionPanel");
            int targetIndex = decisionPanel != null
                ? decisionPanel.GetSiblingIndex() + 1
                : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("badge2TryAgainPanel").objectReferenceValue      = panelGO;
            so.FindProperty("badge2TryAgainBackButton").objectReferenceValue = backGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Badge2TryAgainPanel built. Adjust invisible button anchors in Inspector to match artwork. Press Ctrl+S to save.");
        }
    }
}
