using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// タイトル画面の制御。
/// ・「次へ」でコース選択画面（CourseSelect）へ
/// ・「オプション」でペンギンの色を選ぶパネルを開く（選んだ色は PlayerPrefs に保存され、
///   レース中のプレイヤーペンギンに PenguinColorOption が反映する）
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [Header("遷移先シーン名")]
    [SerializeField] private string nextSceneName = "CourseSelect";

    [Header("UI参照")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button optionButton;
    [SerializeField] private Button optionCloseButton;
    [SerializeField] private GameObject optionPanel;

    [Header("色の選択（0:そのまま 1:#A7F542 2:#4298F5）")]
    [SerializeField] private Button[] colorButtons;
    [Tooltip("選択中の色に出す枠。colorButtons と同じ並びにする")]
    [SerializeField] private GameObject[] colorSelectedMarks;

    private void Start()
    {
        if (optionPanel != null) optionPanel.SetActive(false);

        if (nextButton != null) nextButton.onClick.AddListener(OnNextPressed);
        if (optionButton != null) optionButton.onClick.AddListener(OnOptionPressed);
        if (optionCloseButton != null) optionCloseButton.onClick.AddListener(OnOptionClosePressed);

        for (int i = 0; colorButtons != null && i < colorButtons.Length; i++)
        {
            if (colorButtons[i] == null) continue;
            int index = i; // クロージャ対策
            colorButtons[i].onClick.AddListener(() => OnColorSelected(index));
        }

        RefreshColorMarks();
    }

    public void OnNextPressed()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void OnOptionPressed()
    {
        if (optionPanel != null) optionPanel.SetActive(true);
    }

    public void OnOptionClosePressed()
    {
        if (optionPanel != null) optionPanel.SetActive(false);
    }

    /// <summary>色を選んで保存する</summary>
    public void OnColorSelected(int index)
    {
        PlayerPrefs.SetInt(PenguinColorOption.PrefsKey, index);
        PlayerPrefs.Save();
        RefreshColorMarks();
    }

    /// <summary>選択中の色にだけ枠を出す</summary>
    private void RefreshColorMarks()
    {
        if (colorSelectedMarks == null) return;

        int selected = PenguinColorOption.GetSelectedIndex();
        for (int i = 0; i < colorSelectedMarks.Length; i++)
        {
            if (colorSelectedMarks[i] != null) colorSelectedMarks[i].SetActive(i == selected);
        }
    }
}
