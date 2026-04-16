public static class LevelLoader
{
    /// <summary>
    /// The prefab name of the level to load.
    /// Set before transitioning to GameScene; read by GameManager on Start.
    /// </summary>
    public static string SelectedLevelId { get; set; } = "";
}