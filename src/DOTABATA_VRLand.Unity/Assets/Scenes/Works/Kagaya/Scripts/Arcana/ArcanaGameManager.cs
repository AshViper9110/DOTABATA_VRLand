using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using static GestureRecognizer;

public class ArcanaGameManager : MonoBehaviour {
    [SerializeField] private GestureRecognizer gestureRecognizer;

    private PlayerData mySelf;
    private Transform rightHand;

    // スポーツ位置
    [SerializeField] private List<Transform> spawnPoints;

    // プレイヤーのUI
    [SerializeField] private GameObject playerUICanvas;

    // 魔法のPrefabList
    [SerializeField] private GameObject magicBallPrefab;
    // 魔法のVFX
    [SerializeField] private List<GameObject> magicVFXList;
    // 魔法のマテリアル
    [SerializeField] private List<Material> magicMaterialList;

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
    private ArcanaPlayerController myController;

    // playerのカメラUI
    [SerializeField] private GameObject playerCameraUI;

    [SerializeField] private GameObject drawPointer;
    [SerializeField] private Material lineMaterial;

    // 判定VFX
    [SerializeField] private GameObject recognizeVFX;

    private void Awake() {
        gestureRecognizer.CompleteRecognize += CreateMagic;
    }

    private async void Start() {
        RoomModel.I.OnDead += OnDead;
        RoomModel.I.OnArcanaGameSeted += OnArcanaGameSeted;

        AudioManager.StopBgm();
        SteamVR_Fade.View(new Color(0, 0, 0, 0), 1.0f);

        // 自分のインスタンスを保持
        mySelf = InRoomPlayerData.I.MySelf;
        if (mySelf.joinedUser.JoinOrder == 1) {
            // サーバーのArcanaContextを初期化
            await RoomModel.I.ArcanaInitGameAsync();
        }

        // 自身にScriptを付与
        myController = mySelf.playerObj.AddComponent<ArcanaPlayerController>();

        // 全員にCanvasとオブジェクトを配置してScriptを付与
        foreach (PlayerData playerData in InRoomPlayerData.I.PlayerList.Values) {
            GameObject myUI = Instantiate(playerUICanvas, playerData.playerObj.transform);
            GameObject pCamUI = Instantiate(playerCameraUI, playerData.playerObj.transform);
            GameObject drawBoadObj = pCamUI.transform.GetChild(0).gameObject;

            // 自分だったら
            if (playerData.joinedUser.ConnectionId == mySelf.joinedUser.ConnectionId) {
                drawBoadObj.layer = LayerMask.NameToLayer("DrawBoad");
                myUI.layer = LayerMask.NameToLayer("MyUI");
                rightHand = mySelf.playerObj.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");
                myController.SetField(rightHand, drawBoadObj, targetCircle);
                DrawVRPointer drawVR = rightHand.AddComponent<DrawVRPointer>();
                drawVR.SetField(drawBoadObj, drawPointer, lineMaterial, recognizeVFX);
            }

            GameObject createdShield = Instantiate(shieldEffect, playerData.playerObj.transform.position, Quaternion.identity, playerData.playerObj.transform);
            createdShield.SetActive(false);
            playerData.playerObj.AddComponent<PlayerStatus>().SetField(this, createdShield, pCamUI.GetComponentsInChildren<Slider>().First(_=>_.gameObject.name == "ShieldInfoSlider"));
            playerData.playerObj.AddComponent<SyncDrawBoad>().SetField(drawBoadObj);
        }

        // スポーン位置に移動
        mySelf.playerObj.transform.position = spawnPoints[mySelf.joinedUser.JoinOrder - 1].position;
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

    private void Update() {
        
    }

    /// <summary>
    /// 魔法生成
    /// </summary>
    private async void CreateMagic(GestureClass gesture, float score) {
        // 魔法生成
        Transform createdTransform = Instantiate(magicBallPrefab, new Vector3(0, 10, 0), Quaternion.identity).transform;
        int rnd = UnityEngine.Random.Range(0, magicVFXList.Count);
        // VFX生成
        Instantiate(magicVFXList[rnd], createdTransform);
        // Material適応
        createdTransform.GetComponent<MeshRenderer>().material = magicMaterialList[rnd];

        await UniTask.DelayFrame(2);

        // オブジェクトId取得
        Guid objectId = createdTransform.GetComponent<SyncObject>().ObjectId;

        // オブジェクトのフィールド同期
        await RoomModel.I.SyncMagicBallAsync(objectId, gesture.ToString());

        myController.SetMagicObj(createdTransform.gameObject, gesture, handMagicCircleList[rnd]);
    }

    /// <summary>
    /// 志望動機
    /// </summary>
    public async void DeathAsync() {
        // VFX
        Instantiate(deathEffect, mySelf.playerObj.transform.position, Quaternion.identity);
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
        Debug.Log($"勝者は{InRoomPlayerData.I.PlayerList[winnerConId].joinedUser.Name}");

        Destroy(myController);
        Destroy(rightHand.GetComponent<DrawVRPointer>());

        foreach (PlayerData playerData in InRoomPlayerData.I.PlayerList.Values) {
            foreach (Transform child in playerData.playerObj.transform) {
                if (child.gameObject.name.StartsWith("ArcanaUICanvas") ||
                    child.gameObject.name.StartsWith("DrawBoad")) {
                    Destroy(child);
                }
            }
            Destroy(playerData.playerObj.GetComponent<PlayerStatus>());
            Destroy(playerData.playerObj.GetComponent<SyncDrawBoad>());
        }
    }
}
