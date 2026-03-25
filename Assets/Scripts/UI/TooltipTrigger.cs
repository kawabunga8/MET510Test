using UnityEngine;
using UnityEngine.EventSystems;

namespace ETEC510.UI
{
    /// <summary>
    /// Attach to any UI element. Shows a tooltip on pointer enter, hides on exit.
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [TextArea(2, 5)]
        public string tooltipText;

        public void OnPointerEnter(PointerEventData _)
        {
            if (!string.IsNullOrEmpty(tooltipText))
                TooltipController.Show(tooltipText, (RectTransform)transform);
        }

        public void OnPointerExit(PointerEventData _)
        {
            TooltipController.Hide();
        }

        void OnDisable()
        {
            TooltipController.Hide();
        }
    }
}
