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
    /// Adds a "Go Back" button to EvidenceBoardPanel that returns to Mission Briefing.
    /// Wires evidenceBoardBackButton on CaseRunner. Safe to re-run.
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

            // Remove old button if it exists
            var existing = boardPanel.Find("EvidenceBoardBackButton");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
                Debug.Log("  Removed existing EvidenceBoardBackButton.");
            }

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // ── Button ────────────────────────────────────────────────────────
            var btnGO = new GameObject("EvidenceBoardBackButton", typeof(RectTransform));
            btnGO.transform.SetParent(boardPanel, false);

            var btnRT = (RectTransform)btnGO.transform;
            btnRT.anchorMin        = new Vector2(0.02f, 0.02f);
            btnRT.anchorMax        = new Vector2(0.24f, 0.10f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.raycastTarget = true;

            btnGO.AddComponent<Button>();

            // ── Label ─────────────────────────────────────────────────────────
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(btnGO.transform, false);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "<< Go Back";

            // Apply full DetectiveStyleGuide treatment (gold border, BgFill, Shine, shadow)
            DetectiveStyleGuide.StyleButton(btnGO, DetectiveStyleGuide.ButtonRole.Nav);

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var controllerT = ct.Find("CaseController");
            if (controllerT == null) { Debug.LogWarning("ETEC510: CaseController not found — wire manually."); }
            else
            {
                var runner = controllerT.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("evidenceBoardBackButton").objectReferenceValue = btnGO.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  evidenceBoardBackButton wired on CaseRunner.");
                }
            }

            EditorUtility.SetDirty(btnGO);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: EvidenceBoardBackButton added. Ctrl+S to save.");
        }
    }
}
