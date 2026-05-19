using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BlazeAISpace;

namespace etchebarren
{
    public class EnemyArcheryManager : MonoBehaviour
    {
        [Header("Scripts and Object References")]
        public Enemy enemy;
        public GameObject arrowPrefab;
        public GameObject staticArrow;
        public CoverShooterBehaviour coverShooterBehaviour;
        public StaticArrowAlignment staticArrowAlignment;

        [Header("Bowstring References")]
        [Tooltip("The point on the bowstring that controls the stretching of the string.")]
        public Transform bowstringPivot;
        [Tooltip("The parent that the bow string pivot will attach to on the enemy's hand.")]
        public Transform enemyHandBowstringAttachmentPoint;
        [Tooltip("The original parent of the bowstring, to return it to its poisiton after firing.")]
        public Transform originalBowstringParent;
        [Tooltip("The original position of the bowstring pivot, before being attached. Used for resetting.")]
        public Vector3 originalBowstringPosition;

        [Header("Projectile Settings")]
        [Range(0, 100)]
        [Tooltip("For clarity, higher number means better accuracy. Max = Perfect accuracy.")]
        public int enemyAccuracy = 75;
        public Vector3 targetOffset;
        public float arrowForce = 2.0f;
        private GameObject target;

        [Header("Audio")]
        public AudioSource bowAudio;
        public AudioClip stringTension;
        public AudioClip release;

        [Header("Debug Options")]
        public GameObject testSphere;
        public bool debugTargetPosition = false;

        public void AttachBowstring()
        {
            // Make the bowstringPivot a child of the enemyHandBowstringAttachmentPoint and zero out its local position
            bowstringPivot.SetParent(enemyHandBowstringAttachmentPoint);
            bowstringPivot.localPosition = Vector3.zero;
            //Debug.Log("Bow String Attached");
            EnableStrafing();
        }

        public void ReleaseBowstring()
        {
            // Make the bowstringPivot a child of the originalBowstringParent and restore the position to originalBowstringPosition
            bowstringPivot.SetParent(originalBowstringParent);
            bowstringPivot.localPosition = originalBowstringPosition;
        }

        public void DisplayStaticArrow()
        {
            staticArrow.SetActive(true);
            ///Debug.Log("Static Arrow: true");
        }

        public void HideStaticArrow()
        {
            staticArrow.SetActive(false);
            //Debug.Log("Static Arrow: false");
        }

        public void InstantiateAndFire()
        {
            if(enemy.blaze.enemyToAttack != null)
            {
                HideStaticArrow();

                target = enemy.blaze.enemyToAttack;

                Vector3 targetPosition = target.transform.position + targetOffset;

                if (debugTargetPosition && ScenePersistentPlayerObject.instance.debugShowColliders)
                {
                    var archerTarget_DEBUG = Instantiate(testSphere, targetPosition, target.transform.rotation);
                    Destroy(archerTarget_DEBUG, 10.0f);
                }

                // Calculate the direction from the current object to the target
                Vector3 directionToTarget = targetPosition - staticArrow.transform.position;

                // Calculate the rotation needed to face the target
                Quaternion rotation = Quaternion.LookRotation(directionToTarget);

                // Introduce randomness to the rotation to simulate accuracy
                float maxRandomAngle = GetAccuracy();
                Debug.Log("MAX random angle for Accuracy " + enemyAccuracy + " is: " + maxRandomAngle);
                rotation *= Quaternion.Euler(Random.Range(-maxRandomAngle, maxRandomAngle), Random.Range(-maxRandomAngle, maxRandomAngle), Random.Range(-maxRandomAngle, maxRandomAngle));

                // Instantiate the object with the calculated rotation
                var enemyProjectile = Instantiate(arrowPrefab, staticArrow.transform.position, rotation);

                enemyProjectile.GetComponent<DamageTriggerCollider>().enemy = enemy;

                enemyProjectile.GetComponent<DamageTriggerCollider>().enemyTransform = enemy.gameObject.transform;

                PlayReleaseAudio();

                enemyProjectile.GetComponent<Rigidbody>().AddForce(enemyProjectile.transform.forward * arrowForce, ForceMode.Impulse);

                ReleaseBowstring();

                DisableStrafing();
            } 
        }

        public float GetAccuracy()
        {
            // Map accuracy from range [0, 100] to range [10, 0]
            float maxRandomAngle = Mathf.Lerp(10f, 0f, enemyAccuracy / 100f);
            return maxRandomAngle;
        }

        public void DisableStrafing()
        {
            coverShooterBehaviour.strafe = false;
        }

        public void EnableStrafing()
        {
            coverShooterBehaviour.strafe = true;
        }

        /* This function is called on animations that occur after nocking arrow
         * If for some reason the draw animation is interrupted, this function
         * will correct any awkward occurences with static arrow, bowstring or 
         * with strafing enable/disable */
        public void CorrectArcherState()
        {
            EnableStrafing();
            AttachBowstring();
            DisplayStaticArrow();
        }

        public void OnDeath()
        {
            ReleaseBowstring();
            staticArrowAlignment.align = false;
        }

        #region Audios

        public void PlayStringTensionAudio()
        {
            bowAudio.PlayOneShot(stringTension, 1.0f);
        }

        public void PlayReleaseAudio()
        {
            bowAudio.PlayOneShot(release, 0.5f);
        }

        #endregion

    }
}