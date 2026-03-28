using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Add Evidence Board Back Button
    /// Adds two navigation buttons to EvidenceBoardPanel:
    ///   - Back button (bottom-left)  → Badge1Panel
    ///   - Badge 2 button (next to it) → Badge2Panel
    /// Wires evidenceBoardBackButton and evidenceBoardBadge2Button on CaseRunner.
    /// Safe to re-run.
    /// </summary>
    public static class EvidenceBoardBackButtonBuilder
    {
        [MenuItem("ETEC510/Add Evidence Board Back Button")]
        public static void AddButton()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var boardPanel = ct.Find("EvidenceBoardPanel");
            if (boardPanel == null) { Debug.LogError("ETEC510: EvidenceBoardPanel not found."); return; }

            // Remove existing buttons
            foreach (var name in new[] { "EvidenceBoardBackButton", "EvidenceBoardBadge2Button" })
            {
                var old = boardPanel.Find(name);
                if (old != null) { Object.DestroyImmediate(old.gameObject); Debug.Log($"  Removed existing {name}."); }
            }

            // ── Back button → Badge1Panel ─────────────────────────────────────
            var backGO = MakeButton("EvidenceBoardBackButton", boardPanel,
                new Vector2(0.02f, 0.02f), new Vector2(0.21f, 0.10f), "Back", DetectiveStyleGuide.ButtonRole.Nav);

            // ── Badge 2 button → Badge2Panel ──────────────────────────────────
            var badge2GO = MakeButton("EvidenceBoardBadge2Button", boardPanel,
                new Vector2(0.23f, 0.02f), new Vector2(0.44f, 0.10f), "Badge 2", DetectiveStyleGuide.ButtonRole.Primary);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var controllerT = ct.Find("CaseController");
            if (controllerT == null) { Debug.LogWarning("ETEC510: CaseController not found — wire manually."); }
            else
            {
                var runner = controllerT.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("evidenceBoardBackButton").objectReferenceValue   = backGO.GetComponent<Button>();
                    so.FindProperty("evidenceBoardBadge2Button").objectReferenceValue = badge2GO.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  evidenceBoardBackButton and evidenceBoardBadge2Button wired on CaseRunner.");
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Evidence Board navigation buttons added. Ctrl+S to save.");
        }

        static GameObject MakeButton(string name, Transform parent,
            Vector2 anchorMin, Vector2 anchorMax, string label, DetectiveStyleGuide.ButtonRole role)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>();
            go.AddComponent<Button>();
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero; labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero; labelRT.offsetMax = Vector2.zero;
            labelGO.AddComponent<TextMeshProUGUI>().text = label;
            DetectiveStyleGuide.StyleButton(go, role);
            return go;
        }
    }
}
