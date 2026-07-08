using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class MinigameScript : MonoBehaviour
{

    [SerializeField] MinigameFlowController controller;　//ミニゲームのネットワーク
    [SerializeField] private List<Transform> playerPos = new List<Transform>(); //Playerの初期位置
    private bool isClear;　//ミニゲームのクリア判定

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction += StartCountdown;
    }

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCountdownAction -= StartCountdown;
    }

    void Start()
    {
        AudioManager.StopBgm(); ;

        //シーン移行後の位置配置
        var myId = NetworkManager.I.myConnectionId;
        int index = InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;
        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position = playerPos[index].position;
        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.rotation = playerPos[index].rotation;
    }

    // ゲームループの実装
    void FixedUpdate()
    {
        if (controller.isGameStarted)
        {
            //この中にゲームループを実装

            if (!isClear)
            {
                controller.isGameStarted = false;
                Debug.Log("GameClear");
                isClear = true;
                AudioManager.PlaySE(AudioManager.SE.Bank_Open);
                //SendGameClear(int);
                return;
            }
        }
    }

    //クライアントがクリアした際に通知する関数
    //スコアが大きい人が上位になる。
    void SendGameClear(int score)
    {
        controller.OnSendScore(score);
    }
    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(AudioManager.BGM.Bank);
        }
    }
}
