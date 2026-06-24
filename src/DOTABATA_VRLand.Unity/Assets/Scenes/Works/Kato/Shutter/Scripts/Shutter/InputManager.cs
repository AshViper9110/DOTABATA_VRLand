using UnityEngine;

public class InputManager : MonoBehaviour
{
    // シャッターゲーム全体を管理するスクリプト
    public ShutterGameManager shuttergameManager;

    // ドラッグ開始位置
    Vector2 startPos;

    // ドラッグ中かどうか
    bool isDragging = false;

    void Update()
    {
        // ゲーム終了時は入力を受け付けない
        if (shuttergameManager.isGameOver)
            return;

        // プレイヤーが移動中は入力を受け付けない
        if (!shuttergameManager.canInput)
            return;

        // ドラッグ開始
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
        }

        // ドラッグ終了時にスワイプ方向を判定
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 endPos = Input.mousePosition;
            Vector2 dir = (endPos - startPos).normalized;

            // スワイプ方向をシャッター用の方向に変換
            Shutter.Direction inputDir = GetDirection(dir);

            // 現在のシャッターを取得
            Shutter current = shuttergameManager.GetCurrentShutter();

            // シャッターが存在すれば開く判定を行う
            if (current != null)
            {
                current.TryOpen(inputDir);
            }

            isDragging = false;
        }
    }

    /// <summary>
    /// スワイプ方向を上下左右のDirectionに変換する
    /// </summary>
    Shutter.Direction GetDirection(Vector2 dir)
    {
        if (Vector2.Dot(dir, Vector2.up) > 0.7f)
            return Shutter.Direction.Up;

        if (Vector2.Dot(dir, Vector2.down) > 0.7f)
            return Shutter.Direction.Down;

        if (Vector2.Dot(dir, Vector2.right) > 0.7f)
            return Shutter.Direction.Right;

        // 上下右以外は左として扱う
        return Shutter.Direction.Left;
    }
}