using System;
using UnityEngine;

public class MagicController : MonoBehaviour {
    // 生成したプレイヤー
    private Guid attackerConId;
    // ダメージ
    private float attackDamage;
    // ライフ
    [SerializeField] private float life;

    private float lifeTimer = 0; 

    private bool isAttacked = false;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(Guid playerConId, float damage) {
        attackerConId = playerConId;
        attackDamage = damage;
    }

    private void OnTriggerEnter(Collider other) {
        if (isAttacked) return;

        // プレイヤーに当たったら
        if (!other.gameObject.CompareTag("Player")) return;

        PlayerStatus otherStatus = other.gameObject.GetComponent<PlayerStatus>();
        if (otherStatus == null) return;

        isAttacked = true;
        otherStatus.OnDamage(attackDamage);

        Destroy(this.gameObject);
    }

    private void Update() {
        // ライフが0になったら削除
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= life) {
            Destroy(this.gameObject);
        }
    }
}