using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ItemDrop : MonoBehaviour
    {
        [Header("Visual Settings and References")]
        public Material[] materials;
        public Renderer renderer;
        public int materialIndex = 4;
        public GameObject[] sparkleEffects;
        public AudioSource audioSource;

        [Header("Generated Item")]
        public Item item;

        [Header("Set Gold Amount to make this item gold drop, ignoring Item above")]
        public int goldAmount = 0;

        [Header("Quest Item Only")]
        public QuestItemSpawnControl questItemSpawnControl;

        [Header("World Item Only")]
        public WorldItem worldItem;

        [Header("Physics Settings")]
        public float minForce = 2f;
        public float maxForce = 3f;
        public float horizontalForce = 1f;
        public float chestPhysicsMultiplier = 0.75f;

        [Header("Audio Settings")]
        public float[] volumes;

        [Header("Temp Values")]
        public Vector3 spawnedLocation;

        public void OnEnable()
        {
            spawnedLocation = transform.position;
        }

        public void SetForcesAndApply(bool fromChest)
        {
            if(fromChest)
            {
                minForce *= chestPhysicsMultiplier;
                maxForce *= chestPhysicsMultiplier;
                horizontalForce *= chestPhysicsMultiplier;
            }
            ApplyRandomUpwardForce();
        }

        public void SetColors(int rarity) // And Volume
        {
            renderer.material = materials[rarity];

            for (int i = 1; i < sparkleEffects.Length; i++)
            {
                if(i == rarity)
                {
                    sparkleEffects[i].SetActive(true);
                    sparkleEffects[0].transform.GetChild(0).gameObject.SetActive(false);
                    audioSource.volume = volumes[i];
                }
            }
        }

        public void OnTriggerEnter(Collider other)
        {
            if(other.tag == "ItemCollector")
            {
                InteractPrompt.instance.AddInteraction(this);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.tag == "ItemCollector")
            {
                InteractPrompt.instance.RemoveInteraction(this);
            }
        }

        void ApplyRandomUpwardForce()
        {
            // Generate a random force vector for upwards force
            float forceMagnitude = Random.Range(minForce, maxForce);

            // Apply upward force
            Vector3 upwardVector = Vector3.up * forceMagnitude;
            gameObject.GetComponent<Rigidbody>().AddForce(upwardVector, ForceMode.Impulse);

            // Apply random horizontal force
            Vector3 horizontalVector = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
            gameObject.GetComponent<Rigidbody>().AddForce(horizontalVector.normalized * horizontalForce, ForceMode.Impulse);
        }

    }
}
