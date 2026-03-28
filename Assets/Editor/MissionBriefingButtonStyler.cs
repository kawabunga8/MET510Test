using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Style Mission Briefing Buttons
    /// Applies the blue pill + arrow-badge style to StartButton and BriefingRepeatButton.
    /// Safe to re-run.
    ///
    /// Button hierarchy:
    ///   ButtonGO  (Image: blue pill, Button, Shadow)
    ///     Label   (TMP: white bold, left-padded)
    ///     Arrow   (Image: dark-blue circle, right-anchored)
    ///       ArrowText (TMP: ">>" white bold)
    /// </summary>
    public static class MissionBriefingButtonStyler
    {
        // ── Palette ───────────────────────────────────────────────────────────
        static readonly Color BtnBlue       = Hex("#1A6BC8");
        static readonly Color BtnBlueDark   = Hex("#0D4A99");
        static readonly Color BtnBlueShadow = Hex("#0A3575");
        static readonly Color White         = Color.white;

        static readonly string[] ButtonNames = { "StartButton", "BriefingRepeatButton", "BriefingSkipButton" };
        static readonly string[] ButtonLabels = { "Start Investigation", "Replay Message", "Skip" };

        [MenuItem("ETEC510/Style Mission Briefing Buttons")]
        public static void StyleButtons()
        {
            var panelT = FindInactive("MissionBriefingPanel");
            if (panelT == null) { Debug.LogError("ETEC510: MissionBriefingPanel not found."); return; }

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // Anchors: x 0.22-0.78, stacked vertically
            var buttonAnchors = new[]
            {
                new Vector4(0.22f, 0.02f, 0.78f, 0.16f), // StartButton          (14% tall)
                new Vector4(0.22f, 0.17f, 0.78f, 0.31f), // BriefingRepeatButton (14% tall)
                new Vector4(0.78f, 0.02f, 0.96f, 0.16f), // BriefingSkipButton   (14% tall)
            };

            for (int i = 0; i < ButtonNames.Length; i++)
            {
                var btnT = panelT.Find(ButtonNames[i]);
                if (btnT == null)
                {
                    // Create the missing button
                    var newGO = new GameObject(ButtonNames[i], typeof(RectTransform));
                    newGO.transform.SetParent(panelT, false);
                    btnT = newGO.transform;
                    var rt = (RectTransform)btnT;
                    rt.anchorMin = new Vector2(buttonAnchors[i].x, buttonAnchors[i].y);
                    rt.anchorMax = new Vector2(buttonAnchors[i].z, buttonAnchors[i].w);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    Debug.Log($"  Created: {ButtonNames[i]}");
                }
                ApplyPillStyle(btnT.gameObject, ButtonLabels[i], rounded);
                Debug.Log($"  Styled: {ButtonNames[i]}");
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Mission Briefing buttons styled. Press Ctrl+S to save.");
        }

        static void ApplyPillStyle(GameObject go, string labelText, Sprite rounded)
        {
            // ── Remove legacy Text child added by Unity's default Button ──────
            var legacyText = go.transform.Find("Text");
            if (legacyText != null && legacyText.GetComponent<TextMeshProUGUI>() == null)
                Object.DestroyImmediate(legacyText.gameObject);

            // ── Outer pill ────────────────────────────────────────────────────
            var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
            img.sprite                  = rounded;
            img.type                    = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.06f;   // very round = pill shape
            img.color                   = BtnBlue;
            img.raycastTarget           = true;
            EditorUtility.SetDirty(img);

            if (go.GetComponent<Button>() == null) go.AddComponent<Button>();

            var shadow = go.GetComponent<Shadow>() ?? go.AddComponent<Shadow>();
            shadow.effectColor    = new Color(0f, 0f, 0f, 0.45f);
            shadow.effectDistance = new Vector2(0f, -4f);
            EditorUtility.SetDirty(shadow);

            // ── Remove Arrow badge if it exists from a previous style pass ─────
            var oldArrow = go.transform.Find("Arrow");
            if (oldArrow != null) Object.DestroyImmediate(oldArrow.gameObject);

            // ── Label — fills the full pill, text centered ────────────────────
            var labelT = go.transform.Find("Label");
            if (labelT == null)
            {
                var lGO = new GameObject("Label", typeof(RectTransform));
                lGO.transform.SetParent(go.transform, false);
                labelT = lGO.transform;
            }
            labelT.SetAsLastSibling();
            var lRT = (RectTransform)labelT;
            lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one;
            lRT.offsetMin = Vector2.zero; lRT.offsetMax = Vector2.zero;

            var lTMP = labelT.GetComponent<TextMeshProUGUI>() ?? labelT.gameObject.AddComponent<TextMeshProUGUI>();
            lTMP.text             = labelText;
            lTMP.fontSize         = 30f;
            lTMP.fontStyle        = FontStyles.Bold;
            lTMP.color            = White;
            lTMP.alignment        = TextAlignmentOptions.Center;
            lTMP.characterSpacing = 1f;
            EditorUtility.SetDirty(lTMP);
        }

        static Transform FindInactive(string name)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go.name == name && go.scene.IsValid()) return go.transform;
            return null;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var c);
            return c;
        }
    }
}
