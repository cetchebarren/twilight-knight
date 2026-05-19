using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/KeyItem")]

    public class KeyItem : Item
    {
        public int keyItem_ID;

        public int keyItemSpriteID;

        public Note note;

        public bool lostWithUse = true;
    }
}
