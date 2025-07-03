using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }
    [SerializeField] private TMP_Text cash;
    [SerializeField] private TMP_Text crudeQuantiy;
    [SerializeField] private TMP_Text crudePrice;
    [SerializeField] private TMP_Text crudeTotal;

    [SerializeField] private TMP_Text gasQuantiy;
    [SerializeField] private TMP_Text gasPrice;
    [SerializeField] private TMP_Text gasTotal;

    [SerializeField] private TMP_Text jetQuantiy;
    [SerializeField] private TMP_Text jetPrice;
    [SerializeField] private TMP_Text jetTotal;

    [SerializeField] private TMP_Text dieselQuantiy;
    [SerializeField] private TMP_Text dieselPrice;
    [SerializeField] private TMP_Text dieselTotal;

    [SerializeField] private TMP_Text tankQuantiy;
    [SerializeField] private TMP_Text tankPrice;
    [SerializeField] private TMP_Text tankTotal;

    [SerializeField] private TMP_Text refineryQuantiy;
    [SerializeField] private TMP_Text refineryPrice;
    [SerializeField] private TMP_Text refineryTotal;

    [SerializeField] private TMP_Text[] digitTexts; // Should be length 7, left to right
    [SerializeField] private float digitRollSpeed = 0.05f;
    [SerializeField] private GameObject gameOverPanel;

    [SerializeField] GameObject loadingScreen;
    [SerializeField] Slider loadingSlider;
    [SerializeField] TMP_Text loadingProgress;

    private Animator gameOverAnimator;
    private int netMoneyThisGame = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        loadingScreen.SetActive(false);
    }

    void Start()
    {
        gameOverAnimator = gameOverPanel.GetComponent<Animator>();
    }

    public void TogglePanel()
    {
        bool isOpen = gameOverAnimator.GetBool("Open");
        gameOverAnimator.SetBool("Open", !isOpen);

        if (!isOpen)
        {
            AudioManager.Instance.Stop("GameBG");
            AudioManager.Instance.Stop("MainMenuBG");
            AudioManager.Instance.Play("GameOverBG");
            AudioManager.Instance.Stop("Drill");
        }

    }

    public void Menu()
    {
        AudioManager.Instance.Play("Button");

        TogglePanel();
        StartCoroutine(LoadAsynchronously(0));
    }

    public void Replay()
    {
        AudioManager.Instance.Play("Button");
        TogglePanel();
        StartCoroutine(LoadAsynchronously(1));
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

    public void SetNetWorth()
    {
        AudioManager.Instance.Play("MoneyChanging");
        StartCoroutine(AnimateOdometer(netMoneyThisGame));
    }
    private IEnumerator AnimateOdometer(int amount)
    {
        string amountStr = amount.ToString();
        int digitCount = amountStr.Length;
        int startIndex = digitTexts.Length - digitCount;

        // Step 1: Clear unused left digits
        for (int i = 0; i < startIndex; i++)
        {
            digitTexts[i].text = "";
        }

        int rollingDigits = 0;

        // Step 2: Animate only changing digits
        for (int i = 0; i < digitCount; i++)
        {
            int textIndex = startIndex + i;
            int targetDigit = int.Parse(amountStr[i].ToString());

            int currentDigit = 0;
            if (int.TryParse(digitTexts[textIndex].text, out int parsed))
                currentDigit = parsed;

            if (currentDigit != targetDigit)
            {
                rollingDigits++;
                StartCoroutine(RollDigit(digitTexts[textIndex], targetDigit, () => rollingDigits--));
                yield return new WaitForSeconds(digitRollSpeed); // Only delay if digit changes
            }
            else
            {
                digitTexts[textIndex].text = targetDigit.ToString();
            }
        }

        if (rollingDigits > 0)
        {
            yield return new WaitUntil(() => rollingDigits == 0);
        }

        AudioManager.Instance.Stop("MoneyChanging");
    }
    private IEnumerator RollDigit(TMP_Text digitText, int targetDigit, Action onComplete)
    {
        int current = 0;
        if (int.TryParse(digitText.text, out int parsed))
            current = parsed;

        while (current != targetDigit)
        {
            digitText.text = current.ToString();
            current = (current + 1) % 10;
            yield return new WaitForSeconds(0.05f);
        }

        digitText.text = targetDigit.ToString();
        onComplete?.Invoke();
    }

    public void UpdateCash(int money)
    {
        netMoneyThisGame += money;
        cash.text = $"$ {money}";
    }

    public void UpdateCrudeOil(int quantity, int price)
    {
        crudeQuantiy.text = quantity.ToString();
        crudePrice.text = price.ToString();
        crudeTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;

    }

    public void UpdateGasoline(int quantity, int price)
    {
        gasQuantiy.text = quantity.ToString();
        gasPrice.text = price.ToString();
        gasTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;
    }

    public void UpdateJetFuel(int quantity, int price)
    {
        jetQuantiy.text = quantity.ToString();
        jetPrice.text = price.ToString();
        jetTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;
    }

    public void UpdateDiesel(int quantity, int price)
    {
        dieselQuantiy.text = quantity.ToString();
        dieselPrice.text = price.ToString();
        dieselTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;
    }

    public void UpdateTanks(int quantity, int price)
    {
        tankQuantiy.text = quantity.ToString();
        tankPrice.text = price.ToString();
        tankTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;
    }

    public void UpdateRefinery(int quantity, int price)
    {
        refineryQuantiy.text = quantity.ToString();
        refineryPrice.text = price.ToString();
        refineryTotal.text = (quantity * price).ToString();

        netMoneyThisGame += quantity * price;
    }
}
