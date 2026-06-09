using UnityEngine;
using TMPro;

/// <summary>
/// 制限時間を管理するクラス
/// タイマー表示とゲーム終了判定を行う
/// </summary>

public class SiderGameTimer : MonoBehaviour
{
    // 制限時間
    public float timer = 10f;

    // ゲーム終了フラグ
    public bool gameEnded;

    // タイマー表示用テキスト
    public TextMeshProUGUI timerText;

    void Update()
    {
        // ゲーム終了後はタイマー停止
        if (gameEnded) return;

        // 制限時間を減らす
        timer -= Time.deltaTime;

        // 秒数を切り上げて表示
        // 9.2秒 → 10 と表示される
        timerText.text =
            Mathf.Ceil(timer).ToString();

        // 制限時間終了
        if (timer <= 0)
        {
            gameEnded = true;

            Debug.Log("終了！");
        }
    }
}