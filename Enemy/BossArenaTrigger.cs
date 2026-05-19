using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace etchebarren
{
    public class BossArenaTrigger : MonoBehaviour
    {
        [Header("Enemy to Toggle Immunity")]
        public Enemy boss;
        public bool endBossImmunityOnWalls = true;

        [Header("Walls to enable upon entering boss arena")]
        public GameObject[] bossArenaWalls;
        public float wallsDelay = 0f;

        public UnityEvent optionalEvents;

        [SerializeField] private bool alreadyTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!alreadyTriggered)
                {
                    alreadyTriggered = true;

                    optionalEvents.Invoke();

                    StartCoroutine(WallsDelay());
                }
            }
        }

        private IEnumerator WallsDelay()
        {
            yield return new WaitForSeconds(wallsDelay);

            if (endBossImmunityOnWalls) boss.SetImmune(false);

            foreach (GameObject wall in bossArenaWalls)
            {
                wall.SetActive(true);
            }
        }
    }
}
