using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestMenuTab : MonoBehaviour
{
    public TextMeshProUGUI activeQuestsText;
    public TextMeshProUGUI completedText;

    public int selectedfontSize = 50;
    public int unselectedfontSize = 45;

    public Color selected = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    public Color unselected = new Color(0.61f, 0.61f, 0.61f, 1.0f);

    public void SelectActiveQuestTab()
    {
        activeQuestsText.fontSize = selectedfontSize;
        completedText.fontSize = unselectedfontSize;

        activeQuestsText.color = selected;
        completedText.color = unselected;
    }

    public void SelectCompletedQuestTab()
    {
        completedText.fontSize = selectedfontSize;
        activeQuestsText.fontSize = unselectedfontSize;

        completedText.color = selected;
        activeQuestsText.color = unselected;
    }
}
