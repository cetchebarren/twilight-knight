using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlazeAISpace;

namespace etchebarren
{
    public class ArvokathEvents : MonoBehaviour
    {
        int test = 0;

        [Header("General References")]
        public Enemy arvokath;
        public BlazeAI blaze;
        public AttackStateBehaviour attackStateBehaviour;

        [Header("Physical Attack Colliders")]
        public DamageTriggerCollider[] attackColliders;

        [Header("Fire Slam Attack")]
        public GameObject fireSlamPrefab;
        public Transform fireSlamSpawnPoint;
        public float fireSlamColliderSpawnDelay = 0.1f;
        public float fireSlamColliderDuration = 2.0f;

        [Header("Meteor Attack")]
        public GameObject meteorPrefab;
        public float meteorColliderSpawnDelay = 0.5f;
        public float meteorColliderDuration = 1.0f;

        [Header("Firebreath")]
        public GameObject firebreathColliderPrefab;
        public Transform firebreathSpawnPoint;
        public ParticleSystem firebreathParticleSystem;
        public Coroutine firebreath;
        public float firebreathInterval = 1f;
        public float firebreathColliderLifetime = 6f;
        public ArvokathFirebreathTargeting targeting;
        public float firebreathColliderSpawnDuration = 6f;
        public GameObject firebreathPointLight;

        [Header("Wingfire")]
        public ParticleSystem wingfireR;
        public ParticleSystem wingfireL;
        public float damageTakenAfterWingFire = 0.7f;
        public float damageDealtAfterWingFire = 1.25f;

        [Header("Spawn Lava")]
        public Transform eventLocation;
        public GameObject lavaPoolsCameraShake;
        public LavaPool[] lavaPools;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip longRoarClip;
        public AudioSource firebreathAudioSource;
        public AudioSource footstepAudioSource;
        public AudioClip[] stepAudioClips;
        public AudioSource lavaPoolsAudioSource;

        #region Fire Slam
        public void SpawnFireSlam()
        {
            StartCoroutine(FireSlam());
        }

        private IEnumerator FireSlam()
        {
            // Instantiate and assign enemy variable to damage trigger collider 
            GameObject fireSlam = Instantiate(fireSlamPrefab, fireSlamSpawnPoint);
            fireSlam.transform.parent = null;
            SphereCollider col = fireSlam.GetComponent<SphereCollider>();
            fireSlam.GetComponent<DamageTriggerCollider>().enemy = arvokath;

            // Delay before activating damage collider
            yield return new WaitForSeconds(fireSlamColliderSpawnDelay);
            col.enabled = true;

            // Delay before deactivating damage collider
            yield return new WaitForSeconds(fireSlamColliderDuration);
            col.enabled = false;
        }
        #endregion

        #region Meteor
        public void SpawnMeteor()
        {
            StartCoroutine(Meteor());
        }

        private IEnumerator Meteor()
        {
            // Instantiate and assign enemy variable to damage trigger collider 
            Transform spawnPos = ScenePersistentPlayerObject.instance.player.transform;
            GameObject meteor = Instantiate(meteorPrefab, spawnPos);
            meteor.transform.parent = null;
            SphereCollider col = meteor.GetComponent<SphereCollider>();
            meteor.GetComponent<DamageTriggerCollider>().enemy = arvokath;

            // Delay before activating damage collider
            yield return new WaitForSeconds(meteorColliderSpawnDelay);
            col.enabled = true;

            // Delay before deactivating damage collider
            yield return new WaitForSeconds(meteorColliderDuration);
            col.enabled = false;

            Destroy(meteor, 6f);
        }
        #endregion

        #region Firebreath
        public void StartFirebreath()
        {
            Debug.Log("Starting FB");
            if(firebreath != null)
            {
                firebreathParticleSystem.Stop();
                StopFirebreath();
            }

            firebreath = StartCoroutine(Firebreath());
            StartCoroutine(FirebreathCountdown());
            firebreathPointLight.SetActive(true);
            blaze.GetComponent<AttackStateBehaviour>().onAttackRotate = true;
        }

        private void StopFirebreath()
        {
            Debug.Log("Stopping FB");
            if(firebreath != null) StopCoroutine(firebreath);
            targeting.SetTargeting(false);
            firebreath = null;
            firebreathPointLight.SetActive(false);
            firebreathPointLight.GetComponent<Light>().intensity = 0f;
            blaze.GetComponent<AttackStateBehaviour>().onAttackRotate = false;
        }

        private IEnumerator FirebreathCountdown()
        {
            yield return new WaitForSeconds(firebreathColliderSpawnDuration);
            StopFirebreath();
        }

        private IEnumerator Firebreath()
        {
            targeting.SetTargeting(true);

            firebreathAudioSource.Play();

            firebreathParticleSystem.Play();

            // faceplayer?
            while (true)
            {
                // Instantiate and assign enemy variable to damage trigger collider 
                GameObject fire = Instantiate(firebreathColliderPrefab, firebreathSpawnPoint);
                fire.transform.parent = null;
                fire.GetComponent<DamageTriggerCollider>().enemy = arvokath;

                Destroy(fire, firebreathColliderLifetime);

                // Wait for the specified interval before the next instantiation
                yield return new WaitForSeconds(firebreathInterval);
            }
            firebreath = null;
        }
        #endregion

        #region Wing Fire
        public void WingFire(int play=1)
        {
            // Using integer for animation event argument (no bools)
            if (play == 1)
            {
                wingfireL.gameObject.SetActive(true);
                wingfireR.gameObject.SetActive(true);
                wingfireL.Play();
                wingfireR.Play();

                arvokath.damageTakenModifier = damageTakenAfterWingFire;
                arvokath.damageDealtModifier = damageDealtAfterWingFire;

                TextNotificationsManager.instance.NewTextNotifaction("Arvokath's Attack and Defense has increased", true);
            }
            else
            {
                wingfireL.Stop();
                wingfireR.Stop();
            }
        }

        #endregion

        #region Health Events
        public void StartSpawnLava()
        {
            StartCoroutine(SpawnLavaCouroutine());
        }

        private IEnumerator SpawnLavaCouroutine()
        {
            // Start Moving to Target Location
            arvokath.immune = true;
            blaze.friendly = true;
            blaze.ChangeState("normal");
            blaze.MoveToLocation(eventLocation.position);

            float acceptableThreshold = 1.5f; // Threshold for distance
            float failsafeDuration = 10f;         // Failsafe duration in seconds
            float elapsedTime = 0f;           // Timer to track elapsed time
            float checkInterval = 0.1f;       // Frequency of checking which helps to avoid frame-by-frame checks

            while (elapsedTime < failsafeDuration)
            {
                float distance = Vector2.Distance(new Vector2(gameObject.transform.position.x, gameObject.transform.position.z),
                                                  new Vector2(eventLocation.position.x, eventLocation.position.z));

                if (distance <= acceptableThreshold)
                {
                    Debug.LogError("Breaking loop. Distance: " + distance);
                    break;
                }

                Debug.Log($"Dist: {distance}, Elapsed Time: {elapsedTime}");

                // Wait for the next frame or a specified interval
                yield return new WaitForSeconds(checkInterval);

                // Increment elapsed time
                elapsedTime += checkInterval;
            }

            Debug.LogError("ENEMY REACHED DESTINATION");
            blaze.IgnoreMoveToLocation();
            blaze.enabled = false;

            // Enemy has reached position
            blaze.animManager.Play("SpawnLava", 0.2f);
        }

        public void SpawnLava()
        {
            lavaPoolsCameraShake.SetActive(true);

            lavaPoolsAudioSource.Play();

            foreach (LavaPool lavaPool in lavaPools)
            {
                lavaPool.MoveToTargetPosition();
            }

            attackStateBehaviour.attackInIntervalsTime = new Vector2(1f, 1.5f);
            TextNotificationsManager.instance.NewTextNotifaction("Arvokath grows more <b>aggressive</b>", true);
        }

        public void StartWingFire()
        {
            // Start Moving to Target Location
            blaze.friendly = true;
            blaze.ChangeState("normal");
            blaze.enabled = false;

            // Enemy has reached position
            blaze.animManager.Play("StartWingFire", 0.2f);

            //StartCoroutine(WingFireCoroutine());
        }

        public void PlayRoarAudio()
        {
            audioSource.PlayOneShot(longRoarClip, 1.0f);
        }

        public void PlayStepAudio()
        {
            if (stepAudioClips.Length > 0) // Ensure the array is not empty
            {
                footstepAudioSource.PlayOneShot(stepAudioClips[Random.Range(0, stepAudioClips.Length)], 1.0f);
            }
            else
            {
                Debug.LogError("No step audio clips assigned to Arvokath Footstep Audio Clips!");
            }
        }

        public void RenableBehavior()
        {
            arvokath.immune = false;
            blaze.friendly = false;
            blaze.enabled = true;
            blaze.SetTarget(ScenePersistentPlayerObject.instance.player, false, true);
        }
        #endregion
    }
}
