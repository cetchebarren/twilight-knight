using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class Item : ScriptableObject
    {
        [Header("Item Information")]
        public string itemName;
        public Sprite itemIcon;
        [TextArea(5, 10)] public string itemDescription;
        public int itemID;
        public bool sellable = false;
        public int rarity = 0;
        public int goldValue = 0;
        public int count = 0;
        public bool flaggedAsNew = true;
    }

    /* Item ID Log:

    Consumables:
        #701 — Minor Healing Potion
        #702 — Minor Mana Potion
        #703 — Minor Stamina Potion
        #704 — Healing Potion
        #705 — Mana Potion
        #706 — Stamina Potion
        #707 — Major Healing Potion
        #708 — Major Mana Potion
        #709 — Major Stamina Potion
        #710 — Ultimate Healing Potion
        #711 — Ultimate Mana Potion
        #712 — Ultimate Stamina Potion
        #713 — Pure Elixir

    Key Items:
        #801 — Small Key
        #802 - Teddy Bear

    */
}
