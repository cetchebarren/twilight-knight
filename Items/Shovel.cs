using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shovel : MonoBehaviour
{
    public Transform leftHand; // the left hand bone
    public Quaternion defaultLocalRotation;
    public bool defaultLocalRotationSet = false;

    void OnEnable()
    {
        if (!defaultLocalRotationSet)
        {
            defaultLocalRotation = transform.localRotation;
            defaultLocalRotationSet = true;
        }
        transform.localRotation = defaultLocalRotation;
    }

    void LateUpdate()
    {
        Vector3 direction = leftHand.position - transform.position;
        transform.rotation = Quaternion.LookRotation(direction, transform.up);
    }
}
