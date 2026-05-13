using Assets.Scenes.Works.otake.script;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
<<<<<<< HEAD
using Unity.Android.Gradle.Manifest;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.XR.CoreUtils;
using System;
=======
>>>>>>> main


public class GameManager : MonoBehaviour
{
    public List<string> miniGames = new List<string>();
    public List<string> miniGameNames = new List<string>();

    static public bool rally = true;
    static public bool freePlay = false;

    public List<Transform> playerPos = new List<Transform>();

    public InputActionReference rightHandPrimaryAction;
    InputAction action;

    [SerializeField] GameObject CrownPrefab;
    public float crownDistance;

    ///あとで消す
    [SerializeField]Transform crowntrans;
     
     
    
    /// <summary>
    /// 進行UI関係
    /// </summary>

    public Text MainText;
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

    [SerializeField] List<Sprite> miniGameTitleImages = new List<Sprite>();

    bool isSpin;

    //ランキングUI
    public List<RectTransform> rankingPosList;
    public List<RectTransform> rankingUis;



    public Dictionary<int, int> playerWinlist = new Dictionary<int, int>()
    {
        { 1,8},{2,0 },{3,1},{4,0}
    };//勝利数

    public List<Rank> RankingList = new List<Rank>()
    {
        new Rank(1,3),
        new Rank(2,1),
        new Rank(3,4),
        new Rank(4,2),
    };


    public List<Rank> miniRankingList = new List<Rank>()
    {
        new Rank(1,0),
        new Rank(2,0),
        new Rank(3,0),
        new Rank(4,0),
    };

    public int winPlayerId;

    TitleMana mana;

    private void Awake()
    {
        action = rightHandPrimaryAction.action;
        action.performed += MoveText;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //List<GameObject> crowns = new List<GameObject>();

        ////Transform transform = InRoomPlayerData.I.PlayerList[guid].playerObj.GetComponent<PlayerTransform>().crownParent.GetComponent<PlayerTransform>().crownParent;

        //for (int i = 0; i < playerWinlist[1]; i++)
        //{
        //    //GameObject crown = Instantiate(CrownPrefab,
        //    //    transform);
        //    //crowns.Add(crown);

        //    GameObject crown = Instantiate(CrownPrefab,
        //      crowntrans);
        //    crowns.Add(crown);
        //}
        //int index = 0;

        //foreach (GameObject crown in crowns)
        //{
        //    crown.transform.position = new Vector3(crown.transform.position.x, crownDistance * index, crown.transform.position.z);
        //    index++;
        //}

        // mana = GameObject.Find("TitleManager").GetComponent<TitleMana>();
        if (rally)
        {
            InitRally();
        }



     

    }

    // Update is called once per frame
    void Update()
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
                MainText.text = "";
                Debug.Log(miniGameNames[selPointManager.SelectId] + "にゲームが決まりました");
                MainText.DOText(miniGameNames[selPointManager.SelectId] + "にゲームが決まりました", 1.0f);

                onSelect = true;
                onResult = false;
                onEnd = false;

            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            
    

        }


        //TODO：自身がホストの場合はミニゲーム一覧の回転同期
    }

    public void InitRally()
    {
        SetMiniGame();
        CenterObjRb = CenterObj.GetComponent<Rigidbody>();
        selPointManager = selectPoint.GetComponent<SelPointManager>();
        isSpin = false;
        onSelect = false;
        onResult = false;
        onEnd = false;

       


        //シーン移行後の位置配置
        var myId = NetworkManager.I.myConnectionId;

        int index =
            InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position =
            playerPos[index].position;

        var rayInteractor = InRoomPlayerData.I.PlayerList[myId].playerObj.GetComponent<XRRayInteractor>();
        rayInteractor.attachTransform = InRoomPlayerData.I.PlayerList[myId].playerObj.transform;

        foreach(Guid guid in InRoomPlayerData.I.PlayerList.Keys)
        {
            SetCrown(guid, InRoomPlayerData.I.PlayerList[guid].joinedUser.JoinOrder);
        }


        //ここで順位が決定されている状態だったらランキング処理のフラグを建てる
        if (miniRankingList[0].rank != 0)
        {
            onResult = true;
            MainText.text = "";
            textIndex = 0;


            MainText.DOText(AfterText[textIndex], 1.0f);
            SetResult();
        }
        else
        {


            MainText.text = "";
            textIndex = 0;
            MainText.DOText(StartText[textIndex], 1.0f);
        }

       
        SetRanking();
    }

    //ミニゲーム抽選開始(ホストのみ実行)
    public void SelectMiniGame()
    {
        isSpin = true;
        //TODO：抽選開始通知

        float spinPower = UnityEngine.Random.Range(3, 6);

        CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    public void MoveScene(string scene)
    {

        Initiate.Fade(scene, Color.black, 1.0f);
    }

    public void SetMiniGame()
    {
        // 角度を計算
        float angle = 3 * Mathf.PI * 2 / 4;

        // 位置を計算 (X, Z平面)
        Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        selectPoint.transform.position = CenterObj.transform.position + pos;

        int count = miniGames.Count;
        for (int i = 0; i < count; i++)
        {
            // 角度を計算
            angle = i * Mathf.PI * 2 / count;

            // 位置を計算 (X, Z平面)
            pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            // 生成して回転を適用
            GameObject obj = Instantiate(MinigamePrefab,
                CenterObj.transform.position + pos,
                Quaternion.identity,
                CenterObj.transform);

            MiniGameObjManager manager = obj.GetComponent<MiniGameObjManager>();
            RawImage Image = manager.GetComponentInChildren<RawImage>();
            manager.ID = i;
            Image.texture = miniGameTitleImages[i].texture;
        }
    }

    public void SetCrown(Guid guid,int ID)
    {
        List<GameObject> crowns = new List<GameObject>();
        
        Transform transform = InRoomPlayerData.I.PlayerList[guid].playerObj.GetComponent<PlayerTransform>().crownParent.GetComponent<PlayerTransform>().crownParent;

        for (int i = 0; i < playerWinlist[ID]; i++)
        {
            GameObject crown = Instantiate(CrownPrefab,
                transform);
            crowns.Add(crown);


        }
        int index = 0;

        foreach (GameObject crown in crowns)
        {
            crown.transform.position = new Vector3(crown.transform.position.x,crownDistance*index,crown.transform.position.z);
            index++;
        }
    }
    public void SetResult()
    {
        for (int i = 0; i < miniRankingList.Count; i++)
        {

            if (miniRankingList[i].rank == 1)
            {

                winPlayerId = miniRankingList[i].Id;
                Debug.Log(miniRankingList[i].Id + "  " + miniRankingList[i].rank);
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

            RankingList[ID - 1].rank = index;
            index++;

        }

        for (int i = 0; i < RankingList.Count; i++)
        {
            rankingUis[i].DOAnchorPosY(rankingPosList[RankingList[i].rank - 1].anchoredPosition.y, 1f);
        }

    }

    private void MoveText(InputAction.CallbackContext context)
    {
        textIndex++;
        MainText.text = "";
        if (onResult)
        {
            if (textIndex >= AfterText.Count && !isSpin)
            {
                SelectMiniGame();
                return;
            }

            if (AfterText[textIndex] == "!!! おめでとう！")
            {

                MainText.DOText($"プレイヤー{winPlayerId}" + AfterText[textIndex], 1.0f);
                playerWinlist[RankingList[winPlayerId - 1].Id]++;

                SetRanking();

                if (playerWinlist[RankingList[winPlayerId - 1].Id] >= 3)
                {
                    onEnd = true;
                    onResult = false;
                    textIndex = -1;
                }
            }
            else
            {
                MainText.DOText(AfterText[textIndex], 1.0f);
            }
        }
        else if (onEnd)
        {
            if (textIndex >= FinishText.Count)
            {
                //タイトルに戻る
                Initiate.Fade("GameScene", Color.black, 1.0f);
                return;
            }

            if (FinishText[textIndex] == "!!! おめでとう！")
            {

                MainText.DOText($"プレイヤー{winPlayerId}" + FinishText[textIndex], 1.0f);

            }
            else
            {
                MainText.DOText(FinishText[textIndex], 1.0f);
            }
        }
        else if (onSelect)
        {
            MoveScene(miniGames[selPointManager.SelectId]);
        }
        else
        {
            if (textIndex >= StartText.Count && !isSpin)
            {
                SelectMiniGame();
                return;
            }
            MainText.DOText(StartText[textIndex], 1.0f);
        }

    }

}
