using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETEC510.UI
{
    /// <summary>
    /// Singleton popup panel for the detective toolkit.
    /// Call via static methods — no direct reference needed from callers.
    ///
    ///   PopupController.Show("Title", "Body", "OK", onConfirm);
    ///   PopupController.ShowTwoButton("Title", "Body", "Yes", onYes, "No", onNo);
    ///   PopupController.ShowToast("Message", 2.5f);   // auto-dismisses
    /// </summary>
    public class PopupController : MonoBehaviour
    {
        public static PopupController Instance { get; private set; }

        [Header("Popup Root")]
        public GameObject popupRoot;    // whole overlay (blocker + card); toggled active

        [Header("Card")]
        public GameObject popupCard;    // scaled in on show
        public Image      cardBackground;          // Image component on the card (optional)
        public Sprite     defaultBackgroundSprite; // used for correct / neutral popups
        public Sprite     incorrectBackgroundSprite; // used when isCorrect = false
        public TMP_Text   titleText;
        public TMP_Text   bodyText;

        [Header("Buttons")]
        public Button   confirmButton;
        public TMP_Text confirmLabel;
        public Button   cancelButton;   // hidden when not needed
        public TMP_Text cancelLabel;
        public Button   dismissButton;  // always visible — closes popup with no callback

        private System.Action _onConfirm;
        private System.Action _onCancel;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        void Awake()
        {
            Instance = this;
            if (popupRoot) popupRoot.SetActive(false);
        }

        // ── Static API ────────────────────────────────────────────────────────

        /// <summary>Single-button modal. Pass isCorrect=false to use the incorrect background.</summary>
        public static void Show(string title, string body,
            string btnLabel = "OK", System.Action onConfirm = null, bool isCorrect = true)
        {
            Ensure();
            Instance.ShowInternal(title, body, btnLabel, onConfirm, null, null, isCorrect);
        }

        /// <summary>Two-button modal (confirm left, cancel right).</summary>
        public static void ShowTwoButton(string title, string body,
            string confirmLbl, System.Action onConfirm,
            string cancelLbl,  System.Action onCancel = null)
        {
            Ensure();
            Instance.ShowInternal(title, body, confirmLbl, onConfirm, cancelLbl, onCancel);
        }

        /// <summary>Toast — shows briefly then auto-dismisses; no buttons.</summary>
        public static void ShowToast(string body, float duration = 2.5f)
        {
            Ensure();
            Instance.StartCoroutine(Instance.ToastRoutine(body, duration));
        }

        public static void Hide()
        {
            if (Instance) Instance.HideInternal();
        }

        // ── Internal ──────────────────────────────────────────────────────────

        void ShowInternal(string title, string body,
            string confirmLbl, System.Action onConfirm,
            string cancelLbl,  System.Action onCancel,
            bool isCorrect = true)
        {
            StopAllCoroutines();

            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            // Swap card background based on correct/incorrect state
            if (cardBackground != null)
            {
                var target = isCorrect ? defaultBackgroundSprite : incorrectBackgroundSprite;
                if (target != null) cardBackground.sprite = target;
            }

            if (titleText)  titleText.text  = title ?? "";
            if (bodyText)   bodyText.text   = body  ?? "";

            bool twoBtn = !string.IsNullOrEmpty(cancelLbl);
            if (cancelButton) cancelButton.gameObject.SetActive(twoBtn);
            if (twoBtn && cancelLabel) cancelLabel.text = cancelLbl;

            if (confirmButton)
            {
                confirmButton.gameObject.SetActive(!string.IsNullOrEmpty(confirmLbl));
                confirmButton.onClick.RemoveAllListeners();
                confirmButton.onClick.AddListener(OnConfirmClicked);
                if (confirmLabel) confirmLabel.text = confirmLbl ?? "OK";
            }
            if (cancelButton)
            {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(OnCancelClicked);
            }
            if (dismissButton)
            {
                dismissButton.gameObject.SetActive(true);
                dismissButton.onClick.RemoveAllListeners();
                dismissButton.onClick.AddListener(HideInternal);
            }

            if (popupRoot) popupRoot.SetActive(true);
            if (popupCard) StartCoroutine(ScaleIn(popupCard));
        }

        void HideInternal()
        {
            StopAllCoroutines();
            if (popupRoot) popupRoot.SetActive(false);
            if (popupCard) popupCard.transform.localScale = Vector3.one;
        }

        void OnConfirmClicked()
        {
            HideInternal();
            _onConfirm?.Invoke();
        }

        void OnCancelClicked()
        {
            HideInternal();
            _onCancel?.Invoke();
        }

        IEnumerator ScaleIn(GameObject card)
        {
            var t = card.transform;
            t.localScale = Vector3.one * 0.82f;
            float elapsed = 0f, dur = 0.18f;
            while (elapsed < dur)
            {
                elapsed += Time.unscaledDeltaTime;
                float s = Mathf.SmoothStep(0.82f, 1f, elapsed / dur);
                t.localScale = Vector3.one * s;
                yield return null;
            }
            t.localScale = Vector3.one;
        }

        IEnumerator ToastRoutine(string body, float duration)
        {
            ShowInternal("", body, null, null, null, null);
            if (confirmButton)  confirmButton.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(duration);
            HideInternal();
            if (confirmButton)  confirmButton.gameObject.SetActive(true);
        }

        static void Ensure()
        {
            if (Instance == null)
                Instance = FindFirstObjectByType<PopupController>();
            if (Instance == null)
                Instance = FindFirstObjectByType<PopupController>(FindObjectsInactive.Include);
            if (Instance == null)
                Debug.LogWarning("[PopupController] No instance found in scene.");
        }
    }
}
