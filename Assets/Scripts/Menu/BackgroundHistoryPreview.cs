using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows recent/top run history directly on background/menu scene.
/// Attach this to a TextMeshProUGUI object.
/// </summary>
public class BackgroundHistoryPreview : MonoBehaviour
{
    public enum PreviewSortMode
    {
        ByScoreDescending = 0,
        ByEndTimeNewestFirst = 1
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private string title = "Play History";
    [SerializeField] private string emptyMessage = "No play history yet.";
    [SerializeField] private int maxEntries = 5;

    [Header("Display")]
    [SerializeField] private PreviewSortMode sortMode = PreviewSortMode.ByScoreDescending;
    [SerializeField] private string rowFormat = "{0}. Score {1} | {2} | {3}";
    [SerializeField] private bool refreshOnEnable = true;

    void Reset()
    {
        targetText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (refreshOnEnable)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (targetText == null)
        {
            return;
        }

        var runs = sortMode == PreviewSortMode.ByEndTimeNewestFirst
            ? LocalScoreStorage.GetRunsSortedByDateDescending()
            : LocalScoreStorage.GetRunsSortedByScoreDescending();

        if (runs == null || runs.Count == 0)
        {
            targetText.text = string.IsNullOrWhiteSpace(title)
                ? emptyMessage
                : title + "\n" + emptyMessage;
            return;
        }

        int count = Mathf.Clamp(maxEntries, 1, runs.Count);
        StringBuilder sb = new StringBuilder(256);

        if (!string.IsNullOrWhiteSpace(title))
        {
            sb.AppendLine(title);
        }

        for (int i = 0; i < count; i++)
        {
            RunRecord run = runs[i];
            string playTime = LocalScoreStorage.FormatPlayTimeMmSs(run.timeElapsedSeconds);
            string when = run.unixTimestamp > 0
                ? LocalScoreStorage.UnixToLocalDateTime(run.unixTimestamp).ToString("dd/MM/yyyy HH:mm")
                : "-";

            sb.AppendLine(string.Format(rowFormat, i + 1, run.score, playTime, when));
        }

        targetText.text = sb.ToString().TrimEnd();
    }
}
