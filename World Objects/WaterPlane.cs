using System.Collections;
using System.Collections.Generic;
using MalbersAnimations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace etchebarren
{
    public class WaterPlane : MonoBehaviour
    {
        [SerializeField] private bool alreadyTriggeredSwim = false;
        [SerializeField] private bool alreadyTriggeredShallow = false;
        [SerializeField] private bool alreadyTriggeredDeep = false;
        public  bool alreadyTriggeredSwimMount = false;
        [SerializeField] private bool alreadyTriggeredShallowHorse = false;
        [SerializeField] 
        
        
        private bool alreadyTriggeredDeepHorse = false;

        public bool stillWater = false;

        public float minHeight = 15f;               // Minimum height of the tide
        public float maxHeight = 16f;             // Maximum height of the tide
        public float speed = 30f;
        public AnimationCurve waveCurve;            // Curve for wave behavior
        [SerializeField] float waveCurveDuration;

        private float currentHeight;                // Current height of the tide
        private Vector3 startPosition;              // Starting position of the GameObject

        private StepsManager stepsManagerAI;
        private StepsManager stepsManagerMount;
        public float currentTime = 0f;

        private void Start()
        {
            startPosition = transform.position;

            waveCurveDuration = waveCurve.keys[waveCurve.length - 1].time;

            if (stillWater)
            {
                Vector3 newPosition = startPosition;
                newPosition.y = minHeight;
                transform.position = newPosition;
            }

            // Null if main menu, skip if we are in main menu scene
            if(SceneManager.GetActiveScene().buildIndex != 0)
            {
                stepsManagerAI = MountManager.instance.stepsManagerAI;
                stepsManagerMount = MountManager.instance.stepsManagerMount;
            }
        }

        private void Update()
        {

            if (stillWater) return;
            // Increment current time based on speed
            currentTime = (currentTime + (Time.deltaTime * speed)) % waveCurveDuration;

            // Evaluate wave behavior using AnimationCurve at the current time
            float waveOffset = waveCurve.Evaluate(currentTime);

            // Calculate current tide height based on wave behavior
            currentHeight = Mathf.Lerp(minHeight, maxHeight, waveOffset);

            // Update GameObject position to simulate tide movement
            Vector3 newPosition = startPosition;
            newPosition.y = currentHeight;
            transform.position = newPosition;
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.tag == "SwimmingCollider" && !alreadyTriggeredSwim)
            {
                alreadyTriggeredSwim = true;
                PlayerLocomotion.instance.SetSwimming(true, gameObject);
                Debug.Log("isSwimming = TRUE");
            }
            else if (other.tag == "ShallowWaterCollider" && !alreadyTriggeredShallow)
            {
                alreadyTriggeredShallow = true;
                PlayerLocomotion.instance.touchingShallowWater = true;
            }
            else if (other.tag == "DeepWaterCollider" && !alreadyTriggeredDeep)
            {
                alreadyTriggeredDeep = true;
                PlayerLocomotion.instance.touchingDeepWater = true;
            }

            if (other.tag == "SwimmingColliderMount" && !alreadyTriggeredSwimMount)
            {
                alreadyTriggeredSwimMount = true;
                MountLocomotion.instance.SetSwimming(true, gameObject);
                Debug.Log("isSwimming = TRUE");
            }
            else if (other.tag == "ShallowWaterColliderHorse" && !alreadyTriggeredShallowHorse)
            {
                alreadyTriggeredShallowHorse = true;
                stepsManagerMount.inShallowWater = true;
                stepsManagerAI.inShallowWater = true;
            }
            else if (other.tag == "DeepWaterColliderHorse" && !alreadyTriggeredDeepHorse)
            {
                alreadyTriggeredDeepHorse = true;
                stepsManagerMount.inDeepWater = true;
                stepsManagerAI.inDeepWater = true;
            }

        }

        public void OnTriggerExit(Collider other)
        {
            if (other.tag == "SwimmingCollider" && alreadyTriggeredSwim)
            {
                alreadyTriggeredSwim = false;
                PlayerLocomotion.instance.SetSwimming(false, null);
                Debug.Log("isSwimming = FALSE");
            }
            else if (other.tag == "ShallowWaterCollider" && alreadyTriggeredShallow)
            {
                alreadyTriggeredShallow = false;
                PlayerLocomotion.instance.touchingShallowWater = false;
            }
            else if (other.tag == "DeepWaterCollider" && alreadyTriggeredDeep)
            {
                alreadyTriggeredDeep = false;
                PlayerLocomotion.instance.touchingDeepWater = false;
            }

            if (other.tag == "SwimmingColliderMount" && alreadyTriggeredSwimMount)
            {
                alreadyTriggeredSwimMount = false;
                MountLocomotion.instance.SetSwimming(false, null);
                Debug.Log("isSwimming = FALSE");
            }
            else if (other.tag == "ShallowWaterColliderHorse" && alreadyTriggeredShallowHorse)
            {
                alreadyTriggeredShallowHorse = false;
                stepsManagerMount.inShallowWater = false;
                stepsManagerAI.inShallowWater = false;
            }
            else if (other.tag == "DeepWaterColliderHorse" && alreadyTriggeredDeepHorse)
            {
                alreadyTriggeredDeepHorse = false;
                stepsManagerMount.inDeepWater = false;
                stepsManagerAI.inDeepWater = false;
            }
        }
    }
}
