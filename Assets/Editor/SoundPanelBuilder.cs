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

            var bgImg = panelGO.AddComponent<Image>();
            bgImg.color = DetectiveStyleGuide.BgDeep;

            // ── Case file badge label ─────────────────────────────────────────
            var badgeGO  = new GameObject("BadgeText", typeof(RectTransform));
            badgeGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(badgeGO, 0.25f, 0.78f, 0.75f, 0.88f);
            var badgeTMP = badgeGO.AddComponent<TextMeshProUGUI>();
            badgeTMP.text = "— DETECTIVE HQ —";
            DetectiveStyleGuide.ApplyTextStyle(badgeTMP, DetectiveStyleGuide.TextRole.Badge);

            // ── Title ─────────────────────────────────────────────────────────
            var titleGO  = new GameObject("SoundTitleText", typeof(RectTransform));
            titleGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(titleGO, 0.08f, 0.60f, 0.92f, 0.80f);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "Before you begin...";
            DetectiveStyleGuide.ApplyTextStyle(titleTMP, DetectiveStyleGuide.TextRole.Title);

            // ── Question box ──────────────────────────────────────────────────
            var boxGO  = new GameObject("SoundQuestionBox", typeof(RectTransform));
            boxGO.transform.SetParent(panelGO.transform, false);
            SetAnchors(boxGO, 0.12f, 0.46f, 0.88f, 0.62f);
            var boxImg = boxGO.AddComponent<Image>();
            boxImg.color = DetectiveStyleGuide.BgPanel;

            var subGO  = new GameObject("SoundSubtitleText", typeof(RectTransform));
            subGO.transform.SetParent(boxGO.transform, false);
            SetAnchors(subGO, 0.03f, 0.05f, 0.97f, 0.95f);
            var subTMP = subGO.AddComponent<TextMeshProUGUI>();
            subTMP.text = "Will you need audio, Detective?";
            DetectiveStyleGuide.ApplyTextStyle(subTMP, DetectiveStyleGuide.TextRole.Question);

            // ── Sound On button (left) ────────────────────────────────────────
            var soundOnGO  = CreateButton(panelGO.transform, "SoundOnButton",
                0.22f, 0.25f, 0.46f, 0.38f, "Yes, sound on",
                DetectiveStyleGuide.ButtonRole.Confirm);

            // ── Sound Off button (right) ──────────────────────────────────────
            var soundOffGO = CreateButton(panelGO.transform, "SoundOffButton",
                0.54f, 0.25f, 0.78f, 0.38f, "No, silence",
                DetectiveStyleGuide.ButtonRole.Alternate);

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
            string label, DetectiveStyleGuide.ButtonRole role)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetAnchors(go, xMin, yMin, xMax, yMax);
            go.AddComponent<Image>();
            go.AddComponent<Button>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;

            DetectiveStyleGuide.StyleButton(go, role);
            return go;
        }
    }
}
