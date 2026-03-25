using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETEC510.UI
{
    /// <summary>
    /// Singleton tooltip panel. Call Show/Hide from TooltipTrigger.
    /// The panel repositions to stay near the hovered element.
    /// </summary>
    public class TooltipController : MonoBehaviour
    {
        public static TooltipController Instance { get; private set; }

        [Header("Panel")]
        public RectTransform tooltipPanel;
        public TMP_Text      tooltipText;

        private Canvas _canvas;

        void Awake()
        {
            Instance = this;
            _canvas = GetComponentInParent<Canvas>();
            if (tooltipPanel) tooltipPanel.gameObject.SetActive(false);
        }

        public static void Show(string text, RectTransform anchor)
        {
            Ensure();
            if (Instance == null) return;
            Instance.ShowInternal(text, anchor);
        }

        public static void Hide()
        {
            if (Instance && Instance.tooltipPanel)
                Instance.tooltipPanel.gameObject.SetActive(false);
        }

        void ShowInternal(string text, RectTransform anchor)
        {
            if (tooltipText)  tooltipText.text = text;
            if (!tooltipPanel) return;

            // Position above the anchor element
            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            // corners[1] = top-left, corners[2] = top-right
            Vector3 worldTop = (corners[1] + corners[2]) * 0.5f;

            // Convert to local position in the canvas
            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null : _canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)_canvas.transform,
                RectTransformUtility.WorldToScreenPoint(cam, worldTop),
                cam,
                out Vector2 localPt);

            tooltipPanel.anchoredPosition = localPt + Vector2.up * 12f;
            tooltipPanel.gameObject.SetActive(true);
        }

        static void Ensure()
        {
            if (Instance == null)
                Instance = FindFirstObjectByType<TooltipController>();
        }
    }
}
