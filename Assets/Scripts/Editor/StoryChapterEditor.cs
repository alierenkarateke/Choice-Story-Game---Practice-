using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(StoryChapter))]
public class StoryChapterEditor : Editor
{
    ReorderableList nodesList;
    int selectedNodeIndex;
    string lastValidationSummary = "";

    void OnEnable()
    {
        nodesList = new ReorderableList(serializedObject, serializedObject.FindProperty("nodes"), true, true, true, true);
        nodesList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Nodes");
        nodesList.drawElementCallback = (rect, index, active, focused) =>
        {
            SerializedProperty element = nodesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        };
        nodesList.onRemoveCallback = list =>
        {
            if (EditorUtility.DisplayDialog("Remove Node", "Remove this node from the chapter list? The asset file will not be deleted.", "Remove", "Cancel"))
            {
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        StoryChapter chapter = (StoryChapter)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("chapterId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startNode"));

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Chapter Tools", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Node"))
            CreateNodeAsset(chapter);
        if (GUILayout.Button("Validate Chapter"))
            RunValidation(chapter);
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(lastValidationSummary))
            EditorGUILayout.HelpBox(lastValidationSummary, lastValidationSummary.StartsWith("OK") ? MessageType.Info : MessageType.Warning);

        EditorGUILayout.Space(4);
        nodesList.DoLayoutList();

        if (chapter.nodes == null)
            chapter.nodes = new System.Collections.Generic.List<StoryNode>();

        selectedNodeIndex = Mathf.Clamp(selectedNodeIndex, 0, Mathf.Max(0, chapter.nodes.Count - 1));
        if (chapter.nodes.Count > 0)
        {
            selectedNodeIndex = EditorGUILayout.Popup("Selected Node", selectedNodeIndex,
                GetNodePopupLabels(chapter));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set as Start"))
            {
                StoryNode selected = chapter.nodes[selectedNodeIndex];
                chapter.startNode = selected;
                EditorUtility.SetDirty(chapter);
            }

            if (GUILayout.Button("Ping Selected"))
            {
                StoryNode selected = chapter.nodes[selectedNodeIndex];
                if (selected != null)
                    EditorGUIUtility.PingObject(selected);
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    void CreateNodeAsset(StoryChapter chapter)
    {
        string chapterName = string.IsNullOrWhiteSpace(chapter.displayName) ? chapter.name : chapter.displayName;
        chapterName = SanitizeFileName(chapterName);

        string folder = "Assets/Story/Nodes";
        EnsureFolderExists("Assets/Story");
        EnsureFolderExists(folder);

        int index = chapter.nodes != null ? chapter.nodes.Count + 1 : 1;
        string assetPath = $"{folder}/{chapterName}_Node_{index}.asset";
        while (File.Exists(assetPath))
        {
            index++;
            assetPath = $"{folder}/{chapterName}_Node_{index}.asset";
        }

        StoryNode node = CreateInstance<StoryNode>();
        node.nodeId = $"{chapterName}_node_{index}";
        node.displayName = $"Node {index}";
        node.storyText = "Yeni sahne metni...";
        node.parentChapter = chapter;

        AssetDatabase.CreateAsset(node, assetPath);
        AssetDatabase.SaveAssets();

        if (chapter.nodes == null)
            chapter.nodes = new System.Collections.Generic.List<StoryNode>();

        chapter.nodes.Add(node);

        if (chapter.startNode == null)
            chapter.startNode = node;

        EditorUtility.SetDirty(chapter);
        AssetDatabase.SaveAssets();
        EditorGUIUtility.PingObject(node);
        Selection.activeObject = node;
    }

    void RunValidation(StoryChapter chapter)
    {
        StoryValidation.ValidationResult result = StoryValidation.Validate(chapter);

        foreach (string error in result.errors)
            Debug.LogError($"[StoryChapter] {chapter.name}: {error}", chapter);

        foreach (string warning in result.warnings)
            Debug.LogWarning($"[StoryChapter] {chapter.name}: {warning}", chapter);

        if (result.IsValid && result.warnings.Count == 0)
            lastValidationSummary = "OK — No issues found.";
        else if (result.IsValid)
            lastValidationSummary = $"OK with {result.warnings.Count} warning(s). See Console.";
        else
            lastValidationSummary = $"{result.errors.Count} error(s), {result.warnings.Count} warning(s). See Console.";
    }

    static string[] GetNodePopupLabels(StoryChapter chapter)
    {
        string[] labels = new string[chapter.nodes.Count];
        for (int i = 0; i < chapter.nodes.Count; i++)
            labels[i] = StoryValidation.GetNodeLabel(chapter.nodes[i]);
        return labels;
    }

    static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name.Replace(' ', '_');
    }

    static void EnsureFolderExists(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolderExists(parent);

        AssetDatabase.CreateFolder(parent, folderName);
    }
}
