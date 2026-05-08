using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinigameFlowController : MonoBehaviour
{
    public GameObject introUI;
    public GameObject gameUI;

    public GameObject descriptionPanel;
    public GameObject readyPanel;

    public Text titleText;
    public Text descriptionText;
    public Text readyText;

    public MinigameInfo info;

    public Button readyButton;
    public Text waitingText;

    public Image fadeImage;
    public Text countdownText;

    public GameObject resultUI;

    public Text rank1Text;
    public Text rank2Text;
    public Text rank3Text;
    public Text rank4Text;

    private bool[] ready = new bool[4];

    private bool isReadyPhase = false;
    private bool isGameStarted = false;
    private bool isResultShown = false;

    void Start()
    {
        StartCoroutine(GameFlow());
        waitingText.gameObject.SetActive(false);
    }

    void Update()
    {
        // ▼ リザルト
        if (gameUI.activeSelf && !isResultShown && Input.GetKeyDown(KeyCode.Space))
        {
            isResultShown = true;
            StartCoroutine(ShowResult());
        }

        if (waitingText.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ready[1] = true;
            if (Input.GetKeyDown(KeyCode.Alpha2)) ready[2] = true;
            if (Input.GetKeyDown(KeyCode.Alpha3)) ready[3] = true;

            UpdateReadyUI();

            if (AllReady() && !isGameStarted)
            {
                StartCoroutine(StartGameFlow());
            }
        }
    }

    // =========================
    // メインフロー
    // =========================
    IEnumerator GameFlow()
    {
        introUI.SetActive(false);
        gameUI.SetActive(false);
        resultUI.SetActive(false);
        countdownText.gameObject.SetActive(false);

        // 最初のフェード（残す）
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // 説明表示
        introUI.SetActive(true);
        titleText.text = info.gameName;
        descriptionText.text = info.description;
    }

    // =========================
    // Ready UI
    // =========================
    void UpdateReadyUI()
    {
        int readyCount = 0;

        foreach (bool r in ready)
        {
            if (r) readyCount++;
        }

        readyText.text = $"{readyCount}/4 プレイヤー準備完了";
    }

    bool AllReady()
    {
        foreach (bool r in ready)
        {
            if (!r) return false;
        }
        return true;
    }

    public void OnReadyButton()
    {
        // 仮でPlayer1だけReadyにする
        ready[0] = true;

        UpdateReadyUI();

        // ボタン消す
        readyButton.gameObject.SetActive(false);

        // 待機表示
        waitingText.gameObject.SetActive(true);

        // 全員揃ったら開始
        if (AllReady())
        {
            StartCoroutine(StartGameFlow());
        }
    }

    // =========================
    // ゲーム開始
    // =========================
    IEnumerator StartGameFlow()
    {
        if (isGameStarted) yield break;

        isGameStarted = true;

        // ★ 説明とReady両方消す
        descriptionPanel.SetActive(false);
        readyPanel.SetActive(false);

        yield return StartCoroutine(Countdown());

        introUI.SetActive(false);
        gameUI.SetActive(true);
    }

    // =========================
    // カウントダウン
    // =========================
    IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "3";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "2";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "1";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.text = "START!";
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);
    }

    // =========================
    // フェード（最初だけ使用）
    // =========================
    IEnumerator Fade(float start, float end, float duration)
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < duration)
        {
            float alpha = Mathf.Lerp(start, end, time / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, end);
    }

    // =========================
    // リザルト
    // =========================
    IEnumerator ShowResult()
    {
        gameUI.SetActive(false);
        resultUI.SetActive(true);

        ShowRanksInstant();

        // Enter待ち
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        // フェード＋テキスト同時消し
        yield return StartCoroutine(FadeWithResult(0f, 1f, 1f));

        // 終了処理
        EndGame();
    }

    IEnumerator FadeWithResult(float start, float end, float duration)
    {
        float time = 0f;
        Color fadeColor = fadeImage.color;

        while (time < duration)
        {
            float t = time / duration;
            float alpha = Mathf.Lerp(start, end, t);

            // 背景フェード
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);

            // テキストもフェードアウト（逆にする）
            float textAlpha = 1f - t;

            SetTextAlpha(rank1Text, textAlpha);
            SetTextAlpha(rank2Text, textAlpha);
            SetTextAlpha(rank3Text, textAlpha);
            SetTextAlpha(rank4Text, textAlpha);

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, end);
    }

    void SetTextAlpha(Text text, float alpha)
    {
        Color c = text.color;
        text.color = new Color(c.r, c.g, c.b, alpha);
    }

    void ShowRanksInstant()
    {
        rank1Text.text = "1位";
        rank2Text.text = "2位";
        rank3Text.text = "3位";
        rank4Text.text = "4位";

        rank1Text.color = new Color(1f, 0.84f, 0f);
        rank2Text.color = new Color(0.75f, 0.75f, 0.75f);
        rank3Text.color = new Color(0.8f, 0.5f, 0.2f);
        rank4Text.color = new Color(0.5f, 0.7f, 1f);

        rank1Text.gameObject.SetActive(true);
        rank2Text.gameObject.SetActive(true);
        rank3Text.gameObject.SetActive(true);
        rank4Text.gameObject.SetActive(true);
    }


    void EndGame()
    {
        Debug.Log("ゲーム終了！");

        // 仮：止めるだけ
        //Time.timeScale = 0f;
    }
}