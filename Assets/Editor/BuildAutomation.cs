using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ETEC510.Editor
{
    public static class BuildAutomation
    {
        private const string ScenePath = "Assets/Scenes/Case01_DetectiveRoom.unity";
        private const string BuildFolder = "Builds/PC";
        private const string ExeName = "DigitalDetectiveToolkit.exe";

        [MenuItem("ETEC510/Build PC Windows x64")]
        public static void BuildPC()
        {
            var options = new BuildPlayerOptions
            {
                scenes       = new[] { ScenePath },
                locationPathName = System.IO.Path.Combine(BuildFolder, ExeName),
                target       = BuildTarget.StandaloneWindows64,
                options      = BuildOptions.None,
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
                Debug.Log($"[BuildAutomation] PC build succeeded → {options.locationPathName}  ({report.summary.totalSize / 1_000_000} MB)");
            else
                Debug.LogError($"[BuildAutomation] PC build FAILED: {report.summary.result}");
        }
    }
}
