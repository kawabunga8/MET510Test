using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Fix Background Images
    /// Adds AspectRatioFitter (EnvelopeParent) to every background Image so it
    /// zooms in to fill the panel without stretching — like CSS background-size: cover.
    /// Safe to re-run.
    /// </summary>
    public static class BackgroundFitter
    {
        [MenuItem("ETEC510/Fix Background Images")]
        public static void FixBackgrounds()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            string[] paths =
            {
                "EvidenceBoardPanel/EvidenceBoardImage",
                "MissionBriefingPanel/BriefingImage",
                "LevelCompletePanel/CompletionImage",
                "HintsFromChiefPanel/ChiefImage",
            };

            foreach (var path in paths)
                ApplyEnvelopeFit(ct, path);

            CreatePanelBackground(ct, "PasswordLockPanel", "PasswordBgImage",
                "Assets/Images/CodeEnterScreen2.png");
            CreatePanelBackground(ct, "EvidenceDetailPanel", "JudgementBgImage",
                "Assets/Images/Judgement_Dark.png");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Background images set to EnvelopeParent. Press Ctrl+S to save.");
        }

        static void CreatePanelBackground(Transform ct, string panelName, string bgName, string spritePath)
        {
            var panel = ct.Find(panelName);
            if (panel == null) { Debug.LogWarning($"ETEC510: {panelName} not found."); return; }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null) { Debug.LogWarning($"ETEC510: sprite not found — {spritePath}"); return; }

            var existing = panel.Find(bgName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var bgGO = new GameObject(bgName, typeof(RectTransform));
            bgGO.transform.SetParent(panel, false);
            bgGO.transform.SetSiblingIndex(0);

            var rt = (RectTransform)bgGO.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var img = bgGO.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = false;

            var fitter = bgGO.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            EditorUtility.SetDirty(bgGO);
            Debug.Log($"  {bgName} created in {panelName}.");
        }

        static void ApplyEnvelopeFit(Transform ct, string path)
        {
            var t = ct.Find(path);
            if (t == null) { Debug.LogWarning($"ETEC510: path not found — {path}"); return; }

            // Stretch to fill parent first
            var rt = (RectTransform)t;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // preserveAspect on Image conflicts with the fitter — disable it
            var img = t.GetComponent<Image>();
            if (img != null) img.preserveAspect = false;

            // Add/update AspectRatioFitter
            var fitter = t.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = t.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            EditorUtility.SetDirty(t.gameObject);
            Debug.Log($"  EnvelopeParent → {path}");
        }
    }
}
