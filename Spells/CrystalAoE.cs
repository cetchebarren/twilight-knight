using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class CrystalAoE : MonoBehaviour
    {
        public GlacialShatter parentSpell;
        public SphereCollider sphereCollider; // trigger for dealing damage
        public MeshCollider meshCollider; // collider for walking into crystal
        public AoESpellDamageCollider crystalExplosionTriggerCollider;

        public ParticleSystem[] explosionEffects;
        public MeshRenderer meshRenderer;
        public AudioSource explosionAudioSource;
        public Transform shockwaveTransform;
        public AudioSource charingAudioSource;
        public AudioClip explosionAudioClip;
        [Range(0.0f, 1.0f)]
        public float explosionVolume = 0.5f;             //Volume slider bar; set this between 0 and 1 in the Inspector.
        [Range(0.0f, 1.0f)]
        public float explosionPitch = 1.0f;
        [Range(0.0f, 0.2f)]
        public float explosionVolumeVariance = 0.04f;    //Variance in volume levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.04f.
        [Range(0.0f, 0.2f)]
        public float explosionPitchVariance = 0.08f;     //Variance in pitch levels per footstep; set this between 0.0 and 0.2 in the inspector. Default is 0.08f.

        [Header("Charge to Explode Settings")]
        public Vector3 startSize = new Vector3(0.1f, 0.1f, 0.1f);
        public float totalGrowthTime = 5.0f;
        public Vector3 finalSize = new Vector3(0.75f, 0.75f, 0.75f);

        public float damageColliderDelay = 1.0f;
        public float disableColliderDelay = 0.5f;

        [Header("Do not set manually:")]
        [SerializeField] private List<Enemy> hitEnemies = new List<Enemy>();
        private int glacialBurstSkillLevel;

        void OnEnable()
        {
            glacialBurstSkillLevel = PlayerStats.instance.GetSkillRankByID(23);
            DetermineRadiusAndChargeTime();
            transform.localScale = startSize;
            StartCoroutine(DelayThenExplode());
        }

        public void SetCrystalStats()
        {
            crystalExplosionTriggerCollider.SetSpellData(parentSpell.spellBaseDamage, parentSpell.spell_ID, parentSpell.element);
        }

        private IEnumerator DelayThenExplode()
        {
            Vector3 initialScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < totalGrowthTime)
            {
                // Increase the size gradually over time
                float t = elapsedTime / totalGrowthTime;
                transform.localScale = Vector3.Lerp(initialScale, finalSize, t);

                // Update the elapsed time
                elapsedTime += Time.deltaTime;

                yield return null;
            }

            // Ensure the final size is set exactly
            transform.localScale = finalSize;

            // Call the function to explode
            StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            PlayExplosionAudio();
            foreach (ParticleSystem effect in explosionEffects)
            {
                effect.Play();
            }
            meshRenderer.enabled = false;
            meshCollider.enabled = false;
            yield return new WaitForSeconds(damageColliderDelay);
            sphereCollider.enabled = true;
            yield return new WaitForSeconds(disableColliderDelay);
            sphereCollider.enabled = false;
        }

        private void PlayExplosionAudio()
        {
            explosionAudioSource.volume = Mathf.Clamp(explosionVolume + Random.Range(-explosionVolumeVariance, explosionVolumeVariance), 0f, 1f);
            explosionAudioSource.pitch = Mathf.Clamp(explosionPitch + Random.Range(-explosionPitchVariance, explosionPitchVariance), 0f, 1f);
            explosionAudioSource.clip = explosionAudioClip;
            explosionAudioSource.Play();
            charingAudioSource.Stop();
        }

        private void DetermineRadiusAndChargeTime()
        {
            Vector3 currentShockwaveScale;
            float randomVariance = Random.Range(-0.5f, 0.5f);
            totalGrowthTime += randomVariance;

            if (glacialBurstSkillLevel == 2)
            {
                totalGrowthTime *= 0.85f;
                sphereCollider.radius *= 1.15f;
                currentShockwaveScale = shockwaveTransform.localScale;
                currentShockwaveScale.x *= 1.5f;
                currentShockwaveScale.y *= 1.5f;
                shockwaveTransform.localScale = currentShockwaveScale;

            }
            else if (glacialBurstSkillLevel == 3)
            {
                totalGrowthTime *= 0.70f;
                sphereCollider.radius *= 1.30f;
                currentShockwaveScale = shockwaveTransform.localScale;
                currentShockwaveScale.x *= 1.5f;
                currentShockwaveScale.y *= 1.5f;
                shockwaveTransform.localScale = currentShockwaveScale;

            }
        }

    }
}
