using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;

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

    private bool isGameStarted = false;
    private bool isResultShown = false;

    // =====================================================
    // Start
    // =====================================================

    async void Start()
    {
        // RoomModelイベント購読
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction += OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction += OnUpdatePlayerReady;

        StartCoroutine(GameFlow());

        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        resultUI.SetActive(false);
        gameUI.SetActive(false);

        readyText.text = "0/4 プレイヤー準備完了";
    }

    // =====================================================
    // Update
    // =====================================================

    void Update()
    {
        // 仮スコア送信
        // あとで実際のゲーム終了タイミングに変更
        if (gameUI.activeSelf && !isResultShown)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isResultShown = true;

                int score = 100;

                RoomModel.I.SendScore(score);
            }
        }
    }

    // =====================================================
    // Destroy
    // =====================================================

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;

        //RoomModel.I.OnCountdownAction -= StartCountdown;
        //RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;
        //RoomModel.I.OnUpdatedAllReadyStateAction -= OnAllReadyState;
        //RoomModel.I.OnUpdatedReadyStateAction -= OnUpdatePlayerReady;
    }

    // =====================================================
    // 初期フロー
    // =====================================================

    IEnumerator GameFlow()
    {
        introUI.SetActive(false);

        // フェードイン
        yield return StartCoroutine(Fade(1f, 0f, 1f));

        introUI.SetActive(true);

        titleText.text = info.gameName;
        descriptionText.text = info.description;
    }

    // =====================================================
    // Readyボタン
    // =====================================================

    public void OnReadyButton()
    {
        bool willReady =
            readyButton.GetComponentInChildren<Text>().text == "準備OK！";

        // サーバー送信
        RoomModel.I.SendReadyState(willReady);

        // UI更新
        if (willReady)
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
    // プレイヤーReady更新
    // =====================================================

    void OnUpdatePlayerReady(JoinedUser user, bool isReady)
    {
        Debug.Log($"{user.Name} Ready : {isReady}");

        // TODO:
        // プレイヤー一覧UI更新
    }

    // =====================================================
    // 全員Ready通知
    // =====================================================

    void OnAllReadyState(bool isAllReady)
    {
        if (!isAllReady) return;

        Debug.Log("全員Ready");

        StartCoroutine(StartGameFlow());

        // ホストだけが呼ぶようにするなら
        // 後でホスト判定追加
        RoomModel.I.StartCountdown();
    }

    // =====================================================
    // ゲーム開始準備
    // =====================================================

    IEnumerator StartGameFlow()
    {
        if (isGameStarted) yield break;

        isGameStarted = true;

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
    // ゲーム開始
    // =====================================================

    IEnumerator BeginGameAfterStart()
    {
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);

        introUI.SetActive(false);

        gameUI.SetActive(true);

        // 必要ならシーン遷移
        // SceneManager.LoadScene("GameScene");
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

            fadeImage.color =
                new Color(color.r, color.g, color.b, alpha);

            time += Time.unscaledDeltaTime;

            yield return null;
        }

        fadeImage.color =
            new Color(color.r, color.g, color.b, end);
    }

    // =====================================================
    // ランキング受信
    // =====================================================

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        List<string> names = new List<string>();

        foreach (var user in rankOrder)
        {
            names.Add(user.Name);
        }

        ShowRanking(names);
    }

    // =====================================================
    // ランキング表示開始
    // =====================================================

    void ShowRanking(List<string> rankOrder)
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

        if (rankOrder.Count > 0)
            rank1Text.text = $"1位 {rankOrder[0]}";

        if (rankOrder.Count > 1)
            rank2Text.text = $"2位 {rankOrder[1]}";

        if (rankOrder.Count > 2)
            rank3Text.text = $"3位 {rankOrder[2]}";

        if (rankOrder.Count > 3)
            rank4Text.text = $"4位 {rankOrder[3]}";

        yield return new WaitUntil(() =>
            Input.GetKeyDown(KeyCode.Return));

        yield return StartCoroutine(
            FadeWithResult(0f, 1f, 1f));

        EndGame();
    }

    // =====================================================
    // リザルトフェード
    // =====================================================

    IEnumerator FadeWithResult(
        float start,
        float end,
        float duration)
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

        text.color =
            new Color(c.r, c.g, c.b, alpha);
    }

    // =====================================================
    // 終了
    // =====================================================

    void EndGame()
    {
        Debug.Log("ゲーム終了！");
    }
}