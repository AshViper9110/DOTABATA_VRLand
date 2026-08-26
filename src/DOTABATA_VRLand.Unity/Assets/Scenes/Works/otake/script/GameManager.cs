using Assets.Scenes.Works.otake.script;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using Valve.VR;
using UnityEditor;
using TMPro;
using Cysharp.Threading.Tasks.Triggers;
using Unity.VisualScripting;
using DOTABATA_VRLand.Shared.Models.Entities;
using System.Threading.Tasks;


public class GameManager : MonoBehaviour
{


    [SerializeField] GameObject RallyObjcts;
    [SerializeField] GameObject FreeObjects;
    // Inspectorから設定
    public SteamVR_Action_Boolean grabAction;
    public SteamVR_Input_Sources handType;

    public List<string> miniGames = new List<string>();
    public List<string> miniGameNames = new List<string>();

    static public bool rally = true;
    static public bool freePlay = false;

    public List<Transform> playerPos = new List<Transform>();

    public InputActionReference rightHandPrimaryAction;
    InputAction action;

    [SerializeField] GameObject CrownPrefab;
    public float crownDistance;


    static public List<string> PlayedMiniGame = new List<string>();

    //



    /// <summary>
    /// 進行UI関係
    /// </summary>

    public Text DummyText;
    public TextMeshProUGUI MainText;
    public int textIndex;

    public bool onSelect;
    public bool onResult;
    public bool onEnd;

    //進行テキスト(最初)
    List<string> StartText = new List<string>()
    {
        "ミニゲーム大会を始めるよ！",
        "先に三勝したプレイヤーが勝ちだよ！",
        "それじゃあ早速ミニゲームを決めていくよ！"
    };

    //進行テキスト(ミニゲーム後)
    List<string> AfterText = new List<string>()
    {
        "ミニゲームお疲れ様!",
        "今回勝ったひとは...",
        "!!! おめでとう！",//あとから勝ったプレイヤー名を挿入,
        "それじゃあ次のミニゲームを決めていくよ!",
    };

    //進行テキスト(メインゲーム終了時)
    List<string> FinishText = new List<string>()
    {
        "ここでゲーム大会の勝者が決まったみたいだね",
        "今回優勝した人は...",
        "!!! おめでとう！",//あとから勝ったプレイヤー名を挿入,
        "他のみんなも遊んでくれてありがとう！",
        "また遊んでね！バイバーイ！"
    };

    //ミニゲームのUI配置関係
    public float radius;
    [SerializeField] GameObject MinigamePrefab;

    [SerializeField] GameObject CenterObj;
    Rigidbody CenterObjRb;

    [SerializeField] GameObject selectPoint;
    SelPointManager selPointManager;
    [SerializeField] float SelPointHeght;

    [SerializeField] List<Sprite> miniGameTitleImages = new List<Sprite>();

    bool isSpin;
    bool EndProgress;

    //ランキングUI
    public List<RectTransform> rankingPosList;
    public List<RectTransform> rankingUis;
    public List<Material> rankingMaterials;

    [SerializeField] HostManager hostManager;

     public Dictionary<int, int> playerWinlist = new Dictionary<int, int>()
    {
        { 1,0},{2,0},{3,0},{4,0}
    };//勝利数

    public Dictionary<int, int> RankingList = new Dictionary<int, int>()
    {
        {1,0},
        {2,0},
        { 3,0},
        { 4,0}

    };


    public Dictionary<Guid, int> miniRankingList = new Dictionary<Guid, int>()
    {
        
        
    };

    public Guid winPlayerId;

    AudioSource audio;
    [SerializeField] AudioClip Roll;
    [SerializeField] AudioClip RollEnd;

  
    FreePlayManager freePlayManager;
    bool isAddCrown;
    int GetRankIndex;


    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnMovedScene += MoveScene;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnMovedScene -= MoveScene;

    }

    private void Awake()
    {
        action = rightHandPrimaryAction.action;
       // action.performed += MoveText;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<SmoothLocomotion>().enabled = true;
       audio = GetComponent<AudioSource>();
        SteamVR_Fade.View(new Color(0,0,0,0),2);
        EndProgress = false;
        AudioManager.ChangeBGM(AudioManager.BGM.Main_Normal);

        RallyObjcts.SetActive(false);
        FreeObjects.SetActive(false);

        if (NetworkManager.I.gameModeId == 0)
        {
            freePlay = true;
            rally = false;
            FreeObjects.SetActive(true);

        }
        else
        {
            freePlay=false;
            rally=true;
            RallyObjcts.SetActive(true);
        }

        if (rally)
        {
            InitRally();
        }
        else if (freePlay)
        {
            InitFreePlay();
        }





     

    }

    // Update is called once per frame
    void Update()
    {
        if (rally)
        {
            if (!isSpin)
            {
                if (CenterObjRb.angularVelocity.y < 0.29f)
                {
                    CenterObjRb.angularVelocity = new Vector3(0, 0.3f, 0);
                }
            }
            else if (isSpin && !onSelect)
            {
                if (CenterObjRb.angularVelocity.y < 0.01f)
                {
                    DummyText.text = "";
                    Debug.Log(selPointManager.titleName + "にゲームが決まりました");
                    DummyText.DOText(selPointManager.titleName + "にゲームが決まりました", 1.0f);

                    audio.Stop();
                    audio.PlayOneShot(RollEnd);

                    onSelect = true;
                    onResult = false;
                    onEnd = false;

                }
            }

            MainText.text = DummyText.text;


            if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
            {
                if (Input.GetMouseButtonDown(0) || grabAction.GetStateDown(handType))
                {
                    Debug.Log("会話進めます");
                    NetworkManager.I.SendHostProgress();

                }

                if (!isSpin)
                {
                    if (CenterObjRb.angularVelocity.y < 0.29f)
                    {
                        CenterObjRb.angularVelocity = new Vector3(0, 0.3f, 0);
                    }
                }
            }
        }
    }

    public void InitRally()
    {
        SetMiniGameAsync();
        CenterObjRb = CenterObj.GetComponent<Rigidbody>();
        selPointManager = selectPoint.GetComponent<SelPointManager>();
        isSpin = false;
        onSelect = false;
        onResult = false;
        onEnd = false;


    

        SetRanking();


        //シーン移行後の位置配置
        var myId = NetworkManager.I.myConnectionId;

        int index =
            InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position =
            playerPos[index].position;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.rotation =
           playerPos[index].rotation;

        //ここで全体ランキング、勝利数の取得、王冠の配置
        NetworkManager.I.ReqestRanking();

        //ここで前回のミニゲーム結果,勝利数を反映
        foreach (Guid guid in InRoomPlayerData.I.PlayerList.Keys)
        {
            NetworkManager.I.ReqestMinigameRanking(guid);

            SetRankText(guid, InRoomPlayerData.I.PlayerList[guid].joinedUser.JoinOrder);
        }




     
            DummyText.text = "";
            textIndex = 0;
            DummyText.DOText(StartText[textIndex], 1.0f);
        

       
        
    }

    public void InitFreePlay()
    {
        freePlayManager = GetComponent<FreePlayManager>();

        freePlayManager.SetMinigames();
        isAddCrown = false;
        winPlayerId = Guid.Empty;
        GetRankIndex = 0;
        //シーン移行後の位置配置
        var myId = NetworkManager.I.myConnectionId;

        int index =
            InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position =
            playerPos[index].position;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.rotation =
           playerPos[index].rotation;

        //ここで全体ランキング、勝利数の取得、王冠の配置
        NetworkManager.I.ReqestRanking();

        //ここで前回のミニゲーム結果,勝利数を反映
        foreach (Guid guid in InRoomPlayerData.I.PlayerList.Keys)
        {
            NetworkManager.I.ReqestMinigameRanking(guid);

           
        }
    }

    public　void InitResult()
    {
        onResult = true;
        DummyText.text = "";
        textIndex = 0;


        DummyText.DOText(AfterText[textIndex], 1.0f);
        SetResult();
    }

    public void SetRankText(Guid guid,int Id)
    {
        rankingUis[Id - 1].GetComponent<TextMeshProUGUI>().text = InRoomPlayerData.I.PlayerList[guid].joinedUser.Name + "  win×"+
            playerWinlist[Id];

        SetRanking();
    }

    //ミニゲーム抽選開始(ホストのみ実行)
    public void SelectMiniGame()
    {
        isSpin = true;

        audio.clip = Roll;
        audio.Play();
        audio.loop = true;

        float spinPower = UnityEngine.Random.Range(5, 30);

        CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    public void MoveScene(string scene)
    {
        foreach (Guid guid in InRoomPlayerData.I.PlayerList.Keys)
        {
            DeleteCrown(guid, InRoomPlayerData.I.PlayerList[guid].joinedUser.JoinOrder);
        }
        PlayedMiniGame.Add(scene);

        SteamVR_Fade.View(new Color(1,1,1,1), 2);
        Initiate.Fade(scene, new Color(0, 0, 0, 0), 0.5f);
        AudioManager.PlaySE(AudioManager.SE.MoveScene);

        if (name == "TitleScene")
        {
            PlayedMiniGame = new List<string>();
            RoomModel.I.LeaveRoomAsync();
        }
    }

    public async Task SetMiniGameAsync()
    {
        List<MiniGameInfo> miniGames = await RoomModel.I.GetAllMiniGameAsync();

        int count = miniGames.Count;

        for (int i = 0; i < count; i++)
        {
            // 円周上に等間隔配置
            float angle = i * Mathf.PI * 2f / count;

            // 位置を計算 (X, Z平面)
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            GameObject minigame = Instantiate(
                MinigamePrefab,
                CenterObj.transform.position + pos,
                Quaternion.identity,
                CenterObj.transform);

            // 中心を向かせる
            minigame.transform.LookAt(CenterObj.transform.position);

            MiniGameObjManager free = minigame.GetComponent<MiniGameObjManager>();
            free.sceneName = miniGames[i].SceneName;
            free.titleName = miniGames[i].TitleName;

            RawImage image = free.GetComponentInChildren<RawImage>();
            image.texture = CreateTextureFromBytes(miniGames[i].BinaryImg);
        }
    }

    private Texture2D CreateTextureFromBytes(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return null;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogError("画像の読み込みに失敗しました。");
            Destroy(texture);
            return null;
        }

        return texture;
    }

    public void SetCrown(Guid guid,int ID)
    {
        Debug.Log("SetCrown");
        List<GameObject> crowns = new List<GameObject>();
        
        Transform transform = InRoomPlayerData.I.PlayerList[guid].playerObj.GetComponent<PlayerTransform>().crownParent;

        if (isAddCrown)
        {
            if(guid == winPlayerId)
            {
                Debug.Log("追加済みだから減算");
                playerWinlist[ID]--;
            }
        }
 

        for (int i = 0; i < playerWinlist[ID]; i++)
        {
            GameObject crown = Instantiate(CrownPrefab,
                transform);
            crowns.Add(crown);


        }
        int index = 0;

        foreach (GameObject crown in crowns)
        {
            crown.transform.position = new Vector3(crown.transform.position.x,transform.position.y+(crownDistance*index),crown.transform.position.z);
            index++;
        }
    }

    public void AddCrown(Guid guid, int ID)
    {
        Debug.Log("AddCrown");
        PlayerTransform playerTransform = InRoomPlayerData.I.PlayerList[guid].playerObj.GetComponent<PlayerTransform>();
        Transform transform = playerTransform.crownParent;
        GameObject crown = Instantiate(CrownPrefab,
               transform);

  


        crown.transform.position = new Vector3(crown.transform.position.x, transform.position.y + (crownDistance * playerWinlist[ID])+3f, crown.transform.position.z);

        CrownManager manager = crown.GetComponent<CrownManager>();
        manager.isNew = true;
        manager.ParentTrans = transform;


        playerWinlist[ID]++;

        if (freePlay) { 
            isAddCrown = true;
            winPlayerId = guid;
            playerTransform.StartSpotLight(13);
            return; 
        }

        if (playerWinlist[ID] >= 3)
        {
            onEnd = true;
            onResult = false;
            textIndex = -1;
           
        }

        SetRankText(guid,ID);
        playerTransform.StartSpotLight(3);

    }

    public void DeleteCrown(Guid guid, int ID)
    {
        Transform transform = InRoomPlayerData.I.PlayerList[guid].playerObj.GetComponent<PlayerTransform>().crownParent;

        
        foreach(Transform crown in transform)
        {
            Destroy(crown.gameObject);
        }
     
    }
    public void SetResult()
    {
        foreach(Guid guid in miniRankingList.Keys)
        {

            if (miniRankingList[guid] == 1)
            {

                winPlayerId = guid;
               
            }

            if(freePlay)
            {
                freePlayManager.RankingText[miniRankingList[guid]-1].text = $"{InRoomPlayerData.I.PlayerList[guid].joinedUser.Name}";
                freePlayManager.RankingBord.SetActive(true);
            }
        }

        if (freePlay)
        {
            GetRankIndex++;
           

            if (InRoomPlayerData.I.PlayerList.Count >= GetRankIndex)
            {


                if (winPlayerId == NetworkManager.I.myConnectionId)
                {
                    if (!isAddCrown)
                    {
                        isAddCrown = true;
                        RoomModel.I.RequestWinCountUp(NetworkManager.I.myConnectionId);
                    }
                }

            }
        }

    }

    public void SetRanking()
    {
        //勝利数でソート→ID参照でランキング付け→テキスト入れ替え

        playerWinlist = playerWinlist.OrderByDescending(x => x.Value)
                       .ToDictionary(x => x.Key, x => x.Value); ;

        int index = 1;
        int temp = 0;
        foreach (int ID in playerWinlist.Keys)
        {

            RankingList[ID] = index;
            index++;

        }

        for (int i = 0; i < RankingList.Count; i++)
        {
            rankingUis[i].DOAnchorPosY(rankingPosList[RankingList[i + 1] - 1].anchoredPosition.y, 1f);
            rankingUis[i].GetComponent<TextMeshProUGUI>().material = rankingMaterials[i];
            
        }

    }

    public void MoveText()
    {
        hostManager.ChengeFace(HostManager.facial.Normal);
        if (EndProgress) return;
        if (!isSpin)
        {
            textIndex++;
        }
        else if (isSpin && !onSelect)
        {
            if (CenterObjRb.angularVelocity.y < 0.01f)
            {
                audio.Stop();
                audio.PlayOneShot(RollEnd);
                DummyText.text = "";
                Debug.Log(selPointManager.titleName + "にゲームが決まりました");
                DummyText.DOText(selPointManager.titleName + "にゲームが決まりました", 1.0f);

                onSelect = true;
                onResult = false;
                onEnd = false;
               

            }
        }



        DummyText.text = "";
        if (onResult)
        {
            if (textIndex >= AfterText.Count && !isSpin)
            {
                SelectMiniGame();
              
                return;
            }

            if(isSpin)
            {
                return;
            }

            if (AfterText[textIndex] == "!!! おめでとう！")
            {
                hostManager.ChengeFace(HostManager.facial.Smile);
                DummyText.DOText($"{InRoomPlayerData.I.PlayerList[winPlayerId].joinedUser.Name}!!" + AfterText[textIndex], 1.0f);
               
                if (winPlayerId == NetworkManager.I.myConnectionId)
                {
                    RoomModel.I.RequestWinCountUp(NetworkManager.I.myConnectionId);
                        }

                SetRanking();
                //一旦仮で入れてます。本実装は優勝者のGuidいれてください。



            }
            else
            {
                DummyText.DOText(AfterText[textIndex], 1.0f);
            }
        }
        else if (onEnd)
        {
            if (textIndex >= FinishText.Count)
            {
                //タイトルに戻る
                if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
                {
                    RoomModel.I.MoveSceneAsync("TitleScene");
                    RoomModel.I.LeaveRoomAsync();
                }
                EndProgress = true;
                return;
            }

            if (FinishText[textIndex] == "!!! おめでとう！")
            {
                hostManager.ChengeFace(HostManager.facial.Smile);
                AudioManager.ChangeBGM(AudioManager.BGM.Main_End);
                DummyText.DOText($"{InRoomPlayerData.I.PlayerList[winPlayerId].joinedUser.Name}!!" + FinishText[textIndex], 1.0f);
                InRoomPlayerData.I.PlayerList[winPlayerId].playerObj.GetComponent<PlayerTransform>().StartSpotLight(10);

            }
            else
            {
                DummyText.DOText(FinishText[textIndex], 1.0f);
            }
        }
        else if (onSelect)
        {
            RoomModel.I.MoveSceneAsync(selPointManager.sceneName);
            EndProgress = true;
        }
        else
        {
            if (textIndex >= StartText.Count && !isSpin)
            {
                SelectMiniGame();
               
                return;
            }
            else if(textIndex < StartText.Count) 
            {
                DummyText.DOText(StartText[textIndex], 1.0f);
            }
        }

    }

}
