using UnityEngine;

public static class ScoreSystem
{
    public static int playerScore = 0;

    public static void AddPlayerPoints(int amount)
    {
        playerScore += amount;
        Debug.Log($"[PLAYER] +{amount}  Total: {playerScore}");
    }
}
