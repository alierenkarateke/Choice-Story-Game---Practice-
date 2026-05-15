using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public static class StorySceneSetup
{
    [MenuItem("Story/Setup Scene For Story Game")]
    public static void SetupScene()
    {
        StorySampleGenerator.GenerateSampleContent();

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("StoryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Story Canvas");
        }

        Transform canvasTransform = canvas.transform;

        TextMeshProUGUI storyText = canvasTransform.GetComponentInChildren<TextMeshProUGUI>();
        if (storyText == null || storyText.name != "StoryText")
        {
            GameObject textGo = new GameObject("StoryText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(canvasTransform, false);
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.1f, 0.45f);
            textRect.anchorMax = new Vector2(0.9f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            storyText = textGo.GetComponent<TextMeshProUGUI>();
            storyText.fontSize = 28;
            storyText.alignment = TextAlignmentOptions.TopLeft;
            storyText.text = "Story text";
            Undo.RegisterCreatedObjectUndo(textGo, "Create Story Text");
        }

        Transform container = canvasTransform.Find("ChoiceContainer");
        if (container != null)
            FixChoiceContainer(container.gameObject);

        if (container == null)
        {
            GameObject containerGo = new GameObject("ChoiceContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            containerGo.transform.SetParent(canvasTransform, false);
            RectTransform containerRect = containerGo.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.15f, 0.05f);
            containerRect.anchorMax = new Vector2(0.85f, 0.4f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = containerGo.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = containerGo.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            container = containerGo.transform;
            Undo.RegisterCreatedObjectUndo(containerGo, "Create Choice Container");
        }

        UIController uiController = Object.FindAnyObjectByType<UIController>();
        if (uiController == null)
        {
            GameObject uiGo = new GameObject("UIController", typeof(UIController));
            uiGo.transform.SetParent(canvasTransform, false);
            uiController = uiGo.GetComponent<UIController>();
            Undo.RegisterCreatedObjectUndo(uiGo, "Create UIController");
        }

        Button prefab = AssetDatabase.LoadAssetAtPath<Button>("Assets/Prefabs/ChoiceButton.prefab");
        uiController.storyText = storyText;
        uiController.choiceButtonPrefab = prefab;
        uiController.choiceButtonContainer = container;
        EditorUtility.SetDirty(uiController);

        StoryManager storyManager = Object.FindAnyObjectByType<StoryManager>();
        if (storyManager == null)
        {
            GameObject managerGo = new GameObject("StoryManager", typeof(StoryManager));
            storyManager = managerGo.GetComponent<StoryManager>();
            Undo.RegisterCreatedObjectUndo(managerGo, "Create StoryManager");
        }

        StoryChapter chapter = AssetDatabase.LoadAssetAtPath<StoryChapter>("Assets/Story/Chapters/SampleChapter.asset");
        storyManager.chapter = chapter;
        EditorUtility.SetDirty(storyManager);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[Story] Scene setup complete. Press Play to test the sample chapter.");
        Selection.activeGameObject = storyManager.gameObject;
    }

    [MenuItem("Story/Fix Choice Container Layout")]
    public static void FixChoiceContainerMenu()
    {
        GameObject containerGo = GameObject.Find("ChoiceContainer");
        if (containerGo == null)
        {
            Debug.LogWarning("[Story] ChoiceContainer not found in scene.");
            return;
        }

        FixChoiceContainer(containerGo);
        EditorUtility.SetDirty(containerGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[Story] ChoiceContainer fixed. Content Size Fitter removed in Edit mode.");
    }

    public static void FixChoiceContainer(GameObject containerGo)
    {
        ContentSizeFitter fitter = containerGo.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Object.DestroyImmediate(fitter);

        RectTransform rect = containerGo.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.15f, 0.05f);
            rect.anchorMax = new Vector2(0.85f, 0.4f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        VerticalLayoutGroup layout = containerGo.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
            layout = containerGo.AddComponent<VerticalLayoutGroup>();

        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }
}
