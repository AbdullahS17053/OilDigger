using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int maxTurns = 30;
    [SerializeField] private int currentTurn = 1;
    [SerializeField] private int money = 1000000;
    [SerializeField] private GameSceneUI gameSceneUIRef;
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

    // private HashSet<int> selectedDays = new HashSet<int>();
    #endregion
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        curdeOilCP = gameSceneUIRef.curdeOilIP;
        gasolineCP = gameSceneUIRef.gasolineIP;
        jetFuelCP = gameSceneUIRef.jetFuelIP;
        dieselCP = gameSceneUIRef.dieselIP;
        gameSceneUIRef.UpdateMoney(money);
        gameSceneUIRef.UpdateDay(currentTurn);
        gameSceneUIRef.UpdateBarrelsPanel();
        UpdateFluctuations();
        ShuffleMarketEvents();
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
            money -= amount;
            UpdateMoney();
            return true;
        }
        return false;
    }

    public void AddMoney(int amount)
    {
        money += amount;
        UpdateMoney();
    }

    private void UpdateMoney()
    {
        gameSceneUIRef.UpdateMoney(money);
        if (money >= 10000) gameSceneUIRef.EnableBuyTank();
        else gameSceneUIRef.DisableBuyTank();
        if (money >= 30) gameSceneUIRef.EnableRefine();
        else gameSceneUIRef.DisableRefine();
    }

    #endregion

    public void EndTurn()
    {
        if (currentTurn <= maxTurns)
        {
            tankManagerRef.EndDay(currentTurn);
            UpdatePrices(currentTurn);

            currentTurn++;
            gameSceneUIRef.UpdateDay(currentTurn);
            gameSceneUIRef.UpdateBarrelsPanel();
            hasInteractedThisTurn = false;
        }
        else
        {
            Debug.Log("Game Over");
        }
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

        gameSceneUIRef.UpdateMarketChange(0, crudeOilChange);
        gameSceneUIRef.UpdateMarketChange(1, gasolineChange);
        gameSceneUIRef.UpdateMarketChange(2, jetFuelChange);
        gameSceneUIRef.UpdateMarketChange(3, dieselChange);

        if (crudeOilC != 0)
            curdeOilCP += (int)((float)crudeOilC / 100f * curdeOilCP);

        if (gasolineC != 0)
            gasolineCP += (int)((float)gasolineC / 100f * gasolineCP);

        if (jetFuelC != 0)
            jetFuelCP += (int)((float)jetFuelC / 100f * jetFuelCP);

        if (dieselC != 0)
            dieselCP += (int)((float)dieselC / 100f * dieselCP);

        curdeOilCP = Mathf.Max(curdeOilCP, gameSceneUIRef.curdeOilIP);
        gasolineCP = Mathf.Max(gasolineCP, gameSceneUIRef.gasolineIP);
        jetFuelCP = Mathf.Max(jetFuelCP, gameSceneUIRef.jetFuelIP);
        dieselCP = Mathf.Max(dieselCP, gameSceneUIRef.dieselIP);

        gameSceneUIRef.UpdateMarketPrices(0, curdeOilCP);
        gameSceneUIRef.UpdateMarketPrices(1, gasolineCP);
        gameSceneUIRef.UpdateMarketPrices(2, jetFuelCP);
        gameSceneUIRef.UpdateMarketPrices(3, dieselCP);

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

        gameSceneUIRef.UpdateMarketEventText(marketEventText);
        gameSceneUIRef.UpdateMaxChange(maxChange);
    }
    #endregion
}