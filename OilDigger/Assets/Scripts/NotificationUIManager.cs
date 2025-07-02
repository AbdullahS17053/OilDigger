using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotificationUIManager : MonoBehaviour
{
    public static NotificationUIManager Instance;

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject dayGroupPrefab;
    [SerializeField] private GameObject messageBoxPrefab;

    private Dictionary<int, Transform> dayGroups = new();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddNotifications(int day, List<string> messages)
    {
        Transform group;

        // Check if this day group already exists
        if (!dayGroups.TryGetValue(day, out group))
        {
            GameObject dayGroupGO = Instantiate(dayGroupPrefab, contentParent);
            group = dayGroupGO.transform;
            dayGroups.Add(day, group);

            TMP_Text dayTitle = group.GetComponentInChildren<TMP_Text>();
            if (dayTitle != null)
                dayTitle.text = $"Day {day}";
        }

        foreach (string msg in messages)
        {
            GameObject msgBox = Instantiate(messageBoxPrefab, group);
            TMP_Text textComp = msgBox.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
                textComp.text = msg;
        }
    }
}
