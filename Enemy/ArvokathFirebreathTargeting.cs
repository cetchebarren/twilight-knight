using System.Collections;
using UnityEngine;

namespace etchebarren
{
    public class ArvokathFirebreathTargeting : MonoBehaviour
    {
        [SerializeField] private bool targeting = false;

        [SerializeField] private Transform target;                    // The target to look at
        [SerializeField] private Vector3 targetOffset;
        [SerializeField] private float horizontalMinRotation = -70f;  // Minimum rotation angle for Y-axis (horizontal)
        [SerializeField] private float horizontalMaxRotation = 70f;   // Maximum rotation angle for Y-axis (horizontal)
        [SerializeField] private float verticalMinRotation = -60f;    // Minimum rotation angle for X-axis (vertical)
        [SerializeField] private float verticalMaxRotation = 60f;     // Maximum rotation angle for X-axis (vertical)
        [SerializeField] private float rotationSpeed = 5f;            // Speed of rotation

        private Coroutine targetingCoroutine;

        void Start()
        {
            if (target == null)
            {
                target = ScenePersistentPlayerObject.instance.player.transform;
            }
        }

        public void SetTargeting(bool isActive)
        {
            if (target == null)
            {
                Debug.LogError("Firebreath could not target player, null target reference.");
                return;
            }
            targeting = isActive;
            if (isActive)
                StartTargeting();
            else
                StopTargeting();
        }

        private void StartTargeting()
        {
            if (targetingCoroutine == null)
            {
                targetingCoroutine = StartCoroutine(TargetingCoroutine());
            }
        }

        private void StopTargeting()
        {
            if (targetingCoroutine != null)
            {
                StopCoroutine(targetingCoroutine);
                targetingCoroutine = null;
            }
        }

        private IEnumerator TargetingCoroutine()
        {
            while (targeting && target != null)
            {
                Vector3 targetPos = target.position + targetOffset;

                // Calculate the direction to the target in world space
                Vector3 directionToTarget = targetPos - transform.position;

                // Convert this world direction to local space
                Vector3 localDirectionToTarget = transform.parent.InverseTransformDirection(directionToTarget);

                // Determine the desired rotation in local space
                Quaternion targetLocalRotation = Quaternion.LookRotation(localDirectionToTarget, Vector3.up);

                // Apply rotation constraints
                Quaternion limitedLocalRotation = ClampRotation(targetLocalRotation);

                // Smoothly rotate towards the clamped target rotation in local space
                transform.localRotation = Quaternion.Slerp(transform.localRotation, limitedLocalRotation, rotationSpeed * Time.deltaTime);

                yield return null;
            }
        }

        private Quaternion ClampRotation(Quaternion targetRotation)
        {
            // Convert the target rotation to local Euler angles
            Vector3 targetEulerAngles = targetRotation.eulerAngles;

            // Normalize angles to ensure they're within -180 to 180 for clamping
            targetEulerAngles.x = NormalizeAngle(targetEulerAngles.x);
            targetEulerAngles.y = NormalizeAngle(targetEulerAngles.y);

            // Apply constraints for horizontal rotation (Y-axis) without locking
            targetEulerAngles.y = Mathf.Clamp(targetEulerAngles.y, horizontalMinRotation, horizontalMaxRotation);

            // Apply constraints for vertical rotation (X-axis) without locking
            targetEulerAngles.x = Mathf.Clamp(targetEulerAngles.x, verticalMinRotation, verticalMaxRotation);

            // Return the constrained rotation as a Quaternion
            return Quaternion.Euler(targetEulerAngles);
        }

        private float NormalizeAngle(float angle)
        {
            // Normalize the angle to the range -180 to 180
            angle = angle % 360;
            if (angle > 180) angle -= 360;
            return angle;
        }
    }
}
