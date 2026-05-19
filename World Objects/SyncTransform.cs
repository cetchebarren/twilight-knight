using UnityEngine;

/// <summary>
/// Keeps this object synced to a target Transform's position and/or rotation.
/// Designed for stable, predictable following without root dependencies.
/// </summary>
namespace etchebarren
{
    public class SyncTransform : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The Transform this object should follow.")]
        public Transform target;

        [Header("Sync Options")]
        public bool syncPosition = true;
        public bool syncRotation = true;

        [Header("Update Mode")]
        [Tooltip("Use Update for normal objects, LateUpdate for cameras, FixedUpdate for physics bodies.")]
        public UpdateMode updateMode = UpdateMode.Update;

        public enum UpdateMode
        {
            Update,
            LateUpdate,
            FixedUpdate
        }

        private void Update()
        {
            if (updateMode == UpdateMode.Update)
                ApplySync();
        }

        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
                ApplySync();
        }

        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
                ApplySync();
        }

        /// <summary>
        /// Applies position and rotation syncing based on enabled options.
        /// </summary>
        private void ApplySync()
        {
            if (target == null)
                return;

            if (syncPosition)
                transform.position = target.position;

            if (syncRotation)
                transform.rotation = target.rotation;
        }
    }
}
