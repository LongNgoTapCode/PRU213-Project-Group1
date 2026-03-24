using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages persistent storage of game run history
/// Stores scores, play times, and completion dates
/// Provides sorting and formatting utilities
/// </summary>
public static class LocalScoreStorage
{
    private const string RUNS_PREF_KEY = "game.runs_history";
    private static List<RunRecord> cachedRuns = null;

    /// <summary>
    /// Get all runs sorted by score (highest first)
    /// </summary>
    public static List<RunRecord> GetRunsSortedByScoreDescending()
    {
        var runs = LoadRuns();
        runs.Sort((a, b) => b.score.CompareTo(a.score));
        return runs;
    }

    /// <summary>
    /// Get all runs sorted by completion date (newest first)
    /// </summary>
    public static List<RunRecord> GetRunsSortedByDateDescending()
    {
        var runs = LoadRuns();
        runs.Sort((a, b) => b.unixTimestamp.CompareTo(a.unixTimestamp));
        return runs;
    }

    /// <summary>
    /// Get highest score from local history. Returns 0 if no runs.
    /// </summary>
    public static int GetHighestScore()
    {
        var runs = LoadRuns();
        int highest = 0;
        for (int i = 0; i < runs.Count; i++)
        {
            if (runs[i].score > highest)
                highest = runs[i].score;
        }

        return highest;
    }

    /// <summary>
    /// Save a completed run to history
    /// </summary>
    public static void SaveRun(int finalScore, int timeElapsedSeconds)
    {
        var run = new RunRecord
        {
            score = finalScore,
            timeElapsedSeconds = timeElapsedSeconds,
            unixTimestamp = (long)GetCurrentUnixTimestamp()
        };

        var runs = LoadRuns();
        runs.Add(run);
        SaveRunsToPlayerPrefs(runs);
        cachedRuns = null; // Invalidate cache
    }

    /// <summary>
    /// Clear all run history
    /// </summary>
    public static void ClearHistory()
    {
        PlayerPrefs.DeleteKey(RUNS_PREF_KEY);
        cachedRuns = null;
    }

    /// <summary>
    /// Format seconds as MM:SS or HH:MM:SS if over an hour
    /// </summary>
    public static string FormatPlayTimeMmSs(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hours > 0)
            return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
        else
            return string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    /// <summary>
    /// Convert Unix timestamp to local DateTime
    /// </summary>
    public static DateTime UnixToLocalDateTime(long unixTimestamp)
    {
        DateTime utc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        utc = utc.AddSeconds(unixTimestamp);
        return utc.ToLocalTime();
    }

    //--- Private Helpers ---

    private static List<RunRecord> LoadRuns()
    {
        if (cachedRuns != null)
            return new List<RunRecord>(cachedRuns);

        string json = PlayerPrefs.GetString(RUNS_PREF_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            cachedRuns = new List<RunRecord>();
            return new List<RunRecord>(cachedRuns);
        }

        RunRecordList parsed = JsonUtility.FromJson<RunRecordList>(json);
        var runs = parsed != null && parsed.runs != null ? parsed.runs : new List<RunRecord>();
        cachedRuns = runs ?? new List<RunRecord>();
        return new List<RunRecord>(cachedRuns);
    }

    private static void SaveRunsToPlayerPrefs(List<RunRecord> runs)
    {
        var list = new RunRecordList { runs = runs };
        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(RUNS_PREF_KEY, json);
        PlayerPrefs.Save();
    }

    private static double GetCurrentUnixTimestamp()
    {
        return (System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Wrapper class for JSON serialization (Unity's JsonUtility requires this)
    /// </summary>
    [System.Serializable]
    private class RunRecordList
    {
        public List<RunRecord> runs = new List<RunRecord>();
    }
}
