using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StoryNode))]
public class StoryNodeEditor : Editor
{
    ReorderableList choicesList;
    StoryChapter cachedChapter;

    void OnEnable()
    {
        StoryNode node = (StoryNode)target;
        cachedChapter = node.parentChapter != null ? node.parentChapter : FindChapterContaining(node);

        choicesList = new ReorderableList(serializedObject, serializedObject.FindProperty("choices"), true, true, true, true);
        choicesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Choices");
        choicesList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 6;
        choicesList.drawElementCallback = DrawChoiceElement;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        StoryNode node = (StoryNode)target;
        if (cachedChapter == null)
            cachedChapter = node.parentChapter != null ? node.parentChapter : FindChapterContaining(node);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("nodeId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("storyText"));

        if (node.IsEndNode)
            EditorGUILayout.HelpBox("No choices — this is an end node.", MessageType.Info);

        if (cachedChapter == null)
            EditorGUILayout.HelpBox("Add this node to a StoryChapter list to get next-node dropdowns.", MessageType.Warning);
        else
            EditorGUILayout.LabelField("Chapter", cachedChapter.name);

        EditorGUILayout.Space(4);
        choicesList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            EditorUtility.SetDirty(node);
    }

    void DrawChoiceElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty choiceProp = choicesList.serializedProperty.GetArrayElementAtIndex(index);
        SerializedProperty textProp = choiceProp.FindPropertyRelative("choiceText");
        SerializedProperty nextProp = choiceProp.FindPropertyRelative("nextNode");

        float line = EditorGUIUtility.singleLineHeight;
        Rect textRect = new Rect(rect.x, rect.y + 2, rect.width, line);
        Rect nextRect = new Rect(rect.x, rect.y + line + 4, rect.width, line);

        EditorGUI.PropertyField(textRect, textProp, new GUIContent("Text"));

        StoryNode currentTarget = nextProp.objectReferenceValue as StoryNode;
        if (cachedChapter != null && cachedChapter.nodes != null && cachedChapter.nodes.Count > 0)
        {
            int selected = GetNodeIndex(cachedChapter, currentTarget);
            string[] options = BuildNodeOptions(cachedChapter);
            int newSelected = EditorGUI.Popup(nextRect, "Next Node", selected, options);
            if (newSelected != selected)
                nextProp.objectReferenceValue = newSelected <= 0 ? null : cachedChapter.nodes[newSelected - 1];
        }
        else
        {
            EditorGUI.PropertyField(nextRect, nextProp, new GUIContent("Next Node"));
        }
    }

    static int GetNodeIndex(StoryChapter chapter, StoryNode node)
    {
        if (node == null)
            return 0;

        for (int i = 0; i < chapter.nodes.Count; i++)
        {
            if (chapter.nodes[i] == node)
                return i + 1;
        }

        return 0;
    }

    static string[] BuildNodeOptions(StoryChapter chapter)
    {
        string[] options = new string[chapter.nodes.Count + 1];
        options[0] = "(None - End)";
        for (int i = 0; i < chapter.nodes.Count; i++)
            options[i + 1] = StoryValidation.GetNodeLabel(chapter.nodes[i]);
        return options;
    }

    static StoryChapter FindChapterContaining(StoryNode node)
    {
        string[] guids = AssetDatabase.FindAssets("t:StoryChapter");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            StoryChapter chapter = AssetDatabase.LoadAssetAtPath<StoryChapter>(path);
            if (chapter?.nodes != null && chapter.nodes.Contains(node))
                return chapter;
        }

        return null;
    }
}
