using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Settings")]
public class LevelSettings : ScriptableObject
{
    public List<LevelConfig> allLevels;
    public LevelConfig selectedLevel;
    public string currentPlayerName;
    public int currentPlayerTotalScore;
}