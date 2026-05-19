using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class SelfEffectsManager : MonoBehaviour
    {
        public static SelfEffectsManager instance;
        public PlayerStats playerStats;

        [Header("Effects")]
        public ParticleSystem levelUp;
        public ParticleSystem restoreHealth;
        public ParticleSystem restoreMana;
        public ParticleSystem restoreStamina;

        public GameObject[] marsSkinnedMeshes;
        public GameObject[] venusSkinnedMeshes;

        public GameObject[] marsShockShields;
        public GameObject[] venusShockShields;

        public ParticleSystem lifeDrainHealEffect;
        public AudioSource lifeDrainAudioSource;

        public ParticleSystem lifeLightEffect;
        public GameObject lifeLightGameObject;
        public Vector3 lifeLightStartingPos;
        public Vector3 lifeLightTargetPos;
        public AudioSource lifeLightAudioSource;
        public float lifeLightStartingVolume = 0.05f;
        public float lifeLightTargetVolume = 0.45f;

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LevelUpEffect()
        {
            levelUp.Play();
            levelUp.GetComponent<AudioSource>().Play();
        }

        public void RestoreHealthEffect()
        {
            restoreHealth.Play();
        }

        public void RestoreManaEffect()
        {
            restoreMana.Play();
        }

        public void RestoreStaminaEffect()
        {
            restoreStamina.Play();
        }

        public void EnableShockShield()
        {
            if(playerStats.gender == "mars" || playerStats.gender == "Mars")
            {
                for(int i = 0; i < marsSkinnedMeshes.Length; i++)
                {
                    if (marsSkinnedMeshes[i].activeSelf) marsShockShields[i].SetActive(true);
                }
            }
            else
            {
                for (int i = 0; i < venusSkinnedMeshes.Length; i++)
                {
                    if (venusSkinnedMeshes[i].activeSelf) venusShockShields[i].SetActive(true);
                }
            }
        }

        public void DisableShockShield()
        {
            foreach(GameObject shockShield in marsShockShields)
            {
                shockShield.SetActive(false);
            }

            foreach (GameObject shockShield in venusShockShields)
            {
                shockShield.SetActive(false);
            }
        }

        public void PlayLifeDrainHealEffect()
        {
            lifeDrainHealEffect.Play();
            lifeDrainAudioSource.Play();
        }

        public IEnumerator PlayLifeLightEffect(float duration)
        {
            // Get starting and target values
            Vector3 startingPosition = lifeLightStartingPos;
            Vector3 targetPosition = lifeLightTargetPos;

            // Audio fading parameters
            float fadeInVolumeStartTime = 0.2f; // Time after which the volume starts fading in
            float fadeInVolumeDuration = 5f; // Duration of the fade-in effect

            // Activate the game object
            lifeLightGameObject.SetActive(true);

            // Start playing the particle effect
            lifeLightEffect.Play();

            // Start playing audio
            lifeLightAudioSource.volume = 0f;
            lifeLightAudioSource.Play();
            lifeLightAudioSource.volume = lifeLightStartingVolume; // Ensure the volume starts at 0

            // Get the starting position
            lifeLightGameObject.transform.localPosition = startingPosition;
            Vector3 startPosition = lifeLightGameObject.transform.localPosition;

            // Initialize elapsed time
            float elapsedTime = 0f;

            // Loop over the duration to smoothly transition the position and fade in the volume
            while (elapsedTime < duration)
            {
                // Update elapsed time
                elapsedTime += Time.deltaTime;

                // Calculate the interpolation factor for position
                float tPosition = Mathf.Clamp01(elapsedTime / duration);

                // Interpolate the position
                lifeLightGameObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, tPosition);

                // Calculate the interpolation factor for volume if within fade-in period
                if (elapsedTime >= fadeInVolumeStartTime && elapsedTime <= fadeInVolumeStartTime + fadeInVolumeDuration)
                {
                    float tVolume = Mathf.Clamp01((elapsedTime - fadeInVolumeStartTime) / fadeInVolumeDuration);
                    lifeLightAudioSource.volume = Mathf.Lerp(lifeLightStartingVolume, lifeLightTargetVolume, tVolume);
                }
                else if (elapsedTime > fadeInVolumeStartTime + fadeInVolumeDuration)
                {
                    // Ensure volume is exactly the target volume after fade-in period
                    lifeLightAudioSource.volume = lifeLightTargetVolume;
                }

                // Wait for the next frame
                yield return null;
            }

            // Ensure the final position is exactly the target position
            lifeLightGameObject.transform.localPosition = targetPosition;

            // Ensure the volume is set to the target volume after the duration
            lifeLightAudioSource.volume = lifeLightTargetVolume;

            // Stop the audio source
            lifeLightAudioSource.Stop();
        }

    }
}
