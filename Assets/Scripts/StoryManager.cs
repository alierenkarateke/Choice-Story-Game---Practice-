using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance;
    public StoryNode startNode;
    private StoryNode currentNode;

    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        currentNode = startNode;
        UIController.instance.DisplayNode(currentNode);
    }

    public void ChoiceMade(int choiceIndex)
    {
        currentNode = currentNode.choices[choiceIndex].nextNode;
        UIController.instance.DisplayNode(currentNode);
    }

}
