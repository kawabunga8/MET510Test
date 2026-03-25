using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Add Code Sequence Label
    /// Adds a "CODE SEQUENCE:" label to the left of the digit row on the Evidence Board.
    /// Also shifts the digit container right to make balanced room.
    /// Safe to re-run.
    /// </summary>
    public static class EvidenceBoardLabelAdder
    {
        [MenuItem("ETEC510/Add Code Sequence Label")]
        public static void AddLabel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var boardT = canvasGO.transform.Find("EvidenceBoardPanel");
            if (boardT == null) { Debug.LogError("ETEC510: EvidenceBoardPanel not found."); return; }

            // Find the digit container (parent of Digit1Text)
            RectTransform digitContainer = null;
            foreach (RectTransform child in boardT)
            {
                if (child.Find("Digit1Text") != null) { digitContainer = child; break; }
            }
            if (digitContainer == null) { Debug.LogError("ETEC510: Digit container not found."); return; }

            // Remove old label if re-running
            var old = boardT.Find("CodeSequenceLabel");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            // Shift digit container right slightly to give label room
            // Original: x 0.30-0.70 — shift to x 0.45-0.95
            digitContainer.anchorMin = new Vector2(0.45f, digitContainer.anchorMin.y);
            digitContainer.anchorMax = new Vector2(0.95f, digitContainer.anchorMax.y);
            digitContainer.offsetMin = Vector2.zero;
            digitContainer.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(digitContainer.gameObject);

            // Add "CODE SEQUENCE:" label to the left
            var labelGO = new GameObject("CodeSequenceLabel", typeof(RectTransform));
            labelGO.transform.SetParent(boardT, false);
            var rt = (RectTransform)labelGO.transform;
            rt.anchorMin = new Vector2(0.03f, digitContainer.anchorMin.y);
            rt.anchorMax = new Vector2(0.43f, digitContainer.anchorMax.y);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text             = "CODE SEQUENCE:";
            tmp.alignment        = TextAlignmentOptions.MidlineRight;
            tmp.fontSize         = 22;
            tmp.color            = new Color(0.72f, 0.57f, 0.22f, 1f); // gold
            tmp.fontStyle        = FontStyles.Bold;
            tmp.characterSpacing = 3f;

            // Try to use mono font to match the rest of the UI
            var guids = AssetDatabase.FindAssets("t:TMP_FontAsset LiberationMono");
            if (guids.Length == 0) guids = AssetDatabase.FindAssets("t:TMP_FontAsset RobotoMono");
            if (guids.Length > 0)
            {
                var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    AssetDatabase.GUIDToAssetPath(guids[0]));
                if (f != null) tmp.font = f;
            }

            EditorUtility.SetDirty(labelGO);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Code Sequence label added. Press Ctrl+S to save.");
        }
    }
}
