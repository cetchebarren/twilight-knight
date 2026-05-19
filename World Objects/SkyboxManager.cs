using System.Collections;
using UnityEngine;

namespace etchebarren
{
    public class SkyboxManager : MonoBehaviour
    {
        [Header("Defaults")]
        public bool setToDayAtStart = true;
        private Material startingSkybox;
        private Color startingFogColor;
        private float startingFogDensity = 0.23f;

        [Header("Skybox References")]
        public Material skyboxMaterial;  // The skybox material with the custom shader
        public Material eclipseSkybox;

        [Header("Settings")]
        public float fadeDuration = 2f;  // The duration of the fade
        public float dayAmbientIntensity = 1f;
        public float dayReflectionIntensity = 1f;
        public float nightAmbientIntensity = 0f;
        public float nightReflectionIntensity = 0f;

        private void Start()
        {
            if (setToDayAtStart)
            {
                SetToDay();
            }
            // Optionally, set the initial blend value here or use the current material value
            startingSkybox = RenderSettings.skybox;
            startingFogColor = RenderSettings.fogColor;
            startingFogDensity = RenderSettings.fogDensity;
        }

        public void CustomFade(float customBlend)
        {
            customBlend = Mathf.Clamp(customBlend, 0f, 1f);
            StartCoroutine(FadeSkybox(customBlend));
        }

        // Starts the fade to day skybox (Blend = 0)
        public void FadeToDay()
        {
            StartCoroutine(FadeSkybox(1f));
        }

        // Starts the fade to night skybox (Blend = 1)
        public void FadeToNight()
        {
            StartCoroutine(FadeSkybox(0f));
        }

        public void SetFadeDuration(float duration)
        {
            fadeDuration = duration;
        }

        // Coroutine to fade between skyboxes based on targetBlend (0 for Day, 1 for Night)
        private IEnumerator FadeSkybox(float targetBlend)
        {
            float currentBlend = skyboxMaterial.GetFloat("_Blend");
            float currentAmbientIntensity = RenderSettings.ambientIntensity;
            float currentReflectionIntensity = RenderSettings.reflectionIntensity;

            // Get the corresponding ambient and reflection intensities based on target blend(0, 1, or in between)
            float targetAmbientIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, targetBlend);
            float targetReflectionIntensity = Mathf.Lerp(nightReflectionIntensity, dayReflectionIntensity, targetBlend);
            // Debug.Log("TAI: " + targetAmbientIntensity);
            // Debug.Log("TRI: " + targetReflectionIntensity);

            // Use deltaTime to track the progress over frames
            float timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                // Accumulate the time passed using deltaTime
                timeElapsed += Time.deltaTime;

                // Calculate the progress of the fade
                float t = Mathf.Clamp01(timeElapsed / fadeDuration);  // Ensure t is between 0 and 1

                // Lerp the values to create a smooth transition
                float newBlend = Mathf.Lerp(currentBlend, targetBlend, t);
                skyboxMaterial.SetFloat("_Blend", newBlend);

                float newAmbientIntensity = Mathf.Lerp(currentAmbientIntensity, targetAmbientIntensity, t);
                RenderSettings.ambientIntensity = newAmbientIntensity;

                float newReflectionIntensity = Mathf.Lerp(currentReflectionIntensity, targetReflectionIntensity, t);
                RenderSettings.reflectionIntensity = newReflectionIntensity;

                yield return null;  // Wait until the next frame
            }

            // Ensure the final values are exactly the target values (for edge cases)
            skyboxMaterial.SetFloat("_Blend", targetBlend);
            RenderSettings.ambientIntensity = targetAmbientIntensity;
            RenderSettings.reflectionIntensity = targetReflectionIntensity;
        }

        public void SetToDay()
        {
            StopAllCoroutines();
            skyboxMaterial.SetFloat("_Blend", 1f);
            RenderSettings.ambientIntensity = dayAmbientIntensity;
            RenderSettings.reflectionIntensity = dayReflectionIntensity;
        }

        public void SetToNight()
        {
            StopAllCoroutines();
            skyboxMaterial.SetFloat("_Blend", 0f);
            RenderSettings.ambientIntensity = nightAmbientIntensity;
            RenderSettings.reflectionIntensity = nightReflectionIntensity;
        }

        public void SetFog(bool enabled)
        {
            RenderSettings.fog = enabled;
        }

        public void SetEclipse(bool enabled)
        {
            WorldStateManager.instance.eclipseActive = enabled;
            if (enabled)
            {
                RenderSettings.skybox = eclipseSkybox;
                RenderSettings.ambientIntensity = 0f;
                RenderSettings.reflectionIntensity = 0f;
                RenderSettings.fog = true;
                RenderSettings.fogColor = Color.black;
                RenderSettings.fogDensity = 0.23f;
            }
            else
            {
                RenderSettings.skybox = startingSkybox;
                RenderSettings.ambientIntensity = dayAmbientIntensity;
                RenderSettings.reflectionIntensity = dayReflectionIntensity;
                RenderSettings.fog = true;
                RenderSettings.fogColor = startingFogColor;
                RenderSettings.fogDensity = startingFogDensity;
            }

        }

        public void ResetSkybox()
        {
            RenderSettings.skybox = startingSkybox;
        }
    }
}
