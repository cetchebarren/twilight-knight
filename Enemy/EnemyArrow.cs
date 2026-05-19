using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class EnemyArrow : MonoBehaviour
    {
        [Header("Attachment & Physics")]
        public Rigidbody rb;
        public Transform arrowBody;
        public float maximumRotationTime = 10.0f; // Adjust this value to control how long it takes to reach maximum rotation
        private float rotationTimer = 0.0f;
        public float maximumForwardRotation = 25.0f;
        private bool dropping = true;
        private bool useGravity = true;
        public float arrowGravity = 5.0f;
        public Collider arrowCollider;

        [Header("Audio")]
        public AudioSource arrowAudio;
        public AudioClip[] impactAudioClips;

        void OnEnable()
        {
            AnimEvents.instance.UpdateEnemyProjectileCollidersList(arrowCollider, true);
        }

        void OnDisable()
        {
            AnimEvents.instance.UpdateEnemyProjectileCollidersList(arrowCollider, false);
        }

        void Update()
        {
            if (dropping)
            {
                // Calculate the rotation angle for this frame
                float rotationAngle = Mathf.Lerp(0, maximumForwardRotation, rotationTimer / maximumRotationTime);

                // Rotate the arrow stick around its local forward axis
                Vector3 currentRotation = arrowBody.localEulerAngles;
                currentRotation.x = rotationAngle;
                arrowBody.localEulerAngles = currentRotation;

                // Update the rotation timer
                rotationTimer += Time.deltaTime;

                // Check if reached maximum rotation
                if (rotationTimer >= maximumRotationTime)
                {
                    dropping = false;
                }
            }
        }

        void FixedUpdate()
        {
            if (useGravity)
            {
                rb.AddForce(new Vector3(0, -1.0f, 0) * rb.mass * arrowGravity);
            }
        }

        public void PlayImpactAudio()
        {
            arrowAudio.PlayOneShot(impactAudioClips[Random.Range(0, impactAudioClips.Length)], 1.0f);
        }

        public void Impact(GameObject hitObject, bool blocked = false)
        {
            dropping = false;
            PlayImpactAudio();
            // Freeze the arrow after impact
            useGravity = false;
            rb.isKinematic = true;

            if (hitObject.name == "Projectile Shield Collider")
            {
                // 1. Find active shield
                Transform activeShield = null;
                foreach (Transform child in AnimEvents.instance.shieldItemParent.transform)
                {
                    if (child.gameObject.activeInHierarchy)
                    {
                        activeShield = child;
                        break;
                    }
                }
                if (activeShield == null) return;

                // 2. Find shield collider by tag
                Collider shieldCol = null;
                foreach (Collider col in activeShield.GetComponentsInChildren<Collider>(true))
                {
                    if (col.CompareTag("ShieldCollider"))
                    {
                        shieldCol = col;
                        break;
                    }
                }
                if (shieldCol == null) return;

                // 3. Get arrow collider
                Collider arrowCol = GetComponent<Collider>();
                if (arrowCol == null) return;

                // --- THE IMPORTANT FIX ---
                // First get a point on the shield that is closest to the arrow center
                Vector3 shieldClosestToArrow = shieldCol.ClosestPoint(arrowCol.bounds.center);

                // Now get the closest point on the arrow to THAT point
                Vector3 arrowPoint = arrowCol.ClosestPoint(shieldClosestToArrow);

                // And the closest point on the shield to THAT arrow point
                Vector3 shieldPoint = shieldCol.ClosestPoint(arrowPoint);
                // --------------------------

                // Compute delta
                Vector3 delta = shieldPoint - arrowPoint;

                /*
                Debug.Log("Arrow collider bounds center: " + arrowCol.bounds.center);
                Debug.Log("Arrow collider bounds extents: " + arrowCol.bounds.extents);
                Debug.Log("Shield collider bounds center: " + shieldCol.bounds.center);
                Debug.Log("Shield collider bounds extents: " + shieldCol.bounds.extents);

                Debug.Log("ArrowPoint: " + arrowPoint);
                Debug.Log("ShieldPoint: " + shieldPoint);
                Debug.Log("Delta: " + delta + " | Magnitude: " + delta.magnitude);

                Debug.Log("Arrow inside shield? " + shieldCol.bounds.Contains(arrowCol.bounds.center));*/


                // Only move if necessary
                float dist = delta.magnitude;
                if (dist > AnimEvents.instance.arrowCorrectionMinDistance)
                {
                    transform.position += delta;

                    Vector3 normal = (arrowPoint - shieldPoint).normalized;
                    transform.position -= normal * AnimEvents.instance.arrowCorrectionBoundsBonus;
                }
                else
                {
                    Vector3 normal = (arrowPoint - shieldPoint).normalized;
                    transform.position -= normal * AnimEvents.instance.arrowCorrectionBoundsBonus;
                }

                // Parent to shield
                transform.SetParent(activeShield);
            }
            else
            {
                // Normal behavior: parent to whatever was hit
                transform.SetParent(hitObject.transform);
            }

            Destroy(gameObject, 7.5f);
        }
    }
}
