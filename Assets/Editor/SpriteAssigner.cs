using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    public static class SpriteAssigner
    {
        [MenuItem("ETEC510/Assign All Sprites")]
        public static void AssignSprites()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var map       = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/map.png");
            var chief     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chief silouetter.png");
            var detBg     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/BriefingRoom16_9.png");
            var detBgTech = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Detective background TECH.png");

            Assign(ct, "EvidenceBoardPanel/EvidenceBoardImage",  map);
            Assign(ct, "MissionBriefingPanel/BriefingImage",     detBg);
            Assign(ct, "HintsFromChiefPanel/ChiefImage",         chief);
            Assign(ct, "LevelCompletePanel/CompletionImage",      detBgTech);

            CreateBoardWarningText(ct);
            CreateRestartButton(ct);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: All sprites assigned.");
        }

        static void CreateBoardWarningText(Transform ct)
        {
            var boardPanel = ct.Find("EvidenceBoardPanel");
            if (boardPanel == null) { Debug.LogWarning("EvidenceBoardPanel not found."); return; }

            // Destroy existing to recreate cleanly
            var existing = boardPanel.Find("BoardWarningText");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Create with RectTransform from the start (required for UI elements)
            var warningGO = new GameObject("BoardWarningText", typeof(RectTransform));
            warningGO.transform.SetParent(boardPanel, false);

            var rt = (RectTransform)warningGO.transform;
            rt.anchorMin = new Vector2(0.15f, 0.06f);
            rt.anchorMax = new Vector2(0.85f, 0.20f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = warningGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 28;
            tmp.color     = new Color(1f, 0.9f, 0.2f, 1f);
            tmp.fontStyle = FontStyles.Bold;

            warningGO.SetActive(false);

            // Wire to CaseRunner
            var controller = ct.Find("CaseController");
            if (controller != null)
            {
                var runner = controller.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("boardWarningText").objectReferenceValue = tmp;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  BoardWarningText created and wired to CaseRunner.");
                }
            }
        }

        static void CreateRestartButton(Transform ct)
        {
            var levelPanel = ct.Find("LevelCompletePanel");
            if (levelPanel == null) { Debug.LogWarning("LevelCompletePanel not found."); return; }

            var existing = levelPanel.Find("RestartButton");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var btnGO = new GameObject("RestartButton", typeof(RectTransform));
            btnGO.transform.SetParent(levelPanel, false);

            var rt = (RectTransform)btnGO.transform;
            rt.anchorMin = new Vector2(0.35f, 0.08f);
            rt.anchorMax = new Vector2(0.65f, 0.20f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 1f, 1f);

            var btn = btnGO.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "Restart";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize  = 32;
            tmp.color     = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            // Wire to CaseRunner
            var controller = ct.Find("CaseController");
            if (controller != null)
            {
                var runner = controller.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("restartButton").objectReferenceValue = btn;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  RestartButton created and wired to CaseRunner.");
                }
            }
        }

        static void Assign(Transform ct, string path, Sprite sprite)
        {
            if (sprite == null) { Debug.LogWarning($"Sprite not found for {path}"); return; }
            var t = ct.Find(path);
            if (t == null) { Debug.LogWarning($"Path not found: {path}"); return; }
            var img = t.GetComponent<Image>();
            if (img == null) { Debug.LogWarning($"No Image component at: {path}"); return; }
            var so = new SerializedObject(img);
            so.FindProperty("m_Sprite").objectReferenceValue = sprite;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(img);
            Debug.Log($"  Assigned {sprite.name} → {path}");
        }
    }
}
