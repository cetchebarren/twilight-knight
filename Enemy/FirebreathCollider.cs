using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebreathCollider : MonoBehaviour
{
    public float speed = 1f; // Speed at which the object moves along the x-axis
    public float growthRate = 0.05f; // Rate at which the object scales

    void Update()
    {
        // Move the GameObject along the x-axis at the specified speed
        if (speed != 0)
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
        }

        // Scale the GameObject uniformly based on growth rate
        if (growthRate != 0)
        {
            transform.localScale *= (1 + growthRate * Time.deltaTime);
        }
    }
}
