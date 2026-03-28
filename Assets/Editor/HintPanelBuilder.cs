using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Rebuild Hint Panel
    /// Restructures HintsFromChiefPanel so:
    ///   - HintVideoDisplay fills the entire panel as background
    ///   - A semi-opaque overlay strip sits at the bottom containing
    ///     HintBodyText and HintTryAgainButton
    /// Run once; safe to re-run (destroys and recreates HintOverlay).
    /// </summary>
    public static class HintPanelBuilder
    {
        [MenuItem("ETEC510/Rebuild Hint Panel")]
        public static void RebuildHintPanel()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            var hintPanel = ct.Find("HintsFromChiefPanel");
            if (hintPanel == null) { Debug.LogError("ETEC510: HintsFromChiefPanel not found."); return; }

            // ── Cache references before any reparenting ───────────────────────
            var videoDisplay  = hintPanel.Find("HintVideoDisplay");
            var bodyText      = hintPanel.Find("HintBodyText");
            var tryAgainBtn   = hintPanel.Find("HintTryAgainButton");
            var titleText     = hintPanel.Find("TitleText");
            var chiefImage    = hintPanel.Find("ChiefImage");

            // ── 1. HintVideoDisplay → fullscreen background ───────────────────
            if (videoDisplay != null)
            {
                var rt = (RectTransform)videoDisplay;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                videoDisplay.SetSiblingIndex(0);
                EditorUtility.SetDirty(videoDisplay.gameObject);
                Debug.Log("  HintVideoDisplay set to fullscreen.");
            }
            else
            {
                Debug.LogWarning("ETEC510: HintVideoDisplay not found.");
            }

            // ── 2. Hide TitleText and ChiefImage ─────────────────────────────
            if (titleText  != null) titleText.gameObject.SetActive(false);
            if (chiefImage != null) chiefImage.gameObject.SetActive(false);

            // ── 3. Create semi-opaque overlay (bottom 42 % of panel) ──────────
            var existingOverlay = hintPanel.Find("HintOverlay");
            var savedOverlay = DetectiveStyleGuide.SaveChildRects(existingOverlay);
            if (existingOverlay != null) Object.DestroyImmediate(existingOverlay.gameObject);

            var overlayGO = new GameObject("HintOverlay", typeof(RectTransform));
            overlayGO.transform.SetParent(hintPanel, false);

            var overlayRT = (RectTransform)overlayGO.transform;
            overlayRT.anchorMin = new Vector2(0f, 0f);
            overlayRT.anchorMax = new Vector2(1f, 0.42f);
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(overlayRT, "", savedOverlay);

            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.72f);

            // ── 4. Reparent HintBodyText into overlay ─────────────────────────
            if (bodyText != null)
            {
                bodyText.SetParent(overlayGO.transform, false);
                var brt = (RectTransform)bodyText;
                brt.anchorMin = new Vector2(0.04f, 0.28f);
                brt.anchorMax = new Vector2(0.96f, 0.96f);
                brt.offsetMin = Vector2.zero;
                brt.offsetMax = Vector2.zero;
                Debug.Log("  HintBodyText reparented into HintOverlay.");
            }

            // ── 5. Reparent HintTryAgainButton into overlay (right) ──────────
            if (tryAgainBtn != null)
            {
                tryAgainBtn.SetParent(overlayGO.transform, false);
                var brt = (RectTransform)tryAgainBtn;
                brt.anchorMin = new Vector2(0.35f, 0.05f);
                brt.anchorMax = new Vector2(0.98f, 0.24f);
                brt.offsetMin = Vector2.zero;
                brt.offsetMax = Vector2.zero;
                Debug.Log("  HintTryAgainButton reparented into HintOverlay.");
            }

            // ── 6. Create HintReturnButton in overlay (right) ─────────────────
            var existingReturn = overlayGO.transform.Find("HintReturnButton");
            if (existingReturn != null) Object.DestroyImmediate(existingReturn.gameObject);

            var returnGO = new GameObject("HintReturnButton", typeof(RectTransform));
            returnGO.transform.SetParent(overlayGO.transform, false);
            var rrt = (RectTransform)returnGO.transform;
            rrt.anchorMin = new Vector2(0.02f, 0.05f);
            rrt.anchorMax = new Vector2(0.30f, 0.24f);
            rrt.offsetMin = Vector2.zero;
            rrt.offsetMax = Vector2.zero;
            DetectiveStyleGuide.ApplySavedRect(rrt, "HintReturnButton", savedOverlay);

            var returnImg = returnGO.AddComponent<Image>();
            returnImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            returnGO.AddComponent<Button>();

            var returnLabelGO = new GameObject("Label", typeof(RectTransform));
            returnLabelGO.transform.SetParent(returnGO.transform, false);
            var rlrt = (RectTransform)returnLabelGO.transform;
            rlrt.anchorMin = Vector2.zero;
            rlrt.anchorMax = Vector2.one;
            rlrt.offsetMin = Vector2.zero;
            rlrt.offsetMax = Vector2.zero;

            var returnTmp = returnLabelGO.AddComponent<TextMeshProUGUI>();
            returnTmp.text      = "Return to Evidence";
            returnTmp.alignment = TextAlignmentOptions.Center;
            returnTmp.fontSize  = 28;
            returnTmp.color     = Color.white;
            returnTmp.fontStyle = FontStyles.Bold;

            Debug.Log("  HintReturnButton created in HintOverlay.");

            // ── 7. Re-wire CaseRunner fields ──────────────────────────────────
            var controller = ct.Find("CaseController");
            if (controller != null)
            {
                var runner = controller.GetComponent<ETEC510.UI.CaseRunner>();
                if (runner != null)
                {
                    var so = new SerializedObject(runner);
                    if (bodyText != null)
                        so.FindProperty("hintBodyText").objectReferenceValue =
                            bodyText.GetComponent<TMP_Text>();
                    so.FindProperty("hintReturnButton").objectReferenceValue =
                        returnGO.GetComponent<Button>();
                    so.FindProperty("hintOverlay").objectReferenceValue = overlayGO;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(runner);
                    Debug.Log("  hintBodyText, hintReturnButton, hintOverlay re-wired on CaseRunner.");
                }
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Hint panel rebuilt. Press Ctrl+S to save.");
        }
    }
}
