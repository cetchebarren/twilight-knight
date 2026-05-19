using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOL : MonoBehaviour
{
    public bool beginInActiveState = true;

    // Start is called before the first frame update
    void Awake()
    {
        // Do not Destroy object between scenes
        DontDestroyOnLoad(this.gameObject);
        // At the start of the game, determine if active
        gameObject.SetActive(beginInActiveState);
    }
}
