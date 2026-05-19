using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class TextNotification : MonoBehaviour
    {
        /* How to send notification:
        TextNotificationsManager.instance.NewTextNotifaction("Not enough in inventory", true/false); //true prevents duplicate messages for 3 seconds
        */
        
        public float lifetime = 5.0f;
        public float fadeDuration = 1.0f; // Duration of the fade (in seconds)
        public float scaleAnimationDuration = 0.5f; // Duration of the scale animation (in seconds)
        public Vector3 startingScale = new Vector3(1.5f, 1.5f, 1.5f);

        private float elapsedTime = 0.0f;

        private TextMeshProUGUI textComponent;
        private Color initialColor;
        private Color targetColor;

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
            textComponent.color = initialColor;

            elapsedTime = 0.0f;

            StartCoroutine(ScaleAnimation());

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
                //unscaled delta time ignores timescale, works when paused
                elapsedTime += Time.unscaledDeltaTime;
            }

            gameObject.transform.parent.transform.gameObject.SetActive(false);
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
                elapsedScaleAnimationTime += Time.unscaledDeltaTime;
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
    }
}
