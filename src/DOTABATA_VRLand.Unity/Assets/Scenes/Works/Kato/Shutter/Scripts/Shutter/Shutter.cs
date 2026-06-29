using UnityEngine;
using TMPro;
using System.Collections;

public class Shutter : MonoBehaviour
{
    // シャッターが開く正しい方向
    public enum Direction { Up, Down, Left, Right }

    // 正解の方向
    public Direction correctDirection;

    // 矢印表示用テキスト
    public TextMeshPro arrowText;

    // ゲーム全体を管理するスクリプト
    public ShutterGameManager shuttergameManager;

    // このシャッターが所属するレーン番号
    [Header("このシャッターのレーン番号")]
    public int laneId;

    void Start()
    {
        // ランダムで正解方向を決定
        correctDirection = (Direction)Random.Range(0, 4);

        // 矢印を更新
        UpdateArrow();
    }

    /// <summary>
    /// プレイヤーの入力方向を判定する
    /// </summary>
    public void TryOpen(Direction input)
    {
        // 自分のレーン以外は操作できない
        if (laneId != PlayerLane.MyLane)
        {
            return;
        }

        // 正しい方向ならシャッターを開く
        if (input == correctDirection)
        {
            Debug.Log("開いた！");
            StartCoroutine(OpenShutter());

            //全員に通知
        }
        else
        {
            Debug.Log("ミス！");
        }
    }

    /// <summary>
    /// 開く方向に応じた移動方向を取得
    /// </summary>
    Vector3 GetMoveDirection()
    {
        switch (correctDirection)
        {
            case Direction.Up:
                return Vector3.up;

            case Direction.Down:
                return Vector3.down;

            case Direction.Left:
                return Vector3.left;

            case Direction.Right:
                return Vector3.right;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// シャッターを開くアニメーション
    /// </summary>
    IEnumerator OpenShutter()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + GetMoveDirection() * 5f;

        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime * 3f;

            transform.position =
                Vector3.Lerp(startPos, endPos, time);

            yield return null;
        }

        // 次のシャッターへ進む
        shuttergameManager.NextShutter();

        // 必要なら非表示にする
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// 正解方向に応じて矢印を表示する
    /// </summary>
    void UpdateArrow()
    {
        switch (correctDirection)
        {
            case Direction.Up:
                arrowText.text = "↑";
                break;

            case Direction.Down:
                arrowText.text = "↓";
                break;

            case Direction.Left:
                arrowText.text = "←";
                break;

            case Direction.Right:
                arrowText.text = "→";
                break;
        }
    }
}