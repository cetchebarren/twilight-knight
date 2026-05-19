using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/AmuletItem")]

    public class AmuletItem : Item
    {
        [Header("Amulet Model")]
        public int amuletModelID = 0;

        public int level = 1;

        public bool equipped = false;

        public int legendaryStat = -1;

        [Header("Secondary Stat Bonuses")]
        public float criticalChance = 0.0f;     //weapon, hands, ring
        public float criticalDamage = 0.0f;     //weapon, hands, ring
        public float fireResistance = 0.0f;   //shield, legs
        public float iceResistance = 0.0f;    //shield, legs
        public float shockResistance = 0.0f;  //shield, legs
    }
}
