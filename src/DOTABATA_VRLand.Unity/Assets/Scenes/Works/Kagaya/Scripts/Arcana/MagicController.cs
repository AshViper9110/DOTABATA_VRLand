using DG.Tweening;
using System;
using UnityEngine;
using static GestureRecognizer;

public class MagicController : MonoBehaviour {
    private SyncObject syncObject;
    private Rigidbody myRb;

    // 生成したプレイヤー
    private Guid attackerConId;
    // ダメージ
    private float attackDamage;
    // ライフ
    [SerializeField] private float life;
    private float lifeTimer = 0;

    // 追尾するプレイヤー
    private Transform targetPlayer;

    // ヒットVFX
    [SerializeField] private GameObject hitVFX;


    private bool isAttacked = false;

    // 手に持たれているか
    private bool isHand = true;

    [SerializeField] private float Speed; // 追従速度
    [SerializeField] private float MaxForce; // 最大の力
    [SerializeField] private float Kp; // P項係数
    [SerializeField] private float Ki; // I項係数
    [SerializeField] private float Kd; // D項係数

    private Vector3 SpeedErrInteg;
    private Vector3 PresentSpeedErr;

    private void Awake() {
        RoomModel.I.OnSyncdMagicBall += OnSyncdMagicBall;
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnSyncdMagicBall -= OnSyncdMagicBall;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Start() {
        syncObject = GetComponent<SyncObject>();
        myRb = GetComponent<Rigidbody>();
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
    /// ターゲット設定
    /// </summary>
    public void SetTarget(Transform target) {
        targetPlayer = target;
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
                return 6;
            case GestureClass.Square:
                return 5;
            case GestureClass.Triangle:
                return 5;
            case GestureClass.Heart:
                return 8;
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
        if (!syncObject.IsOwner) return;
        if (isHand) return;

        // ライフが0になったら削除
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= life) {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate() {
        if (!syncObject.IsOwner) return;
        if (isHand) return;

        Homing();
    }

    private void Homing() {
        if (!targetPlayer) return;

        float dt = Time.fixedDeltaTime;
        Vector3 tgtPos = targetPlayer.position + new Vector3(0, 1, 0);
        Vector3 diffDir = (tgtPos - transform.position).normalized; // ターゲットの方向
        Vector3 tgtSpeed = diffDir * Speed;
        Vector3 speedErr = tgtSpeed - myRb.linearVelocity;
        SpeedErrInteg += speedErr * dt;
        Vector3 prevSpeedErr = PresentSpeedErr;
        PresentSpeedErr = speedErr;
        Vector3 speedErrDiff = (PresentSpeedErr - prevSpeedErr) / dt;
        Vector3 force = Kp * speedErr + Ki * SpeedErrInteg + Kd * speedErrDiff; // PID制御
        float forceMagnitude = force.magnitude;
        if (forceMagnitude > MaxForce) {
            force = force / forceMagnitude * MaxForce; // 力を最大値にする
        }

        myRb.AddForce(force, ForceMode.Force);
    }

    /// <summary>
    /// [サーバー通知]
    /// 魔法オブジェクトのフィールド同期
    /// </summary>
    public void OnSyncdMagicBall(Guid objectId, Guid createrConId, string gestureClassName) {
        if (objectId != syncObject.ObjectId) return;
        attackerConId = createrConId;
        attackDamage = GetDamage(EnumExs.ParseFromString<GestureClass>(gestureClassName, true));
        isHand = false;
    }
}