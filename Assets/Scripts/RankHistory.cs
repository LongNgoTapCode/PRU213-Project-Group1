using UnityEngine;

/// <summary>
/// Attach to any GameObject to provide rank history save functionality.
/// Call SaveCurrentRun() to persist a game session result.
/// </summary>
public class RankHistory : MonoBehaviour
{
    /// <summary>
    /// Save a completed run to persistent storage.
    /// Called automatically by GameManager on GameOver.
    /// </summary>
    public static void SaveCurrentRun(int finalScore, int timeElapsedSeconds)
    {
        LocalScoreStorage.SaveRun(finalScore, timeElapsedSeconds);
        Debug.Log($"[RankHistory] Run saved — Score: {finalScore}, Time: {timeElapsedSeconds}s");
    }
}
