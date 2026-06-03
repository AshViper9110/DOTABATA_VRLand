using UnityEngine;

public class PlayerStatus : MonoBehaviour {
    private float hp;

    /// <summary>
    /// ダメージ受ける処理
    /// </summary>
    public void OnDamage(float damage) {
        hp -= damage;
        if (hp < 0) {

        }
    }
}
