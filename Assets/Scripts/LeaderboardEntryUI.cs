using TMPro;
using UnityEngine;

public class LeaderboardEntryUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI totalTimeText;

    public void SetLeaderboardEntry(int rank, string name, float totalTime)
    {
        rankText.text = rank.ToString();
        nameText.text = name;
        int numHours = Mathf.FloorToInt(totalTime / 3600);
        int numMinutes = Mathf.FloorToInt((totalTime % 3600) / 60);
        int numSeconds = Mathf.FloorToInt(totalTime % 60);
        totalTimeText.text = $"{numHours}h {numMinutes}m {numSeconds}s";
    }
}