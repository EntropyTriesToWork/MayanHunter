using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private const string FileName = "save.json";
    private const int MaxLeaderboard = 10;

    private string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    private SaveData _data;
    public SaveData Data => _data;
    private readonly Dictionary<string, int> _sessionScores = new Dictionary<string, int>();
    private List<string> _orderedLevelIds = new List<string>();

    public int SessionTotalScore
    {
        get
        {
            int total = 0;
            foreach (var v in _sessionScores.Values) total += v;
            return total;
        }
    }

    public int GetSessionScore(string levelId)
        => _sessionScores.TryGetValue(levelId, out int s) ? s : 0;

    private string GetLevelIdFromConfig(LevelConfig config)
    {
        // Option 1: Use the config's name (asset name)
        return config != null ? config.name : null;

        // Option 2: If LevelConfig has a dedicated field like "levelId", use:
        // return config != null ? config.levelId : null;
    }
    public void SetLevelOrder(LevelSettings levelSettings)
    {
        if (levelSettings == null || levelSettings.allLevels == null)
        {
            Debug.LogWarning("[SaveManager] SetLevelOrder called with invalid LevelSettings.");
            return;
        }

        _orderedLevelIds.Clear();
        foreach (var levelConfig in levelSettings.allLevels)
        {
            if (levelConfig != null)
            {
                string id = GetLevelIdFromConfig(levelConfig);
                if (!string.IsNullOrEmpty(id))
                    _orderedLevelIds.Add(id);
            }
        }

        Debug.Log($"[SaveManager] Level order set. Found {_orderedLevelIds.Count} levels.");
    }

    public List<string> BuildLevelIdList(List<LevelConfig> levelConfigs)
    {
        var ids = new List<string>();
        foreach (var config in levelConfigs)
        {
            if (config != null)
            {
                string id = GetLevelIdFromConfig(config);
                if (!string.IsNullOrEmpty(id))
                    ids.Add(id);
            }
        }
        return ids;
    }
    public List<string> BuildLevelIdList(List<GameObject> allLevels)
    {
        var ids = new List<string>();
        foreach (var prefab in allLevels)
            if (prefab != null) ids.Add(prefab.name);
        return ids;
    }

    private void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                _data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                Debug.Log($"[SaveManager] Loaded save from {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load: {e.Message}. Starting fresh.");
                _data = new SaveData();
            }
        }
        else
        {
            _data = new SaveData();
            Debug.Log("[SaveManager] No save file found. Starting fresh.");
        }
    }

    private void WriteToDisk()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(_data, prettyPrint: true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Failed to write save: {e.Message}");
        }
    }

    public void InitialiseLevelList(List<string> orderedLevelIds)
    {
        List<string> idsToUse = _orderedLevelIds.Count > 0 ? _orderedLevelIds : orderedLevelIds;

        foreach (string id in idsToUse)
        {
            if (_data.levels.Find(e => e.levelId == id) == null)
                _data.levels.Add(new LevelSaveEntry { levelId = id });
        }

        if (_data.levels.Count > 0)
            _data.levels[0].isUnlocked = true;

        WriteToDisk();
    }

    public LevelSaveEntry GetLevelEntry(string levelId)
        => _data.levels.Find(e => e.levelId == levelId);

    public void SaveLevelResult(LevelResultData result)
    {
        LevelSaveEntry entry = GetLevelEntry(result.levelId);
        if (entry == null)
        {
            entry = new LevelSaveEntry { levelId = result.levelId, isUnlocked = true };
            _data.levels.Add(entry);
        }

        bool newBestScore = result.totalScore > entry.bestScore;
        bool newBestStars = result.stars > entry.bestStars;

        if (newBestScore) entry.bestScore = result.totalScore;
        if (newBestStars) entry.bestStars = result.stars;
        entry.isCompleted = true;

        if (_orderedLevelIds != null && _orderedLevelIds.Count > 0)
        {
            int idx = _orderedLevelIds.IndexOf(result.levelId);
            if (idx >= 0 && idx + 1 < _orderedLevelIds.Count)
            {
                string nextLevelId = _orderedLevelIds[idx + 1];
                LevelSaveEntry next = GetLevelEntry(nextLevelId);
                if (next != null) next.isUnlocked = true;
            }
        }
        else
        {
            Debug.LogWarning("[SaveManager] Cannot unlock next level: no level order set. Call SetLevelOrder first.");
        }

        WriteToDisk();
        _sessionScores[result.levelId] = result.totalScore;

        Debug.Log($"[SaveManager] '{result.levelId}' — total={result.totalScore} " +
                  $"stars={result.stars} newBestScore={newBestScore} newBestStars={newBestStars} " +
                  $"sessionTotal={SessionTotalScore}");
    }
    public List<LeaderboardEntry> GetLeaderboard() => _data.leaderboard;

    public void SubmitSessionToLeaderboard(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "Unknown";

        int score = SessionTotalScore;

        _data.leaderboard.Add(new LeaderboardEntry
        {
            playerName = playerName.Trim(),
            totalScore = score,
            dateString = DateTime.Now.ToString("yyyy-MM-dd"),
        });

        _data.leaderboard.Sort((a, b) => b.totalScore.CompareTo(a.totalScore));

        if (_data.leaderboard.Count > MaxLeaderboard)
            _data.leaderboard.RemoveRange(MaxLeaderboard, _data.leaderboard.Count - MaxLeaderboard);

        WriteToDisk();
        ClearSessionProgress();

        Debug.Log($"[SaveManager] Submitted '{playerName}' with score {score} to leaderboard.");
    }
    public void ClearSessionProgress()
    {
        _sessionScores.Clear();
        Debug.Log("[SaveManager] Session progress cleared.");
    }

    public void ClearLeaderboard()
    {
        _data.leaderboard.Clear();
        WriteToDisk();
        Debug.Log("[SaveManager] Leaderboard cleared.");
    }

    public void DeleteAllSaveData()
    {
        _data = new SaveData();
        _sessionScores.Clear();
        WriteToDisk();
        Debug.Log("[SaveManager] All save data deleted.");
    }
    public int GetNextLevelIndex(string currentLevelId)
    {
        if (_orderedLevelIds == null || _orderedLevelIds.Count == 0)
        {
            Debug.LogWarning("[SaveManager] Cannot get next level index: level order not set.");
            return -1;
        }

        int idx = _orderedLevelIds.IndexOf(currentLevelId);
        if (idx < 0 || idx + 1 >= _orderedLevelIds.Count)
            return -1;

        return idx + 1;
    }

    /// <summary>
    /// Returns the LevelConfig asset for the next level.
    /// </summary>
    /// <param name="currentLevelId">ID of the current level.</param>
    /// <param name="levelSettings">Reference to the LevelSettings ScriptableObject that contains the list of all LevelConfigs.</param>
    /// <returns>LevelConfig of the next level, or null if none or not found.</returns>
    public LevelConfig GetNextLevelConfig(string currentLevelId, LevelSettings levelSettings)
    {
        if (levelSettings == null || levelSettings.allLevels == null)
        {
            Debug.LogWarning("[SaveManager] Invalid LevelSettings provided.");
            return null;
        }

        int nextIndex = GetNextLevelIndex(currentLevelId);
        if (nextIndex < 0 || nextIndex >= levelSettings.allLevels.Count)
            return null;

        return levelSettings.allLevels[nextIndex];
    }
}