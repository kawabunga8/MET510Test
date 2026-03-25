using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    public static class SpriteAssigner
    {
        [MenuItem("ETEC510/Assign All Sprites")]
        public static void AssignSprites()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var map          = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/map.png");
            var chief        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Chief silouetter.png");
            var detBg        = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/BriefingRoom16_9.png");
            var detBgTech    = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/Detective background TECH.png");

            Assign(ct, "EvidenceBoardPanel/EvidenceBoardImage",       map);
            Assign(ct, "MissionBriefingPanel/BriefingImage",          detBg);
            Assign(ct, "HintsFromChiefPanel/ChiefImage",              chief);
            Assign(ct, "LevelCompletePanel/CompletionImage",          detBgTech);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: All sprites assigned.");
        }

        static void Assign(Transform ct, string path, Sprite sprite)
        {
            if (sprite == null) { Debug.LogWarning($"Sprite not found for {path}"); return; }
            var t = ct.Find(path);
            if (t == null) { Debug.LogWarning($"Path not found: {path}"); return; }
            var img = t.GetComponent<Image>();
            if (img == null) { Debug.LogWarning($"No Image component at: {path}"); return; }
            var so = new SerializedObject(img);
            so.FindProperty("m_Sprite").objectReferenceValue = sprite;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(img);
            Debug.Log($"  Assigned {sprite.name} → {path}");
        }
    }
}
