using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Student Info Panel
    ///
    /// Creates StudentInfoPanel on GameCanvas — shown after sound choice, before intro.
    /// Students enter name and class before the investigation begins.
    /// Safe to re-run — destroys and recreates.
    /// </summary>
    public static class StudentInfoPanelBuilder
    {
        [MenuItem("ETEC510/Build Student Info Panel")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }

            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found."); return; }

            // Remove existing
            var existing = canvasGO.transform.Find("StudentInfoPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Root panel — semi-transparent dark overlay ────────────────────
            var panelGO = new GameObject("StudentInfoPanel", typeof(RectTransform));
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRT = (RectTransform)panelGO.transform;
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.offsetMin = panelRT.offsetMax = Vector2.zero;
            var panelBg = panelGO.AddComponent<Image>();
            panelBg.color = new Color(0f, 0f, 0f, 0.75f);
            panelGO.SetActive(false);

            // ── Border frame (1 px larger on each side, accent colour) ────────
            var border = new GameObject("Border", typeof(RectTransform));
            border.transform.SetParent(panelGO.transform, false);
            var borderRT = (RectTransform)border.transform;
            borderRT.anchorMin = borderRT.anchorMax = borderRT.pivot = new Vector2(0.5f, 0.5f);
            borderRT.sizeDelta      = new Vector2(516f, 476f);
            borderRT.anchoredPosition = Vector2.zero;
            border.AddComponent<Image>().color = new Color(0.30f, 0.55f, 0.90f, 1f);

            // ── Card ───────────────────────────────────────────────────────────
            var card = new GameObject("Card", typeof(RectTransform));
            card.transform.SetParent(border.transform, false);
            var cardRT = (RectTransform)card.transform;
            cardRT.anchorMin = cardRT.anchorMax = cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.sizeDelta = new Vector2(508f, 468f);
            cardRT.anchoredPosition = Vector2.zero;
            card.AddComponent<Image>().color = new Color(0.06f, 0.08f, 0.18f, 1f);

            // ── Header bar ────────────────────────────────────────────────────
            var header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(card.transform, false);
            var headerRT = (RectTransform)header.transform;
            headerRT.anchorMin = new Vector2(0f, 0.82f);
            headerRT.anchorMax = Vector2.one;
            headerRT.offsetMin = headerRT.offsetMax = Vector2.zero;
            header.AddComponent<Image>().color = new Color(0.12f, 0.25f, 0.55f, 1f);

            // Title inside header
            MakeTMP(headerRT, "Title", "DETECTIVE REGISTRATION", 28, FontStyles.Bold,
                new Vector2(0.04f, 0.1f), new Vector2(0.96f, 0.9f));

            // Subtitle below header
            var sub = MakeTMP(cardRT, "Subtitle", "Enter your details before the investigation begins.", 17, FontStyles.Normal,
                new Vector2(0.05f, 0.73f), new Vector2(0.95f, 0.82f));
            sub.color = new Color(0.65f, 0.78f, 1f, 1f);

            // Name label + input
            MakeLabel(cardRT, "NameLabel", "Name", new Vector2(0.06f, 0.60f), new Vector2(0.94f, 0.70f));
            var nameField = MakeInputField(cardRT, "StudentNameField", "Enter your name...",
                new Vector2(0.06f, 0.48f), new Vector2(0.94f, 0.61f));

            // Class label + input
            MakeLabel(cardRT, "ClassLabel", "Class / Period", new Vector2(0.06f, 0.36f), new Vector2(0.94f, 0.46f));
            var classField = MakeInputField(cardRT, "StudentClassField", "Enter your class...",
                new Vector2(0.06f, 0.24f), new Vector2(0.94f, 0.37f));

            // Begin button — standard game button style
            var btnGO = new GameObject("StudentStartButton", typeof(RectTransform));
            btnGO.transform.SetParent(card.transform, false);
            var btnRT = (RectTransform)btnGO.transform;
            btnRT.anchorMin = new Vector2(0.15f, 0.05f);
            btnRT.anchorMax = new Vector2(0.85f, 0.19f);
            btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
            btnGO.AddComponent<Image>().color = new Color(0.18f, 0.45f, 0.85f, 1f);
            var startBtn = btnGO.AddComponent<Button>();

            var btnLbl = new GameObject("Label", typeof(RectTransform));
            btnLbl.transform.SetParent(btnGO.transform, false);
            var btnLblRT = (RectTransform)btnLbl.transform;
            btnLblRT.anchorMin = Vector2.zero; btnLblRT.anchorMax = Vector2.one;
            btnLblRT.offsetMin = btnLblRT.offsetMax = Vector2.zero;
            var btnTMP = btnLbl.AddComponent<TextMeshProUGUI>();
            btnTMP.text      = "Begin Investigation";
            btnTMP.fontSize  = 28;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;
            btnTMP.color     = Color.white;

            // ── Position panel just after SoundTogglePanel ─────────────────────
            var soundPanel = canvasGO.transform.Find("SoundTogglePanel");
            int idx = soundPanel != null ? soundPanel.GetSiblingIndex() + 1 : 0;
            panelGO.transform.SetSiblingIndex(idx);

            // ── Wire CaseRunner ────────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("studentInfoPanel").objectReferenceValue   = panelGO;
            so.FindProperty("studentNameField").objectReferenceValue   = nameField;
            so.FindProperty("studentClassField").objectReferenceValue  = classField;
            so.FindProperty("studentStartButton").objectReferenceValue = startBtn;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: StudentInfoPanel built. Press Ctrl+S to save.");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        static TextMeshProUGUI MakeTMP(RectTransform parent, string name, string text,
            float size, FontStyles style, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = size;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        static void MakeLabel(RectTransform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color     = new Color(0.7f, 0.8f, 1f, 1f);
        }

        static TMP_InputField MakeInputField(RectTransform parent, string name,
            string placeholder, Vector2 anchorMin, Vector2 anchorMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin; rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.14f, 1f);
            var field = go.AddComponent<TMP_InputField>();

            // Text area
            var textArea = new GameObject("Text Area", typeof(RectTransform));
            textArea.transform.SetParent(go.transform, false);
            var taRT = (RectTransform)textArea.transform;
            taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
            taRT.offsetMin = new Vector2(8, 4); taRT.offsetMax = new Vector2(-8, -4);
            textArea.AddComponent<RectMask2D>();

            // Placeholder
            var ph = new GameObject("Placeholder", typeof(RectTransform));
            ph.transform.SetParent(textArea.transform, false);
            var phRT = (RectTransform)ph.transform;
            phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
            phRT.offsetMin = phRT.offsetMax = Vector2.zero;
            var phTMP = ph.AddComponent<TextMeshProUGUI>();
            phTMP.text      = placeholder;
            phTMP.fontSize  = 20;
            phTMP.color     = new Color(0.5f, 0.55f, 0.7f, 1f);
            phTMP.fontStyle = FontStyles.Italic;
            phTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Input text
            var txt = new GameObject("Text", typeof(RectTransform));
            txt.transform.SetParent(textArea.transform, false);
            var txtRT = (RectTransform)txt.transform;
            txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
            var txtTMP = txt.AddComponent<TextMeshProUGUI>();
            txtTMP.fontSize  = 20;
            txtTMP.color     = Color.white;
            txtTMP.alignment = TextAlignmentOptions.MidlineLeft;

            field.textViewport   = taRT;
            field.placeholder    = phTMP;
            field.textComponent  = txtTMP;
            field.characterLimit = 60;

            return field;
        }
    }
}
