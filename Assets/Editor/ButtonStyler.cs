using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Apply Button Styles
    /// Walks every Button in GameCanvas and applies the DetectiveStyleGuide palette.
    /// Role is inferred from the GameObject name — re-run any time after layout changes.
    /// </summary>
    public static class ButtonStyler
    {
        [MenuItem("ETEC510/Apply Button Styles")]
        public static void ApplyButtonStyles()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            int count = 0;
            foreach (var btn in canvasGO.GetComponentsInChildren<Button>(includeInactive: true))
            {
                var role = DetectiveStyleGuide.InferRole(btn.gameObject.name);
                DetectiveStyleGuide.StyleButton(btn.gameObject, role);
                EditorUtility.SetDirty(btn.gameObject);
                Debug.Log($"  [{role}] {btn.gameObject.name}");
                count++;
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"ETEC510: Styled {count} buttons. Press Ctrl+S to save.");
        }
    }
}
