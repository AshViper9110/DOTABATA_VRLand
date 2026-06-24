using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using Valve.VR;

public class MinigameFlowController : MonoBehaviour
{
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

    public bool isGameStarted = false;
    private bool isResultShown = false;

    public List<string> names;
    bool willReady = false;

    // =====================================================
    // Start
    // =====================================================

    async void Start()
    {
        SteamVR_Fade.Start(new Color(0,0,0,0),1.0f);
        //RoomModelイベント購読
        RoomModel.I.OnCountdownAction += StartCountdown;
        RoomModel.I.OnRegisterScoreAction += OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction += OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction += OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction += StartGameFlow;
        InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.transform.position = Vector3.zero;//プレイヤー座標初期化(0,0,0)
        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)//他プレイヤー非表示
        {
            if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
            player.playerObj.SetActive(false);
        }
           
        waitingText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        resultUI.SetActive(false);
        gameUI.SetActive(false);

        introUI.SetActive(false);
        StartButton.gameObject.SetActive(false);
       
        introUI.SetActive(true);

        titleText.text = info.gameName;
        descriptionText.text = info.description;

        readyText.text = "0/"+ InRoomPlayerData.I.PlayerList.Count + "プレイヤー準備完了";
    }

    // =====================================================
    // Update
    // =====================================================

    void Update()
    {  
    }

    // =====================================================
    // Destroy
    // =====================================================

    private void OnDestroy()
    {
        if (RoomModel.I == null) return;

        RoomModel.I.OnCountdownAction -= StartCountdown;
        RoomModel.I.OnRegisterScoreAction -= OnReceiveRanking;
        RoomModel.I.OnUpdatedAllReadyStateAction -= OnAllReadyState;
        RoomModel.I.OnUpdatedReadyStateAction -= OnUpdatePlayerReady;
        RoomModel.I.OnGameStartAction -= StartGameFlow;
    }

    // =====================================================
    // Ready切り替え
    // =====================================================

    public void OnReadyButton()
    {
         willReady = !willReady;//状態切り替え

        // サーバー送信
        RoomModel.I.SendReadyState(willReady);

        // UI更新
        if (willReady)
        {
            readyButton.GetComponentInChildren<Text>().text = "取り消し";

            waitingText.gameObject.SetActive(true);
        }
        else{
        
            readyButton.GetComponentInChildren<Text>().text = "準備OK！";

            waitingText.gameObject.SetActive(false);
        }
    }

    // =====================================================
    // プレイヤーReady更新
    // =====================================================

    void OnUpdatePlayerReady(JoinedUser[] users, bool[] isReadyList)
    {
        int readyCount = 0;
        //状態のログ表示
        for (int i = 0; i < users.Length; i++)
        {
          Debug.Log(isReadyList[i] ? $"{users[i].Name} : 準備OK" : $"{users[i].Name} : 待機中");
            if (isReadyList[i])
            {
                readyCount++;
            }
        }

        readyText.text = readyCount +"/" + users.Length + "プレイヤー準備完了";
    }

    // =====================================================
    // 全員Ready通知
    // =====================================================

    void OnAllReadyState(bool isAllReady)
    {        
        if(isAllReady)Debug.Log("全員Ready");
        else Debug.Log("誰かの準備ができていません");

        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder != 1) return;

        //各ボタン切り替え
        readyButton.gameObject.SetActive(!isAllReady);

        StartButton.gameObject.SetActive(isAllReady);
    
    }

    // =====================================================
    // ゲーム開始送信
    // =====================================================

    public void GameStrat()
    {
        RoomModel.I.OnGameStartAsync();
    }

    // =====================================================
    // ゲーム開始取得
    // =====================================================

    void StartGameFlow()
    {
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
    // ミニゲーム開始
    // =====================================================

    IEnumerator BeginGameAfterStart()
    {
        yield return new WaitForSecondsRealtime(1f);

        countdownText.gameObject.SetActive(false);//カウントダウン削除

        introUI.SetActive(false);

        gameUI.SetActive(true);
        
        foreach (PlayerData player in InRoomPlayerData.I.PlayerList.Values)//プレイヤー表示
        {
            if (player.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) continue;
            player.playerObj.SetActive(true);
        }
        
        isGameStarted = true;//ゲーム開始判定

    }

    // =====================================================
    // Score送信
    // =====================================================

    public void OnSendScore()
    {
        int Score = 100;

        // サーバー送信
        RoomModel.I.SendScore(Score);

    }

   
    // =====================================================
    // ランキング受信
    // =====================================================

    void OnReceiveRanking(List<JoinedUser> rankOrder)
    {
        names.Clear();//前回のユーザー情報クリア

        foreach (var user in rankOrder)//今回の情報取得
        {
            names.Add(user.Name);
        }
        Debug.Log("OnReceiveRanking受信");
        foreach (var user in names)//取得チェック
        {
            Debug.Log($"ランキング受信:{user}");
        }

        isGameStarted = false;
        EndGame();
        SceneManager.LoadScene("GameScene");//シーン移行

       
    }

    // =====================================================
    // 終了
    // =====================================================

    void EndGame()
    {
        Debug.Log("ゲーム終了！");
    }
}