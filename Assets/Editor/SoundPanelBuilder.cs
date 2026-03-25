using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Sound Toggle Panel
    /// Creates a SoundTogglePanel that appears before the intro video.
    /// Player chooses Sound On or Sound Off, then the startup video begins.
    /// Safe to re-run (destroys and recreates the panel).
    /// </summary>
    public static class SoundPanelBuilder
    {
        [MenuItem("ETEC510/Build Sound Toggle Panel")]
        public static void BuildSoundTogglePanel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // Remove existing
            var existing = ct.Find("SoundTogglePanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel ────────────────────────────────────────────────────
            var panelGO = new GameObject("SoundTogglePanel", typeof(RectTransform));
            panelGO.transform.SetParent(ct, false);
            panelGO.transform.SetSiblingIndex(0);   // on top of siblings

            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = Vector2.zero;
            panelRT.offsetMax = Vector2.zero;

            // Deep charcoal background — noir detective atmosphere
            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.05f, 0.07f, 1f);

            // ── Case file badge label ─────────────────────────────────────────
            var badgeGO = new GameObject("BadgeText", typeof(RectTransform));
            badgeGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(badgeGO, 0.25f, 0.78f, 0.75f, 0.88f);
            var badgeTMP = badgeGO.AddComponent<TextMeshProUGUI>();
            badgeTMP.text            = "— DETECTIVE HQ —";
            badgeTMP.alignment       = TextAlignmentOptions.Center;
            badgeTMP.fontSize        = 22;
            badgeTMP.color           = new Color(0.72f, 0.56f, 0.22f, 1f);   // aged gold
            badgeTMP.characterSpacing = 6f;
            badgeTMP.fontStyle       = FontStyles.Bold | FontStyles.UpperCase;

            // ── Title ─────────────────────────────────────────────────────────
            var titleGO = new GameObject("SoundTitleText", typeof(RectTransform));
            titleGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(titleGO, 0.08f, 0.60f, 0.92f, 0.80f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text      = "Before you begin...";
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.fontSize  = 56;
            titleTMP.color     = new Color(0.92f, 0.88f, 0.72f, 1f);   // parchment
            titleTMP.fontStyle = FontStyles.Bold | FontStyles.Italic;

            // ── Question ──────────────────────────────────────────────────────
            var subGO = new GameObject("SoundSubtitleText", typeof(RectTransform));
            subGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(subGO, 0.08f, 0.44f, 0.92f, 0.62f);
            var subTMP = subGO.AddComponent<TextMeshProUGUI>();
            subTMP.text      = "Will you need audio, Detective?";
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.fontSize  = 46;
            subTMP.color     = Color.white;

            // ── Sound On button (left, smaller) ───────────────────────────────
            var soundOnGO  = CreateButton(panelGO.transform, "SoundOnButton",
                0.22f, 0.25f, 0.46f, 0.38f,
                "Yes, sound on",
                new Color(0.14f, 0.30f, 0.16f, 1f),    // dark forest green
                new Color(0.72f, 0.56f, 0.22f, 1f));    // gold border tint

            // ── Sound Off button (right, smaller) ────────────────────────────
            var soundOffGO = CreateButton(panelGO.transform, "SoundOffButton",
                0.54f, 0.25f, 0.78f, 0.38f,
                "No, silence",
                new Color(0.22f, 0.14f, 0.10f, 1f),    // dark sepia
                new Color(0.72f, 0.56f, 0.22f, 1f));    // gold border tint

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var controller = ct.Find("CaseController");
            if (controller != null)
            {
                var runner = controller.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    so.FindProperty("soundTogglePanel").objectReferenceValue = panelGO;
                    so.FindProperty("soundOnButton").objectReferenceValue    = soundOnGO.GetComponent<Button>();
                    so.FindProperty("soundOffButton").objectReferenceValue   = soundOffGO.GetComponent<Button>();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  soundTogglePanel, soundOnButton, soundOffButton wired on CaseRunner.");
                }
                else Debug.LogWarning("ETEC510: CaseRunner not found on CaseController.");
            }
            else Debug.LogWarning("ETEC510: CaseController not found.");

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Sound Toggle Panel built. Press Ctrl+S to save.");
        }

        static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(xMin, yMin);
            rt.anchorMax = new Vector2(xMax, yMax);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static GameObject CreateButton(Transform parent, string name,
            float xMin, float yMin, float xMax, float yMax,
            string label, Color bgColor, Color labelColor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            go.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text             = label;
            tmp.alignment        = TextAlignmentOptions.Center;
            tmp.fontSize         = 32;
            tmp.color            = labelColor;
            tmp.fontStyle        = FontStyles.Bold;
            tmp.characterSpacing = 2f;

            return go;
        }
    }
}
