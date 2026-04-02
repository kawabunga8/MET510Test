using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace ETEC510.UI
{
    /// <summary>
    /// Attach to any panel that has a Skip button.
    /// Pressing Return/Enter invokes the skip action.
    /// The skip button's label pulses between white and a soft cyan glow.
    /// </summary>
    [DisallowMultipleComponent]
    public class SkipKeyListener : MonoBehaviour
    {
        public Button skipButton;

        private TMP_Text _label;

        private static readonly Color BaseColour = Color.white;
        private static readonly Color GlowColour = new Color(0.6f, 0.88f, 1f, 1f);
        private const float GlowSpeed = 2f;

        private void Start()
        {
            if (skipButton != null)
                _label = skipButton.GetComponentInChildren<TMP_Text>();
        }

        private void Update()
        {
            if (skipButton != null
                && skipButton.gameObject.activeInHierarchy
                && skipButton.interactable
                && (Keyboard.current?.enterKey.wasPressedThisFrame ?? false))
                skipButton.onClick.Invoke();

            if (_label != null)
            {
                float t = (Mathf.Sin(Time.unscaledTime * GlowSpeed) + 1f) * 0.5f;
                _label.color = Color.Lerp(BaseColour, GlowColour, t);
            }
        }
    }
}
