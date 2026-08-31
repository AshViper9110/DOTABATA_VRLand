using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Valve.VR.InteractionSystem;

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

    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float countdownInterval = 1f;

    [SerializeField] private TextMeshProUGUI ScoreTextLog;

    private bool waitingForLastBall = false;

    // 現在何球目か
    private int currentShot = 0;

    // 現在のプレイヤーのScore
    private float homeRunScore = 0;

    // 次の投球時間
    private float nextShotTime = 0f;

    // 自分の番か
    private bool isMyTurn = false;

    // カウントダウン中か
    private bool isCountdown = false;

    // 5球終了処理をすでに実行したか
    private bool finishedTurn = false;

    // バット
    private GameObject bat;

    // 現在の打者
    private int currentOrder = 1;

    // カウントダウンCoroutine
    private Coroutine countdownCoroutine;


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
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null)
            return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnBallingNexted -= OnBallingNexted;
        RoomModel.I.OnBallingPinAsynced -= OnBallingPinAsync;
        RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }

    private void Start()
    {
        AudioManager.StopBgm();

        // 最初は1番のプレイヤーから開始
        UpdatePlayerPosition(1);
    }

    private void Update()
    {
        // ゲーム開始前
        if (!controller.isGameStarted)
            return;

        // 自分の番ではない
        if (!isMyTurn)
            return;

        // カウントダウン中
        if (isCountdown)
            return;

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
    /// 次のプレイヤーの番になったときに呼ばれる
    /// </summary>
    private void OnBallingNexted(
        int order,
        JoinedUser joinedUser,
        int pinCount)
    {
        Debug.Log($"[HomeRunRush] Player {order} の番");

        ScoreTextLog.text +=
            $"\n{joinedUser.Name} : {pinCount}Pt";

        currentOrder = order;

        // 5球カウントをリセット
        currentShot = 0;

        // 最後のボール待ちをリセット
        waitingForLastBall = false;

        // ターン終了フラグをリセット
        finishedTurn = false;

        // ホームラン数をリセット
        homeRunScore = 0;

        // -----------------------------------------------------
        // まずプレイヤー位置を更新
        // -----------------------------------------------------

        UpdatePlayerPosition(order);

        // -----------------------------------------------------
        // 自分の番ならカウントダウン開始
        // -----------------------------------------------------

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

        int myOrder = playerData.joinedUser.JoinOrder;

        if (myOrder == order)
        {
            // カウントダウン中は投球不可
            isMyTurn = false;

            // 既にカウントダウンが動いていたら停止
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            countdownCoroutine =
                StartCoroutine(StartPlayerTurnCountdown());
        }
        else
        {
            isMyTurn = false;
        }
    }


    /// <summary>
    /// 自分の番が始まるときのカウントダウン
    /// </summary>
    private IEnumerator StartPlayerTurnCountdown()
    {
        isCountdown = true;

        Debug.Log("[HomeRunRush] 自分の番！ カウントダウン開始");

        // 3
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3";
        }

        yield return new WaitForSeconds(countdownInterval);

        // 2
        if (countdownText != null)
        {
            countdownText.text = "2";
        }

        yield return new WaitForSeconds(countdownInterval);

        // 1
        if (countdownText != null)
        {
            countdownText.text = "1";
        }

        yield return new WaitForSeconds(countdownInterval);

        // START
        if (countdownText != null)
        {
            countdownText.text = "START!";
        }

        Debug.Log("[HomeRunRush] カウントダウン終了。投球開始");

        // 投球可能
        isCountdown = false;
        isMyTurn = true;

        // すぐに1球目を投げる
        nextShotTime = Time.time;

        yield return new WaitForSeconds(0.5f);

        // START表示を消す
        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }

        countdownCoroutine = null;
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

        // バットが存在する場合のみ処理
        if (bat != null)
        {
            Interactable interactable =
                bat.GetComponent<Interactable>();

            if (interactable != null &&
                interactable.attachedToHand != null)
            {
                interactable.attachedToHand.DetachObject(bat);
            }

            Destroy(bat);
            bat = null;
        }

        Debug.Log(
            $"[HomeRunRush] Player {currentOrder} 終了 " +
            $"HR: {homeRunScore}"
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

        RoomModel.I.SendScore((int)homeRunScore);

        await RoomModel.I.BallingNext(
            (int)homeRunScore,
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
        isMyTurn = myOrder == order && !isCountdown;

        int index;

        if (myOrder == order)
        {
            // 現在の打者
            index = 0;

            // バットがまだ存在しなければ生成
            if (bat == null &&
                batPrefab != null &&
                batPos != null)
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

        if (rb == null)
        {
            Debug.LogError(
                "[HomeRunRush] ballPrefabにRigidbodyがありません"
            );

            Destroy(ball);

            return;
        }

        rb.useGravity = true;

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

    public void AddScore(float score)
    {
        homeRunScore += score;
    }

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
    }


    private void HomeRun()
    {
        homeRunScore += 100;

        Debug.Log(
            $"[HomeRunRush] " +
            $"Player {currentOrder} " +
            $"ホームラン数: {homeRunScore}"
        );
    }


    // =========================================================
    // Network Result
    // =========================================================

    private void OnBallingPinAsync(
        int count,
        JoinedUser joinedUser)
    {
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


    // =========================================================
    // Ranking
    // =========================================================

    private void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        if (bat == null)
            return;

        Interactable interactable =
            bat.GetComponent<Interactable>();

        if (interactable != null &&
            interactable.attachedToHand != null)
        {
            interactable.attachedToHand.DetachObject(bat);
        }

        Destroy(bat);
        bat = null;
    }
}