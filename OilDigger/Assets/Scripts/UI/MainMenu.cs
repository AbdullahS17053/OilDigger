using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] GameObject narrativePanel;
    [SerializeField] Slider loadingSlider;
    [SerializeField] TMP_Text loadingProgress;
    [SerializeField] TMP_Text narrativeText;
    [SerializeField] GameObject letsGoButton;
    [SerializeField] private LeaderboardUI leaderboardUI;
    
    private string fullText;
    [SerializeField] float typingSpeed = 0.05f;
    private Animator mainMenuAnimator;
    private Animator settingsAnimator;

    void Awake()
    {
        AudioManager.Instance.Stop("GameBG");
        AudioManager.Instance.Stop("GameOverBG");
        AudioManager.Instance.Play("MainMenuBG");

        loadingScreen.SetActive(false);
    }

    void Start()
    {
        mainMenuAnimator = mainMenuPanel.GetComponent<Animator>();
        settingsAnimator = settingsPanel.GetComponent<Animator>();

        string isFirstTime = PlayerPrefs.GetString("IsFirstTime", "true");
        if (isFirstTime == "true" )
        {
            narrativePanel.SetActive(true);
            letsGoButton.SetActive(false);
            fullText = narrativeText.text;
            StartCoroutine(TypeText());

        }
        else if (isFirstTime == "false")
        {
            narrativePanel.SetActive(false);
        }
        
        if (LeaderboardManager.Instance == null)
        {
            var leaderboardManagerPrefab = Resources.Load<GameObject>("LeaderboardManager");
            if (leaderboardManagerPrefab != null)
            {
                Instantiate(leaderboardManagerPrefab);
            }
            else
            {
                var go = new GameObject("LeaderboardManager");
                go.AddComponent<LeaderboardManager>();
            }
        }
    }

    private IEnumerator TypeText()
    {
        AudioManager.Instance.Play("Keyboard");
        // AudioManager.Instance.Stop("MainMenuBG");
        narrativeText.text = "";
        foreach (char c in fullText)
        {
            narrativeText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        letsGoButton.SetActive(true);
        AudioManager.Instance.Stop("Keyboard");

    }

    public void CloseNarrative()
    {
        AudioManager.Instance.Play("Button");
        narrativePanel.SetActive(false);
        PlayerPrefs.SetString("IsFirstTime", "false");
        // AudioManager.Instance.Play("MainMenuBG");
    }

    public void ToggleMainMenu()
    {
        AudioManager.Instance.Play("Button");
        bool isOpen = mainMenuAnimator.GetBool("Open");
        mainMenuAnimator.SetBool("Open", !isOpen);

        bool isOpen2 = settingsAnimator.GetBool("Open");
        settingsAnimator.SetBool("Open", !isOpen2);
    }

    public void ShowLeaderboard()
    {
        AudioManager.Instance.Play("Button");
        if (leaderboardUI != null)
        {
            leaderboardUI.ShowLeaderboard();
        }
        else
        {
            Debug.LogWarning("LeaderboardUI reference not set in MainMenu");
        }
    }

    // public void ToggleSettings()
    // {
    //     bool isOpen = settingsAnimator.GetBool("Open");
    //     settingsAnimator.SetBool("Open", !isOpen);
    // }
    public void LoadScene(int index)
    {
        AudioManager.Instance.Play("Button");
        StartCoroutine(LoadAsynchronously(index));
    }

    IEnumerator LoadAsynchronously(int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f);

            loadingSlider.value = progress;

            loadingProgress.text = progress * 100f + " %";

            yield return null;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
