using UnityEngine;

public class ShutterGameManager : MonoBehaviour
{
    // シャッターを順番に管理する配列
    public Shutter[] shutters;

    // 現在操作するシャッターの番号
    int currentIndex = 0;

    // プレイヤーが入力可能かどうか
    public bool canInput = false;

    // クリア表示用UI
    public GameObject finishText;

    // ゲーム終了状態
    public bool isGameOver = false;

    void Start()
    {
        // 最初のシャッターを設定
        UpdateShutters();
    }

    /// <summary>
    /// 次のシャッターへ進む
    /// </summary>
    public void NextShutter()
    {
        currentIndex++;

        UpdateShutters();

        // 全シャッターを開いたらゲームクリア
        if (currentIndex >= shutters.Length)
        {
            Debug.Log("クリア！");
            finishText.SetActive(true);
        }
    }

    /// <summary>
    /// シャッター状態の更新
    /// </summary>
    void UpdateShutters()
    {
        for (int i = 0; i < shutters.Length; i++)
        {
            bool isCurrent = (i == currentIndex);
        }
    }

    /// <summary>
    /// 現在操作対象のシャッターを取得
    /// </summary>
    public Shutter GetCurrentShutter()
    {
        if (currentIndex < shutters.Length)
            return shutters[currentIndex];

        return null;
    }
}