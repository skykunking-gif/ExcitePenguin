using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// ゴール後のリザルト画面。
/// レース終了処理（ゴール判定）側から ShowResult(勝者名, タイム秒) を呼び出すと結果パネルが表示される。
///
/// 【Hierarchy構成例】
/// Canvas
///  └ ResultPanel (最初は非アクティブにしておく)
///      ├ "result" (TMP_Text, 見出し)
///      ├ WinnerText (TMP_Text) → "勝者：AAA"
///      ├ TimeText (TMP_Text)   → "タイム：M:SS:m"
///      ├ RetryButton (Button)
///      └ TitleButton (Button)
///
/// 【使い方】
/// ゴール判定をしているスクリプト（レースマネージャー等）から:
///   resultController.ShowResult("AAA", elapsedTime);
/// のように呼び出してください。
/// </summary>
public class ResultController : MonoBehaviour
{
    [Header("UI参照")]
    [SerializeField] private GameObject resultPanel;   // 結果表示パネル（最初は非表示）
    [SerializeField] private TMP_Text winnerText;       // 「勝者：AAA」
    [SerializeField] private TMP_Text timeText;         // 「タイム：M:SS:m」
    [SerializeField] private Button retryButton;
    [SerializeField] private Button titleButton;

    [Header("シーン名")]
    [SerializeField] private string titleSceneName = "Title";

    private void Awake()
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (retryButton != null) retryButton.onClick.AddListener(OnRetry);
        if (titleButton != null) titleButton.onClick.AddListener(OnBackToTitle);
    }

    /// <summary>
    /// レース終了時に呼び出す。
    /// </summary>
    /// <param name="winnerName">勝者名（例："AAA"）</param>
    /// <param name="raceTimeSeconds">経過タイム（秒）</param>
    public void ShowResult(string winnerName, float raceTimeSeconds)
    {
        if (winnerText != null) winnerText.text = $"勝者：{winnerName}";
        if (timeText != null) timeText.text = $"タイム：{FormatTime(raceTimeSeconds)}";

        if (resultPanel != null) resultPanel.SetActive(true);
    }

    /// <summary>
    /// 秒数を "M:SS:m" 形式（分:秒:10ミリ秒単位）に変換する
    /// </summary>
    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);
        int centis = Mathf.FloorToInt((seconds * 100f) % 100f); // 1/100秒

        return $"{minutes}:{secs:00}:{centis:00}";
    }

    private void OnRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnBackToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
