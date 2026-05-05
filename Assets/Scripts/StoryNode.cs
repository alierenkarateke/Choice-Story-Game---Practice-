using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName ="Story/Node")]
public class StoryNode : ScriptableObject
{
    [TextArea(3,6)]
    public string storyText;

    public Choice[] choices;
}

[System.Serializable]
public class Choice
{
    public string choiceText;
    
    public StoryNode nextNode;
}