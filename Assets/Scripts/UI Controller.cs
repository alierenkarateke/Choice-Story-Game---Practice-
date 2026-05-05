using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public TextMeshProUGUI storyText;
    public Button[] choiceButtons;

    void Awake()
    {
        instance = this;
    }

    public void DisplayNode(StoryNode node)
    {
        storyText.text = node.storyText;

        for(int i = 0; i < choiceButtons.Length; i++)
        {
            if(i < node.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = node.choices[i].choiceText;

                int index = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => StoryManager.instance.ChoiceMade(index));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }
}
