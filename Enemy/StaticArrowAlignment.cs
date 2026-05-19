using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticArrowAlignment : MonoBehaviour
{
    [Tooltip("The point on the bow that the static arrow should rotate to align with.")]
    public Transform bowAttachmentPoint;
    public bool align = true;

    void Update()
    {
        if(align) RotateArrowTowardsBow();
    }

    public void RotateArrowTowardsBow()
    {
        // Calculate direction from arrow to bow attachment point
        Vector3 directionToBow = bowAttachmentPoint.position - transform.position;

        // Calculate rotation to face bow attachment point
        Quaternion rotationToBow = Quaternion.LookRotation(directionToBow);

        // Set arrow's world rotation directly to the rotation towards the bow attachment point
        //transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToBow, rotationSpeed * Time.deltaTime);

        // Set arrow's world rotation directly to the rotation towards the bow attachment point
        transform.rotation = rotationToBow;
    }


}
