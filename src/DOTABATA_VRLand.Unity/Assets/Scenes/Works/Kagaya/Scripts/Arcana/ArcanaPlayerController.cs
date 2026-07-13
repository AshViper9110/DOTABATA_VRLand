using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;
using Valve.VR;
using static GestureRecognizer;

public class ArcanaPlayerController : MonoBehaviour {
    private SyncPlayer syncPlayer;
    private PlayerStatus playerStatus;

    // 魔法を当てるターゲット
    private Transform targetPlayer;
    private LayerMask targetLayerMask = -1;
    private GameObject targetCircle;
    private GameObject createdTargetCircle;

    // 魔法オブジェクト
    private GameObject myMagicObj;
    // 魔法人
    private GameObject magicCircle;

    // 右手
    private Transform rightHand;

    // 絵描き板
    private GameObject drawBoadObj;

    // Input
    // HandType
    private SteamVR_Input_Sources leftHandType;
    private SteamVR_Input_Sources rightHandType;

    // 絵描き用
    private SteamVR_Action_Boolean drawBoadAction;

    // ターゲット選定
    private SteamVR_Action_Boolean grabGripAction;

    // 右手のうで振り
    private SteamVR_Behaviour_Pose controllerPose;

    // 魔法を撃つ力
    [SerializeField] private float shotPower = 5f;

    private void Start() {
        syncPlayer = GetComponent<SyncPlayer>();
        playerStatus = GetComponent<PlayerStatus>();

        grabGripAction = SteamVR_Actions.default_GrabGrip;
        rightHandType = SteamVR_Input_Sources.RightHand;
        drawBoadAction = SteamVR_Actions.default_InteractUI;
        leftHandType = SteamVR_Input_Sources.LeftHand;
        targetLayerMask = targetLayerMask.Remove("MySelf");
    }

    private void Update() {
        SelectTarget();
        TrackingHand();
        TrackingTargetPlayer();
        ShotMagic();
        SwitchDrawBoadActive();
        UseShield();
    }

    /// <summary>
    /// フィールド設定
    /// </summary>
    public void SetField(Transform rHand, GameObject obj, GameObject targetCircle) {
        this.drawBoadObj = obj;
        this.rightHand = rHand;
        controllerPose = rightHand.GetComponent<SteamVR_Behaviour_Pose>();
        this.targetCircle = targetCircle;
    }

    /// <summary>
    /// ターゲット選定
    /// </summary>
    private void SelectTarget() {
        if (!grabGripAction.GetState(rightHandType)) return;
        RaycastHit[] hits = Physics.RaycastAll(rightHand.position, Camera.main.transform.forward, 50f, targetLayerMask);
        hits = hits.OrderBy(hit => hit.distance).ToArray();
        foreach (RaycastHit hit in hits) {
            if (!hit.collider.gameObject.CompareTag("Player")) return;
            targetPlayer = hit.collider.transform;
            Debug.Log($"ターゲット選定：{hit.collider.gameObject.name}");
        }
    }

    /// <summary>
    /// ターゲットに追従
    /// </summary>
    private void TrackingTargetPlayer() {
        if (targetPlayer) {
            if (!createdTargetCircle) {
                createdTargetCircle = Instantiate(targetCircle);
            }
            createdTargetCircle.transform.position = targetPlayer.transform.position;
        }
        else {
            Destroy(createdTargetCircle);
        }
    }

    /// <summary>
    /// 魔法のオブジェクトをセット
    /// </summary>
    public void SetMagicObj(GameObject magicObject, GestureClass gesture, GameObject magicCircleObj) {
        if (!magicObject) return;
        if (myMagicObj) return;

        myMagicObj = magicObject;
        myMagicObj.GetComponent<MagicController>().Init(RoomModel.I.ConnectionId, gesture);

        if (magicCircle) {
            Destroy(magicCircle);
        }
        magicCircle = Instantiate(magicCircleObj, (rightHand.position + -rightHand.right), Quaternion.identity);
    }

    /// <summary>
    /// 魔法を撃つ
    /// </summary>
    private void ShotMagic() {
        if (!myMagicObj) return;

        Vector3 velocity = controllerPose.GetVelocity();

        if (velocity.magnitude > 1.7f) {
            Debug.Log("腕振り検知");
            Rigidbody magicObjRb = myMagicObj.GetComponent<Rigidbody>();
            myMagicObj = null;
            magicObjRb.linearVelocity = Vector3.zero;
            magicObjRb.AddForce(Camera.main.transform.forward.normalized * shotPower, ForceMode.Impulse);
            magicObjRb.GetComponent<MagicController>().SetTarget(targetPlayer);
            magicObjRb.GetComponent<MagicController>().ReleaseHand();

            Destroy(magicCircle);
            magicCircle = null;
        }
    }

    /// <summary>
    /// 魔法を手に追従
    /// </summary>
    private void TrackingHand() {

        if (!myMagicObj) return;
        // 追従
        myMagicObj.transform.position = rightHand.position + -rightHand.right * 0.2f + -rightHand.up * 0.02f + -rightHand.forward * 0.07f;
        // 回転
        myMagicObj.transform.Rotate(Time.deltaTime * 10, Time.deltaTime * 4, Time.deltaTime * 10);

        if (!magicCircle) return;
        // 追従
        magicCircle.transform.position = rightHand.position + -rightHand.right * 0.02f + -rightHand.up * 0.02f + -rightHand.forward * 0.07f;
        // 向き
        magicCircle.transform.up = -rightHand.right;
    }

    /// <summary>
    /// DrawBoadのアクティブ変更
    /// </summary>
    private void SwitchDrawBoadActive() {
        if (drawBoadAction.GetStateDown(leftHandType) && drawBoadObj) {
            drawBoadObj.SetActive(!drawBoadObj.activeSelf);
            RoomModel.I.SwitchDrawBoadActiveAsync(drawBoadObj.activeSelf).Forget();
        }
    }

    /// <summary>
    /// シールドを使う
    /// </summary>
    private void UseShield() {
        if (!grabGripAction.GetStateDown(leftHandType)) return;
        playerStatus.EnableShield();
    }
}
