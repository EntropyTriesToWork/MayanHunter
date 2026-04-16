using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<LevelSaveEntry> levels = new List<LevelSaveEntry>();
    public List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();
}
[Serializable]
public class LevelSaveEntry
{
    public string levelId = "";
    public bool isUnlocked = false;
    public bool isCompleted = false;
    public int bestScore = 0;
    public int bestStars = 0;   // 0–3
}
public class LevelResultData
{
    public string levelId;
    public int rawScore;
    public int throwBonus;
    public int totalScore;
    public int stars;           
    public int throwsRemaining;
}
[Serializable]
public class LeaderboardEntry
{
    public string playerName = "";
    public int totalScore = 0;
    public string dateString = "";   // ISO 8601 — "yyyy-MM-dd"
}
public class CompareResult
{
    public bool isFirstClear = false;
    public int scoreDelta = 0;   // positive = new best
    public int starsDelta = 0;
    public int previousScore = 0;
    public int previousStars = 0;
}