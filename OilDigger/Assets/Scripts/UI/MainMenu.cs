using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject settinsPanel;
    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingSlider;
    [SerializeField] TMP_Text loadingProgress;
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
        settingsAnimator = settinsPanel.GetComponent<Animator>();

    }

    public void ToggleMainMenu()
    {
        AudioManager.Instance.Play("Button");
        bool isOpen = mainMenuAnimator.GetBool("Open");
        mainMenuAnimator.SetBool("Open", !isOpen);

        bool isOpen2 = settingsAnimator.GetBool("Open");
        settingsAnimator.SetBool("Open", !isOpen2);
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
