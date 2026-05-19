using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimLocationM : MonoBehaviour
{
    public static AnimLocationM instance;

    public Transform playerTransform;
    public Animator anim;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
