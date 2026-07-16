using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;

public class BlockBreakGameManager : MonoBehaviour {
    [SerializeField] private MinigameFlowController minigameFlowController;

    private BlockBreakUIManager uiManager;
    [SerializeField] private BlockBreakBlockObjectsManager objectsManager;
    [SerializeField] private ColliderChecker detectionCollider;

    // スポーン位置リスト
    [SerializeField] private List<Transform> spawnPointList;

    private PlayerData mySelf;
    private BlockBreakPlayerController myPlayerController;
    private Dictionary<Guid, BlockBreakPlayerController> playerControllerList = new Dictionary<Guid, BlockBreakPlayerController>();

    // スクリプトに渡すデータ
    [SerializeField] private SerializableDictionary<string, InitializeDataSO> initialzeDatas;

    // 銃のPrefab
    [SerializeField] private GameObject gunPrefab;

    // スコア表示用UI
    [SerializeField] private GameObject scoreUI;

    // 現在のターンのプレイヤーId (JoinOrder)
    public int CurrentTurnPlayerId { get; private set; } = 0;

    // 生成したブロックオブジェクト
    private GameObject createdBlockObject;

    // プレイヤーが持ってるオブジェクト
    public Dictionary<Guid, PlayerHavingObject> playerHavingObjectList = new Dictionary<Guid, PlayerHavingObject>();

    // 初期化終了人数
    public int initializedPlayer = 0;

    // 現在のラウンド数
    public int currentRound = 1;

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

    private async void Start() {
        AudioManager.StopBgm();
        SteamVR_Fade.View(new Color(0, 0, 0, 0), 1.0f);

        await UniTask.WaitUntil(() => minigameFlowController.isGameStarted == true);

        // コライダーのイベント設定
        detectionCollider.onColliderEnter.AddListener(OnEnterWallCollider);

        mySelf = InRoomPlayerData.I.MySelf;
        mySelf.playerObj.transform.position = spawnPointList[mySelf.joinedUser.JoinOrder - 1].position;

        scoreUI.SetActive(true);
        scoreUI.transform.parent = mySelf.playerObj.transform;
        scoreUI.transform.localPosition = new Vector3(0, 1.3f, 1);

        // 全員
        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            Transform rightHandTransform = playerData.Value.playerObj.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");
            Transform createdGunT = Instantiate(gunPrefab, rightHandTransform).transform;

            // 保持
            playerHavingObjectList[playerData.Key] = new PlayerHavingObject() {
                objects = {
                    { "gunObj", createdGunT.gameObject },
                    { "Pointer", null },
                }
            };

            // プレイヤーコントローラー付与
            BlockBreakPlayerController createdPlayerController = playerData.Value.playerObj.AddComponent<BlockBreakPlayerController>();
            playerControllerList[playerData.Key] = createdPlayerController;

            // 自分だったら
            if (mySelf.joinedUser.ConnectionId == playerData.Key) {
                createdPlayerController.SetInitiarizeData(initialzeDatas["BlockBreakPlayerController"]);
                createdPlayerController.gameManager = this;
                createdPlayerController.uIManager = uiManager;
                createdPlayerController.gunTransform = createdGunT;
                myPlayerController = createdPlayerController;
            }
            // 自分以外
            else {
                createdGunT.localPosition = new Vector3(-0.04f, 0.8f, 1.3f);
                createdGunT.localScale = new Vector3(5, 5, 5);
            }
        }

        // モニターに名前設定
        uiManager.SetPlayerNameText(InRoomPlayerData.I.PlayerList.Values.Select(_ => _.joinedUser.Name).ToArray());

        await UniTask.WaitUntil(() => initializedPlayer == InRoomPlayerData.I.PlayerList.Count);

        // プレイヤー1にターンを移行
        NextTurn();
    }

    private void Update() {

    }

    /// <summary>
    /// スコア送信
    /// </summary>
    public async void SendScore(int score) {
        await RoomModel.I.BlockBreakSendScoreAsync(score);
    }

    /// <summary>
    /// 外の壁に当たったらスコア獲得
    /// </summary>
    public void OnEnterWallCollider(Collision collision) {
        if (!myPlayerController.IsMyTurn()){
            return;
        }

        if (collision.gameObject.CompareTag("BBBullet")) {
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag ("BBBlock")) {
            myPlayerController.AddScore();
            Destroy(collision.gameObject);
        }
    }

    /// <summary>
    /// 次のプレイヤーにターンを移行
    /// </summary>
    private async void NextTurn() {
        if (CurrentTurnPlayerId < InRoomPlayerData.I.PlayerList.Count) {
            CurrentTurnPlayerId++;

            // 現在のターンのプレイヤー名の表示
            uiManager.SetCurrentTurnPlayerName(InRoomPlayerData.I.PlayerList[GetPlayerConIdFromId(CurrentTurnPlayerId)].joinedUser.Name);

            // ポインター制御
            foreach (var playerData in InRoomPlayerData.I.PlayerList) {
                GameObject pointerObj = (GameObject)playerHavingObjectList[playerData.Key].objects["Pointer"];
                BlockBreakPointerController pointerCon = pointerObj.GetComponent<BlockBreakPointerController>();
                pointerCon.Hide();

                // ターンのプレイヤーのポインターを表示
                if (playerData.Value.joinedUser.JoinOrder == CurrentTurnPlayerId) {
                    pointerCon.Show();
                }
            }
            
            // 自分のターンだったら自分がブロックオブジェクトを生成
            if (myPlayerController.IsMyTurn()) {
                await objectsManager.SetObjects();
                myPlayerController.canShot = true;
            }
        }
        else if (currentRound < 3) {
            currentRound++;
            CurrentTurnPlayerId = 0;
            uiManager.UpdateRoundText(currentRound);
            NextTurn();
        }
        else {
            EndGame();
        }
    }

    /// <summary>
    /// ゲーム終了
    /// </summary>
    private void EndGame() {
        Debug.Log("ゲーム終了");
        Destroy(scoreUI);
        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            Destroy((GameObject)playerHavingObjectList[playerData.Key].objects["gunObj"]);
            Destroy(playerControllerList[playerData.Key]);
        }

        minigameFlowController.OnSendScore(myPlayerController.MyTotalScore);
    }

    /// <summary>
    /// [サーバー通知]
    /// スコア送信通知
    /// </summary>
    private void OnBlockBreakSendedScore(Guid playerConId, int score) {
        BlockBreakPlayerController pCon = playerControllerList[playerConId];
        pCon.SetMyScore(score);
        uiManager.SetPlayerScoreText(GetPlayerIdFromConId(playerConId), pCon.MyTotalScore);
        
        NextTurn();
    }

    /// <summary>
    /// プレイヤーId --> ConnectionId
    /// </summary>
    public Guid GetPlayerConIdFromId(int id) {
        return InRoomPlayerData.I.PlayerList.First(_ => _.Value.joinedUser.JoinOrder == id).Key;
    }

    /// <summary>
    /// ConnectionId --> プレイヤーId
    /// </summary>
    public int GetPlayerIdFromConId(Guid conId) {
        return InRoomPlayerData.I.PlayerList[conId].joinedUser.JoinOrder;
    }
}
