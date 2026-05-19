using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbableEdge : MonoBehaviour
{
    [Header("Point for determining height of player")]
    public Transform heightSet;

    [Header("Line for determining position of player")]
    public Transform linePointLeft;
    public Transform linePointRight;

    public Vector3 GetClosestPointOnLine(Vector3 playerPosition)
    {
        Vector3 lineVector = linePointRight.position - linePointLeft.position;
        Vector3 pointVector = playerPosition - linePointLeft.position;

        float t = Vector3.Dot(pointVector, lineVector) / lineVector.sqrMagnitude;
        t = Mathf.Clamp01(t);

        return linePointLeft.position + t * lineVector;
    }
}
