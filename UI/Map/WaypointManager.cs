using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class WaypointManager : MonoBehaviour
    {
        /*Note: Target Icon is handleded in InputHandler and CameraHander*/

        [Header("Horse Icon Settings")]
        public GameObject horseIcon;
        public float horseIconDuration = 7.5f;
        private Coroutine horseIconCoroutine;

        public void StartHorseIconCoroutine()
        {
            if(horseIconCoroutine != null)
            {
                StopCoroutine(horseIconCoroutine);
            }
            horseIconCoroutine = StartCoroutine(HorseIconCoroutine());
        }

        private IEnumerator HorseIconCoroutine()
        {
            horseIcon.SetActive(false);
            horseIcon.SetActive(true);
            yield return new WaitForSeconds(horseIconDuration);
            horseIcon.SetActive(false);
            horseIconCoroutine = null;
        }

        public void HorseIconOff()
        {
            if (horseIconCoroutine != null)
            {
                StopCoroutine(horseIconCoroutine);
            }
            horseIcon.SetActive(false);
        }

    }
}
