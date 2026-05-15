using System.Collections.Generic;
using UnityEngine;

public static class StoryValidation
{
    public struct ValidationResult
    {
        public List<string> errors;
        public List<string> warnings;

        public bool IsValid => errors == null || errors.Count == 0;
    }

    public static ValidationResult Validate(StoryChapter chapter)
    {
        var result = new ValidationResult
        {
            errors = new List<string>(),
            warnings = new List<string>()
        };

        if (chapter == null)
        {
            result.errors.Add("Chapter is null.");
            return result;
        }

        if (chapter.startNode == null)
            result.errors.Add("Start node is not assigned.");

        if (chapter.nodes == null || chapter.nodes.Count == 0)
            result.warnings.Add("Chapter has no nodes in the registry list.");

        HashSet<StoryNode> reachable = new HashSet<StoryNode>();
        if (chapter.startNode != null)
            CollectReachable(chapter.startNode, reachable);

        if (chapter.nodes != null)
        {
            foreach (StoryNode node in chapter.nodes)
            {
                if (node == null)
                {
                    result.warnings.Add("Chapter node list contains a null entry.");
                    continue;
                }

                if (chapter.startNode != null && !reachable.Contains(node))
                    result.warnings.Add($"Orphan node (unreachable from start): {GetNodeLabel(node)}");

                if (string.IsNullOrWhiteSpace(node.nodeId))
                    result.warnings.Add($"Node missing nodeId: {GetNodeLabel(node)}");

                if (node.choices == null)
                    continue;

                for (int i = 0; i < node.choices.Length; i++)
                {
                    Choice choice = node.choices[i];
                    if (choice == null)
                    {
                        result.warnings.Add($"{GetNodeLabel(node)} choice {i} is null.");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(choice.choiceText))
                        result.warnings.Add($"{GetNodeLabel(node)} choice {i} has empty text.");

                    if (choice.nextNode == null)
                        result.warnings.Add($"{GetNodeLabel(node)} choice \"{choice.choiceText}\" has no target (ends story when picked).");

                    if (choice.nextNode == node)
                        result.warnings.Add($"{GetNodeLabel(node)} choice \"{choice.choiceText}\" points to itself.");
                }
            }
        }

        return result;
    }

    static void CollectReachable(StoryNode node, HashSet<StoryNode> visited)
    {
        if (node == null || !visited.Add(node))
            return;

        if (node.choices == null)
            return;

        foreach (Choice choice in node.choices)
        {
            if (choice?.nextNode != null)
                CollectReachable(choice.nextNode, visited);
        }
    }

    public static string GetNodeLabel(StoryNode node)
    {
        if (node == null)
            return "(null)";

        if (!string.IsNullOrWhiteSpace(node.displayName))
            return node.displayName;

        if (!string.IsNullOrWhiteSpace(node.nodeId))
            return node.nodeId;

        return node.name;
    }
}
