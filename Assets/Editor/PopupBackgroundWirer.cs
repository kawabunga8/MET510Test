using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Wire Popup Backgrounds
    /// Assigns incorrectBackgroundSprite on PopupController using
    /// "8) Badge 1 Try Again.png" from Assets/Images/.
    /// Also wires cardBackground to the Image component on the popup card.
    /// Safe to re-run.
    /// </summary>
    public static class PopupBackgroundWirer
    {
        [MenuItem("ETEC510/Wire Popup Backgrounds")]
        public static void Wire()
        {
            var popup = Object.FindFirstObjectByType<PopupController>();
            if (popup == null) { Debug.LogError("ETEC510: PopupController not found in scene."); return; }

            // Find the incorrect background sprite
            var guids = AssetDatabase.FindAssets("8) Badge 1 Try Again t:Texture2D", new[] { "Assets/Images" });
            if (guids.Length == 0)
            {
                Debug.LogError("ETEC510: Could not find '8) Badge 1 Try Again' in Assets/Images.");
                return;
            }
            var path   = AssetDatabase.GUIDToAssetPath(guids[0]);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                // Texture may not be set to Sprite type — load as Texture2D and warn
                Debug.LogError($"ETEC510: Asset at '{path}' is not imported as Sprite. " +
                               "Select it in the Project window, set Texture Type = Sprite, and re-run.");
                return;
            }

            var so = new SerializedObject(popup);

            // Wire incorrectBackgroundSprite
            so.FindProperty("incorrectBackgroundSprite").objectReferenceValue = sprite;

            // Auto-discover cardBackground if not already set
            if (so.FindProperty("cardBackground").objectReferenceValue == null && popup.popupCard != null)
            {
                var img = popup.popupCard.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    so.FindProperty("cardBackground").objectReferenceValue = img;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(popup);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"ETEC510: incorrectBackgroundSprite set to '{sprite.name}'. Press Ctrl+S to save.");
        }
    }
}
