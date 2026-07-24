using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class BlockBreakPlayerController : MonoBehaviour {
    public BlockBreakGameManager gameManager;
    public BlockBreakUIManager uIManager;

    private SyncPlayer syncPlayer;

    // 手の位置
    private Transform leftHandTransform;
    private Transform rightHandTransform;

    // Pointer
    private Transform pointer;

    // Gun
    public Transform gunTransform;
    private Transform shotPos;

    // 弾の親
    private Transform bulletParent;

    // ----------
    // Input
    // ----------
    
    // HandType
    private SteamVR_Input_Sources leftHandType;
    private SteamVR_Input_Sources rightHandType;

    // InteractUI
    private SteamVR_Action_Boolean interactUiAction;
    // GrabGrip
    private SteamVR_Action_Boolean grabGripAction;

    // ----------
    // Prefab
    // ----------

    // Bullet
    private GameObject bulletPrefab;
    // Pointer
    private GameObject pointerPrefab;

    // ----------
    // ゲーム
    // ----------

    // スコアを送るまでのじかん
    private float waitTime = 5;

    // プレイヤーId (JoinOrder)
    private int myPlayerId;

    // 自分の合計スコア
    public int MyTotalScore { get; private set; } = 0;
    // 自分のターンのスコア
    private int myTurnScore = 0;

    // 弾を撃ったか
    private bool isShot = false;

    // 弾を撃てるか
    public bool canShot = false;

    private void Start() {
        syncPlayer = this.GetComponent<SyncPlayer>();
        if (!syncPlayer.IsOwner) return;

        myPlayerId = InRoomPlayerData.I.MySelf.joinedUser.JoinOrder;

        bulletParent = GameObject.Find("Bullets").transform;

        leftHandTransform = this.gameObject.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "LeftHand");
        rightHandTransform =this.gameObject.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");

        leftHandType = SteamVR_Input_Sources.LeftHand;
        rightHandType = SteamVR_Input_Sources.RightHand;

        interactUiAction = SteamVR_Actions.default_InteractUI;
        grabGripAction = SteamVR_Actions.default_GrabGrip;

        shotPos = gunTransform.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "ShotPos");

        pointer = Instantiate(pointerPrefab).transform;
    }

    private void Update() {
        if (!syncPlayer.IsOwner) return;
        MovePointer();
        if (!IsMyTurn()) return;
        ShotBullet();
    }

    /// <summary>
    /// 初期化用データセット
    /// </summary>
    public void SetInitiarizeData(InitializeDataSO iData) {
        bulletPrefab = iData.datas["bulletPrefab"];
        pointerPrefab = iData.datas["pointerPrefab"];
    }

    /// <summary>
    /// 自分のターンかどうか
    /// </summary>
    public bool IsMyTurn() {
        bool isMyTurn = gameManager.CurrentTurnPlayerId == myPlayerId;
        if (pointer) {
            pointer.GetComponent<BlockBreakPointerController>().SwitchShowHide(isMyTurn);
        }
        return isMyTurn;
    } 

    /// <summary>
    /// 弾を打つ
    /// </summary>
    private async void ShotBullet() {
        if (!canShot) return;
        if (isShot) return;
        if (!interactUiAction.GetStateDown(rightHandType)) return;
        isShot = true;
        canShot = false;

        SyncObject bulletSyncObject = Instantiate(bulletPrefab, shotPos.position, Quaternion.identity, bulletParent).GetComponent<SyncObject>();
        await UniTask.WaitUntil(() => bulletSyncObject.Initialized == true);

        Rigidbody bulletRb = bulletSyncObject.GetComponent<Rigidbody>();
        bulletRb.AddForce(shotPos.forward.normalized * 30f, ForceMode.Impulse);

        uIManager.StartTimer(waitTime);

        await UniTask.WaitForSeconds(waitTime);

        uIManager.StopTimer();

        TurnEnd();
    }

    /// <summary>
    /// ポインター移動
    /// </summary>
    private void MovePointer() {
        pointer.position = shotPos.position + shotPos.forward.normalized * 7f;
    }

    /// <summary>
    /// スコア獲得
    /// </summary>
    public void AddScore() {
        myTurnScore++;
    }

    /// <summary>
    /// スコアをセット
    /// </summary>
    public void SetMyScore(int score) {
        MyTotalScore += score;
    }

    /// <summary>
    /// ターンの終了
    /// </summary>
    public void TurnEnd() {
        gameManager.SendScore(myTurnScore);
        myTurnScore = 0;
        isShot = false;
    }
}
