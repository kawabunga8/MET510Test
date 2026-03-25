using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ETEC510.Editor
{
    /// <summary>
    /// Menu: ETEC510 > Resize Buttons
    /// Scales down the oversized action/back buttons across all panels.
    /// Safe to re-run.
    /// </summary>
    public static class ButtonResizer
    {
        [MenuItem("ETEC510/Resize Buttons")]
        public static void ResizeButtons()
        {
            var canvasGO = GameObject.Find("GameCanvas");
            if (canvasGO == null) { Debug.LogError("ETEC510: GameCanvas not found."); return; }
            var ct = canvasGO.transform;

            // ── Try Again (HintsFromChiefPanel) ───────────────────────────────
            SetAnchors(ct, "HintsFromChiefPanel/HintTryAgainButton",
                new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.18f));
            SetAnchors(ct, "HintsFromChiefPanel/HintOverlay/HintTryAgainButton",
                new Vector2(0.35f, 0.05f), new Vector2(0.98f, 0.20f));

            // ── Return to Evidence (HintsFromChiefPanel overlay) — bottom-left ─
            SetAnchors(ct, "HintsFromChiefPanel/HintOverlay/HintReturnButton",
                new Vector2(0.02f, 0.05f), new Vector2(0.30f, 0.20f));

            // ── Restart (LevelCompletePanel) — bottom-left corner ─────────────
            SetAnchors(ct, "LevelCompletePanel/RestartButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.28f, 0.11f));

            // ── Back / navigation buttons ─────────────────────────────────────
            SetAnchors(ct, "SpotTheCluePanel/SpotBackButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.25f, 0.10f));
            SetAnchors(ct, "GutCheckPanel/GutBackButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.25f, 0.10f));
            SetAnchors(ct, "FindTheMotivePanel/MotiveBackButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.25f, 0.10f));
            SetAnchors(ct, "EvidenceDetailPanel/VerdictBackButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.30f, 0.10f));
            SetAnchors(ct, "PasswordLockPanel/PasswordBackButton",
                new Vector2(0.02f, 0.02f), new Vector2(0.25f, 0.10f));

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("ETEC510: Buttons resized. Press Ctrl+S to save.");
        }

        static void SetAnchors(Transform ct, string path, Vector2 min, Vector2 max)
        {
            var t = ct.Find(path);
            if (t == null) return;   // silently skip missing paths
            var rt = t as RectTransform;
            if (rt == null) return;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            EditorUtility.SetDirty(t.gameObject);
            Debug.Log($"  Resized: {path}");
        }
    }
}
