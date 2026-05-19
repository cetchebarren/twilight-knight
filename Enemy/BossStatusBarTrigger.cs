using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class BossStatusBarTrigger : MonoBehaviour
    {
        public Enemy enemy;
        public Vector3 targetScale = new Vector3(0.25f, 0.1f, 0.25f);
        public float expandDuration = 5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                enemy.StartExpandingStatusBars(targetScale, expandDuration);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                enemy.StartCollapsingStatusBars(false, true);
            }
        }
    }
}

