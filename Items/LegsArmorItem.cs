using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/LegsArmors")]

    public class LegsArmorItem : Item
    {
        [Header("Legs Armor Model")]
        public int legsArmorModelID = 0;

        public int level = 1;

        public bool equipped = false;

        [Header("Armor Base Stats")]
        public int physicalArmorRating = 0;
        public int magicalArmorRating = 0;

        [Header("Primary Stat Bonuses")]
        //public int strength = 0;        //hands
        public int endurance = 0;       //legs
        public int vitality = 0;        //legs
        //public int precision = 0;       //hands
        public int dexterity = 0;       //legs
        public int expertise = 0;       //legs
        //public int intelligence = 0;    //hands
        //public int spirit = 0;          //hands
        public int willpower = 0;       //legs
        //public int luck = 0;            //hands

        [Header("Secondary Stat Bonuses")]
        //public float criticalChance = 0.0f;     //weapon, hands, ring
        //public float criticalDamage = 0.0f;     //weapon, hands, ring
        public float fireResistance = 0.0f;   //shield, legs
        public float iceResistance = 0.0f;    //shield, legs
        public float shockResistance = 0.0f;  //shield, legs


    }
}
