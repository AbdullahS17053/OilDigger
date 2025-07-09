using UnityEngine;

public class PauseMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;

    private Animator pauseAnimator;
    private Animator settingsAnimator;
    void Start()
    {
        pauseAnimator = pauseMenu.GetComponent<Animator>();
        settingsAnimator = settingsMenu.GetComponent<Animator>();
    }
    public void TogglePauseMenu()
    {
        if (pauseMenuUI.activeSelf)
        {
            pauseMenuUI.SetActive(false);
            // Time.timeScale = 1f; // Resume the game
        }
        else
        {
            pauseMenuUI.SetActive(true);
            // Time.timeScale = 0f; // Pause the game
        }
    }

    public void ToggleSettingsMenu()
    {
        AudioManager.Instance.Play("Button");
        bool isOpen = pauseAnimator.GetBool("Open");
        pauseAnimator.SetBool("Open", !isOpen);

        bool isOpen2 = settingsAnimator.GetBool("Open");
        settingsAnimator.SetBool("Open", !isOpen2);
    }

}
