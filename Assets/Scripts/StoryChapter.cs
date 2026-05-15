using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Story/Chapter")]
public class StoryChapter : ScriptableObject
{
    public string chapterId;
    public string displayName;
    public StoryNode startNode;
    public List<StoryNode> nodes = new List<StoryNode>();
}
