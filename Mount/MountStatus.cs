using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace etchebarren
{
    public class MountStatus : MonoBehaviour
    {
        public UI_StatBar healthBar;
        public UI_StatBar staminaBar;
        public UI_StatBar expBar;

        public MountStats mountStats;
        public TextMeshProUGUI mountNameText;

        void OnEnable()
        {
            mountNameText.text = mountStats.horseName;

            healthBar?.SetMaxStat(mountStats.maxHealth, false);
            healthBar?.SetStat(mountStats.health, false, false);

            staminaBar?.SetMaxStat(mountStats.maxStamina, false);
            staminaBar?.SetStat(mountStats.stamina, false, false);

            expBar?.SetMaxStat(mountStats.baseExpCost, false);
            expBar?.SetStat(mountStats.exp, false, false);
        }
    }
}
