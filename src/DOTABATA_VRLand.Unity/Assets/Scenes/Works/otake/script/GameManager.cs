using Assets.Scenes.Works.otake.script;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;


public class GameManager : MonoBehaviour
{
    public List<string> miniGames = new List<string>();

    static public bool rally = true;
    static public bool freePlay = false;

    public List<Transform> playerPos = new List<Transform>();

    public InputActionReference rightHandPrimaryAction;
    InputAction action;

    /// <summary>
    /// �i�sUI�֌W
    /// </summary>

    public Text MainText;
    public int textIndex;

    public bool onSelect;
    public bool onResult;
    public bool onEnd;

    //�i�s�e�L�X�g(�ŏ�)
    List<string> StartText = new List<string>()
    {
        "�~�j�Q�[������n�߂��I",
        "��ɎO�������v���C���[����������I",
        "���ꂶ�Ⴀ�����~�j�Q�[������߂Ă�����I"
    };

    //�i�s�e�L�X�g(�~�j�Q�[����)
    List<string> AfterText = new List<string>()
    {
        "�~�j�Q�[�������l!",
        "���񏟂����ЂƂ�...",
        "!!! ���߂łƂ��I",//���Ƃ��珟�����v���C���[����}��,
        "���ꂶ�Ⴀ���̃~�j�Q�[������߂Ă�����!",
    };

    //�i�s�e�L�X�g(���C���Q�[���I����)
    List<string> FinishText = new List<string>()
    {
        "�����ŃQ�[�����̏��҂����܂����݂�������",
        "����D�������l��...",
        "!!! ���߂łƂ��I",//���Ƃ��珟�����v���C���[����}��,
        "���݂̂�Ȃ�V��ł���Ă��肪�Ƃ��I",
        "�܂��V��łˁI�o�C�o�[�C�I"
    };

    //�~�j�Q�[����UI�z�u�֌W
    public float radius;
    [SerializeField] GameObject MinigamePrefab;

    [SerializeField] GameObject CenterObj;
    Rigidbody CenterObjRb;

    [SerializeField] GameObject selectPoint;
    SelPointManager selPointManager;

    [SerializeField] List<Sprite> miniGameTitleImages = new List<Sprite>();

    bool isSpin;

    //�����L���OUI
    public List<RectTransform> rankingPosList;
    public List<RectTransform> rankingUis;



    public Dictionary<int, int> playerWinlist = new Dictionary<int, int>()
    {
        { 1,1 },{2,0 },{3,1},{4,0}
    };//������

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
                Debug.Log(selPointManager.SelectId + "�ɃQ�[�������܂�܂���");
                MainText.DOText(selPointManager.SelectId + "�ɃQ�[�������܂�܂���", 1.0f);

                onSelect = true;
                onResult = false;
                onEnd = false;

            }
        }


        if (Input.GetMouseButtonDown(0))
        {
            
    

        }


        //TODO�F���g���z�X�g�̏ꍇ�̓~�j�Q�[���ꗗ�̉�]����
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

        
        //�V�[���ڍs��̈ʒu�z�u
        var myId = NetworkManager.I.myConnectionId;

        int index =
            InRoomPlayerData.I.PlayerList[myId].joinedUser.JoinOrder - 1;

        InRoomPlayerData.I.PlayerList[myId].playerObj.transform.position =
            playerPos[index].position;

        


        //�����ŏ��ʂ����肳��Ă����Ԃ������烉���L���O�����̃t���O����Ă�
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

    //�~�j�Q�[�����I�J�n(�z�X�g�̂ݎ��s)
    public void SelectMiniGame()
    {
        isSpin = true;
        //TODO�F���I�J�n�ʒm

        float spinPower = Random.Range(3, 6);

        CenterObjRb.angularVelocity = new Vector3(0, spinPower, 0);
    }

    public void MoveScene(string scene)
    {

        Initiate.Fade(scene, Color.black, 1.0f);
    }

    public void SetMiniGame()
    {
        // �p�x��v�Z
        float angle = 3 * Mathf.PI * 2 / 4;

        // �ʒu��v�Z (X, Z����)
        Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        selectPoint.transform.position = CenterObj.transform.position + pos;

        int count = miniGames.Count;
        for (int i = 0; i < count; i++)
        {
            // �p�x��v�Z
            angle = i * Mathf.PI * 2 / count;

            // �ʒu��v�Z (X, Z����)
            pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            // �������ĉ�]��K�p
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
        //�������Ń\�[�g��ID�Q�ƂŃ����L���O�t�����e�L�X�g����ւ�

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

            if (AfterText[textIndex] == "!!! ���߂łƂ��I")
            {

                MainText.DOText($"�v���C���[{winPlayerId}" + AfterText[textIndex], 1.0f);
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
                //�^�C�g���ɖ߂�
                Initiate.Fade("GameScene", Color.black, 1.0f);
                return;
            }

            if (FinishText[textIndex] == "!!! ���߂łƂ��I")
            {

                MainText.DOText($"�v���C���[{winPlayerId}" + FinishText[textIndex], 1.0f);

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
