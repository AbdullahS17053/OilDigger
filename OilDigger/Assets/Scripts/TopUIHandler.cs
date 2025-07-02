using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopUIHandler : MonoBehaviour
{
    public static TopUIHandler Instance { get; private set; }

    [SerializeField] private TMP_Text[] digitTexts; // Should be length 7, left to right
    [SerializeField] private float digitRollSpeed = 0.05f; // Delay per digit step

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
    }

    private IEnumerator AnimateOdometer(int amount)
    {
        string amountStr = amount.ToString();
        int digitCount = amountStr.Length;
        int startIndex = digitTexts.Length - digitCount;

        // Step 1: Clear unused left digits
        for (int i = 0; i < startIndex; i++)
        {
            digitTexts[i].text = ""; // Leading blanks
        }

        // Step 2: Animate each digit
        for (int i = 0; i < digitCount; i++)
        {
            int textIndex = startIndex + i;
            int targetDigit = int.Parse(amountStr[i].ToString());

            StartCoroutine(RollDigit(digitTexts[textIndex], targetDigit));
            yield return new WaitForSeconds(digitRollSpeed); // Slight delay per digit
        }
    }

    private IEnumerator RollDigit(TMP_Text digitText, int targetDigit)
    {
        int current = 0;

        while (current != targetDigit)
        {
            digitText.text = current.ToString();
            current = (current + 1) % 10;
            yield return new WaitForSeconds(0.02f); // Per step roll speed
        }

        digitText.text = targetDigit.ToString();
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
