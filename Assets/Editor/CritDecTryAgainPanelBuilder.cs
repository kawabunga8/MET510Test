using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build CritDec Try Again Panel
    ///
    /// Creates CritDecTryAgainPanel under GameCanvas:
    ///   - Full-screen background: 8) Badge 1 Try Again.png
    ///   - Invisible button overlaid on the "Try Again" area in the artwork
    ///     → returns to CriticalDecisionPointPanel (shows the background + Select buttons)
    ///
    /// Adjust the invisible button anchors in the Inspector to match artwork.
    /// Safe to re-run — destroys and recreates the panel.
    /// </summary>
    public static class CritDecTryAgainPanelBuilder
    {
        [MenuItem("ETEC510/Build CritDec Try Again Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("CritDecTryAgainPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel — full stretch ──────────────────────────────────────
            var panelGO = new GameObject("CritDecTryAgainPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;
            panelGO.SetActive(false);

            // ── Background image ───────────────────────────────────────────────
            var bgImg = panelGO.AddComponent<Image>();
            bgImg.raycastTarget = true;

            var guids = AssetDatabase.FindAssets("8) Badge 1 Try Again t:Texture2D", new[] { "Assets/Images" });
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
                bgImg.color = new Color(0.15f, 0.04f, 0.04f, 1f);
                Debug.LogWarning("ETEC510: '8) Badge 1 Try Again.png' not found — using solid colour.");
            }

            // ── Invisible "Try Again" button — overlaid on artwork ────────────
            // Default anchors cover the centre of the image; adjust in Inspector.
            var retryGO = new GameObject("CritDecTryAgainRetryButton", typeof(RectTransform));
            retryGO.transform.SetParent(panelGO.transform, false);
            var retryRT = (RectTransform)retryGO.transform;
            retryRT.anchorMin = new Vector2(0.30f, 0.10f);
            retryRT.anchorMax = new Vector2(0.70f, 0.25f);
            retryRT.offsetMin = Vector2.zero;
            retryRT.offsetMax = Vector2.zero;

            var retryImg = retryGO.AddComponent<Image>();
            retryImg.color         = Color.clear;
            retryImg.raycastTarget = true;

            retryGO.AddComponent<Button>();

            // ── Sibling order — just after CriticalDecisionPointPanel ──────────
            var critPanel = canvasGO.transform.Find("CriticalDecisionPointPanel");
            int targetIndex = critPanel != null
                ? critPanel.GetSiblingIndex() + 1
                : canvasGO.transform.childCount;
            panelGO.transform.SetSiblingIndex(targetIndex);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("critDecTryAgainPanel").objectReferenceValue        = panelGO;
            so.FindProperty("critDecTryAgainRetryButton").objectReferenceValue  = retryGO.GetComponent<Button>();
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: CritDecTryAgainPanel built. Adjust invisible button anchors in Inspector to match artwork. Press Ctrl+S to save.");
        }
    }
}
