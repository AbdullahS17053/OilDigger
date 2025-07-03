using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 30;
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int money = 1000000;
    [SerializeField] private TankManager tankManagerRef;

    #region Vars & Lists
    private bool hasInteractedThisTurn = false;
    public bool isInteractionGoing = false;

    private int[] crudeOilFluctuations = new int[30];
    private int[] gasolineFluctuations = new int[30];
    private int[] jetFuelFluctuations = new int[30];
    private int[] dieselFluctuations = new int[30];

    private int curdeOilCP;
    private int gasolineCP;
    private int jetFuelCP;
    private int dieselCP;
    public List<int> marketEventDays = new List<int>();
    private List<string> marketEvents = new List<string>
    {
        "War in the Middle East",
        "New Oil Discovery",
        "OPEC Production Cut",
        "Global Recession",
        "Environmental Regulations Tightened"
    };
    private int marketNotificationIndex = 0;
    private int marketEventIndex = 0;

    private readonly string[] monthAbbreviations = new string[]
{
    "JAN", "FEB", "MAR", "APR", "MAY", "JUN",
    "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
};
    private int currentMonthIndex;
    private int netWorth = 0;
    private int moneyBeforeTurn = 0;

    private int currentYear = 2025;
    // private HashSet<int> selectedDays = new HashSet<int>();
    #endregion
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        AudioManager.Instance.Stop("MainMenuBG");
        AudioManager.Instance.Stop("GameOverBG");
        AudioManager.Instance.Play("GameBG");

    }

    void Start()
    {
        curdeOilCP = MarketManager.Instance.curdeOilIP;
        gasolineCP = MarketManager.Instance.gasolineIP;
        jetFuelCP = MarketManager.Instance.jetFuelIP;
        dieselCP = MarketManager.Instance.dieselIP;
        // TopUIHandler.Instance.SetMoney(money);
        TopUIHandler.Instance.UpdateDay(currentTurn);
        MarketManager.Instance.UpdateBarrelsPanel();

        UpdateFluctuations();
        ShuffleMarketEvents();

        currentMonthIndex = PlayerPrefs.GetInt("MonthIndex", 0); // Default to 0 = JAN
        currentYear = PlayerPrefs.GetInt("Year", 2025);
        netWorth = PlayerPrefs.GetInt("NetWorth", 0);


        TopUIHandler.Instance.UpdateMonth(monthAbbreviations[currentMonthIndex]);

        moneyBeforeTurn = money;
    }

    public int GetMoney()
    {
        return money;
    }

    private void ShuffleMarketEvents()
    {
        for (int i = marketEvents.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            string temp = marketEvents[i];
            marketEvents[i] = marketEvents[j];
            marketEvents[j] = temp;
        }
    }

    #region Getters & Setters
    public int CurrentTurn => currentTurn;
    public int Money => money;
    public bool HasInteractedThisTurn => hasInteractedThisTurn;

    public void RegisterInteraction()
    {
        hasInteractedThisTurn = true;
        isInteractionGoing = false;
    }
    public bool TrySpend(int amount)
    {
        if (money >= amount)
        {
            AudioManager.Instance.Play("MoneyDeduct");

            money -= amount;
            UpdateMoney();
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        AudioManager.Instance.Play("MoneyDeduct");

        UpdateMoney();
    }

    private void UpdateMoney()
    {
        TopUIHandler.Instance.SetMoney(money);
        if (money >= 10000) OpsHandler.Instance.EnableBuyTank();
        else OpsHandler.Instance.DisableBuyTank();
        if (money >= 30) OpsHandler.Instance.EnableRefine();
        else OpsHandler.Instance.DisableRefine();
    }

    #endregion

    public void EndTurn()
    {
        if (currentTurn < maxTurns)
        {
            tankManagerRef.EndDay(currentTurn);
            DaySummaryHandler.Instance.ShowPanel();
            DaySummaryHandler.Instance.UpdateDay(currentTurn);
            DaySummaryHandler.Instance.UpdateMoneySpent(moneyBeforeTurn - money);
            string surveyMessage = "No Survey";
            Lot selectedLot = InputManager.Instance.selectedLot;
            if (selectedLot != null)
            {
                if (selectedLot.IsSurveyed)
                {
                    surveyMessage = $"{selectedLot.oilChance} % Chance";
                }
            }

            DaySummaryHandler.Instance.UpdateSurveyChance(surveyMessage);

            moneyBeforeTurn = money;
            StartCoroutine("LateUpdateEndDay");

        }
        else
        {
            currentMonthIndex = (currentMonthIndex + 1) % 12;
            PlayerPrefs.SetInt("MonthIndex", currentMonthIndex);
            if (currentMonthIndex == 0)
            {
                PlayerPrefs.SetInt("Year", currentYear + 1);
            }

            GameOverManager.Instance.UpdateCash(money);
            MarketManager.Instance.GameOver();
            TankManager.Instance.GameOver();
            StartCoroutine(LateUpdateGameOver());
        }
    }
    IEnumerator LateUpdateGameOver()
    {
        yield return new WaitForSeconds(2f);
        GameOverManager.Instance.TogglePanel();
        StartCoroutine(LateUpdateNetWorth());
    }

    IEnumerator LateUpdateNetWorth()
    {
        yield return new WaitForSeconds(1f);
        GameOverManager.Instance.SetNetWorth();
    }

    IEnumerator LateUpdateEndDay()
    {
        yield return new WaitForSeconds(2f);
        UpdatePrices(currentTurn);

        currentTurn++;
        TopUIHandler.Instance.UpdateDay(currentTurn);
        MarketManager.Instance.UpdateBarrelsPanel();

        hasInteractedThisTurn = false;
    }
    #region Market Fluctuations
    public void UpdateFluctuations()
    {
        System.Random rand = new System.Random(DateTime.Now.Millisecond);

        // Step 1: Base random values between -10 to 10 for each day
        for (int i = 0; i < 30; i++)
        {
            crudeOilFluctuations[i] = rand.Next(-10, 11);
            gasolineFluctuations[i] = rand.Next(-10, 11);
            jetFuelFluctuations[i] = rand.Next(-10, 11);
            dieselFluctuations[i] = rand.Next(-10, 11);
        }

        // Step 2: Market event spikes between -40 to 40
        int minEventDays = 1;
        int maxEventDays = 5;
        int nMarketEvents = rand.Next(minEventDays, maxEventDays + 1);

        HashSet<int> selectedDays = new HashSet<int>();
        while (selectedDays.Count < nMarketEvents)
        {
            selectedDays.Add(rand.Next(0, 30)); // Ensures unique days
        }

        marketEventDays = selectedDays.ToList();
        marketEventDays.Sort();

        foreach (int day in marketEventDays)
        {
            int notificationDay = UnityEngine.Random.Range(day - 5, day);
            string eventType = marketEvents[marketNotificationIndex];
            marketNotificationIndex = (marketNotificationIndex + 1) % marketEvents.Count;
            string suffix = TankManager.Instance.notfiMessagesSuffix[UnityEngine.Random.Range(0, TankManager.Instance.notfiMessagesSuffix.Count)];

            string message = $"Market notification: {eventType} {suffix}";
            TankManager.Instance.AddNotification(notificationDay, message);
        }

        foreach (int day in marketEventDays)
        {
            crudeOilFluctuations[day] = rand.Next(-40, 41);
            gasolineFluctuations[day] = rand.Next(-40, 41);
            jetFuelFluctuations[day] = rand.Next(-40, 41);
            dieselFluctuations[day] = rand.Next(-40, 41);
        }
    }

    public void UpdatePrices(int currentDay)
    {
        int crudeOilC = crudeOilFluctuations[currentDay];
        int gasolineC = gasolineFluctuations[currentDay];
        int jetFuelC = jetFuelFluctuations[currentDay];
        int dieselC = dieselFluctuations[currentDay];
        string crudeOilChange = crudeOilC > 0 ? $"+{crudeOilC}" : $"{crudeOilC}";
        string gasolineChange = gasolineC > 0 ? $"+{gasolineC}" : $"{gasolineC}";
        string jetFuelChange = jetFuelC > 0 ? $"+{jetFuelC}" : $"{jetFuelC}";
        string dieselChange = dieselC > 0 ? $"+{dieselC}" : $"{dieselC}";

        // MarketManager.Instance.UpdateMarketChange(0, crudeOilChange);
        MarketManager.Instance.UpdateMarketChange(1, gasolineChange);
        MarketManager.Instance.UpdateMarketChange(2, jetFuelChange);
        MarketManager.Instance.UpdateMarketChange(3, dieselChange);

        if (crudeOilC != 0)
            curdeOilCP += (int)((float)crudeOilC / 100f * curdeOilCP);

        if (gasolineC != 0)
            gasolineCP += (int)((float)gasolineC / 100f * gasolineCP);

        if (jetFuelC != 0)
            jetFuelCP += (int)((float)jetFuelC / 100f * jetFuelCP);

        if (dieselC != 0)
            dieselCP += (int)((float)dieselC / 100f * dieselCP);

        curdeOilCP = Mathf.Max(curdeOilCP, MarketManager.Instance.curdeOilIP);
        gasolineCP = Mathf.Max(gasolineCP, MarketManager.Instance.gasolineIP);
        jetFuelCP = Mathf.Max(jetFuelCP, MarketManager.Instance.jetFuelIP);
        dieselCP = Mathf.Max(dieselCP, MarketManager.Instance.dieselIP);

        // MarketManager.Instance.UpdateMarketPrices(0, curdeOilCP);
        MarketManager.Instance.UpdateMarketPrices(1, gasolineCP);
        MarketManager.Instance.UpdateMarketPrices(2, jetFuelCP);
        MarketManager.Instance.UpdateMarketPrices(3, dieselCP);

        string maxChange = "+- 10%";
        string marketEventText = "";
        foreach (int day in marketEventDays)
        {
            if (currentDay - 1 == day)
            {
                maxChange = "+- 40%";
                marketEventText = marketEvents[marketEventIndex];
                TankManager.Instance.AddNotification(currentDay, marketEventText);
                marketEventIndex = (marketEventIndex + 1) % marketEvents.Count;
            }
        }

        // gameSceneUIRef.UpdateMarketEventText(marketEventText);
        // gameSceneUIRef.UpdateMaxChange(maxChange);
    }
    #endregion
}