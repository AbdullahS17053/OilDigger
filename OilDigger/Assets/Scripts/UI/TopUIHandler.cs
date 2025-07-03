using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopUIHandler : MonoBehaviour
{
    public static TopUIHandler Instance { get; private set; }

    [SerializeField] private TMP_Text[] digitTexts; // Should be length 7, left to right
    [SerializeField] private float digitRollSpeed = 0.5f; // Delay per digit step

    [SerializeField] private Slider capacitySlider;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private TMP_Text dayText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void SetMoney(int amount)
    {
        StartCoroutine(AnimateOdometer(amount));
        AudioManager.Instance.Play("MoneyChanging");

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


    public void SetCapacity(int totalCapacity, int remainingCapacity)
    {
        int percentRemaining = 0;

        if (totalCapacity <= 0)
        {
            capacitySlider.maxValue = 1;
            capacitySlider.value = 1;
            capacityText.text = percentRemaining.ToString() + " %";
            return;
        }
        percentRemaining = (remainingCapacity * 100) / totalCapacity;
        capacitySlider.maxValue = totalCapacity;
        capacitySlider.value = totalCapacity - remainingCapacity;
        capacityText.text = percentRemaining.ToString() + " %";
    }
    public void UpdateMonth(string month)
    {
        monthText.text = month;
    }

    public void UpdateDay(int day)
    {
        if (day < 10)
        {
            dayText.text = "0" + day.ToString();
            return;
        }
        dayText.text = day.ToString();
    }
}
