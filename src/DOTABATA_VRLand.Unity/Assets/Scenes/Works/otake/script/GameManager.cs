using Assets.Scenes.Works.otake.script;
using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.ShaderData;


public class GameManager : MonoBehaviour
{
   public List<SceneAsset> miniGames = new List<SceneAsset>();

    /// <summary>
    /// 進行UI関係
    /// </summary>

    public Text MainText;
    public int textIndex;

    public bool onResult;

    //進行テキスト(最初)
    List<string> StartText = new List<string>()
    {
        "ミニゲーム大会を始めるよ！",
        "先に三勝したプレイヤーが勝ちだよ！",
        "それじゃあ早速ミニゲームを決めていくよ！"
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

    //あとで消す変数
    List<int> rankList = new List<int>();

    public List<int> playerWinlist = new List<int>();//勝利数

    public List<Rank> RankingList = new List<Rank>()
    {
        new Rank(1,2),
        new Rank(2,4),
        new Rank(3,3),
        new Rank(4,1),
    };

  

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMiniGame();
        CenterObjRb = CenterObj.GetComponent<Rigidbody>();
        selPointManager = selectPoint.GetComponent<SelPointManager>();
        isSpin = false;
        onResult = false;

        //ここで順位が決定されている状態だったらランキング処理のフラグを建てる
        if (rankList[0] != 0)
        {
            onResult = true;
        }
        SetRanking();

        MainText.text = "";
        textIndex = 0;
        MainText.DOText(StartText[textIndex],1.0f);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpin)
        {
            if (CenterObjRb.angularVelocity.y < 0.29f) {
                CenterObjRb.angularVelocity = new Vector3(0, 0.3f, 0);
                }
        }
        else if (isSpin)
        {
            if (CenterObjRb.angularVelocity.y < 0.01f)
            {
                Debug.Log(selPointManager.SelectId + "にゲームが決まりました");
                MoveScene(miniGames[selPointManager.SelectId]);
                enabled = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            textIndex++;
            MainText.text = "";
            if (textIndex >= StartText.Count)
            {
                SelectMiniGame();
                return;
            }
           
            MainText.DOText(StartText[textIndex], 1.0f);
        }


        //TODO：自身がホストの場合はミニゲーム一覧の回転同期
    }

    //ミニゲーム抽選開始(ホストのみ実行)
    public void SelectMiniGame()
    {
        isSpin = true;
        //TODO：抽選開始通知

       float spinPower = Random.Range(3, 6);

       CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    public void MoveScene(SceneAsset scene)
    {
        Initiate.Fade(scene.name, Color.black, 1.0f);
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

    public void SetResult()
    {
        for (int i = 0; i < rankList.Count; i++)
        {
            if(rankList[i] == 1)
            {
                playerWinlist[i]++;
            }
        }
    }

    public void SetRanking()
    {
        

        for (int i = 0; i < RankingList.Count; i++)
        {
            rankingUis[i].DOAnchorPosY(rankingPosList[RankingList[i].rank-1].anchoredPosition.y,1f);
        }
   
    }

    //テキスト遷移
    public void MoveText()
    {

    }
}
