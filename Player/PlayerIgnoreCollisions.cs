using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class PlayerIgnoreCollisions : MonoBehaviour
    {
        public static PlayerIgnoreCollisions instance;

        /* This was implemented to prevent collisions between enemy attacks (arrows) and colliders that are not 
         * valid for hit detection, but still trigger collisions because they are children of parent objects that
         * are valid layers for hit detection */
        [Header("Colliders to Ignore Collisions with Enemy Attacks")]
        public Collider[] ignoreCollisionsWithAttacks;
        public Collider fullShieldCollider;
        public Collider[] projectileShieldColliders;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
