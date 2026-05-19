using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace etchebarren
{
    public class ConfirmCharacter : MonoBehaviour
    {
        // Script and Component References
        public PlayerStats playerStats;
        public QuestManager questManager;
        public WorldStateManager worldStateManager;
        public Animator anim, anim2;
        public RuntimeAnimatorController newController;
        public TextMeshProUGUI statsBoxNameText;
        public MountManager mountManager;
        // Flags and GameObject References
        public bool firstTimeCreatingCharacter = true;
        public GameObject characterCreationCanvas;
        //public bool beginLayingDown = false;
        public Cutscene introCutscene;
        public bool playIntroCutscene = false;

        public void BeginGame()
        {
            if (playIntroCutscene && introCutscene != null)
            {
                introCutscene.PlayAllScenes();
            }
            else
            {
                StartNewGame();
            }
        }

        public void StartNewGame()
        {
            Debug.Log("Begin Game. Load scene");

            if (string.IsNullOrWhiteSpace(PlayerStats.instance.playerName))
            {
                PlayerStats.instance.playerName = "Knight";
            }

            statsBoxNameText.text = PlayerStats.instance.playerName + " - Lv." + PlayerStats.instance.playerLevel;

            //FUTURE NOTE: want to move these next two lines somewhere else so that they are cahlled when asyncLoad is done,
            //in order to prevent the animation changing too early
            anim.runtimeAnimatorController = newController; //set back to proper AC
            anim2.runtimeAnimatorController = newController; //set back to proper AC

            InputHandler.instance.inMenu = false;

            if(firstTimeCreatingCharacter)
            {
                firstTimeCreatingCharacter = false;
                if (worldStateManager.newGame) // Proper New Game if (beginLayingDown) // Proper New Game
                {
                    playerStats.dead = true;
                    worldStateManager.ChangeScene("FellrockGlen", false, false);
                }
                else // Testing mode
                {
                    PlayerInventory.instance.GenerateFirstSword();
                    PlayerInventory.instance.GenerateFirstShield();
                    worldStateManager.ChangeScene("TestingGrounds");
                    mountManager.mountUnlocked = true;
                }
                playerStats.Initialize();
                questManager.ResetQuests();
            }
            else
            {
                characterCreationCanvas.SetActive(false);
            }

        }
    }
}
