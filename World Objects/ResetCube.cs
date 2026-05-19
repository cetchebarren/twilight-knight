using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ResetCube : MonoBehaviour
    {
        [Header("References")]
        public SceneSpawnManager sceneSpawnManager;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                sceneSpawnManager.UseDefaultSpawnLocation();
            }
            else if (other.tag == "ItemDrop")
            {
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                other.transform.position = other.GetComponent<ItemDrop>().spawnedLocation;
            }
        }
    }
}
