using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }
    
    const string LeaderboardId = "Oil_Digger";
    
    [Header("Leaderboard Settings")]
    [SerializeField] private bool initializeOnAwake = true;
    
    private bool isInitialized = false;
    private bool isInitializing = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (initializeOnAwake)
            {
                InitializeServices();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private async void InitializeServices()
    {
        if (isInitializing || isInitialized)
            return;
            
        isInitializing = true;
        
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
            
            isInitialized = true;
            Debug.Log("Leaderboard services initialized successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to initialize leaderboard services: {ex.Message}");
        }
        finally
        {
            isInitializing = false;
        }
    }
    
    public async Task<bool> EnsureInitialized()
    {
        if (isInitialized)
            return true;
            
        if (!isInitializing)
        {
            InitializeServices();
        }
        
        // Wait for initialization to complete
        int timeout = 0;
        while (isInitializing && timeout < 50) // 5 second timeout
        {
            await Task.Delay(100);
            timeout++;
        }
        
        return isInitialized;
    }
    
    public async Task<bool> SubmitScore(int score)
    {
        if (!await EnsureInitialized())
        {
            Debug.LogWarning("Leaderboard services not initialized");
            return false;
        }
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Player not signed in");
            return false;
        }
        
        try
        {
            var scoreResponse = await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
            Debug.Log($"Score submitted successfully: {score}. Player rank: {scoreResponse.Rank}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to submit score: {ex.Message}");
            return false;
        }
    }
    
    public async Task<List<LeaderboardEntry>> GetTopScores(int limit = 10)
    {
        if (!await EnsureInitialized())
        {
            Debug.LogWarning("Leaderboard services not initialized");
            return new List<LeaderboardEntry>();
        }
        
        try
        {
            var scoresResponse = await LeaderboardsService.Instance.GetScoresAsync(
                LeaderboardId
            );
            
            List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
            
            foreach (var entry in scoresResponse.Results)
            {
                entries.Add(new LeaderboardEntry
                {
                    PlayerId = entry.PlayerId,
                    PlayerName = entry.PlayerName ?? "Anonymous",
                    Score = (int)entry.Score,
                    Rank = entry.Rank
                });
            }
            
            return entries;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to get leaderboard scores: {ex.Message}");
            return new List<LeaderboardEntry>();
        }
    }
    
    public async Task<LeaderboardEntry> GetPlayerScore()
    {
        if (!await EnsureInitialized() || !AuthenticationService.Instance.IsSignedIn)
        {
            return null;
        }
        
        try
        {
            var scoreResponse = await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
            
            return new LeaderboardEntry
            {
                PlayerId = scoreResponse.PlayerId,
                PlayerName = scoreResponse.PlayerName ?? "You",
                Score = (int)scoreResponse.Score,
                Rank = scoreResponse.Rank
            };
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to get player score: {ex.Message}");
            return null;
        }
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public string PlayerId;
    public string PlayerName;
    public int Score;
    public int Rank;
}