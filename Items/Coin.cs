using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class Coin : MonoBehaviour
    {
        public int amount = 99;

        public bool alreadyTriggered = false;

        [SerializeField] private Transform target;  // The target GameObject to gravitate towards

        public float moveSpeed = 0.1f;

        [Header("Physics Settings")]
        public bool countdownComplete = false;
        public bool followTarget = true;
        public Rigidbody rb;
        public float minForce = 2f;
        public float maxForce = 3f;
        public float horizontalForce = 1f;

        [Header("Collision Audio")]
        public AudioSource coin;
        [Range(0.0f, 1.0f)]
        public float coinCollisionVolume = 0.5f;
        public AudioClip[] coinCollisionSounds;

        public void OnEnable()
        {
            ApplyRandomUpwardForce();
            StartCoroutine(FollowCountdown());
            target = ItemGeneratorManager.instance.coinTarget;
        }

        private void Update()
        {
            if(followTarget && countdownComplete)
            {
                transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "CoinCollector" && !followTarget)
            {
                followTarget = true;
            }
            else if (other.tag == "Horse Coin Collector")
            {
                AddToInventory();
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if ((collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Animal")) && followTarget && !alreadyTriggered)
            {
                AddToInventory();
            }
            else
            {
                coin.PlayOneShot(coinCollisionSounds[Random.Range(0, coinCollisionSounds.Length)], coinCollisionVolume);
            }
        }

        private void AddToInventory()
        {
            alreadyTriggered = true;
            AcquiredNotifications.instance.NewItemNotification(null, amount);
            PlayerInventory.instance.AddGold(amount, true);
            Destroy(gameObject);
        }


        private void ApplyRandomUpwardForce()
        {
            // Generate a random force vector for upwards force
            float forceMagnitude = Random.Range(minForce, maxForce);

            // Apply upward force
            Vector3 upwardVector = Vector3.up * forceMagnitude;
            rb.AddForce(upwardVector, ForceMode.Impulse);

            // Apply random horizontal force
            Vector3 horizontalVector = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            rb.AddForce(horizontalVector.normalized * horizontalForce, ForceMode.Impulse);
        }

        private IEnumerator FollowCountdown()
        {
            yield return new WaitForSeconds(4.0f);
            countdownComplete = true;
            yield return new WaitForFixedUpdate();
            CheckImmediateOverlap();
            rb.useGravity = false;
            gameObject.layer = LayerMask.NameToLayer("UI"); // switch to UI layer (can't make anymre, 32 limit) to bypass all obstacles
        }

        private void CheckImmediateOverlap()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("CoinCollector") || hit.CompareTag("Player") || hit.CompareTag("Animal"))
                {
                    AddToInventory();
                    break;
                }
            }
        }

    }
}
