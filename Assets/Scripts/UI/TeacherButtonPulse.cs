using UnityEngine;
using UnityEngine.UI;

namespace ETEC510.UI
{
    /// <summary>
    /// Attached to the TeacherDashboardTrigger button.
    /// Pulses scale and cycles the background colour for a glowing effect.
    /// </summary>
    public class TeacherButtonPulse : MonoBehaviour
    {
        [Header("Pulse")]
        public float pulseSpeed  = 1.8f;
        public float pulseAmount = 0.06f;   // scale delta ±

        [Header("Glow")]
        public Image  buttonImage;
        public Color  baseColour  = new Color(0.12f, 0.28f, 0.55f, 1f);
        public Color  glowColour  = new Color(0.25f, 0.55f, 1.00f, 1f);

        [Header("Accent")]
        public Image  accentImage;
        public Color  accentBase  = new Color(0.30f, 0.75f, 1.00f, 1f);
        public Color  accentGlow  = new Color(0.70f, 0.95f, 1.00f, 1f);

        private Vector3 _baseScale;

        private void Start()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f; // 0..1

            // Scale pulse
            float s = 1f + pulseAmount * t;
            transform.localScale = _baseScale * s;

            // Colour glow
            if (buttonImage != null)
                buttonImage.color = Color.Lerp(baseColour, glowColour, t);

            if (accentImage != null)
                accentImage.color = Color.Lerp(accentBase, accentGlow, t);
        }
    }
}
