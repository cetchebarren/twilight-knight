using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/Shields")]

    public class ShieldItem : Item
    {
        [Header("Shield Model")]
        ///public GameObject weaponModel;
        public int shieldModelID = 0;

        //[Header('Weapon Requirements')] do i want this?

        public int level = 1;

        public bool equipped = false;

        [Header("Shield Block Rating")]
        public int blockRating = 0;
        //public int magicDamage = 0;

        //[Header("Stamina Costs")]
        //public int baseStaminaCost = 20; //sprint attack, charged, cost more etc

        [Header("Primary Stat Bonuses")]
        public int strength = 0;
        public int endurance = 0;
        public int vitality = 0;
        public int precision = 0;
        public int dexterity = 0;
        public int expertise = 0;
        public int intelligence = 0;
        public int spirit = 0;
        public int willpower = 0;
        public int luck = 0;

        [Header("Secondary Stat Bonuses")]
        //public float criticalChance = 0.0f;
        //public float criticalDamage = 0.0f;
        public float fireResistance = 0.0f;
        public float iceResistance = 0.0f;
        public float shockResistance = 0.0f;

        [Header("Misc. and Quest Related")]
        public bool equipOnPickUp = false;


    }
}
