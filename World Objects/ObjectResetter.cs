using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class ObjectResetter : MonoBehaviour
    {
        private GameObject targetObject;
        public float resetDuration = 0.5f;

        public void ResetObject(GameObject _targetObject)
        {
            targetObject = _targetObject;
            StartCoroutine(ResetCoroutine());
        }

        private IEnumerator ResetCoroutine()
        {
            targetObject.SetActive(false);
            yield return new WaitForSeconds(resetDuration);
            targetObject.SetActive(true);
        }
    }
}
