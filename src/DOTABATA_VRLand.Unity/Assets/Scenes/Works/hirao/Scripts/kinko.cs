using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class kinko : MonoBehaviour
{
    [Serializable]
    class DialData
    {
        public GameObject GameObject;
        public float rot = 0;
        public bool isOpen = false;

        [NonSerialized]
        public Interactable interactable;
    }

    #region Inspector

    [Header("UI")]
    public GameObject introUI;
    public GameObject gameUI;
    public GameObject resultUI;

    [Header("Intro")]
    public GameObject descriptionPanel;
    public GameObject readyPanel;

    public Text titleText;
    public Text descriptionText;
    public Text readyText;

    [Header("Ready")]
    public Button readyButton;
    public Text waitingText;
    public Transform UserReadyObject;//プレイヤー情報整列用オブジェクト
    public GameObject playerNamePrefab;  //プレイヤーテキストプレハブ
    public List<GameObject> UserReadyText;   //プレイヤー準備情報テキスト 
    public Button StartButton;

    [Header("Countdown")]
    public Image fadeImage;
    public Text countdownText;

    [Header("Result")]
    public Text rank1Text;
    public Text rank2Text;
    public Text rank3Text;
    public Text rank4Text;


    [Header("Data")]
    public MinigameInfo info;

    private bool isGameStarted = false;
    private bool isResultShown = false;

    public List<string> names;
    bool willReady = false;

    [SerializeField] private List<DialData> dialList = new();

    [SerializeField] private float openLockTime;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip lockOpenSound;
    [SerializeField] private AudioClip gameClearSound;

    public SteamVR_Input_Sources handType;
    public float power;

    #endregion

    #region Runtime

    private bool isClear;
    private float currentLockTime;

    public SteamVR_Action_Vibration hapticAction =
        SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Hapic");

    #endregion

    void Start()
    {
        //ダイアルの初期設定
        foreach (var dial in dialList)
        {
            dial.rot = UnityEngine.Random.Range(-180, 180);
            dial.isOpen = false;
            dial.interactable = dial.GameObject.GetComponent<Interactable>();
        }

        SteamVR_Fade.Start(new Color(0, 0, 0, 0), 1.0f);
        //RoomModelイベント購読
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction += OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction += OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction += StartGameFlow;
        InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.transform.position = Vector3.zero;
        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)
        {
            if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
            player.playerObj.SetActive(false);
        }


        //StartCoroutine(GameFlow());

        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        resultUI.SetActive(false);
        gameUI.SetActive(false);

        introUI.SetActive(false);
        StartButton.gameObject.SetActive(false);


        introUI.SetActive(true);

        titleText.text = info.gameName;
        descriptionText.text = info.description;

        readyText.text = "0/4 プレイヤー準備完了";
    }

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction -= OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction -= OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction -= StartGameFlow;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var dial in dialList)
        {
            if (dial.isOpen) continue;
            if (dial.interactable.hoveringHand == null) continue;
            //TODO 鍵開けの処理の実装
            float currentRot = dial.GameObject.transform.localEulerAngles.y;
            if (Mathf.Abs(Mathf.DeltaAngle(currentRot, dial.rot)) <= 3f)
            {
                hapticAction.Execute(0, Time.deltaTime, 100, power, handType);
                currentLockTime += 0.1f;
                if (currentLockTime >= openLockTime)
                {
                    dial.isOpen = true;
                    audioSource.PlayOneShot(lockOpenSound);
                }
            }
            else
            {
                if (currentLockTime > 0) currentLockTime = 0;
            }
        }
        //ゲームクリア判定
        if (dialList.Count > 0 && dialList.All(x => x.isOpen) && !isClear)
        {
            Debug.Log("GameClear");
            isClear = true;
            audioSource.PlayOneShot(gameClearSound);
            OnSendScore(100);
        }
    }

    // =====================================================
    // Readyボタン
    // =====================================================

    public void OnReadyButton()
    {
        willReady = !willReady;

        // サーバー送信
        RoomModel.I.SendReadyState(willReady);

        // UI更新
        if (willReady)
        {
            readyButton.GetComponentInChildren<Text>().text = "取り消し";

            waitingText.gameObject.SetActive(true);
        }
        else
        {

            readyButton.GetComponentInChildren<Text>().text = "準備OK！";

            waitingText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // プレイヤーReady更新
    // =====================================================

    void OnUpdatePlayerReady(JoinedUser[] users, bool[] isReadyList)
    {

        // 既存アイテムを全削除
        foreach (var item in UserReadyText)
        {
            Destroy(item);
        }
        UserReadyText.Clear();

        // 人数分生成
        for (int i = 0; i < users.Length; i++)
        {
            GameObject item = Instantiate(playerNamePrefab, UserReadyObject);
            item.GetComponentInChildren<Text>().text =
            isReadyList[i] ? $"{users[i].Name} : 準備OK" : $"{users[i].Name} : 待機中";//isReadyListの状況でテキストを編集
            UserReadyText.Add(item);
        }

        // TODO:
        // プレイヤー一覧UI更新
    }

    // =====================================================
    // 全員Ready通知
    // =====================================================

    void OnAllReadyState(bool isAllReady)
    {
        if (isAllReady) Debug.Log("全員Ready");
        else Debug.Log("誰かの準備ができていません");
        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder != 1) return;
        readyButton.gameObject.SetActive(!isAllReady);
        StartButton.gameObject.SetActive(isAllReady);
    }

    public void GameStart()
    {
        RoomModel.I.OnGameStartAsync();
    }

    // =====================================================
    // ゲーム開始準備
    // =====================================================

    void StartGameFlow()
    {
        isGameStarted = true;
        StartButton.gameObject.SetActive(false);
        descriptionPanel.SetActive(false);
        readyPanel.SetActive(false);
        RoomModel.I.StartCountdown();

    }

    // =====================================================
    // カウントダウン受信
    // =====================================================

    public void StartCountdown(int remain)
    {
        countdownText.gameObject.SetActive(true);

        if (remain > 0)
        {
            countdownText.text = remain.ToString();
        }
        else
        {
            countdownText.text = "START!";

            StartCoroutine(BeginGameAfterStart());
        }
    }

    // =====================================================
    // ゲーム開始
    // =====================================================

    IEnumerator BeginGameAfterStart()
    {
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);



        introUI.SetActive(false);

        gameUI.SetActive(true);

        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)
        {
            if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
            player.playerObj.SetActive(true);
        }
    }

    // =====================================================
    // Score送信
    // =====================================================

    public void OnSendScore(int Score)
    {
        RoomModel.I.SendScore(Score);
    }

    // =====================================================
    // ランキング受信
    // =====================================================

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        names.Clear();

        foreach (var user in rankOrder)
        {
            names.Add(user.Name);
        }
        Debug.Log("OnReceiveRanking受信");
        foreach (var user in names)
        {
            Debug.Log($"ランキング受信{user}");
        }
        ShowRanking(names);
    }

    // =====================================================
    // ランキング表示開始
    // =====================================================

    void ShowRanking(List<string> rankOrder)
    {
        SceneManager.LoadScene("GameScene");
        willReady = false;
        Debug.Log("ShowRanking受信");
    }
}
