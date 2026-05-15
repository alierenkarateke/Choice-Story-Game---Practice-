using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager instance;

    public StoryChapter chapter;

    private StoryNode currentNode;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (chapter == null)
        {
            Debug.LogError("[StoryManager] Chapter is not assigned.", this);
            return;
        }

        if (chapter.startNode == null)
        {
            Debug.LogError("[StoryManager] Chapter has no start node assigned.", this);
            return;
        }

        currentNode = chapter.startNode;
        UIController ui = GetUI();
        if (ui == null)
        {
            Debug.LogError("[StoryManager] UIController not found in scene.", this);
            return;
        }

        ui.DisplayNode(currentNode);
    }

    public void ChoiceMade(int choiceIndex)
    {
        if (currentNode == null || currentNode.choices == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Length)
        {
            Debug.LogWarning($"[StoryManager] Invalid choice index: {choiceIndex}", this);
            return;
        }

        StoryNode next = currentNode.choices[choiceIndex].nextNode;
        if (next == null)
        {
            OnStoryEnded();
            return;
        }

        currentNode = next;
        GetUI()?.DisplayNode(currentNode);
    }

    void OnStoryEnded()
    {
        currentNode = null;
        Debug.Log("[StoryManager] Story ended.");
        GetUI()?.ShowStoryEnded();
    }

    static UIController GetUI()
    {
        if (UIController.instance != null)
            return UIController.instance;

        return FindAnyObjectByType<UIController>();
    }

    public StoryNode CurrentNode => currentNode;
    public StoryChapter Chapter => chapter;
}
