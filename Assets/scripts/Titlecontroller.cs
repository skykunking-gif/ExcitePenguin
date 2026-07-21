using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// タイトル画面の制御。
/// ・「スタート」ボタン、もしくは任意のキー/タップでコース選択画面へ遷移
/// ・フェードイン/アウトと「PRESS START」の点滅演出付き
///
/// 【Hierarchy構成例】
/// Canvas
///  └ Panel (CanvasGroupをアタッチ)
///      ├ Title Logo (Image or TMP_Text)
///      ├ PressStartText (TMP_Text)
///      └ StartButton (Button) ※無くてもキー入力だけで動作可
/// </summary>
public class TitleController : MonoBehaviour
{
    [Header("遷移先シーン名")]
    [SerializeField] private string nextSceneName = "CourseSelect";

    [Header("UI参照")]
    [SerializeField] private Button startButton;       // 任意。使わない場合は空でOK
    [SerializeField] private CanvasGroup canvasGroup;   // フェード演出用
    [SerializeField] private TMP_Text pressAnyKeyText;  // 「PRESS START」等の点滅テキスト

    [Header("演出設定")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.4f;
    [SerializeField] private float blinkInterval = 0.6f;

    private bool isTransitioning = false;

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeCanvas(0f, 1f, fadeInDuration));
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartPressed);
        }

        if (pressAnyKeyText != null)
        {
            StartCoroutine(BlinkText());
        }
    }

    private void Update()
    {
        if (isTransitioning) return;

        // ボタン以外に、任意のキー入力/タップでもスタートできるようにする
        if (Input.anyKeyDown)
        {
            OnStartPressed();
        }
    }

    public void OnStartPressed()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (startButton != null) startButton.interactable = false;

        StartCoroutine(TransitionToNextScene());
    }

    private IEnumerator TransitionToNextScene()
    {
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeOutDuration));
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float t = 0f;
        canvasGroup.alpha = from;
        while (t < duration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }

    private IEnumerator BlinkText()
    {
        while (true)
        {
            pressAnyKeyText.enabled = !pressAnyKeyText.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
