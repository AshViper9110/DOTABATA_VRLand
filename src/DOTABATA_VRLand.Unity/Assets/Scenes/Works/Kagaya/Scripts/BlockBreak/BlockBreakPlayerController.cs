using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class BlockBreakPlayerController : MonoBehaviour {
    public BlockBreakGameManager gameManager;

    // 手の位置
    private Transform leftHantTransform;
    private Transform rightHantTransform;

    // Pointer
    private Transform pointer;

    public Transform gunTransform;

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
    [SerializeField] private GameObject bulletPrefab;
    // Pointer
    [SerializeField] private GameObject pointerPrefab;

    // ----------
    // ゲーム
    // ----------

    // プレイヤーId (JoinOrder)
    private int myPlayerId;

    // 自分のスコア
    public int myScore {  get; private set; }

    private void Start() {
        myPlayerId = InRoomPlayerData.I.MySelf.joinedUser.JoinOrder;

        pointer = Instantiate(pointerPrefab).transform;
        pointer.gameObject.SetActive(false);

        leftHantTransform = this.gameObject.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "LeftHand");
        rightHantTransform =this.gameObject.GetComponentsInChildren<Transform>().First(_ => _.transform.name == "RightHand");

        leftHandType = SteamVR_Input_Sources.LeftHand;
        rightHandType = SteamVR_Input_Sources.RightHand;

        interactUiAction = SteamVR_Actions.default_InteractUI;
        grabGripAction = SteamVR_Actions.default_GrabGrip;
    }

    private void Update() {
        if (!IsMyTurn()) return;
        ShotBullet();
        MovePointer();
    }

    /// <summary>
    /// 自分のターンかどうか
    /// </summary>
    /// <returns></returns>
    public bool IsMyTurn() {
        return gameManager.CurrentTurnPlayerId == myPlayerId; 
    } 

    /// <summary>
    /// 弾を打つ
    /// </summary>
    private async void ShotBullet() {
        if (!interactUiAction.GetStateDown(rightHandType)) return;
        SyncObject bulletSyncObject = Instantiate(bulletPrefab).GetComponent<SyncObject>();
        await UniTask.WaitUntil(() => bulletSyncObject.Initialized == true);

        Rigidbody bulletRb = bulletSyncObject.GetComponent<Rigidbody>();
        bulletRb.AddForce(rightHantTransform.forward.normalized * 50f, ForceMode.Impulse);
    }

    /// <summary>
    /// ポインター移動
    /// </summary>
    private void MovePointer() {
        pointer.position = rightHantTransform.forward.normalized * 5f;
        pointer.rotation = Quaternion.LookRotation(Camera.main.transform.position);
    }

    /// <summary>
    /// スコアをセット
    /// </summary>
    public void SetMyScore(int score) {
        myScore = score;
    }
}
