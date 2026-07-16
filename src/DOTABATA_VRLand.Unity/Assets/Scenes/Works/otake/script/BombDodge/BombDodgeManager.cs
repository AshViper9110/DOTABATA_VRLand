using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR;

public class BombDodgeManager : MonoBehaviour
{
    [SerializeField] GameObject EngelRingPrefab;
    [SerializeField] List<Transform> startpos;
    [SerializeField]public  List<Transform> BombStartpos;
    [SerializeField] Transform  center;

    [SerializeField] GameObject BombPrefab;
    public BombBallManager Bomb;

    bool isStart;

    MinigameFlowController flowController;

    [SerializeField] Canvas introCanvas;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnHitingDodgeBall += OnHitingDodgeBall;
        RoomModel.I.OnHitingBomber += OnHitingBomber;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnHitingDodgeBall -= OnHitingDodgeBall;
        RoomModel.I.OnHitingBomber -= OnHitingBomber;
    }
    void Start()
    {
        AudioManager.StopBgm();
        SteamVR_Fade.Start(new Color(0, 0, 0, 0), 1.0f);
        int index = 0;
        isStart = false;
        
        foreach (var obj in InRoomPlayerData.I.PlayerList.Values)
        {
            if (!obj.playerObj.GetComponent<BombDogePlayer>())
            {
                BombDogePlayer dogePlayer = obj.playerObj.AddComponent<BombDogePlayer>();
                dogePlayer.EngelRing = Instantiate(EngelRingPrefab,
                    obj.playerObj.transform.position,
                    Quaternion.identity,
                    obj.playerObj.transform);
                dogePlayer.EngelRing.transform.Rotate(90,0,0);
            }

            if (obj.joinedUser.ConnectionId == NetworkManager.I.myConnectionId) {
                obj.playerObj.transform.position = startpos[obj.joinedUser.JoinOrder - 1].position;
                obj.playerObj.transform.LookAt(center);
                introCanvas.transform.LookAt(obj.playerObj.transform);
                introCanvas.transform.Rotate(0,180,0);
                introCanvas.transform.rotation = new Quaternion(0, introCanvas.transform.rotation.y,0, introCanvas.transform.rotation.w);
                    }
            index++;

           
        }

        flowController = GetComponent<MinigameFlowController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (flowController.isGameStarted && !isStart)
        {
            if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
            {
                GameObject gameObject = Instantiate(BombPrefab,
                    BombStartpos[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].position,
                    Quaternion.identity);
                gameObject.GetComponent<BombBallManager>().RestartPos = gameObject.transform.position;
                Bomb = gameObject.GetComponent<BombBallManager>();

            }
            AudioManager.ChangeBGM(AudioManager.BGM.Bom_doge);
            isStart = true;
        }

        if(!flowController.isGameStarted && isStart)
        {

            DestroyEngelRing();
            if (Bomb != null)
            {
                Bomb = null;
            }
        }
    }

    void DestroyEngelRing()
    {
        foreach (var obj in InRoomPlayerData.I.PlayerList.Values)
        {
            if (obj.playerObj.GetComponent<BombDogePlayer>())
            {
                BombDogePlayer dogePlayer = obj.playerObj.GetComponent<BombDogePlayer>();
                Destroy(dogePlayer.EngelRing);
                Destroy(dogePlayer);
            }
         
            

        }
    }

    public void OnHitingDodgeBall(Guid ConnectionId)
    {
        if (Bomb != null)
        {
            Bomb.RestartPos = BombStartpos[InRoomPlayerData.I.PlayerList[ConnectionId].joinedUser.JoinOrder - 1].position;
        }
    }

    public void OnHitingBomber(Guid ConnectionId)
    {
        BombDogePlayer player = InRoomPlayerData.I.PlayerList[ConnectionId].playerObj.GetComponent<BombDogePlayer>();
        if (player == null)
        {
            Debug.LogAssertion("マネージャー無かった");
            return;
        }
            player.isDead = true;

        if(NetworkManager.I.myConnectionId == ConnectionId)
        {
            flowController.OnSendScore(0);
        }

        foreach (var p in InRoomPlayerData.I.PlayerList.Values)
        {
            {
                if (p.joinedUser.ConnectionId == NetworkManager.I.myConnectionId)
                {
                    continue;
                }
                BombDogePlayer b = p.playerObj.GetComponent<BombDogePlayer>();
                if (b != null)
                {
                    if (!b.isDead)
                    {//まだ生存者がいたら続行
                        Debug.Log("続行");
                       return ;
                    }
                }
            }

        }

        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<BombDogePlayer>().isDead == false)
        {
            flowController.OnSendScore(1);
        }
    }

    public void StartCreateBall()
    {
        StartCoroutine(CreateBall());
    }


    public IEnumerator CreateBall()
    {

        yield return new WaitForSeconds(3);
        if (!flowController.isGameStarted && isStart) yield break;
        Debug.Log("CreateBall");
        int index = 3;
        foreach (var t in InRoomPlayerData.I.PlayerList.Values)
        {
            BombDogePlayer dogePlayer = t.playerObj.GetComponent<BombDogePlayer>();
            if (dogePlayer != null)
            {
                if (!dogePlayer.isDead)
                {
                    index = t.joinedUser.JoinOrder - 1;
                    break;
                }
            }
        }

        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == index + 1)
        {

            GameObject gameObject = Instantiate(BombPrefab,
                       BombStartpos[index].position,
                       Quaternion.identity);
            BombBallManager bombBallManager = gameObject.GetComponent<BombBallManager>();
            bombBallManager.RestartPos = gameObject.transform.position;
            bombBallManager.InitBall();
        }
    }
}
