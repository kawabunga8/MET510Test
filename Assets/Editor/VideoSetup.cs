using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Setup Intro Video RT
    /// Creates a RenderTexture asset and wires it to the VideoPlayer + VideoBackground RawImage.
    /// Run once after building the scene. Removes the need for dynamic RT creation at runtime.
    /// </summary>
    public static class VideoSetup
    {
        const string RtPath = "Assets/Video/IntroRT.renderTexture";

        [MenuItem("ETEC510/Setup Intro Video RT")]
        public static void SetupVideoRT()
        {
            // ── Create or load the RenderTexture asset ────────────────────────
            var rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(RtPath);
            if (rt == null)
            {
                rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32);
                rt.name = "IntroRT";
                AssetDatabase.CreateAsset(rt, RtPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"ETEC510: Created RenderTexture at {RtPath}");
            }

            // ── Wire VideoPlayer.targetTexture ────────────────────────────────
            var introPanelGO = GameObject.Find("IntroPanel");
            if (introPanelGO == null) { Debug.LogError("ETEC510: IntroPanel not found."); return; }

            var vp = introPanelGO.GetComponent<VideoPlayer>();
            if (vp == null) { Debug.LogError("ETEC510: VideoPlayer not found on IntroPanel."); return; }

            var vpSO = new SerializedObject(vp);
            vpSO.FindProperty("m_TargetTexture").objectReferenceValue = rt;
            vpSO.ApplyModifiedProperties();

            // ── Wire RawImage.texture ─────────────────────────────────────────
            var videoBgTransform = introPanelGO.transform.Find("VideoBackground");
            if (videoBgTransform == null) { Debug.LogError("ETEC510: VideoBackground not found."); return; }

            var rawImage = videoBgTransform.GetComponent<RawImage>();
            if (rawImage == null) { Debug.LogError("ETEC510: RawImage not found on VideoBackground."); return; }

            var riSO = new SerializedObject(rawImage);
            riSO.FindProperty("m_Texture").objectReferenceValue = rt;
            riSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(vp);
            EditorUtility.SetDirty(rawImage);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log("ETEC510: Intro video RT wired. Save scene with Ctrl+S.");
        }
    }
}
