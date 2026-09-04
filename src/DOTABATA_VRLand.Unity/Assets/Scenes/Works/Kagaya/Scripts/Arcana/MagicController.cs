using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Valve.VR.InteractionSystem;
using static GestureRecognizer;

public class MagicController : MonoBehaviour {
    private ArcanaGameManager arcanaGameManager;
    private SyncObject syncObject;
    private Rigidbody myRb;

    // 生成したプレイヤー
    private Guid attackerConId;

    [ReadOnly] public string attackerConIdStr;

    // ライフ
    [SerializeField] private float life;
    private float lifeTimer = 0;

    // 追尾するプレイヤー
    private Transform targetPlayer;

    private GestureClass myGesture;

    // ヒットVFX
    [SerializeField] private GameObject hitVFX;
    // シールドヒットVFX
    [SerializeField] private GameObject shieldHitVFX;
    // ジャストシールドヒットVFX
    [SerializeField] private GameObject justShieldHitVFX;

    // 攻撃力したか
    [ReadOnly] public bool isAttacked = false;

    // 手に持たれているか
    [ReadOnly] public bool isHand = true;

    public float Speed; // 追従速度
    public float MaxForce; // 最大の力
    [SerializeField] private float Kp; // P項係数
    [SerializeField] private float Ki; // I項係数
    [SerializeField] private float Kd; // D項係数

    private Vector3 SpeedErrInteg;
    private Vector3 PresentSpeedErr;

    [ReadOnly] public bool initialized = false;

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
        arcanaGameManager = GameObject.Find("ArcanaGameManager").GetComponent<ArcanaGameManager>();

        initialized = true;
    }

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init(Guid playerConId, GestureClass gesture) {
        attackerConId = playerConId;
        SetStatus(gesture);
        
        syncObject = GetComponent<SyncObject>();
    }

    /// <summary>
    /// ターゲット設定
    /// </summary>
    public void SetTarget(Transform target) {
        targetPlayer = target;
    }

    /// <summary>
    /// 魔法のステータス設定
    /// </summary>
    private void SetStatus(GestureClass gesture) {
        myGesture = gesture;

        float rnd = UnityEngine.Random.Range(5, 10);

        switch (gesture) {
            case GestureClass.Circle:
                Speed = 5;
                MaxForce = 18;
                Kp = 3;
                Ki = 0f;
                Kd = 0.1f;
                return;
            case GestureClass.Star:
                Speed = 15;
                MaxForce = 40;
                Kp = 13;
                Ki = 0.08f;
                Kd = 1.6f;
                return;
            case GestureClass.Diamond:
                Speed = 6;
                MaxForce = 16;
                Kp = 6.5f;
                Ki = 0.05f;
                Kd = 0.7f;
                return;
            case GestureClass.Square:
                Speed = 2;
                MaxForce = 10;
                Kp = 8;
                Ki = 0.02f;
                Kd = 0.5f;
                return;
            case GestureClass.Triangle:
                Speed = 5;
                MaxForce = 17;
                Kp = 5f;
                Ki = 0.01f;
                Kd = 0.2f;
                return;
            case GestureClass.Heart:
                Speed = 10;
                MaxForce = 35;
                Kp = 10;
                Ki = 0.1f;
                Kd = 2.0f;
                return;
        }
    }

    /// <summary>
    /// 魔法を手から離す
    /// </summary>
    public void ReleaseHand() {
        isHand = false;
    }

    private async void OnTriggerEnter(Collider other) {
        if (isAttacked || isHand) return;

        // プレイヤーに当たったら
        if (other.gameObject.CompareTag("Player")) {
            if (other.gameObject.name != "BodyCollider" && other.gameObject.name != "HeadCollider") {
                //Debug.Log("頭、体のコライダーではありません");
                return;
            }

            Player hitPlayer = other.gameObject.GetComponentInParent<Player>();
            if (hitPlayer == null) {
                Debug.Log("プレイヤーコンポーネントがnullです");
                return;
            }

            // 自分自身のオブジェクトだったら
            if (hitPlayer.gameObject.GetComponentInParent<SyncPlayer>().ConnectionId == attackerConId) {
                Debug.Log("自分自身の魔法です");
                return;
            }
            
            PlayerStatus otherStatus = hitPlayer.gameObject.GetComponent<PlayerStatus>();
            if (otherStatus == null) {
                Debug.Log("PlayerStatusコンポーネントがnullです");
                return;
            }

            Debug.Log("Hit");

            isAttacked = true;
            // ダメージを付与
            bool result = await otherStatus.OnDamage(this.gameObject, hitVFX, shieldHitVFX, justShieldHitVFX);
            if (!result) {
                Destroy(this.gameObject);
            }
            else {
                isAttacked = false;
            }
        }
        // 手だったらなにもしない
        else if (other.gameObject.name == "HandColliderLeft(Clone)" || other.gameObject.name == "HandColliderRight(Clone)") {

        }
        // 他のオブジェクト
        else {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }

    private void Update() {
        attackerConIdStr = attackerConId.ToString();

        if (!syncObject.IsOwner) return;
        if (isHand) return;

        SelectTarget();

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

    /// <summary>
    /// ターゲット選定
    /// </summary>
    private void SelectTarget() {
        if (targetPlayer) return;

        Transform target = null;
        float targetDis = 999;

        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values) {
            float dis = Vector3.Distance(this.transform.position, player.playerObj.transform.position);

            if (dis < targetDis) {
                target = player.playerObj.transform;
                targetDis = dis;
            }
        }

        if (targetDis < 8) {
            targetPlayer = target;
        }
    }

    /// <summary>
    /// ホーミング処理
    /// </summary>
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
    /// ジャストシールド
    /// </summary>
    public async void JustShield(Guid justPlayer) {
        lifeTimer = 0;
        bool result = await syncObject.GetComponent<SyncObject>().GetOwnership(true);
        if (!result) {
            Destroy(this.gameObject);
            return;
        }

        if (InRoomPlayerData.I.PlayerList[attackerConId].playerObj && InRoomPlayerData.I.PlayerList[attackerConId].playerObj.activeSelf) {
            targetPlayer = InRoomPlayerData.I.PlayerList[attackerConId].playerObj.transform;
            attackerConId = justPlayer;
            lifeTimer = 0;

            Speed += 5f;
            MaxForce += 13.5f;
            Kp += 2f;
            Ki += 0.01f;
            Kd += 0.3f;

            // オブジェクトのフィールド同期
            await RoomModel.I.SyncMagicBallAsync(syncObject.ObjectId, myGesture.ToString(), -1);

            float rX = UnityEngine.Random.Range(0f, 1f);
            float rY = UnityEngine.Random.Range(0f, 1f);
            float rZ = UnityEngine.Random.Range(0f, 1f);

            myRb.AddForce(new Vector3(rX, rY, rZ).normalized * 20f, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// 魔法オブジェクトのフィールド同期
    /// </summary>
    public async void OnSyncdMagicBall(Guid objectId, Guid createrConId, string gestureClassName, int rndNum) {
        await UniTask.WaitUntil(() => initialized == true && syncObject.Initialized == true);

        if (objectId != syncObject.ObjectId) return;

        Debug.Log($"魔法オブジェクトのフィールド同期：{objectId}");

        attackerConId = createrConId;
        isHand = false;

        if (rndNum == -1) {
            // ジャストシールドで返された時
            Speed += 5f;
            MaxForce += 13.5f;
            Kp += 2f;
            Ki += 0.01f;
            Kd += 0.3f;

            return;
        }
        else {
            SetStatus(EnumExs.ParseFromString<GestureClass>(gestureClassName, true));
        }

        // VFX生成
        Instantiate(arcanaGameManager.magicVFXList[rndNum], this.transform);
        // Material適応
        this.GetComponent<MeshRenderer>().material = arcanaGameManager.magicMaterialList[rndNum];
    }
}