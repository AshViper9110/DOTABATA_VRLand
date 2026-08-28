using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HomeRunRush : MonoBehaviour
{
    [Header("Game Controller")]
    [SerializeField] private MinigameFlowController controller;

    [Header("Ball")]
    [SerializeField] private GameObject ballPrefab;

    [SerializeField] private float shotPower = 10f;
    [SerializeField] private float shotHight = 5f;

    [Header("Ball Spread")]
    [SerializeField] private float horizontalSpread = 0.05f;
    [SerializeField] private float verticalSpread = 0.05f;

    [Header("Shot Settings")]
    [SerializeField] private float shotInterval = 2f;
    [SerializeField] private int maxShots = 5;

    [Header("Bat")]
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private Transform batPos;

    [Header("Player Positions")]
    [SerializeField] private List<Transform> playerPos = new List<Transform>();

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private float panelOffset = 2f;

    private bool waitingForLastBall = false;

    // 現在何球目か
    private int currentShot = 0;

    // 現在のプレイヤーのホームラン数
    private int homeRunCount = 0;

    // 次の投球時間
    private float nextShotTime = 0f;

    // 自分の番か
    private bool isMyTurn = false;

    // 5球終了処理をすでに実行したか
    private bool finishedTurn = false;

    // バット
    private GameObject bat;

    // 現在の打者
    private int currentOrder = 1;


    // =========================================================
    // Unity
    // =========================================================

    private void OnEnable()
    {
        if (RoomModel.I == null)
            return;

        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnBallingNexted += OnBallingNexted;
        RoomModel.I.OnBallingPinAsynced += OnBallingPinAsync;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null)
            return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnBallingNexted -= OnBallingNexted;
        RoomModel.I.OnBallingPinAsynced -= OnBallingPinAsync;
    }

    private void Start()
    {
        AudioManager.StopBgm();

        // 最初は1番のプレイヤーから開始
        UpdatePlayerPosition(1);
    }

    private void Update()
    {
        if (!controller.isGameStarted || !isMyTurn) return;

        // 5球目を投げ終わった後
        if (waitingForLastBall)
        {
            if (Time.time >= nextShotTime)
            {
                FinishTurnAsync();
            }

            return;
        }

        // 次の投球時間まで待つ
        if (Time.time < nextShotTime)
            return;

        ShotBall();

        currentShot++;

        // 5球目
        if (currentShot >= maxShots)
        {
            waitingForLastBall = true;

            // 5球目が飛んでいる時間
            nextShotTime = Time.time + shotInterval;

            return;
        }

        nextShotTime = Time.time + shotInterval;
    }


    // =========================================================
    // Player Turn
    // =========================================================

    /// <summary>
    /// 次のプレイヤーが投球開始したときに呼ばれる
    /// </summary>
    private void OnBallingNexted(
        int order,
        JoinedUser joinedUser,
        int pinCount)
    {
        Debug.Log($"[HomeRunRush] Player {order} の番");

        currentOrder = order;

        // 5球カウントをリセット
        currentShot = 0;

        // ターン終了フラグをリセット
        finishedTurn = false;

        // ホームラン数をリセット
        homeRunCount = 0;

        // 最初の球を少し待ってから投げる
        nextShotTime = Time.time + 4f;

        // プレイヤー位置を更新
        UpdatePlayerPosition(order);
    }


    /// <summary>
    /// 自分の5球が終了した
    /// </summary>
    private async Task FinishTurnAsync()
    {
        // 二重実行防止
        if (finishedTurn)
            return;

        finishedTurn = true;
        isMyTurn = false;

        Debug.Log(
            $"[HomeRunRush] Player {currentOrder} 終了 " +
            $"HR: {homeRunCount}"
        );

        var myId = NetworkManager.I.myConnectionId;

        if (!InRoomPlayerData.I.PlayerList.TryGetValue(
                myId,
                out var playerData))
        {
            Debug.LogError(
                "[HomeRunRush] 自分のPlayerDataが見つかりません"
            );

            return;
        }

        Destroy(bat);
        RoomModel.I.SendScore(homeRunCount);
        await RoomModel.I.BallingNext(
            homeRunCount,
            playerData.joinedUser
        );
    }


    // =========================================================
    // Player Position
    // =========================================================

    private void UpdatePlayerPosition(int order)
    {
        var myId = NetworkManager.I.myConnectionId;

        if (!InRoomPlayerData.I.PlayerList.TryGetValue(
                myId,
                out var playerData))
        {
            Debug.LogError(
                "[HomeRunRush] PlayerDataが見つかりません"
            );

            return;
        }

        int myOrder = playerData.joinedUser.JoinOrder;

        /*
         * 現在のプレイヤーが自分なら、
         * 自分のクライアントだけ投球処理を行う。
         */
        isMyTurn = myOrder == order;

        int index;

        if (myOrder == order)
        {
            // 現在の打者
            index = 0;

            // バットがまだ存在しなければ生成
            if (bat == null && batPrefab != null && batPos != null)
            {
                bat = Instantiate(
                    batPrefab,
                    batPos.position,
                    batPos.rotation
                );

                bat.transform.SetParent(batPos);
            }
        }
        else if (myOrder < order)
        {
            index = myOrder;
        }
        else
        {
            index = myOrder - 1;
        }

        // PlayerPositionの範囲外
        if (index < 0 || index >= playerPos.Count)
        {
            Debug.LogWarning(
                $"[HomeRunRush] playerPos index out of range: {index}"
            );

            return;
        }

        // プレイヤー位置
        playerData.playerObj.transform.position =
            playerPos[index].position;

        // パネル位置
        if (panel != null)
        {
            Vector3 panelPos = new Vector3(
                playerPos[index].position.x,
                panel.transform.position.y,
                playerPos[index].position.z + panelOffset
            );

            panel.transform.position = panelPos;
        }
    }


    // =========================================================
    // Ball
    // =========================================================

    private void ShotBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogError(
                "[HomeRunRush] ballPrefabが設定されていません"
            );

            return;
        }

        GameObject ball = Instantiate(
            ballPrefab,
            transform.position,
            transform.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.useGravity = true;

        if (rb == null)
        {
            Debug.LogError(
                "[HomeRunRush] ballPrefabにRigidbodyがありません"
            );

            Destroy(ball);

            return;
        }

        // -----------------------------------------------------
        // ランダムなブレ
        // -----------------------------------------------------

        float horizontal =
            Random.Range(
                -horizontalSpread,
                horizontalSpread
            );

        float vertical =
            Random.Range(
                -verticalSpread,
                verticalSpread
            );

        Vector3 direction =
            transform.forward +
            transform.right * horizontal +
            transform.up * vertical;

        direction.Normalize();

        // -----------------------------------------------------
        // 発射
        // -----------------------------------------------------

        rb.linearVelocity =
            direction * shotPower +
            Vector3.up * shotHight;

        Debug.Log(
            $"[HomeRunRush] Shot {currentShot + 1}/{maxShots}"
        );
    }


    // =========================================================
    // Home Run
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        // ボール以外は無視
        if (!other.CompareTag("projectile"))
            return;

        // 自分の番ではない場合は無視
        if (!isMyTurn)
            return;

        Debug.Log(
            $"[HomeRunRush] ホームラン！ " +
            $"Player {currentOrder}"
        );

        HomeRun();

        /*
         * 必要ならボールを削除
         *
         * Destroy(other.gameObject);
         *
         * ただし別の場所でボールを管理している場合は
         * そちらに任せてください。
         */
    }


    private void HomeRun()
    {
        homeRunCount++;

        Debug.Log(
            $"[HomeRunRush] " +
            $"Player {currentOrder} " +
            $"ホームラン数: {homeRunCount}"
        );

        /*
         * ここからゲーム側のホームラン処理を追加。
         *
         * 例:
         *
         * controller.HomeRun();
         *
         * UI更新
         * スコア更新
         * SE再生
         * エフェクト
         */
    }


    // =========================================================
    // Network Result
    // =========================================================

    private void OnBallingPinAsync(
        int count,
        JoinedUser joinedUser)
    {
        /*
         * サーバー側から結果を受信したときの処理。
         *
         * ここで次のプレイヤーへの切り替え、
         * 最終結果表示などを行う。
         */

        Debug.Log(
            $"[HomeRunRush] BallingPinAsync " +
            $"count={count}"
        );
    }


    // =========================================================
    // Countdown
    // =========================================================

    public void StartCountdown(int remain)
    {
        if (remain <= 0)
        {
            AudioManager.ChangeBGM(
                AudioManager.BGM.Bowling
            );
        }
    }
}