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

    public Image fadeImage;
    public Text countdownText;

    public GameObject resultUI;

    public Text rank1Text;
    public Text rank2Text;
    public Text rank3Text;
    public Text rank4Text;

    private bool[] ready = new bool[4];

    private bool allReady = false;

    private bool isReadyPhase = false;
    private bool isGameStarted = false;
    private bool isResultShown = false;

    void Start()
    {
        StartCoroutine(GameFlow());
    }

    void Update()
    {
        // ▼ 説明→Ready移行
        if (!isReadyPhase && introUI.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            isReadyPhase = true;

            descriptionPanel.SetActive(false); // ←説明消す
            readyPanel.SetActive(true);        // ←Ready表示

            UpdateReadyUI();
        }

        // ▼ Ready操作
        if (isReadyPhase && !isGameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ready[0] = !ready[0];
            if (Input.GetKeyDown(KeyCode.Alpha2)) ready[1] = !ready[1];
            if (Input.GetKeyDown(KeyCode.Alpha3)) ready[2] = !ready[2];
            if (Input.GetKeyDown(KeyCode.Alpha4)) ready[3] = !ready[3];

            UpdateReadyUI();

            // 全員Readyになったら
            if (AllReady())
            {
                allReady = true;
                readyText.text = "全員準備OK！\nスペースでスタート！";
            }

            // ★ スタート待ち
            if (allReady && Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(StartGameFlow());
            }
        }

        // ▼ リザルト
        if (gameUI.activeSelf && !isResultShown && Input.GetKeyDown(KeyCode.Space))
        {
            isResultShown = true;
            StartCoroutine(ShowResult());
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
        readyText.text =
            $"Player1：{(ready[0] ? "○" : "×")}（1キー）\n" +
            $"Player2：{(ready[1] ? "○" : "×")}（2キー）\n" +
            $"Player3：{(ready[2] ? "○" : "×")}（3キー）\n" +
            $"Player4：{(ready[3] ? "○" : "×")}（4キー）";
    }

    bool AllReady()
    {
        foreach (bool r in ready)
        {
            if (!r) return false;
        }
        return true;
    }

    // =========================
    // ゲーム開始
    // =========================
    IEnumerator StartGameFlow()
    {
        isGameStarted = true;

        // ★ Ready表示を消す
        readyPanel.SetActive(false);

        // （ちょい間を入れると気持ちいい）
        yield return new WaitForSecondsRealtime(0.3f);

        // カウントダウン
        yield return StartCoroutine(Countdown());

        // ゲーム開始
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