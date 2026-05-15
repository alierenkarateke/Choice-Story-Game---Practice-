using UnityEngine;

[CreateAssetMenu(menuName = "Story/Node")]
public class StoryNode : ScriptableObject
{
    public string nodeId;
    public string displayName;

    [TextArea(3, 6)]
    public string storyText;

    public Choice[] choices = System.Array.Empty<Choice>();

    [HideInInspector]
    public StoryChapter parentChapter;

    public bool IsEndNode => choices == null || choices.Length == 0;

    void OnValidate()
    {
        if (choices == null)
            choices = System.Array.Empty<Choice>();
    }
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    public StoryNode nextNode;
}
