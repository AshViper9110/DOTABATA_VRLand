using Assets.Scenes.Works.otake.script;
using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class GameManager : MonoBehaviour
{
   //public List<SceneAsset> miniGames = new List<SceneAsset>();

    //ミニゲームのUI配置関係
    public float radius;
    [SerializeField] GameObject MinigamePrefab;

    [SerializeField] GameObject CenterObj;
    Rigidbody CenterObjRb;

    [SerializeField] GameObject selectPoint;
    SelPointManager selPointManager;

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

        //ここで順位が決定されている状態だったらランキング処理のフラグを建てる
        //if (rankList[0] != 0)
        //{
        //    SetResult();
        //}
        SetRanking();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpin)
        {
            CenterObjRb.angularVelocity = new Vector3(0, 0.3f, 0);
        }
        else if (isSpin)
        {
            if (CenterObjRb.angularVelocity.y < 0.01f)
            {
                Debug.Log(selPointManager.SelectId+"にゲームが決まりました");
                //MoveScene(miniGames[selPointManager.SelectId]);
                enabled = false;
            }
        }
    }

    public void SelectMiniGame()
    {
        isSpin = true;
       float spinPower = Random.Range(3, 6);

       CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    //public void MoveScene(SceneAsset scene)
    //{
    //    //Initiate.Fade(scene.name, Color.black, 1.0f);
    //}

    public void SetMiniGame()
    {
        // 角度を計算
        float angle = 3 * Mathf.PI * 2 / 4;

        // 位置を計算 (X, Z平面)
        Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        selectPoint.transform.position = CenterObj.transform.position + pos;

        //int count = miniGames.Count;
        //for (int i = 0; i < count; i++)
        //{
        //    // 角度を計算
        //     angle = i * Mathf.PI * 2 / count;

        //    // 位置を計算 (X, Z平面)
        //     pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

        //    // 生成して回転を適用
        //    GameObject obj = Instantiate(MinigamePrefab, 
        //        CenterObj.transform.position + pos,
        //        Quaternion.identity,
        //        CenterObj.transform);

        //    MiniGameObjManager manager = obj.GetComponent<MiniGameObjManager>();
        //    manager.ID = i;

        //}
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
        Debug.Log("移動させます");
    }
}
