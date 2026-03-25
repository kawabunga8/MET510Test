using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Add Option Buttons
    /// Adds SpotOptionButton0/1 + SpotFeedbackText to SpotTheCluePanel and
    /// MotiveOptionButton0/1 + MotiveFeedbackText to FindTheMotivePanel,
    /// matching the GutCheck layout.
    /// Safe to re-run (destroys and recreates existing buttons).
    /// </summary>
    public static class OptionButtonBuilder
    {
        [MenuItem("ETEC510/Add Option Buttons")]
        public static void AddOptionButtons()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            BuildOptionPanel(ct, "SpotTheCluePanel",
                "SpotOptionButton0", "SpotOptionButton1", "SpotFeedbackText",
                removeOld: new[] { "SpotConfirmButton", "SpotExplanationText" });

            BuildOptionPanel(ct, "FindTheMotivePanel",
                "MotiveOptionButton0", "MotiveOptionButton1", "MotiveFeedbackText",
                removeOld: new[] { "MotiveConfirmButton", "MotiveExplanationText" });

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Option buttons added. Run ETEC510 > Wire CaseRunner, then Ctrl+S.");
        }

        static void BuildOptionPanel(Transform ct, string panelName,
            string btn0Name, string btn1Name, string feedbackName,
            string[] removeOld)
        {
            var panel = ct.Find(panelName);
            if (panel == null) { Debug.LogWarning($"ETEC510: {panelName} not found."); return; }

            // Remove old confirm button and explanation text
            foreach (var name in removeOld)
            {
                var old = panel.Find(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            // Remove existing option buttons/feedback if re-running
            foreach (var name in new[] { btn0Name, btn1Name, feedbackName })
            {
                var old = panel.Find(name);
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }

            // ── Option Button 0 (left) — matches GutOptionButton0 ────────────
            CreateOptionButton(panel, btn0Name,
                anchorMin: new Vector2(0.05f, 0.20f),
                anchorMax: new Vector2(0.48f, 0.33f));

            // ── Option Button 1 (right) — matches GutOptionButton1 ───────────
            CreateOptionButton(panel, btn1Name,
                anchorMin: new Vector2(0.52f, 0.20f),
                anchorMax: new Vector2(0.95f, 0.33f));

            // ── Feedback text (below buttons, above back button) ──────────────
            var feedbackGO = new GameObject(feedbackName, typeof(RectTransform));
            feedbackGO.transform.SetParent(panel, false);
            var frt = (RectTransform)feedbackGO.transform;
            frt.anchorMin = new Vector2(0.05f, 0.34f);
            frt.anchorMax = new Vector2(0.95f, 0.46f);
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;

            var ftmp = feedbackGO.AddComponent<TextMeshProUGUI>();
            ftmp.text      = "";
            ftmp.fontSize  = 22;
            ftmp.color     = new Color(1f, 0.9f, 0.2f, 1f);
            ftmp.alignment = TextAlignmentOptions.Center;
            ftmp.fontStyle = FontStyles.Bold;

            EditorUtility.SetDirty(panel.gameObject);
            Debug.Log($"  Option buttons added to {panelName}.");
        }

        static void CreateOptionButton(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            var btnGO = new GameObject(name, typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);

            var rt = (RectTransform)btnGO.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 1f, 1f);

            btnGO.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = name;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 24;
            tmp.color     = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }
    }
}
