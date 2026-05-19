using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    [CreateAssetMenu(menuName = "Items/Spell")]

    public class Spell : Item
    {
        public bool equipped = false;

        // In anim events, behavior for spells is defined based on spell_ID
        public int spell_ID = 0;

        public string spellType; // Projectile, AOE, Self

        public string elementType;

        public float baseDamage = 10.0f; //also considered baseHealing

        public float duration = 1.0f; // For timed spells such as buffs

        public string castSpeed = "fast"; //fast, normal, slow, verySlow

        public float manaCost = 15.0f;

        public float splashSize = 0.4f;
    }
}
