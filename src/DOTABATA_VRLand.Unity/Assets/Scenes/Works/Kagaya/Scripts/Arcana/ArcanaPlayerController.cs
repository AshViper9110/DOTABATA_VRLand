using UnityEngine;

public class ArcanaPlayerController : MonoBehaviour {
    // 魔法オブジェクト
    private GameObject myMagicObj;
    // ターゲットプレイヤー
    private Transform targetPlayer;

    /// <summary>
    /// 魔法のオブジェクトをセット
    /// </summary>
    public void SetMagicObj(GameObject magicObject) {
        myMagicObj = magicObject;
    }

    /// <summary>
    /// 魔法を当てるプレイヤーを選択
    /// </summary>
    public void SelectTargetPlayer() {

    }

    /// <summary>
    /// 魔法を撃つ
    /// </summary>
    public void ShotMagic() {
        if (!targetPlayer ||
            !myMagicObj) {
            return;
        }
    }
}
