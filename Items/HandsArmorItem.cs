using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/HandsArmors")]

    public class HandsArmorItem : Item
    {
        [Header("Hand Armor Model")]
        public int handsArmorModelID = 0;

        public int level = 1;

        public bool equipped = false;

        [Header("Armor Base Stats")]
        public int physicalArmorRating = 0;
        public int magicalArmorRating = 0;

        [Header("Primary Stat Bonuses")]
        public int strength = 0;        //hands
        public int precision = 0;       //hands
        public int intelligence = 0;    //hands
        public int spirit = 0;          //hands
        public int luck = 0;            //hands

        [Header("Secondary Stat Bonuses")]
        public float criticalChance = 0.0f;     //weapon, hands, ring
        public float criticalDamage = 0.0f;     //weapon, hands, ring
    }
}
