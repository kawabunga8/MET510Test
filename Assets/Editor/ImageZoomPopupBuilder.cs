using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using ETEC510.UI;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Build Image Zoom Popup
    ///
    /// Creates an ImagePopupPanel overlay that shows a full-screen zoomed view
    /// of a clicked evidence image. Also adds transparent click-buttons on top
    /// of GutEvidenceImage, SpotEvidenceImage, and MotiveEvidenceImage.
    ///
    /// Safe to re-run (destroys and recreates).
    ///
    ///   ImagePopupPanel
    ///     Overlay          (dark semi-transparent background)
    ///     ZoomedImage      (evidence image, AspectRatioFitter)
    ///     BackButton       (← BACK, bottom-centre)
    /// </summary>
    public static class ImageZoomPopupBuilder
    {
        static readonly Color OverlayColor  = new Color(0f, 0.02f, 0.06f, 0.92f);
        static readonly Color AccentCyan    = new Color(0f, 0.83f, 1f, 1f);
        static readonly Color BtnBg         = new Color(0f, 0.25f, 0.35f, 0.90f);
        static readonly Color BtnLabel      = new Color(0f, 0.83f, 1f, 1f);

        [MenuItem("ETEC510/Build Image Zoom Popup")]
        public static void Build()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // ── Destroy existing ──────────────────────────────────────────────
            var existing = ct.Find("ImagePopupPanel");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            // ── Find CaseRunner ───────────────────────────────────────────────
            var runner = Object.FindFirstObjectByType<CaseRunner>();
            if (runner == null) { Debug.LogError("ETEC510: CaseRunner not found in scene."); return; }

            var rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var mono    = FindMonoFont();

            // ── Root panel (full stretch, drawn on top) ───────────────────────
            var panelGO = new GameObject("ImagePopupPanel", typeof(RectTransform));
            panelGO.transform.SetParent(ct, false);
            panelGO.transform.SetAsLastSibling();
            Stretch(panelGO);

            // ── Dark overlay ──────────────────────────────────────────────────
            var overlayGO = Child("Overlay", panelGO.transform);
            Stretch(overlayGO);
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = OverlayColor;
            overlayImg.raycastTarget = true;

            // ── Zoomed evidence image ─────────────────────────────────────────
            // Fills the screen minus space for the back button at bottom
            var imgGO = Child("ZoomedImage", panelGO.transform);
            var imgRT = (RectTransform)imgGO.transform;
            imgRT.anchorMin = new Vector2(0.04f, 0.14f);
            imgRT.anchorMax = new Vector2(0.96f, 0.96f);
            imgRT.offsetMin = Vector2.zero;
            imgRT.offsetMax = Vector2.zero;
            var zoomedImg = imgGO.AddComponent<Image>();
            zoomedImg.preserveAspect = true;
            zoomedImg.raycastTarget  = false;
            // AspectRatioFitter so the image never stretches
            var fitter = imgGO.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            // ── Back button ───────────────────────────────────────────────────
            var backGO = Child("BackButton", panelGO.transform);
            var backRT = (RectTransform)backGO.transform;
            backRT.anchorMin = new Vector2(0.30f, 0.02f);
            backRT.anchorMax = new Vector2(0.70f, 0.11f);
            backRT.offsetMin = Vector2.zero;
            backRT.offsetMax = Vector2.zero;

            var backImg = backGO.AddComponent<Image>();
            backImg.color  = BtnBg;
            backImg.sprite = rounded;
            backImg.type   = Image.Type.Sliced;
            backImg.pixelsPerUnitMultiplier = 0.12f;

            var backBtn = backGO.AddComponent<Button>();
            var colors  = backBtn.colors;
            colors.highlightedColor = new Color(0f, 0.5f, 0.65f, 0.95f);
            colors.pressedColor     = new Color(0f, 0.35f, 0.45f, 0.95f);
            backBtn.colors = colors;

            var labelGO = Child("Label", backGO.transform);
            var labelRT = (RectTransform)labelGO.transform;
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;
            var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
            labelTMP.text             = "BACK";
            labelTMP.alignment        = TextAlignmentOptions.Center;
            labelTMP.fontSize         = 22;
            labelTMP.color            = BtnLabel;
            labelTMP.fontStyle        = FontStyles.Bold;
            labelTMP.characterSpacing = 4f;
            if (mono != null) labelTMP.font = mono;

            // Start hidden
            panelGO.SetActive(false);

            // ── Add transparent click buttons to evidence images ──────────────
            var gutClickBtn   = EnsureClickButton("GutEvidenceImage");
            var spotClickBtn  = EnsureClickButton("SpotEvidenceImage");
            var motiveClickBtn = EnsureClickButton("MotiveEvidenceImage");

            // ── Wire CaseRunner ───────────────────────────────────────────────
            var so = new SerializedObject(runner);
            so.FindProperty("imagePopupPanel").objectReferenceValue      = panelGO;
            so.FindProperty("imagePopupImage").objectReferenceValue      = zoomedImg;
            so.FindProperty("imagePopupBackButton").objectReferenceValue = backBtn;
            if (gutClickBtn   != null) so.FindProperty("gutEvidenceButton").objectReferenceValue   = gutClickBtn;
            if (spotClickBtn  != null) so.FindProperty("spotEvidenceButton").objectReferenceValue  = spotClickBtn;
            if (motiveClickBtn != null) so.FindProperty("motiveEvidenceButton").objectReferenceValue = motiveClickBtn;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(runner);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: ImagePopupPanel built and wired. Press Ctrl+S to save.");
        }

        // Adds (or finds) a full-stretch transparent Button on top of a named GO.
        // This lets the Image be clickable without interfering with its display.
        static Button EnsureClickButton(string targetName)
        {
            // GameObject.Find only finds active objects; use FindObjectsOfType to include inactive
            GameObject targetGO = null;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name == targetName) { targetGO = t.gameObject; break; }
            }
            if (targetGO == null)
            {
                Debug.LogWarning($"ETEC510: {targetName} not found — skipping click button.");
                return null;
            }

            // Remove old click button if any
            var oldClick = targetGO.transform.Find("ClickOverlay");
            if (oldClick != null) Object.DestroyImmediate(oldClick.gameObject);

            var overlayGO = new GameObject("ClickOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(targetGO.transform, false);
            overlayGO.transform.SetAsLastSibling();
            Stretch(overlayGO);

            // Transparent Image so the Button has a raycast target
            var img = overlayGO.AddComponent<Image>();
            img.color = Color.clear;

            var btn = overlayGO.AddComponent<Button>();
            // No color tint — the underlying Image handles the visual
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor      = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.85f);
            colors.pressedColor     = new Color(0.9f, 0.9f, 0.9f, 0.7f);
            btn.colors = colors;
            btn.targetGraphic = img;

            Debug.Log($"ETEC510: Click button added to {targetName}.");
            return btn;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        static void Stretch(GameObject go)
        {
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static GameObject Child(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static TMP_FontAsset FindMonoFont()
        {
            string[] candidates = { "LiberationMono", "RobotoMono", "SourceCodePro",
                                    "Inconsolata", "Courier", "CourierNew", "Typewriter" };
            foreach (var n in candidates)
            {
                var guids = AssetDatabase.FindAssets($"t:TMP_FontAsset {n}");
                if (guids.Length > 0)
                {
                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (f != null) return f;
                }
            }
            return null;
        }
    }
}
