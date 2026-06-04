using UnityEngine;

public class ArcanaPlayerController : MonoBehaviour {
    // 魔法オブジェクト
    private GameObject myMagicObj;
    // ターゲットプレイヤー
    private Transform targetPlayer;

    // 魔法を撃つ力
    [SerializeField] private float shotPower = 10f;

    /// <summary>
    /// 魔法のオブジェクトをセット
    /// </summary>
    public void SetMagicObj(GameObject magicObject) {
        if (!magicObject) return;

        myMagicObj = magicObject;
        myMagicObj.GetComponent<MagicController>().Init(RoomModel.I.ConnectionId, 10f);
    }

    /// <summary>
    /// 魔法を当てるプレイヤーを選択
    /// </summary>
    private void SelectTargetPlayer() {

    }

    /// <summary>
    /// 魔法を撃つ
    /// </summary>
    private void ShotMagic() {
        if (!targetPlayer || !myMagicObj) return;

        if (Input.GetKeyDown(KeyCode.F)) {
            myMagicObj.GetComponent<Rigidbody>().AddForce(this.transform.forward * shotPower, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 魔法を杖に追尾
    /// </summary>
    private void TrackingStaff() {
        if (!myMagicObj) return;

        myMagicObj.transform.position = this.gameObject.transform.position + new Vector3(0, 3, 0);
    }

    private void Update() {
        TrackingStaff();
        ShotMagic();
    }
}
