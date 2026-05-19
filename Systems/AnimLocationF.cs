using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimLocationF : MonoBehaviour
{
    public static AnimLocationF instance;

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
