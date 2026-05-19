using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace etchebarren
{
    public class AcquiredItemText : MonoBehaviour
    {
        [Header("Notification Settings")]
        public float lifetime = 8.0f;
        public float fadeDuration = 1.0f; // Duration of the fade (in seconds)
        public float scaleAnimationDuration = 0.5f; // Duration of the scale animation (in seconds)
        public Vector3 startingScale = new Vector3(1.5f, 1.5f, 1.5f);

        [Header("Text Background Settings")]
        public RectTransform textBackground;
        public float minWidth = 400f;
        public float maxWidth = 650f;
        public float prefWidthLowerBound = 240f;
        public float prefWidthUpperBound = 780f;

        private float elapsedTime = 0.0f;

        private TextMeshProUGUI textComponent;
        private Color initialColor;
        private Color targetColor;

        public bool isGoldText = false;

        void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
            initialColor = textComponent.color;
            targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0); // Transparent color
        }

        void OnEnable()
        {
            StartCoroutine(CountdownAndDeactivate());
        }

        IEnumerator CountdownAndDeactivate()
        {
            gameObject.SetActive(true);
            textComponent.color = initialColor;
            elapsedTime = 0.0f;

            StartCoroutine(ScaleAnimation());

            // Get preferred width
            textComponent.ForceMeshUpdate();
            float preferredWidth = textComponent.preferredWidth;
            yield return null; // Wait for one frame to allow UI updates
            preferredWidth = textComponent.preferredWidth;

            SetBackgroundWidth(preferredWidth);

            while (elapsedTime < lifetime)
            {
                // Calculate the normalized time remaining in the text's lifetime
                float timeRemainingNormalized = 1.0f - (elapsedTime / lifetime);

                // Determine the start and end time for the fade
                float fadeStartTime = lifetime - fadeDuration;
                float fadeEndTime = lifetime;

                if (elapsedTime >= fadeStartTime)
                {
                    // Interpolate between initial and target colors based on time remaining within the fade duration
                    float t = 1.0f - ((fadeEndTime - elapsedTime) / fadeDuration);
                    textComponent.color = Color.Lerp(initialColor, targetColor, t);
                }

                yield return null;
                elapsedTime += Time.unscaledDeltaTime;//changed to allow timer while timescale = 0
                //elapsedTime += Time.deltaTime;//original
            }

            if (isGoldText)
            {
                AcquiredNotifications.instance.goldAcquired = 0;
                lifetime = 5.0f;
            }

            gameObject.transform.parent.gameObject.SetActive(false);

            //check if no other text elements are active, set acquiredText.txt = "", if none are
            AcquiredNotifications.instance.CheckTextsAndDeactivateTitle();
        }

        IEnumerator ScaleAnimation()
        {
            transform.localScale = startingScale;

            float elapsedScaleAnimationTime = 0.0f;

            while (elapsedScaleAnimationTime < scaleAnimationDuration)
            {
                // Calculate the normalized time for the scale animation
                float t = elapsedScaleAnimationTime / scaleAnimationDuration;
                transform.localScale = Vector3.Lerp(startingScale, Vector3.one, t);

                yield return null;
                elapsedScaleAnimationTime += Time.deltaTime;
            }

            // Ensure that the final scale is the target scale
            transform.localScale = Vector3.one;
        }

        public void ResetTimer()
        {
            StopAllCoroutines();
            textComponent.color = initialColor;
            transform.localScale = startingScale;
            StartCoroutine(CountdownAndDeactivate());
        }

        public void ResetGoldTimer()
        {
            StopAllCoroutines();
            StartCoroutine(CountdownAndDeactivate());
        }

        public void SetBackgroundWidth(float preferredWidth)
        {
            // Clamp preferred width within the bounds
            preferredWidth = Mathf.Clamp(preferredWidth, prefWidthLowerBound, prefWidthUpperBound);

            // Calculate the normalized position (0 to 1) of the preferred width within the bounds
            float normalizedWidth = (preferredWidth - prefWidthLowerBound) / (prefWidthUpperBound - prefWidthLowerBound);

            // Map normalized width to the range between minWidth and maxWidth
            float effectiveWidth = Mathf.Lerp(minWidth, maxWidth, normalizedWidth);

            // Set the background width
            Debug.Log($"Preferred Width: {preferredWidth}, Effective Width: {effectiveWidth}");

            textBackground.sizeDelta = new Vector2(effectiveWidth, textBackground.sizeDelta.y);
        }
    }
}
