using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used to reset positions and rotations of objects in cutscenes

namespace etchebarren
{
    public class TransformResetter : MonoBehaviour
    {
        public Vector3 startingPos;
        public Quaternion startingRot;
        public Vector3 startingScale;

        void Awake()
        {
            startingPos = transform.position;
            startingRot = transform.rotation;
            startingScale = transform.localScale;
        }

        public void Reset()
        {
            transform.position = startingPos;
            transform.rotation = startingRot;
            transform.localScale = startingScale;
        }
    }
}
