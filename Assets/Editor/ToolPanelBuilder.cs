using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Tool Panels
    ///
    /// Creates three full-screen tool panels under GameCanvas:
    ///   AnalyzeAccountPanel    — background: "3) Account Profile Tool.png"
    ///   EvaluateCommentsPanel  — background: "4) Comments Section Tool.png"
    ///   ExtractMetaDataPanel   — background: "5) MetaData Tool.png"
    ///
    /// Each panel has one invisible Button overlaid on the "Return" button
    /// printed in the background artwork (bottom-left corner).
    ///
    /// Wires CaseRunner fields:
    ///   analyzeAccountPanel / analyzeAccountBackButton
    ///   evaluateCommentsPanel / evaluateCommentsBackButton
    ///   extractMetaDataPanel / extractMetaDataBackButton
    ///
    /// Safe to re-run — destroys and recreates panels.
    /// </summary>
    public static class ToolPanelBuilder
    {
        [MenuItem("ETEC510/Build Tool Panels")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            var so = new SerializedObject(runner);

            BuildPanel(canvasGO.transform, so,
                "AnalyzeAccountPanel",
                "3) Account Profile Tool",
                "analyzeAccountPanel",
                "analyzeAccountBackButton");

            BuildPanel(canvasGO.transform, so,
                "EvaluateCommentsPanel",
                "4) Comments Section Tool",
                "evaluateCommentsPanel",
                "evaluateCommentsBackButton");

            BuildPanel(canvasGO.transform, so,
                "ExtractMetaDataPanel",
                "5) MetaData Tool",
                "extractMetaDataPanel",
                "extractMetaDataBackButton");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Tool panels built. Press Ctrl+S to save.");
        }

        static void BuildPanel(Transform canvasT, SerializedObject so,
            string panelName, string bgAssetName,
            string panelField, string backField)
        {
            // Remove existing
            var existing = canvasT.Find(panelName);
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // Root — full stretch
            var panelRT = NewStretch(panelName, canvasT);
            panelRT.gameObject.SetActive(false);

            // Background image
            var bgImg = panelRT.gameObject.AddComponent<Image>();
            bgImg.raycastTarget = true;
            var sprite = FindSprite(bgAssetName);
            if (sprite != null)
            {
                bgImg.sprite          = sprite;
                bgImg.type            = Image.Type.Simple;
                bgImg.preserveAspect  = false;
            }
            else
            {
                bgImg.color = new Color(0.06f, 0.06f, 0.10f, 1f);
                Debug.LogWarning($"ETEC510: Sprite '{bgAssetName}' not found — using solid colour.");
            }

            // Invisible Return button — bottom-left, over the Return graphic in the background
            // Adjust anchors in Inspector to align precisely with the artwork.
            var backGO = NewChild("ReturnButton", panelRT);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.01f, 0.03f);
            backRT.anchorMax = new Vector2(0.20f, 0.14f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;
            var backImg = backGO.AddComponent<Image>();
            backImg.color         = Color.clear; // invisible
            backImg.raycastTarget = true;
            backGO.AddComponent<Button>();

            // Position just after Badge1Panel in sibling order
            var badge1 = canvasT.Find("Badge1Panel");
            int targetIndex = badge1 != null ? badge1.GetSiblingIndex() + 1 : canvasT.childCount;
            panelRT.transform.SetSiblingIndex(targetIndex);

            // Wire
            so.FindProperty(panelField).objectReferenceValue = panelRT.gameObject;
            so.FindProperty(backField).objectReferenceValue  = backGO.GetComponent<Button>();

            Debug.Log($"ETEC510: Built {panelName}.");
        }

        static RectTransform NewStretch(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            return rt;
        }

        static GameObject NewChild(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static Sprite FindSprite(string assetName)
        {
            var guids = AssetDatabase.FindAssets($"{assetName} t:Texture2D", new[] { "Assets/Images" });
            if (guids.Length == 0) return null;
            return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
