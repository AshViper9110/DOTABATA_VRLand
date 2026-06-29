using System.Collections;
using UnityEngine;

public class LaneManager : MonoBehaviour
{
    // 各レーンのスポーン位置
    public Transform[] spawnPoints;

    [Header("テスト用")]
    // true：Inspectorで指定したレーンを使用
    // false：ルーム参加順からレーンを決定
    public bool useTestLane = true;

    // テスト時に使用するレーン番号（0～3）
    public int testLane = 0;

    IEnumerator Start()
    {
        int lane;

        // テスト時はInspectorで設定したレーンを使用
        if (useTestLane)
        {
            lane = testLane;
        }
        else
        {
            // ルーム情報の取得完了まで待機
            yield return new WaitUntil(() =>
                InRoomPlayerData.I.MySelf != null);

            // JoinOrder(1～4)を配列用に0～3へ変換
            lane =
                InRoomPlayerData.I.MySelf.joinedUser.JoinOrder - 1;
        }

        Debug.Log($"レーン番号：{lane}");

        // レーン番号が範囲外なら終了
        if (lane < 0 || lane >= spawnPoints.Length)
        {
            Debug.LogError("レーン番号が不正です。");
            yield break;
        }

        // プレイヤー取得待ち
        yield return new WaitUntil(() =>
            InRoomPlayerData.I.MySelf != null &&
            InRoomPlayerData.I.MySelf.playerObj != null);

        // レーンへ移動
        InRoomPlayerData.I.MySelf.playerObj.transform.position =
            spawnPoints[lane].position;
    }
}