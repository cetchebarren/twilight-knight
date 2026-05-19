using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class MoveObject : MonoBehaviour
    {
        // Add Directions enum here, then choose direction to move object, simply enable script. can also include a reset position on disable!
        public enum Direction
        {
            ForwardBackward,
            UpDown,
            RightLeft,
        }

        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private Vector3 originalScale;

        public Direction direction;
        public float speed = 1f; // Speed at which the object moves along the axis
        public float growthRate = 0.05f;
        public bool resetScaleOnDisable = false;
        public bool resetPositionOnDisable = true;

        public bool rotate = false;
        public float leftRightRotation = 0f;
        public float forwardBackwardRotation = 0f;
        public float clockwiseRotation = 0f;
        public bool resetRotationOnDisable = false;

        public bool active = true;

        void Awake()
        {
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalScale = transform.localScale;
        }

        void OnDisable()
        {
            if (resetPositionOnDisable) transform.position = originalPosition;
            if (resetRotationOnDisable) transform.rotation = originalRotation;
            if (resetScaleOnDisable) transform.localScale = originalScale;
        }

        void LateUpdate()
        {
            if (!active) return;

            Vector3 moveDirection = Vector3.zero;

            switch (direction)
            {
                case Direction.UpDown:
                    moveDirection = transform.up;
                    break;
                case Direction.RightLeft:
                    moveDirection = transform.right;
                    break;
                case Direction.ForwardBackward:
                    moveDirection = transform.forward;
                    break;
            }

            transform.position += moveDirection * (speed * Time.deltaTime);

            if (rotate)
            {
                // Create a rotation vector based on input values
                Vector3 rotationVector = new Vector3(forwardBackwardRotation, leftRightRotation, clockwiseRotation);

                // Apply rotation
                transform.Rotate(rotationVector * Time.deltaTime);
            }

            if (growthRate != 0)
            {
                transform.localScale += Vector3.one * growthRate * Time.deltaTime;
            }
        }

        public void IsActive(bool isActive)
        {
            active = isActive;
        }
    }
}
