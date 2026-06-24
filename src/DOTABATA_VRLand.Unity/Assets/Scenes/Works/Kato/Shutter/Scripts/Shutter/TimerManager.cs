using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    // 制限時間（秒）
    public float timeLimit = 30f;

    // タイマー表示用テキスト
    public TextMeshProUGUI timerText;

    // タイマー終了済みかどうか
    bool isFinished = false;

    // ゲーム全体を管理するスクリプト
    public ShutterGameManager shuttergameManager;

    // ゲームオーバー表示用UI
    public GameObject gameOverText;

    void Update()
    {
        // タイマー終了後は更新しない
        if (isFinished)
            return;

        // ゲーム終了時はタイマーを停止
        if (shuttergameManager.isGameOver)
            return;

        // 制限時間を減らす
        timeLimit -= Time.deltaTime;

        // 制限時間切れ
        if (timeLimit <= 0)
        {
            timeLimit = 0;
            isFinished = true;

            // ゲームオーバー状態にする
            shuttergameManager.isGameOver = true;

            // ゲームオーバー表示
            gameOverText.SetActive(true);

            Debug.Log("TIME UP!");
        }

        // 残り時間を切り上げて表示
        timerText.text = Mathf.Ceil(timeLimit).ToString();
    }
}