using UnityEngine;
using TMPro;
public class LeaderboardEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    public void Populate(int rank, LeaderboardEntry entry)
    {
        if (rankText != null) rankText.text = $"#{rank}";
        if (nameText != null) nameText.text = entry.playerName;
        if (scoreText != null) scoreText.text = entry.totalScore.ToString();
    }
}