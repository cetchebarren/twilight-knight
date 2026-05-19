using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace etchebarren
{
    public class WorldIcon : MonoBehaviour
    {
        [Header("References")]
        public Transform targetPosition;
        public Transform iconTransform;
        public Camera camera;

        [Header("Settings")]
        public int margin = 70;
        //public Vector3 offset;

        [Header("Scale")]
        public float minScale = 0.5f;
        public float maxScale = 2f;
        public float distanceForMaxScale = 10f;
        public float distanceForMinScale = 30f;
        private float currentScale;

        [Header("Disable Image Component")]
        public bool disableWithinDistance = false;
        public bool reenableOutsideDistance = false;
        public float disableDistance = 5.0f;

        [Header("Fade Distance")]
        public bool fade = false;
        public float transparentDistance = 5.0f;
        public float opaqueDistance = 10.0f;
        public Image imageComponent;

        void OnEnable()
        {
            imageComponent.enabled = true;
            // Directly set the alpha value to the Image component
            imageComponent.color = new Color(imageComponent.color.r, imageComponent.color.g, imageComponent.color.b, 1.0f);
        }

        void LateUpdate()
        {
            // Get the position on screen, in screen coordinates
            Vector3 screenPos = camera.WorldToScreenPoint(targetPosition.position);
            if (Mathf.Approximately(screenPos.z, 0))
            {
                return;
            }

            // Determine Distance
            float distanceToTarget = Vector3.Distance(targetPosition.position, camera.transform.position);

            // Set the currentScale based on the distance
            if (distanceToTarget <= distanceForMaxScale)
            {
                currentScale = Mathf.Lerp(minScale, maxScale, 1 - distanceToTarget / distanceForMaxScale);
            }
            else
            {
                currentScale = minScale;
            }

            // Apply the scale to the object
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);

            // save half screen resulution because we will need it often
            Vector3 halfScreen = new Vector3(Screen.width, Screen.height) / 2;

            // we don't want the Z-Value in our center-vector because it would
            // cause problems when normalizing it
            Vector3 screenPosNoZ = screenPos;
            screenPosNoZ.z = 0;
            // get the vector from the center of the screen to the
            // calculated screen position
            Vector3 screenCenterPos = screenPosNoZ - halfScreen;

            // we have to invert the vector when we are looking away from the target
            // the vector is just projected on the view-plane, think looking in a mirror
            if (screenPos.z < 0)
            {
                screenCenterPos *= -1;
            }

            // debug check, if the ray is pointing in the wanted direction
            // can only be seen with gizmos enabled, in scene view (3D Mode only)
            //Debug.DrawRay(halfScreen, screenCenterPos.normalized * 100000, Color.red);

            // check if the target is on screen
            if (screenPos.z < 0 || screenPos.x > Screen.width || screenPos.x < 0 ||
                screenPos.y > Screen.height || screenPos.y < 0)
            {
                //Debug.Log("OUT OF SCREEN");
                //screenPos = camera.WorldToScreenPoint(targetPosition.position + offset);
                //screenPosNoZ = screenPos;
                //screenPosNoZ.z = 0;
                //screenCenterPos = screenPosNoZ - halfScreen;
                //if (screenPos.z < 0)
                //{
                //    screenCenterPos *= -1;
                //}

                if(fade)imageComponent.color = new Color(imageComponent.color.r, imageComponent.color.g, imageComponent.color.b, 1.0f);

                // if you have a arrow on your symbol, pointing in the
                // direction, enable it here:
                //_testInstance.rotator.gameObject.SetActive(true);

                // rotate it to point towards the target position
                //_testInstance.rotator.rotation =
                //Quaternion.FromToRotation(Vector3.up, screenCenterPos);

                // normalized ScreenCenterPosition
                Vector3 norSCP = screenCenterPos.normalized;

                // avoid dividing by zero
                if (norSCP.x == 0)
                {
                    norSCP.x = 0.01f;
                }
                if (norSCP.y == 0)
                {
                    norSCP.y = 0.01f;
                }

                // stretch the normalized screenCenterPosition so that X is at the edge
                Vector3 xScreenCP = norSCP * (halfScreen.x / Mathf.Abs(norSCP.x));
                // stretch the normalized screenCenterPosition so that Y is at the edge
                Vector3 yScreenCP = norSCP * (halfScreen.y / Mathf.Abs(norSCP.y));

                // compare the streched vectors in length and use the smaller one
                if (xScreenCP.sqrMagnitude < yScreenCP.sqrMagnitude)
                {
                    screenPos = halfScreen + xScreenCP;
                }
                else
                {
                    screenPos = halfScreen + yScreenCP;
                }
            }
            else
            {
                if (fade)
                {
                    // Determine Opacity based on distance
                    float alpha = Mathf.Clamp01((distanceToTarget - transparentDistance) / (opaqueDistance - transparentDistance));

                    // Directly set the alpha value to the Image component
                    imageComponent.color = new Color(imageComponent.color.r, imageComponent.color.g, imageComponent.color.b, alpha);

                }

                if (disableWithinDistance)
                {
                    if (distanceToTarget <= disableDistance)
                    {
                        // CHATGPT: Also check that the targetPosition object is ON SCREEN
                        imageComponent.enabled = false;
                    }
                }

                //_offset = Vector3.zero;

                // if you have a arrow on your symbol, pointing in the
                // direction, disable it here:
                //_testInstance.rotator.gameObject.SetActive(false);
            }

            if (reenableOutsideDistance)
            {
                if (distanceToTarget > disableDistance)
                {
                    // CHATGPT: Also check that the targetPosition object is ON SCREEN
                    imageComponent.enabled = false;
                }
            }

            // clamp the result, so we can always see the full marker/tracker image
            screenPos.z = 0;
            screenPos.x = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
            screenPos.y = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);



            // set the transform position
            iconTransform.position = screenPos;
        }
    }
}
