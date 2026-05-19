using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class GlacialShatter : MonoBehaviour
    {
        public GameObject[] crystals;

        public float delayToSpawnCrystals = 1.0f;

        [Header("Do not set manually:")]
        public float spellBaseDamage = 1.0f;
        public int spell_ID = 0;
        public string element = "UNASSIGNED";

        // Called in anim events upon spell cast
        public void SetSpellData(float baseDamage, int spellID, string elementType)
        {
            spellBaseDamage = baseDamage;
            spell_ID = spellID;
            element = elementType;
        }

        void Start()
        {
            StartCoroutine(DelayThenSpawnCrystals());
        }

        private IEnumerator DelayThenSpawnCrystals()
        {
            yield return new WaitForSeconds(delayToSpawnCrystals);
            foreach(GameObject crystal in crystals)
            {
                crystal.GetComponent<CrystalAoE>().SetCrystalStats();
                crystal.SetActive(true);
            }
            Destroy(gameObject, 15.0f);
        }
    }
}
