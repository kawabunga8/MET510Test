using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Clean Up Old Scene Objects
    /// Removes root GameObjects that are not part of the current architecture.
    /// Keeps: Main Camera, Directional Light, EventSystem, GameCanvas.
    /// </summary>
    public static class SceneCleanup
    {
        static readonly HashSet<string> Keep = new HashSet<string>
        {
            "Main Camera",
            "Directional Light",
            "EventSystem",
            "GameCanvas",
        };

        [MenuItem("ETEC510/Clean Up Old Scene Objects")]
        public static void Cleanup()
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            int removed = 0;

            foreach (var go in roots)
            {
                // Strip any invisible Unicode from the name before comparing
                var cleanName = go.name.Trim()
                    .Replace("\u202F", "").Replace("\u2060", "").Replace("\uFEFF", "")
                    .Trim();

                if (!Keep.Contains(cleanName))
                {
                    Debug.Log($"ETEC510: Removing old object '{go.name}' (cleaned: '{cleanName}')");
                    Undo.DestroyObjectImmediate(go);
                    removed++;
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"ETEC510: Cleanup done — removed {removed} object(s). Save with Ctrl+S.");
        }
    }
}
