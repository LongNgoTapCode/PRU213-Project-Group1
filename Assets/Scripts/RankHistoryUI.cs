using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RankHistoryUI : MonoBehaviour
{
    public enum SortMode
    {
        ByScoreDescending = 0,
        ByEndTimeNewestFirst = 1
    }

    [FormerlySerializedAs("sortMode")]
    [SerializeField] SortMode sortBy = SortMode.ByEndTimeNewestFirst;

    [SerializeField] string lineFormat = "{0}. Score {1}  |  Play {2}  |  {3}";
    [SerializeField] string emptyMessage = "No play history yet.";

    [SerializeField] RectTransform linesContent;
    [SerializeField] GameObject linePrefab;
    [SerializeField] float rowMinHeight = 36f;

    public SortMode SortBy
    {
        get => sortBy;
        set
        {
            sortBy = value;
            Refresh();
        }
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var runs = sortBy switch
        {
            SortMode.ByScoreDescending => LocalScoreStorage.GetRunsSortedByScoreDescending(),
            SortMode.ByEndTimeNewestFirst => LocalScoreStorage.GetRunsSortedByDateDescending(),
            _ => LocalScoreStorage.GetRunsSortedByScoreDescending()
        };

        if (linesContent != null && linePrefab != null)
            FillScrollLines(linesContent, linePrefab, runs);
    }

    void FillScrollLines(RectTransform content, GameObject prefab, System.Collections.Generic.List<RunRecord> runs)
    {
        ClearContentChildren(content);

        if (runs == null || runs.Count == 0)
        {
            GameObject row = Instantiate(prefab, content);
            ApplyRowLayout(row);
            var t = row.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null) t.text = emptyMessage;
            FinalizeScrollContent(content);
            return;
        }

        for (int i = 0; i < runs.Count; i++)
        {
            RunRecord r = runs[i];
            string playTime = LocalScoreStorage.FormatPlayTimeMmSs(r.timeElapsedSeconds);
            string when = r.unixTimestamp > 0
                ? LocalScoreStorage.UnixToLocalDateTime(r.unixTimestamp).ToString("dd/MM/yyyy HH:mm")
                : "-";

            GameObject row = Instantiate(prefab, content);
            ApplyRowLayout(row);
            var t = row.GetComponentInChildren<TextMeshProUGUI>();
            if (t != null)
            {
                t.text = string.Format(lineFormat, i + 1, r.score, playTime, when);
                t.enableAutoSizing = false;
                t.ForceMeshUpdate();
            }
        }

        FinalizeScrollContent(content);
    }

    static void ClearContentChildren(RectTransform content)
    {
        while (content != null && content.childCount > 0)
            Object.DestroyImmediate(content.GetChild(0).gameObject);
    }

    void ApplyRowLayout(GameObject row)
    {
        row.transform.localScale = Vector3.one;

        LayoutElement le = row.GetComponent<LayoutElement>();
        if (le == null)
le = row.AddComponent<LayoutElement>();
        le.minHeight = rowMinHeight;
        le.preferredHeight = rowMinHeight;
    }

    static void FinalizeScrollContent(RectTransform content)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        Canvas.ForceUpdateCanvases();

        ScrollRect scroll = content.GetComponentInParent<ScrollRect>();
        if (scroll != null)
            scroll.verticalNormalizedPosition = 1f;
    }
}