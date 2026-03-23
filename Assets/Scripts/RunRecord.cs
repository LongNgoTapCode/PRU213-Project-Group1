using System;

/// <summary>
/// Represents a single completed game run/session
/// Stores: final score, play duration, when it was played
/// </summary>
[System.Serializable]
public class RunRecord
{
    public int score;
    public int timeElapsedSeconds;
    public long unixTimestamp; // When the run was completed
}
