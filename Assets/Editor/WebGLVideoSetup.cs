using UnityEngine;
using UnityEditor;
using System.IO;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Setup WebGL Videos
    /// Copies all VideoClip assets from Assets/Video/ into StreamingAssets/Video/
    /// so they are accessible via URL in WebGL builds.
    /// Safe to re-run — skips files that already exist.
    /// </summary>
    public static class WebGLVideoSetup
    {
        [MenuItem("ETEC510/Setup WebGL Videos")]
        public static void CopyVideosToStreamingAssets()
        {
            var destDir = Path.Combine(Application.streamingAssetsPath, "Video");
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var guids = AssetDatabase.FindAssets("t:VideoClip", new[] { "Assets" });
            int copied = 0, skipped = 0;

            foreach (var guid in guids)
            {
                var srcPath = AssetDatabase.GUIDToAssetPath(guid);
                var filename = Path.GetFileName(srcPath);
                var destPath = Path.Combine(destDir, filename);

                // Full source path on disk
                var fullSrc = Path.GetFullPath(srcPath);

                if (File.Exists(destPath))
                {
                    skipped++;
                    continue;
                }

                File.Copy(fullSrc, destPath);
                copied++;
                Debug.Log($"  Copied: {filename}");
            }

            AssetDatabase.Refresh();
            Debug.Log($"ETEC510: WebGL video setup complete — {copied} copied, {skipped} already present.");
            Debug.Log("  Set each CaseData '*VideoFile' field to the filename (e.g. 'StartupVideo.mp4').");
        }
    }
}
