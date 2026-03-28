using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Restyle Tool Panel Buttons
    /// Repositions AccountProfilePanelButton, CommentsSectionPanelButton, and
    /// MetaDataPanelButton to match MotivePromptText's anchor area and styles
    /// them identically to MotiveOptionButton1 (Choice role).
    /// </summary>
    public static class ToolButtonRestyler
    {
        // Right side, above the answer buttons — clearly separated as a tool
        static readonly Vector2 AnchorMin = new Vector2(0.52f, 0.52f);
        static readonly Vector2 AnchorMax = new Vector2(0.98f, 0.64f);

        static readonly string[] ButtonNames =
        {
            "AccountProfilePanelButton",
            "CommentsSectionPanelButton",
            "MetaDataPanelButton",
        };

        [MenuItem("ETEC510/Restyle Tool Panel Buttons")]
        public static void Restyle()
        {
            int done = 0;

            foreach (var btnName in ButtonNames)
            {
                var btnGO = FindInactive(btnName);
                if (btnGO == null)
                {
                    Debug.LogWarning($"ETEC510: {btnName} not found — skipping.");
                    continue;
                }

                // Reposition to MotivePromptText area
                var rt = (RectTransform)btnGO.transform;
                rt.anchorMin = AnchorMin;
                rt.anchorMax = AnchorMax;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                // Apply Tool style — teal, visually distinct from answer buttons
                DetectiveStyleGuide.StyleButton(btnGO, DetectiveStyleGuide.ButtonRole.Tool);

                EditorUtility.SetDirty(btnGO);
                Debug.Log($"ETEC510: Restyled {btnName}.");
                done++;
            }

            if (done > 0)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"ETEC510: {done} tool buttons restyled. Press Ctrl+S to save.");
        }

        static GameObject FindInactive(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name) return t.gameObject;
            return null;
        }
    }
}
