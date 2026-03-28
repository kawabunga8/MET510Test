using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Setup WebGL Videos
    /// Prepares the project for a WebGL build:
    ///   1. Clears all VideoClip references from VideoPlayer components in the scene.
    ///      (WebGL cannot play embedded clips — runtime code loads via StreamingAssets URL.)
    ///   2. Copies video files from Assets/Video/ into StreamingAssets/Video/ if missing.
    ///
    /// Run this before every WebGL build to avoid the
    /// "Embedded video clips are not supported by the WebGL player" warning.
    /// </summary>
    public static class WebGLVideoSetup
    {
        [MenuItem("ETEC510/Setup WebGL Videos")]
        public static void PrepareForWebGL()
        {
            // ── 1. Clear VideoClip references from all VideoPlayers in the scene ──
            int cleared = 0;
            foreach (var vp in Object.FindObjectsByType<VideoPlayer>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (vp.clip != null)
                {
                    Debug.Log($"  Clearing clip from VideoPlayer on '{vp.gameObject.name}': {vp.clip.name}");
                    vp.clip = null;
                    EditorUtility.SetDirty(vp);
                    cleared++;
                }
            }

            if (cleared > 0)
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"ETEC510: Cleared {cleared} VideoPlayer clip(s). Save the scene (Ctrl+S) before building.");

            // ── 2. Copy video files to StreamingAssets/Video if missing ───────────
            var destDir = Path.Combine(Application.streamingAssetsPath, "Video");
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var guids = AssetDatabase.FindAssets("t:VideoClip", new[] { "Assets" });
            int copied = 0, skipped = 0;

            foreach (var guid in guids)
            {
                var srcPath  = AssetDatabase.GUIDToAssetPath(guid);
                var filename = Path.GetFileName(srcPath);
                var destPath = Path.Combine(destDir, filename);
                var fullSrc  = Path.GetFullPath(srcPath);

                if (File.Exists(destPath)) { skipped++; continue; }

                File.Copy(fullSrc, destPath);
                copied++;
                Debug.Log($"  Copied to StreamingAssets: {filename}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"ETEC510: StreamingAssets — {copied} copied, {skipped} already present.");
        }
    }
}
