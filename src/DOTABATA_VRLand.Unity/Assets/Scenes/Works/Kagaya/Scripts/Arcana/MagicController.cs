using System;
using UnityEngine;
using static GestureRecognizer;

public class MagicController : MonoBehaviour {
    private SyncObject syncObject;

    // 生成したプレイヤー
    private Guid attackerConId;
    // ダメージ
    private float attackDamage;
    // ライフ
    [SerializeField] private float life;
    private float lifeTimer = 0;

    private bool isAttacked = false;

    // 手に持たれているか
    private bool isHand = true;

    private void Start() {
        syncObject = GetComponent<SyncObject>();
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(Guid playerConId, GestureClass gesture) {
        attackerConId = playerConId;
        attackDamage = GetDamage(gesture);
        
        syncObject = GetComponent<SyncObject>();
    }

    /// <summary>
    /// ダメージ値取得
    /// </summary>
    private float GetDamage(GestureClass gesture) {
        float rnd = UnityEngine.Random.Range(5, 10);

        switch (gesture) {
            case GestureClass.Circle:
                return 5;
            case GestureClass.Star:
                return 15;
            case GestureClass.Diamond:
                return 5;
            case GestureClass.Square:
                return 5;
            case GestureClass.Triangle:
                return 5;
            case GestureClass.Heart:
                return 10;
        }

        return default;
    }

    /// <summary>
    /// 魔法を手から離す
    /// </summary>
    public void ReleaseHand() {
        isHand = false;
    }

    private void OnTriggerEnter(Collider other) {
        if (isAttacked || isHand) return;

        // プレイヤーに当たったら
        if (other.gameObject.CompareTag("Player")) {
            // 自分自身のオブジェクトだったら
            if (other.gameObject.GetComponentInParent<SyncPlayer>().ConnectionId == attackerConId) return;
            
            PlayerStatus otherStatus = other.gameObject.GetComponent<PlayerStatus>();
            if (otherStatus == null) return;

            isAttacked = true;
            // ダメージを付与
            otherStatus.OnDamage(attackDamage);

            Destroy(this.gameObject);
        }
        // 他のオブジェクト
        else {
            Destroy(this.gameObject);
        }
    }

    private void Update() {
        if (isHand) return;

        // ライフが0になったら削除
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= life) {
            Destroy(this.gameObject);
        }
    }
}