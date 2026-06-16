using DG.Tweening;
using PDollarGestureRecognizer;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static GestureRecognizer;

public class ArcanaPlayerController : MonoBehaviour {
    // 魔法オブジェクト
    private GameObject myMagicObj;
    // 魔法人
    private GameObject magicCircle;

    // 右手
    private Transform rightHand;


    // 絵描き板
    private GameObject drawBoadObj;

    private SteamVR_Action_Boolean drawBoadAction;
    private SteamVR_Input_Sources handType;

    // 右手の
    private SteamVR_Behaviour_Pose controllerPose;

    // 魔法を撃つ力
    [SerializeField] private float shotPower = 10f;

    private void Start() {
        drawBoadAction = SteamVR_Actions.default_InteractUI;
        handType = SteamVR_Input_Sources.LeftHand;
    }

    private void Update() {
        TrackingHand();
        ShotMagic();
        SwitchDrawBoadActive();
    }

    /// <summary>
    /// 右手の設定
    /// </summary>
    public void SetRightHand(Transform rHand) {
        this.rightHand = rHand;
        controllerPose = rightHand.GetComponent<SteamVR_Behaviour_Pose>();
    }
    /// <summary>
    /// DrawBoad設定
    /// </summary>
    public void SetDrawBoad(GameObject obj) {
        this.drawBoadObj = obj;
    }

    /// <summary>
    /// 魔法のオブジェクトをセット
    /// </summary>
    public void SetMagicObj(GameObject magicObject, GestureClass gesture, GameObject magicCircleObj) {
        if (!magicObject) return;
        if (myMagicObj) return;

        myMagicObj = magicObject;
        myMagicObj.GetComponent<MagicController>().Init(RoomModel.I.ConnectionId, gesture);

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
        magicCircle.transform.position = rightHand.position + -rightHand.right * 0.05f + -rightHand.up * 0.02f + -rightHand.forward * 0.07f;
        // 向き
        magicCircle.transform.up = -rightHand.right;
    }

    /// <summary>
    /// DrawBoadのアクティブ変更
    /// </summary>
    private void SwitchDrawBoadActive() {
        if (drawBoadAction.GetStateDown(handType) && drawBoadObj) {
            drawBoadObj.SetActive(!drawBoadObj.activeSelf);
        }
    }
}
