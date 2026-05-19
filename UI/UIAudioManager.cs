using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace etchebarren
{
    public class UIAudioManager : MonoBehaviour
    {
        public static UIAudioManager instance;

        [Header("UI Audio Source")]
        public AudioSource audioSourceUI;

        [Header("UI Audio Clips")]
        public AudioClip openMenu;
        public AudioClip closeMenu;
        [Space(5.0f)]
        public AudioClip dialogueInputClip;
        [Space(5.0f)]
        public AudioClip switchMenuTab;
        public AudioClip[] switchSubMenuTab;
        private int lastRandomIndex = -1;
        [Space(5.0f)]
        public AudioClip increaseStat;
        public AudioClip decreaseStat;
        [Space(5.0f)]
        public AudioClip equipItem;
        public AudioClip unequipItem;
        [Space(5.0f)]
        public AudioClip equipPotion;
        public AudioClip unequipPotion;
        [Space(5.0f)]
        public AudioClip equipSpell;
        public AudioClip unequipSpell;
        [Space(5.0f)]
        public AudioClip openNote;
        public AudioClip closeNote;
        [Space(5.0f)]
        public AudioClip selectButton;
        public AudioClip pressButton;
        [Space(5.0f)]
        public AudioClip purchaseSkillSuccess;
        public AudioClip purchaseSkillFailure;
        [Space(5.0f)]
        public AudioClip buyItem;
        public AudioClip sellItem;
        [Space(5.0f)]
        public AudioClip sliderTick;
        public bool canPlaySliderTick = true;
        public float sliderTickInterval = 0.1f;
        [Space(5.0f)]
        public AudioClip questUpdatedClip;
        public AudioClip questCompleteClip;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayUIAudioClip(AudioClip audioClip, float volume)
        {
            audioSourceUI.PlayOneShot(audioClip, volume);
        }

        public void PlayOpenMenuAudio()
        {
            PlayUIAudioClip(openMenu, 0.25f);
        }

        public void PlayCloseMenuAudio()
        {
            PlayUIAudioClip(closeMenu, 0.35f);
        }

        public void PlaySwitchMenuTabAudio()
        {
            PlayUIAudioClip(switchMenuTab, 0.2f);
        }

        public void PlaySwitchSubMenuTabAudio()
        {
            // Generate a random index in range
            int randomIndex = Random.Range(0, switchSubMenuTab.Length);
            // Now, check that that index isn't the same as the last index, otherwise try again
            while (randomIndex == lastRandomIndex && switchSubMenuTab.Length > 1)
            {
                randomIndex = Random.Range(0, switchSubMenuTab.Length);
            }
            // Update out last random index to this index for next time
            lastRandomIndex = randomIndex;
            // Set clip and play audio
            AudioClip audioClip = switchSubMenuTab[randomIndex];
            PlayUIAudioClip(audioClip, 0.15f);
        }

        public void PlayEquipItemAudio()
        {
            PlayUIAudioClip(equipItem, 0.55f);
        }

        public void PlayUnequipItemAudio()
        {
            PlayUIAudioClip(unequipItem, 0.35f);
        }

        public void PlayEquipPotionAudio()
        {
            PlayUIAudioClip(equipPotion, 0.25f);
        }
        
        public void PlayUnequipPotionAudio()
        {
            PlayUIAudioClip(unequipPotion, 0.25f);
        }

        public void PlayEquipSpellAudio()
        {
            PlayUIAudioClip(equipSpell, 0.35f);
        }

        public void PlayUnequipSpellAudio()
        {
            PlayUIAudioClip(unequipSpell, 0.15f);
        }

        public void PlayOpenNoteAudio()
        {
            PlayUIAudioClip(openNote, 0.65f);
        }

        public void PlayCloseNoteAudio()
        {
            PlayUIAudioClip(closeNote, 0.35f);
        }

        public void PlaySelectButtonAudio()
        {
            PlayUIAudioClip(selectButton, 0.1f);
        }

        public void PlayIncreaseStatAudio()
        {
            PlayUIAudioClip(increaseStat, 0.1f);
        }

        public void PlayDecreaseStatAudio()
        {
            PlayUIAudioClip(decreaseStat, 0.1f);
        }

        public void PlayPressButtonAudio()
        {
            PlayUIAudioClip(pressButton, 0.1f);
        }

        public void PlayBuyItemAudio()
        {
            PlayUIAudioClip(buyItem, 0.25f);
        }

        public void PlaySellItemAudio()
        {
            PlayUIAudioClip(sellItem, 0.03f);
        }

        public void PlaySliderTickAudio()
        {
            if (canPlaySliderTick)
            {
                StartCoroutine(SliderTickTimer());
            }
        }

        private IEnumerator SliderTickTimer()
        {
            canPlaySliderTick = false;
            PlayUIAudioClip(sliderTick, 0.2f);
            yield return new WaitForSecondsRealtime(sliderTickInterval);
            canPlaySliderTick = true;
        }

        public void PlayPurchaseSkillAudio(bool success = true)
        {
            if (success)
            {
                PlayUIAudioClip(pressButton, 0.25f);
                PlayUIAudioClip(purchaseSkillSuccess, 0.05f);
            }
            else
            {
                PlayUIAudioClip(purchaseSkillFailure, 0.3f);
            }
        }

        public void PlayDialogueInputAudio()
        {
            PlayUIAudioClip(dialogueInputClip, 0.05f);
        }

        public void PlayTrackQuestAudio(bool tracked)
        {
            if (tracked)
            {
                PlayEquipSpellAudio();
            }
            else
            {
                PlayUnequipSpellAudio();
            }
        }

        public void PlayQuestUpdatedAudio()
        {
            PlayUIAudioClip(questUpdatedClip, 0.15f);
        }

        public void PlayQuestCompletedAudio()
        {
            PlayUIAudioClip(questCompleteClip, 0.15f);
        }

    }
}
