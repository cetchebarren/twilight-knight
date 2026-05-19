using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class SelectButtonFade : MonoBehaviour
    {
        public float blinkSpeed = 1.0f; // Adjust the speed of the blinking
        public float minAlpha = 0.2f;   // Minimum alpha value
        public float maxAlpha = 0.5f;   // Maximum alpha value

        public Image image;
        private bool fadingIn = true;

        private void OnEnable()
        {
            //fadingIn = true;
            //Debug.Log("ON ENABLE");
            StartCoroutine(Blink());
        }

        private void OnDisable()
        {
            StopCoroutine(Blink());
        }

        private IEnumerator Blink()
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, maxAlpha);

            while (true)
            {
                float targetAlpha = fadingIn ? maxAlpha : minAlpha;
                float startAlpha = image.color.a;
                float t = 0;

                while (t < 1)
                {
                    t += Time.deltaTime * blinkSpeed;
                    float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                    Color newColor = image.color;
                    newColor.a = newAlpha;
                    image.color = newColor;

                    yield return null;
                }

                fadingIn = !fadingIn;
            }
        }
    }
}
