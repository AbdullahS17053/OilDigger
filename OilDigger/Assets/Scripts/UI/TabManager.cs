using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class TabManager : MonoBehaviour
{
    public static TabManager Instance { get; private set; }
    [SerializeField] private GameObject tabPanel;
    [SerializeField] private GameObject[] tabs;

    [SerializeField] private Button[] tabButtons;
    [SerializeField] private Button drillButton;
    [SerializeField] private Button surveyButton;
    [SerializeField] private Button skipButton;

    [SerializeField] private Sprite activeTabSprite, inactiveTabSprite;
    [SerializeField] private Image openTabButtonImage;
    [SerializeField] private Image closeTabButtonImage;

    [SerializeField] private TMP_Text surveyText;
    [SerializeField] private GameObject contentObject;

    private Animator tabAnimator;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Debug.Log("tab Awake");

    }

    void Start()
    {
        tabAnimator = tabPanel.GetComponent<Animator>();
        openTabButtonImage.enabled = true;
        closeTabButtonImage.enabled = false;
    }
    public void SwitchToTab(int tabIndex)
    {
        AudioManager.Instance.Play("Button");
        surveyButton.interactable = false;
        drillButton.interactable = false;
        skipButton.interactable = false;
        surveyText.text = "$ 40,000";
        SetStartAxisVertical();
        if (!tabAnimator.GetBool("Open"))
        {
            TogglePanel();
        }
        foreach (GameObject tab in tabs)
        {
            tab.SetActive(false);
        }
        tabs[tabIndex].SetActive(true);

        foreach (Button tabButton in tabButtons)
        {
            tabButton.image.sprite = inactiveTabSprite;
        }
        tabButtons[tabIndex].image.sprite = activeTabSprite;
    }

    public void TogglePanel()
    {
        bool isOpen = tabAnimator.GetBool("Open");
        tabAnimator.SetBool("Open", !isOpen);
        if (!isOpen)
        {
            SwitchToTab(2);
        }
        ToggleTabImages(!isOpen);
    }

    public void ToggleTabImages(bool isOpen)
    {
        if (isOpen)
        {
            openTabButtonImage.enabled = false;
            closeTabButtonImage.enabled = true;
        }
        else
        {
            openTabButtonImage.enabled = true;
            closeTabButtonImage.enabled = false;
        }
    }
    
    public void SetStartAxisHorizontal()
    {
        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        }
    }

    public void SetStartAxisVertical()
    {
        GridLayoutGroup grid = contentObject.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.startAxis = GridLayoutGroup.Axis.Vertical;
        }
    }
}
