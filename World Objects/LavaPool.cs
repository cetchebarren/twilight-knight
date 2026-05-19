using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class LavaPool : MonoBehaviour
    {
        [Header("Lava Rise Settings")]
        public float transitionDuration = 5f;
        public float spawnHeightOffset = 1f;

        [Header("Do no not set manually")]
        [SerializeField] private Vector3 targetPos;
        [SerializeField] private Vector3 startPos;


        [SerializeField] private bool alreadyTriggered = false;

        // Start is called before the first frame update
        void Start()
        {
            targetPos = transform.localPosition;
            startPos = new Vector3(targetPos.x, (targetPos.y - spawnHeightOffset), targetPos.z);
            transform.localPosition = startPos;
            gameObject.SetActive(false);
        }

        public void MoveToTargetPosition()
        {
            if(alreadyTriggered) return;
            alreadyTriggered = true;

            gameObject.SetActive(true);
            StartCoroutine(MoveCoroutine());
        }

        private IEnumerator MoveCoroutine()
        {
            float elapsedTime = 0f;
            Vector3 initialPosition = startPos;

            while (elapsedTime < transitionDuration)
            {
                // Interpolate between the initial position and the target position
                transform.localPosition = Vector3.Lerp(initialPosition, targetPos, elapsedTime / transitionDuration);

                elapsedTime += Time.deltaTime; // Increment elapsed time
                yield return null; // Wait for the next frame
            }

            // Ensure the final position is exactly the target position
            transform.localPosition = targetPos;
        }

    }
}
