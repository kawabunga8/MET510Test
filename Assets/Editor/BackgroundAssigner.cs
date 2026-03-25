using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Assign Panel Backgrounds
    /// Sets CodeEnterScreen2 on PasswordLockPanel and Detective background TECH on LevelCompletePanel.
    /// Also wires completionMusic on all CaseRunner instances.
    /// Safe to re-run.
    /// </summary>
    public static class BackgroundAssigner
    {
        static Transform FindInactive(string name)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go.name == name && go.scene.IsValid()) return go.transform;
            return null;
        }

        [MenuItem("ETEC510/Assign Panel Backgrounds")]
        public static void AssignBackgrounds()
        {
            // ── PasswordLockPanel > PasswordBgImage ───────────────────────────
            var passwordPanel = FindInactive("PasswordLockPanel");
            if (passwordPanel != null)
            {
                var bgT = passwordPanel.Find("PasswordBgImage") ?? passwordPanel;
                SetBackground(bgT, "Assets/Images/CodeEnterScreen2.png", "PasswordBgImage");
            }
            else Debug.LogWarning("ETEC510: PasswordLockPanel not found.");

            // ── LevelCompletePanel > BgFill ───────────────────────────────────
            var levelPanel = FindInactive("LevelCompletePanel");
            if (levelPanel != null)
            {
                var bgFill = levelPanel.Find("BgFill");
                if (bgFill != null)
                    SetBackground(bgFill, "Assets/Images/Detective background TECH.png", "LevelCompletePanel BgFill");
                else
                    Debug.LogWarning("  BgFill not found inside LevelCompletePanel.");
            }
            else Debug.LogWarning("ETEC510: LevelCompletePanel not found.");

            // ── CaseRunner.completionMusic — wire ALL instances ───────────────
            var clip = LoadAudioClip("Shadows in the File");
            if (clip != null)
            {
                var runners = Resources.FindObjectsOfTypeAll<ETEC510.UI.CaseRunner>();
                foreach (var runner in runners)
                {
                    if (!runner.gameObject.scene.IsValid()) continue;
                    var so = new SerializedObject(runner);
                    so.FindProperty("completionMusic").objectReferenceValue = clip;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log($"  CaseRunner '{runner.gameObject.name}' completionMusic = 'Shadows in the File'.");
                }
            }
            else Debug.LogWarning("  'Shadows in the File' audio clip not found.");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Panel backgrounds + music assigned. Press Ctrl+S to save.");
        }

        static void SetBackground(Transform target, string assetPath, string label)
        {
            // Ensure Sprite import type
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType      = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) { Debug.LogWarning($"  Sprite not found: {assetPath}"); return; }

            var img = target.GetComponent<Image>() ?? target.gameObject.AddComponent<Image>();
            img.sprite           = sprite;
            img.color            = Color.white;
            img.type             = Image.Type.Simple;
            img.preserveAspect   = false;

            // AspectRatioFitter — EnvelopeParent fills the panel and maintains image ratio
            var arf = target.GetComponent<AspectRatioFitter>() ?? target.gameObject.AddComponent<AspectRatioFitter>();
            arf.aspectMode  = AspectRatioFitter.AspectMode.EnvelopeParent;
            arf.aspectRatio = (float)sprite.texture.width / sprite.texture.height;

            EditorUtility.SetDirty(img);
            EditorUtility.SetDirty(arf);
            Debug.Log($"  {label} set to {System.IO.Path.GetFileName(assetPath)}.");
        }

        static AudioClip LoadAudioClip(string name)
        {
            var guids = AssetDatabase.FindAssets($"t:AudioClip {name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null) return clip;
            }
            return null;
        }
    }
}
