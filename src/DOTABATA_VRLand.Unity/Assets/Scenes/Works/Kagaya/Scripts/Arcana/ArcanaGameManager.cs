using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using Newtonsoft.Json;
using PDollarGestureRecognizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static GestureRecognizer;

public class ArcanaGameManager : MonoBehaviour {
    [SerializeField] private MinigameFlowController minigameFlowController;
    [SerializeField] private GestureRecognizer gestureRecognizer;

    private PlayerData mySelf;
    private Transform rightHand;

    // 中心
    [SerializeField] private Transform centerTransform;
    // スポーツ位置
    [SerializeField] private List<Transform> spawnPoints;
    // 最初のスポーン位置
    [SerializeField] private Transform firstSpawnPoint;

    // プレイヤーのUI
    [SerializeField] private GameObject playerUICanvas;

    // 魔法のPrefabList
    [SerializeField] private GameObject magicBallPrefab;
    // 魔法のVFX
    [SerializeField] public List<GameObject> magicVFXList;
    // 魔法のマテリアル
    [SerializeField] public List<Material> magicMaterialList;

    // 手用の魔法人のPrefab
    [SerializeField] private List<GameObject> handMagicCircleList;
    // ターゲットにつける魔法人
    [SerializeField] private GameObject targetCircle;

    // シールドのエフェクト
    [SerializeField] private GameObject shieldEffect;

    // 死亡時のエッフェクト
    [SerializeField] private GameObject deathEffect;

    // 本のオブジェクトリスト
    [SerializeField] private List<GameObject> magicBookObjects;
    // 杖のオブジェクトリスト
    [SerializeField] private List<GameObject> magicStaffObjects;

    // 自分のコントロ
    public ArcanaPlayerController myController {  get; private set; }

    // playerのカメラUI
    [SerializeField] private GameObject playerCameraUI;

    [SerializeField] private GameObject drawPointer;
    [SerializeField] private Material lineMaterial;

    // 判定VFX
    [SerializeField] private GameObject recognizeVFX;

    // プレイヤーが持ってるオブジェクト
    private Dictionary<Guid, PlayerHavingObject> playerObjectList = new Dictionary<Guid, PlayerHavingObject>();

    private bool completeStart = false;

    private int gameTimer = 0;

    private bool gameEnd = false;

    private void Awake() {
        gestureRecognizer.CompleteRecognize += CreateMagic;
    }

    private async void Start() {
        // 自分のインスタンスを保持
        mySelf = InRoomPlayerData.I.MySelf;
        if (mySelf.joinedUser.JoinOrder == 1) {
            // サーバーのArcanaContextを初期化
            await RoomModel.I.ArcanaInitGameAsync();
        }
        //mySelf.playerObj.transform.position = firstSpawnPoint.position;

        RoomModel.I.OnDead += OnDead;
        RoomModel.I.OnArcanaGameSeted += OnArcanaGameSeted;

        AudioManager.StopBgm();

        // スポーン位置に移動
        mySelf.playerObj.transform.position = spawnPoints[mySelf.joinedUser.JoinOrder - 1].position;

        Player player = Player.instance;
        player.transform.LookAt(centerTransform);
        player.transform.eulerAngles = new Vector3(0, player.transform.eulerAngles.y, 0);

        await UniTask.WaitUntil(() => minigameFlowController.isGameStarted == true);

        await UniTask.DelayFrame(5);

        // 自身にScriptを付与
        myController = mySelf.playerObj.AddComponent<ArcanaPlayerController>();

        // 全員にCanvasとオブジェクトを配置してScriptを付与
        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            GameObject myUI = Instantiate(playerUICanvas, playerData.Value.playerObj.transform);
            GameObject pCamUI = Instantiate(playerCameraUI, playerData.Value.playerObj.transform);
            GameObject drawBoadObj = pCamUI.transform.GetChild(0).gameObject;
            GameObject createdShield = Instantiate(shieldEffect, playerData.Value.playerObj.transform);

            createdShield.SetActive(false);
            PlayerStatus playerStatus = playerData.Value.playerObj.AddComponent<PlayerStatus>();
            playerStatus.SetField(this, createdShield, pCamUI.GetComponentsInChildren<Slider>().First(_ => _.gameObject.name == "ShieldInfoSlider"));
            playerData.Value.playerObj.AddComponent<SyncDrawBoad>().SetField(drawBoadObj);

            // 保持
            playerObjectList[playerData.Key] = new PlayerHavingObject() {
                objects = {
                    { "playerUICanvas", myUI },
                    { "playerCameraUI", pCamUI },
                    { "shieldEffect", createdShield },
                }
            };

            // 自分だったら
            if (playerData.Value.joinedUser.ConnectionId == mySelf.joinedUser.ConnectionId) {
                drawBoadObj.layer = LayerMask.NameToLayer("DrawBoad");
                Destroy(myUI);
                rightHand = mySelf.playerObj.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");
                myController.SetField(rightHand, drawBoadObj, targetCircle);
                DrawVRPointer drawVR = rightHand.AddComponent<DrawVRPointer>();
                drawVR.SetField(drawBoadObj, drawPointer, lineMaterial, recognizeVFX, playerStatus);

                gestureRecognizer.resultText = pCamUI.GetComponentsInChildren<TextMeshProUGUI>().First(_=>_.gameObject.name == "RecognizeResultText");
            }
        }

        completeStart = true;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnDead -= OnDead;
            RoomModel.I.OnArcanaGameSeted -= OnArcanaGameSeted;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private async void Update() {
        if (gameEnd) return;
        if (!minigameFlowController.isGameStarted) return;
        if (!completeStart) return;

        gameTimer++;

        // オブジェクトの位置固定
        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            if (!playerData.Value.playerObj.activeSelf) return;

            if ((GameObject)playerObjectList[playerData.Key].objects["playerCameraUI"] is GameObject playerCameraUI) {
                if (!playerCameraUI) return;
                playerCameraUI.transform.localPosition = this.playerCameraUI.transform.position;
            }
            if ((GameObject)playerObjectList[playerData.Key].objects["playerUICanvas"] is GameObject playerUICanvas) {
                if (!playerUICanvas) return;
                playerUICanvas.transform.localPosition = this.playerUICanvas.transform.position;
            }
            if ((GameObject)playerObjectList[playerData.Key].objects["shieldEffect"] is GameObject shieldEffect) {
                if (!shieldEffect) return;
                shieldEffect.transform.localPosition = this.shieldEffect.transform.position;
            }
        }
    }

    /// <summary>
    /// 魔法生成
    /// </summary>
    private async void CreateMagic(GestureClass gesture, Result result) {
        // 手に魔法を持ってたら何もしない
        if (myController.myMagicObj) return;

        // 魔法生成
        Transform createdTransform = Instantiate(magicBallPrefab, new Vector3(0, 10, 0), Quaternion.identity).transform;
        int rnd = UnityEngine.Random.Range(0, magicVFXList.Count);
        // VFX生成
        Instantiate(magicVFXList[rnd], createdTransform);
        // Material適応
        createdTransform.GetComponent<MeshRenderer>().material = magicMaterialList[rnd];

        SyncObject syncObj = createdTransform.GetComponent<SyncObject>();

        await UniTask.WaitUntil(()=> syncObj.Initialized);

        // オブジェクトId取得
        Guid objectId = syncObj.ObjectId;

        Debug.Log($"魔法オブジェクトのフィールド同期送信\n" +
            $"ObjId：{objectId}\n" +
            $"Gesture：{gesture.ToString()}\n" +
            $"Rnd：{rnd}");

        // オブジェクトのフィールド同期
        await RoomModel.I.SyncMagicBallAsync(objectId, gesture.ToString(), rnd);

        myController.SetMagicObj(createdTransform.gameObject, gesture, handMagicCircleList[rnd]);
    }

    /// <summary>
    /// 志望動機
    /// </summary>
    public async void DeathAsync() {
        // VFX
        Instantiate(deathEffect, mySelf.playerObj.transform.position, Quaternion.identity);
        // minigameFlowController.OnSendScore(gameTimer);
        minigameFlowController.OnSendTimeLastWin();
        myController.Dead();
        await RoomModel.I.DeathAsync();
    }

    /// <summary>
    /// [サーバー通知]
    /// 死亡通知
    /// </summary>
    public void OnDead(Guid connectionId) {
        InRoomPlayerData.I.PlayerList[connectionId].playerObj.SetActive(false);
    }

    /// <summary>
    /// [サーバー通知]
    /// アルカナスケッチのゲーム終了
    /// </summary>
    public void OnArcanaGameSeted(Guid winnerConId) {
        gameEnd = true;
        Debug.Log($"勝者は{InRoomPlayerData.I.PlayerList[winnerConId].joinedUser.Name}");

        Destroy(myController);
        Destroy(rightHand.GetComponent<DrawVRPointer>());

        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            Destroy((GameObject)playerObjectList[playerData.Key].objects["playerUICanvas"]);
            Destroy((GameObject)playerObjectList[playerData.Key].objects["playerCameraUI"]);
            Destroy((GameObject)playerObjectList[playerData.Key].objects["shieldEffect"]);

            Destroy(playerData.Value.playerObj.GetComponent<PlayerStatus>());
            Destroy(playerData.Value.playerObj.GetComponent<SyncDrawBoad>());

            playerData.Value.playerObj.SetActive(true);
        }

        if (mySelf.joinedUser.ConnectionId == winnerConId) {
            // minigameFlowController.OnSendScore(gameTimer + 100);
            minigameFlowController.OnSendTimeLastWin();
        }
    }
}
