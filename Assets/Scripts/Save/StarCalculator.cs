using UnityEngine;

public static class StarCalculator
{
    /// <summary>
    /// Calculates the throw bonus and total score, then determines stars.
    /// </summary>
    /// <param name="rawScore">Score earned from destroying objects.</param>
    /// <param name="throwsRemaining">Throws left when the level ended.</param>
    /// <param name="config">Per-level star/bonus configuration.</param>
    /// <param name="throwBonus">Out: total bonus from unused throws.</param>
    /// <returns>Stars awarded (0–3).</returns>
    public static int Calculate(int rawScore, int throwsRemaining, LevelConfig config,
                                out int throwBonus, out int totalScore)
    {
        throwBonus = throwsRemaining * config.throwBonusPerThrow;
        totalScore = rawScore + throwBonus;

        int stars = 0;
        if (totalScore >= config.threeStarScore) stars = 3;
        else if (totalScore >= config.twoStarScore) stars = 2;
        else if (totalScore >= config.oneStarScore) stars = 1;

        return stars;
    }
}