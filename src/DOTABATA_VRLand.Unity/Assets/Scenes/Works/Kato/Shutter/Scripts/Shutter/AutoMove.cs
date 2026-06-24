using UnityEngine;

public class AutoMove : MonoBehaviour
{
    // シャッターゲーム全体を管理するスクリプト
    public ShutterGameManager shuttergameManager;

    // 前進速度
    public float speed = 1f;

    // シャッターの手前で停止する距離
    public float stopDistance = 10f;

    void Start()
    {
        // マルチ対応時に参加順からレーン番号を取得予定
        //if (InRoomPlayerData.I.MySelf == null)
        //{
        //    Debug.LogError("まだルーム参加できてない");
        //    return;
        //}

        //int lane =
        //    InRoomPlayerData.I.MySelf.JoinOrder;
    }

    void Update()
    {
        // ゲーム終了時は移動しない
        if (shuttergameManager.isGameOver)
            return;

        // 現在対象となるシャッターを取得
        Shutter current = shuttergameManager.GetCurrentShutter();

        // シャッターが存在しない場合は何もしない
        if (current == null)
            return;

        // プレイヤーとシャッターとの距離を取得
        float distance =
            Vector3.Distance(transform.position, current.transform.position);

        // シャッターから一定距離以上離れている間は前進
        if (distance > stopDistance)
        {
            // 前方へ移動
            transform.position +=
                Vector3.forward * speed * Time.deltaTime;

            // 移動中は入力を無効化
            shuttergameManager.canInput = false;
        }
        else
        {
            // シャッター前まで到達したら入力を有効化
            shuttergameManager.canInput = true;
        }
    }
}