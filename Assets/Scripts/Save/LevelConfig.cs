using UnityEngine;

[CreateAssetMenu(fileName = "LevelStarConfig", menuName = "Game/Level Star Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Throw Bonus")]
    [Tooltip("Flat score added per unused throw at level end.")]
    public int throwBonusPerThrow = 200;

    [Header("Star Thresholds (total score including throw bonus)")]
    [Tooltip("Minimum total score for 1 star.")]
    public int oneStarScore = 500;

    [Tooltip("Minimum total score for 2 stars.")]
    public int twoStarScore = 1000;

    [Tooltip("Minimum total score for 3 stars.")]
    public int threeStarScore = 1500;

    public int startingThrows = 5;

    public GameObject levelPrefab;

    public Vector2 cameraScrollMax = new Vector2(-2, 15);
}