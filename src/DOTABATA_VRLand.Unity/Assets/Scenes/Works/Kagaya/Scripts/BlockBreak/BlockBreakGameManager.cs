using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockBreakGameManager : MonoBehaviour {
    private BlockBreakUIManager uiManager;

    // スポーン位置リスト
    [SerializeField] private List<Transform> spawnPointList;

    private PlayerData mySelf;
    private BlockBreakPlayerController myPlayerController;

    // 銃のPrefab
    [SerializeField] private GameObject gunPrefab;

    // 現在のターンのプレイヤーId (JoinOrder)
    public int CurrentTurnPlayerId { get; private set; } = 0;

    // 生成したブロックオブジェクト
    private GameObject createdBlockObject;

    private void Awake() {
        RoomModel.I.OnBlockBreakSendedScore += OnBlockBreakSendedScore;

        uiManager = GetComponent<BlockBreakUIManager>();
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnBlockBreakSendedScore -= OnBlockBreakSendedScore;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Start() {
        mySelf = InRoomPlayerData.I.MySelf;
        mySelf.playerObj.transform.position = spawnPointList[mySelf.joinedUser.JoinOrder - 1].position;

        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            // 自分だったら
            if (mySelf.joinedUser.ConnectionId == playerData.Key) {
                BlockBreakPlayerController createdPlayerController = playerData.Value.playerObj.AddComponent<BlockBreakPlayerController>();
                Transform rightHandTransform = playerData.Value.playerObj.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");
                createdPlayerController.gunTransform = Instantiate(gunPrefab, rightHandTransform).transform;
                createdPlayerController.gameManager = this;
                myPlayerController = createdPlayerController;
            }
        }

        // モニターに名前設定
        uiManager.SetPlayerNameToMonitor(InRoomPlayerData.I.PlayerList.Values.Select(_ => _.joinedUser.Name).ToArray());

        // プレイヤー1にターンを移行
        NextTurn();
    }

    private void Update() {

    }

    /// <summary>
    /// スコア送信
    /// </summary>
    public async void SendScore(int score) {
        DestroyBlockObject();
        await RoomModel.I.BlockBreakSendScoreAsync(score);
    }

    /// <summary>
    /// 次のプレイヤーにターンを移行
    /// </summary>
    private void NextTurn() {
        if (CurrentTurnPlayerId < InRoomPlayerData.I.PlayerList.Count) {
            CurrentTurnPlayerId++;
            
            // 自分のターンだったら自分がブロックオブジェクトを生成
            if (myPlayerController.IsMyTurn()) {
                SetBlockObject();
            }
        }
        else {
            EndGame();
        }
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    private void EndGame() {
        Destroy(myPlayerController);
    }

    /// <summary>
    /// ブロックオブジェクトを生成
    /// </summary>
    private void SetBlockObject() {

    }

    /// <summary>
    /// ブロックオブジェクトを削除
    /// </summary>
    private void DestroyBlockObject() {
        Destroy(myPlayerController.gunTransform.gameObject);
        Destroy(myPlayerController);
    }

    /// <summary>
    /// [サーバー通知]
    /// スコア送信通知
    /// </summary>
    public void OnBlockBreakSendedScore(Guid playerConId, int score) {
        uiManager.SetPlayerScoreToMoniter(InRoomPlayerData.I.PlayerList[playerConId].joinedUser.JoinOrder, score);
        if (mySelf.joinedUser.ConnectionId == playerConId) {
            myPlayerController.SetMyScore(score);
        }
        NextTurn();
    }
}
