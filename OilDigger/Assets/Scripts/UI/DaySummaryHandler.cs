using System.Collections;
using TMPro;
using UnityEngine;

public class DaySummaryHandler : MonoBehaviour
{
    public static DaySummaryHandler Instance { get; private set; }

    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text dayNo;
    [SerializeField] private TMP_Text moneySpent;
    [SerializeField] private TMP_Text suveyChance;
    [SerializeField] private TMP_Text drillResult;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        panel.SetActive(false);
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
        AudioManager.Instance.Play("OpenMenu");

        StartCoroutine("ClosePanel");
    }

    IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(3f);
        panel.SetActive(false);
    }

    public void UpdateDay(int day)
    {
        dayNo.text = day.ToString();
    }

    public void UpdateMoneySpent(int money)
    {
        moneySpent.text = money.ToString();
    }

    public void UpdateSurveyChance(string message)
    {
        suveyChance.text = message;
    }

    public void UpdateDailyProduction(int gallons)
    {
        drillResult.text = $"{gallons} Gallons";
    }
}
