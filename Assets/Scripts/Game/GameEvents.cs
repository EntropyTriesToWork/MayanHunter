using System;
public static class GameEvents
{
    /// <summary>
    /// Fired by a LevelObject when its health reaches zero and it is destroyed.
    /// Carries the destroyed LevelObject and its score value.
    /// </summary>
    public static event Action<LevelObject> OnLevelObjectDestroyed;

    /// <summary>
    /// Fired by a LevelObject whenever it takes damage.
    /// Carries the object and the amount of damage dealt this hit.
    /// </summary>
    public static event Action<LevelObject, int> OnLevelObjectDamaged;

    /// <summary>Fired by LevelController when the player wins the level.</summary>
    public static event Action<int> OnLevelVictory;   // int = total score

    /// <summary>Fired by LevelController when the player fails the level.</summary>
    public static event Action OnLevelFailed;

    /// <summary>Fired by LevelController when a new throw begins.</summary>
    public static event Action<int> OnThrowsRemainingChanged;  // int = throws left

    /// <summary>Fired when the game is paused or unpaused.</summary>
    public static event Action<bool> OnPauseStateChanged;   // true = paused

    public static void LevelObjectDestroyed(LevelObject obj)            => OnLevelObjectDestroyed?.Invoke(obj);
    public static void LevelObjectDamaged(LevelObject obj, int damage)  => OnLevelObjectDamaged?.Invoke(obj, damage);
    public static void LevelVictory(int score)                          => OnLevelVictory?.Invoke(score);
    public static void LevelFailed()                                     => OnLevelFailed?.Invoke();
    public static void ThrowsRemainingChanged(int remaining)            => OnThrowsRemainingChanged?.Invoke(remaining);
    public static void PauseStateChanged(bool isPaused)                 => OnPauseStateChanged?.Invoke(isPaused);
}
