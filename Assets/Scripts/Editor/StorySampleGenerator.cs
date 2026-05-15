using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class StorySampleGenerator
{
    const string ChapterPath = "Assets/Story/Chapters/SampleChapter.asset";
    const string NodesFolder = "Assets/Story/Nodes";
    const string PrefabPath = "Assets/Prefabs/ChoiceButton.prefab";

    [MenuItem("Story/Generate Sample Content")]
    public static void GenerateSampleContent()
    {
        EnsureFolder("Assets/Story");
        EnsureFolder("Assets/Story/Chapters");
        EnsureFolder(NodesFolder);
        EnsureFolder("Assets/Prefabs");

        CreateChoiceButtonPrefab();

        StoryChapter chapter = AssetDatabase.LoadAssetAtPath<StoryChapter>(ChapterPath);
        if (chapter == null)
        {
            chapter = ScriptableObject.CreateInstance<StoryChapter>();
            chapter.chapterId = "sample_chapter_01";
            chapter.displayName = "Sample Chapter";
            AssetDatabase.CreateAsset(chapter, ChapterPath);
        }

        if (chapter.nodes == null)
            chapter.nodes = new System.Collections.Generic.List<StoryNode>();
        else
            chapter.nodes.Clear();

        StoryNode intro = CreateNode(chapter, "Intro", "intro_01",
            "Karanlık bir ormandasın. İki yol görüyorsun: solda ışık, sağda sis.",
            new[] { "Soldaki ışığa git", "Sağdaki sise gir" });

        StoryNode lightPath = CreateNode(chapter, "Light Path", "light_01",
            "Işıklı patikada eski bir kulübe buluyorsun.",
            new[] { "Kulübeye gir" });

        StoryNode mistPath = CreateNode(chapter, "Mist Path", "mist_01",
            "Sis seni sarar; uzaktan bir ses duyarsın.",
            new[] { "Sese doğru yürü" });

        StoryNode ending = CreateNode(chapter, "Ending", "ending_01",
            "Macera burada sona eriyor. Tebrikler!", System.Array.Empty<string>());

        LinkChoice(intro, 0, lightPath);
        LinkChoice(intro, 1, mistPath);
        LinkChoice(lightPath, 0, ending);
        LinkChoice(mistPath, 0, ending);

        chapter.startNode = intro;
        EditorUtility.SetDirty(chapter);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Story] Sample chapter created at Assets/Story/Chapters/SampleChapter.asset");
        Debug.Log("[Story] Assign SampleChapter to StoryManager.chapter and wire UIController prefab/container in the scene.");
        Selection.activeObject = chapter;
    }

    static StoryNode CreateNode(StoryChapter chapter, string displayName, string nodeId, string text,
        string[] choiceTexts)
    {
        string path = $"{NodesFolder}/Sample_{displayName.Replace(' ', '_')}.asset";
        StoryNode node = AssetDatabase.LoadAssetAtPath<StoryNode>(path);
        if (node == null)
        {
            node = ScriptableObject.CreateInstance<StoryNode>();
            AssetDatabase.CreateAsset(node, path);
        }

        node.nodeId = nodeId;
        node.displayName = displayName;
        node.storyText = text;
        node.parentChapter = chapter;
        node.choices = new Choice[choiceTexts.Length];

        for (int i = 0; i < choiceTexts.Length; i++)
            node.choices[i] = new Choice { choiceText = choiceTexts[i] };

        if (!chapter.nodes.Contains(node))
            chapter.nodes.Add(node);

        EditorUtility.SetDirty(node);
        return node;
    }

    static void LinkChoice(StoryNode from, int choiceIndex, StoryNode to)
    {
        if (from.choices == null || choiceIndex >= from.choices.Length)
            return;

        from.choices[choiceIndex].nextNode = to;
        EditorUtility.SetDirty(from);
    }

    static void CreateChoiceButtonPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            return;

        GameObject root = new GameObject("ChoiceButton", typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(400, 48);

        Image image = root.GetComponent<Image>();
        image.color = new Color(0.2f, 0.25f, 0.35f, 0.95f);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(root.transform, false);
        RectTransform labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12, 4);
        labelRect.offsetMax = new Vector2(-12, -4);

        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.text = "Choice";
        label.fontSize = 22;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string name = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }
}
