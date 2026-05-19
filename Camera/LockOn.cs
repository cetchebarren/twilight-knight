using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class LockOn : MonoBehaviour
    {
        public Transform lockOnPoint;

        //In CameraHandler, this offset is used to determine the space from the lockonpoint - character estmiated radius
        [HideInInspector] public float characterRadius;

        private void Start()
        {
            characterRadius = GetComponent<CapsuleCollider>().radius;
        }
    }
}
