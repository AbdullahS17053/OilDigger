using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform entryParent;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Button closeButton;
    
    private List<GameObject> entryObjects = new List<GameObject>();
    
    private void Start()
    {
        closeButton.onClick.AddListener(HideLeaderboard);
        leaderboardPanel.SetActive(false);
    }
    
    public async void ShowLeaderboard()
    {
        // Check if LeaderboardManager exists
        if (LeaderboardManager.Instance == null)
        {
            Debug.LogWarning("LeaderboardManager not found. Cannot show leaderboard.");
            return;
        }
        
        leaderboardPanel.SetActive(true);
        
        ClearEntries();
        
        try
        {
            var topScores = await LeaderboardManager.Instance.GetTopScores(10);
            var playerScore = await LeaderboardManager.Instance.GetPlayerScore();

            if (topScores == null || topScores.Count == 0)
            {
                Debug.Log("No leaderboard data available");
                CreateNoDataEntry();
            }
            else
            {
                for (int i = 0; i < topScores.Count; i++)
                {
                    CreateLeaderboardEntry(topScores[i], playerScore != null && topScores[i].PlayerId == playerScore.PlayerId);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading leaderboard: {ex.Message}");
            CreateErrorEntry();
        }
    }
    
    private void CreateLeaderboardEntry(LeaderboardEntry entry, bool isPlayer)
    {
        GameObject entryObj = Instantiate(entryPrefab, entryParent);
        entryObjects.Add(entryObj);
        
        TMP_Text rankText = entryObj.transform.Find("Rank").GetComponent<TMP_Text>();
        TMP_Text nameText = entryObj.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text scoreText = entryObj.transform.Find("Score").GetComponent<TMP_Text>();
        
        rankText.text = $"#{entry.Rank + 1}";
        nameText.text = isPlayer ? "You" : entry.PlayerName;
        scoreText.text = $"${entry.Score:N0}";
        
        if (isPlayer)
        {
            var background = entryObj.GetComponent<Image>();
            if (background != null)
            {
                background.color = new Color(1f, 1f, 0f);
            }
        }
    }
    
    private void CreateNoDataEntry()
    {
        GameObject entryObj = Instantiate(entryPrefab, entryParent);
        entryObjects.Add(entryObj);
        
        TMP_Text rankText = entryObj.transform.Find("Rank").GetComponent<TMP_Text>();
        TMP_Text nameText = entryObj.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text scoreText = entryObj.transform.Find("Score").GetComponent<TMP_Text>();
        
        rankText.text = "-";
        nameText.text = "No data available";
        scoreText.text = "-";
    }
    
    private void CreateErrorEntry()
    {
        GameObject entryObj = Instantiate(entryPrefab, entryParent);
        entryObjects.Add(entryObj);
        
        TMP_Text rankText = entryObj.transform.Find("Rank").GetComponent<TMP_Text>();
        TMP_Text nameText = entryObj.transform.Find("Name").GetComponent<TMP_Text>();
        TMP_Text scoreText = entryObj.transform.Find("Score").GetComponent<TMP_Text>();
        
        rankText.text = "-";
        nameText.text = "Error loading data";
        scoreText.text = "-";
    }
    
    private void ClearEntries()
    {
        foreach (var entry in entryObjects)
        {
            if (entry != null)
                Destroy(entry);
        }
        entryObjects.Clear();
    }
    
    public void HideLeaderboard()
    {
        AudioManager.Instance.Play("Button");
        leaderboardPanel.SetActive(false);
    }
}