using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class WorldItem : MonoBehaviour
    {
        /* Note: handling of picking up item and updating list in world state manager is done in the actual child item drop of the world item object*/

        [Header("World Item ID used to record grabbed items and control spawns")]
        public int worldItemID;

        [Header("Flag used to determine if item should always spawn. Default is False.")]
        public bool respawnOnSceneLoad = false;

        public void OnEnable()
        {
            if (WorldStateManager.instance.obtainedWorldItemIDs.Contains(worldItemID))
            {
                gameObject.SetActive(false);
            }
        }

        public void UpdateWorldItemList()
        {
            if(!respawnOnSceneLoad) WorldStateManager.instance.obtainedWorldItemIDs.Add(worldItemID);
        }
    }
}
