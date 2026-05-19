using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MinigameFlowController : MonoBehaviour
{
    [Header("UI")]
    public GameObject introUI;
    public GameObject gameUI;
    public GameObject resultUI;

    [Header("Intro")]
    public GameObject descriptionPanel;
    public GameObject readyPanel;

    public Text titleText;
    public Text descriptionText;
    public Text readyText;

    [Header("Ready")]
    public Button readyButton;
    public Text waitingText;

    [Header("Countdown")]
    public Image fadeImage;
    public Text countdownText;

    [Header("Result")]
    public Text rank1Text;
    public Text rank2Text;
    public Text rank3Text;
    public Text rank4Text;

    [Header("Data")]
    public MinigameInfo info;

    // 仮実装用
    private bool[] ready = new bool[4];

    private bool isGameStarted = false;
    private bool isResultShown = false;

    // =====================================================
    // Start
    // =====================================================

    void Start()
    {
        StartCoroutine(GameFlow());

        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        resultUI.SetActive(false);
        gameUI.SetActive(false);
    }

    // =====================================================
    // 仮実装
    // サーバー通信完成後削除予定
    // =====================================================

    void Update()
    {
        // 仮Readyデバッグ
        if (waitingText.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ready[1] = true;
            if (Input.GetKeyDown(KeyCode.Alpha2)) ready[2] = true;
            if (Input.GetKeyDown(KeyCode.Alpha3)) ready[3] = true;

            UpdateReadyUI();

            // 仮全員Ready判定
            if (AllReady() && !isGameStarted)
            {
                AllPlayerReady();
            }
        }

        // 仮リザルト表示
        if (gameUI.activeSelf && !isResultShown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isResultShown = true;

                int score = 100;

                // スコア送信
                MinigameNetworkManager.Instance.SendScore(score);
            }
        }
    }

    // =====================================================
    // 初期フロー
    // =====================================================

    IEnumerator GameFlow()
    {
        introUI.SetActive(false);

        // 最初のフェード
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        // 説明表示
        introUI.SetActive(true);

        titleText.text = info.gameName;
        descriptionText.text = info.description;
    }

    // =====================================================
    // Ready UI
    // =====================================================

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

    // =====================================================
    // Readyボタン
    // =====================================================

    public void OnReadyButton()
    {
        // 自分のReady切り替え
        ready[0] = !ready[0];

        // サーバー送信
        MinigameNetworkManager.Instance.SendReadyState(ready[0]);

        UpdateReadyUI();

        // ボタンUI変更
        if (ready[0])
        {
            readyButton.GetComponentInChildren<Text>().text = "取り消し";

            waitingText.gameObject.SetActive(true);
        }
        else
        {
            readyButton.GetComponentInChildren<Text>().text = "準備OK！";

            waitingText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // サーバーからReady更新受信
    // =====================================================

    public void UpdatePlayerReady(string playerName, bool isReady)
    {
        Debug.Log($"{playerName} Ready : {isReady}");

        // TODO:
        // プレイヤー別UI更新
    }

    // =====================================================
    // 全員Ready
    // =====================================================

    public void AllPlayerReady()
    {
        Debug.Log("全員準備完了");

        StartCoroutine(StartGameFlow());

        // サーバーにカウントダウン開始要求
        MinigameNetworkManager.Instance.StartCountdown();
    }

    // =====================================================
    // ゲーム開始
    // =====================================================

    IEnumerator StartGameFlow()
    {
        if (isGameStarted) yield break;

        isGameStarted = true;

        // UI非表示
        descriptionPanel.SetActive(false);
        readyPanel.SetActive(false);

        yield return null;
    }

    // =====================================================
    // カウントダウン受信
    // =====================================================

    public void StartCountdown(int remain)
    {
        countdownText.gameObject.SetActive(true);

        if (remain > 0)
        {
            countdownText.text = remain.ToString();
        }
        else
        {
            countdownText.text = "START!";

            StartCoroutine(BeginGameAfterStart());
        }
    }

    // =====================================================
    // ゲーム開始後
    // =====================================================

    IEnumerator BeginGameAfterStart()
    {
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);

        introUI.SetActive(false);

        // シーン移動
        SceneManager.LoadScene("GameScene");
    }

    // =====================================================
    // フェード
    // =====================================================

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

    // =====================================================
    // ランキング受信
    // =====================================================

    public void ShowRanking(List<string> rankOrder)
    {
        StartCoroutine(ShowResult(rankOrder));
    }

    // =====================================================
    // リザルト表示
    // =====================================================

    IEnumerator ShowResult(List<string> rankOrder)
    {
        gameUI.SetActive(false);

        resultUI.SetActive(true);

        rank1Text.text = $"1位 {rankOrder[0]}";
        rank2Text.text = $"2位 {rankOrder[1]}";
        rank3Text.text = $"3位 {rankOrder[2]}";
        rank4Text.text = $"4位 {rankOrder[3]}";

        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        yield return StartCoroutine(FadeWithResult(0f, 1f, 1f));

        EndGame();
    }

    // =====================================================
    // リザルトフェード
    // =====================================================

    IEnumerator FadeWithResult(float start, float end, float duration)
    {
        float time = 0f;

        Color fadeColor = fadeImage.color;

        while (time < duration)
        {
            float t = time / duration;

            float alpha = Mathf.Lerp(start, end, t);

            // 背景フェード
            fadeImage.color = new Color(
                fadeColor.r,
                fadeColor.g,
                fadeColor.b,
                alpha
            );

            // テキストフェード
            float textAlpha = 1f - t;

            SetTextAlpha(rank1Text, textAlpha);
            SetTextAlpha(rank2Text, textAlpha);
            SetTextAlpha(rank3Text, textAlpha);
            SetTextAlpha(rank4Text, textAlpha);

            time += Time.unscaledDeltaTime;

            yield return null;
        }

        fadeImage.color = new Color(
            fadeColor.r,
            fadeColor.g,
            fadeColor.b,
            end
        );
    }

    // =====================================================
    // テキスト透明度
    // =====================================================

    void SetTextAlpha(Text text, float alpha)
    {
        Color c = text.color;

        text.color = new Color(c.r, c.g, c.b, alpha);
    }

    // =====================================================
    // 終了
    // =====================================================

    void EndGame()
    {
        Debug.Log("ゲーム終了！");
    }
}