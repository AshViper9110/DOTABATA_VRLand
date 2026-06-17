using UnityEngine;

public class PlayerLane : MonoBehaviour
{
    // 自分が担当するレーン番号（0～3）
    // 他のスクリプトからも参照できるようにstaticで保持
    public static int MyLane = 0;

    [Header("テスト用")]

    // true：Inspectorで指定したレーンを使用
    // false：ルーム参加順からレーンを決定
    public bool useTestLane = true;

    // テスト時に使用するレーン番号
    public int testLane = 0;

    void Start()
    {
        // テスト時はInspectorで設定したレーンを使用
        if (useTestLane)
        {
            MyLane = testLane;
        }
        else
        {
            // マルチプレイ時はルーム参加順からレーン番号を決定
            // JoinOrder(1～4)を配列用に0～3へ変換
            MyLane =
                InRoomPlayerData.I.MySelf.joinedUser.JoinOrder - 1;
        }

        // 終わったら消してOK
        Debug.Log($"自分のレーン：{MyLane}");
    }
}