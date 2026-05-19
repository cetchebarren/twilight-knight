using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAllGameObjectsOfParent : MonoBehaviour
{
    public GameObject parentGameObject;
    public GameObject parentGameObjectM;

    public void DisableAllChildren(string mars_or_venus)
    {
        if (mars_or_venus == "venus")
        {
            for(int i = 0; i < parentGameObject.transform.childCount; i++)
            {
                var child = parentGameObject.transform.GetChild(i).gameObject;

                if (child != null)
                {
                    child.SetActive(false);
                }
            }
        }
        else if (mars_or_venus == "mars")
        {
            for (int i = 0; i < parentGameObjectM.transform.childCount; i++)
            {
                var child = parentGameObjectM.transform.GetChild(i).gameObject;

                if (child != null)
                {
                    child.SetActive(false);
                }
            }
        }
    }
}
