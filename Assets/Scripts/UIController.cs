using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    public TextMeshProUGUI storyText;
    public Button choiceButtonPrefab;
    public Transform choiceButtonContainer;
    public string storyEndedMessage = "Hikaye bitti.";

    [SerializeField] float choiceButtonHeight = 52f;
    [SerializeField] float choiceButtonSpacing = 10f;

    readonly List<Button> spawnedButtons = new List<Button>();
    VerticalLayoutGroup layoutGroup;

    void Awake()
    {
        instance = this;
        EnsureContainerLayout();
    }

    void EnsureContainerLayout()
    {
        if (choiceButtonContainer == null)
            return;

        ContentSizeFitter fitter = choiceButtonContainer.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            Destroy(fitter);

        if (choiceButtonContainer is RectTransform containerRect)
        {
            containerRect.anchorMin = new Vector2(0.15f, 0.05f);
            containerRect.anchorMax = new Vector2(0.85f, 0.4f);
            containerRect.pivot = new Vector2(0.5f, 0f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
        }

        layoutGroup = choiceButtonContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
            layoutGroup = choiceButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();

        layoutGroup.spacing = choiceButtonSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;
    }

    public void DisplayNode(StoryNode node)
    {
        if (node == null)
            return;

        ClearChoiceButtons();

        if (storyText != null)
            storyText.text = node.storyText;

        if (node.IsEndNode || node.choices == null)
            return;

        if (choiceButtonPrefab == null || choiceButtonContainer == null)
        {
            Debug.LogError("[UIController] Choice button prefab or container is not assigned.", this);
            return;
        }

        for (int i = 0; i < node.choices.Length; i++)
        {
            Button button = Instantiate(choiceButtonPrefab, choiceButtonContainer);
            button.gameObject.SetActive(true);
            ConfigureButtonLayout(button);

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = node.choices[i].choiceText;

            int index = i;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (StoryManager.instance != null)
                    StoryManager.instance.ChoiceMade(index);
            });

            spawnedButtons.Add(button);
        }

        RebuildChoiceLayout();
    }

    void ConfigureButtonLayout(Button button)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(0f, choiceButtonHeight);

        LayoutElement layoutElement = button.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = button.gameObject.AddComponent<LayoutElement>();

        layoutElement.minWidth = 280f;
        layoutElement.minHeight = choiceButtonHeight;
        layoutElement.preferredHeight = choiceButtonHeight;
        layoutElement.flexibleWidth = 1f;
    }

    void RebuildChoiceLayout()
    {
        if (choiceButtonContainer is RectTransform containerRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
    }

    public void ShowStoryEnded()
    {
        ClearChoiceButtons();

        if (storyText != null)
            storyText.text = storyEndedMessage;
    }

    void ClearChoiceButtons()
    {
        foreach (Button button in spawnedButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }

        spawnedButtons.Clear();
    }
}
