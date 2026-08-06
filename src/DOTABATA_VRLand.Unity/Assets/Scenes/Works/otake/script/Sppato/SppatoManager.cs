using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.VisualScripting;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class SppatoManager : MonoBehaviour
{
    bool isInit;

    public static List<GameObject> MineFragments = new List<GameObject>();

    Transform setpos;
    [SerializeField] GameObject prefab;
    [SerializeField] Cutter cutter;

    [SerializeField] List<GameObject> FoodPrefabs;
    [SerializeField] List<GameObject> PlayerPoses;
    [SerializeField] List<GameObject> ResetPoses;
    [SerializeField] List<GameObject> KnifPoses;

    SyncObjectManager objectManager;
    GameObject myFood;

    float timer;
    [SerializeField] TextMeshProUGUI timerTex;
    bool isCut;

    Dictionary<Guid, bool> CheckList = new Dictionary<Guid, bool>();

    bool sendReset;
    int point;

    int round;
    bool isScoreSend;

    [SerializeField] GameObject KnifePrefab;

    [SerializeField]MinigameFlowController flowController;

    Interactable myKnife;

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCutingFood += CutFood;
        RoomModel.I.OncreatedFood += ResteObject;
   
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnCutingFood -= CutFood;
        RoomModel.I.OncreatedFood -= ResteObject;

    }

    void Start()
    {
        objectManager = GameObject.Find("NetworkManager").GetComponent<SyncObjectManager>();
        timer = 0;
        sendReset = false;
        point = 0;
        round = 0;
        isScoreSend = false;
        isInit = false;

        int pleIndex = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1;
        if (InRoomPlayerData.I.PlayerList != null)
        {
            foreach(var player in InRoomPlayerData.I.PlayerList)
            {
                CheckList.Add(player.Value.joinedUser.ConnectionId,false);
            }
        }
        InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.transform.position =
            PlayerPoses[pleIndex].transform.position;
        setpos = ResetPoses[pleIndex].transform;

        GameObject Knife = Instantiate(KnifePrefab,
            KnifPoses[pleIndex].transform.position, Quaternion.identity);

        Cutter KifeMane = Knife.GetComponent<Cutter>();
        KifeMane.ChengeHandle(pleIndex);
        cutter = KifeMane;

        myKnife = Knife.GetComponent<Interactable>();
        
        flowController = GetComponent<MinigameFlowController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!flowController.isGameStarted)
        {
           

            if (!flowController.willReady)
            {

                if (myKnife.attachedToHand)
                //|| InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == order)
                {
                    flowController.OnReadyButton();
                }

            }
            else
            {
                if (flowController.AllReady && InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
                {
                    flowController.GameStrat();
                }


                if (!myKnife.attachedToHand )
                //|| InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == order)
                {
                    if (!flowController.OnStarted)
                    {
                        flowController.OnReadyButton();

                    }
                    return;
                }


            }
            return;
        }


        if (flowController.isGameStarted && !isInit)
        {
            if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
            {

                int index = UnityEngine.Random.Range(0, FoodPrefabs.Count);

                RoomModel.I.CreateFood(index);
            }
            isInit = true;
        }

        timerTex.text = point.ToString(); 


    }

    public static void Register(GameObject obj)
    {
        MineFragments.Add(obj);
    }

    public static void DestroyAll()
    {
        foreach (GameObject obj in MineFragments)
        {
            if (obj != null)
                Destroy(obj);
        }

        MineFragments.Clear();
    }

    public void ResteObject(int index)
    {
        SppatoManager.DestroyAll();



        GameObject food =　Instantiate(FoodPrefabs[index],setpos.position,Quaternion.identity);
        
        cutter.CutOk = false;
        cutter.cutCount = 0;
        SppatoManager.Register(food);
        myFood = food;
        sendReset = false;
        isCut = false;
        round++;
    }

    public void  SetObject(int index)
    {

    }

    public void CutFood(Guid playerId, Guid ID, Vector3 planePoint, Vector3 planeNormal)
    {
        

        List<GameObject> foods  = new List<GameObject>();

       GameObject.FindGameObjectsWithTag("food", foods);
        GameObject other = foods[0];

        foreach (var item in foods)
        {
            if (item.GetComponent<SyncObject>())
            {
                if (item.GetComponent<SyncObject>().ObjectId == ID)
                {
                    other = item;
                }
     
            }

        }

  　　  if(other == null) {  return; }

  



        MeshFilter mf = other.GetComponent<MeshFilter>();
        if (mf == null)
            return;

        Cuttable cut = other.GetComponent<Cuttable>();
        if (cut == null)
            return;

        var (fragment, original) = MeshCut.CutMesh(
            other.gameObject,
            planePoint,
            planeNormal,
            true,
            cut.cutMaterial);

        if (fragment == null || original == null)
            return;

        cut.cutCount++;
        


        // 次に切断できる時刻を設定 
        float nextTime = Time.time + cut.coolTime;

        Cuttable originalCut = original.GetComponent<Cuttable>();
        if (originalCut != null)
        {
            originalCut.nextCutTime = nextTime;
            originalCut.cutCount = cut.cutCount;
            originalCut.cutMaterial = cut.cutMaterial;

            if (originalCut.arrow != null)
            {
                Destroy(originalCut.arrow);
            }
        }

        Cuttable fragmentCut = fragment.GetComponent<Cuttable>();
        if (fragmentCut != null)
        {
            fragmentCut.nextCutTime = nextTime;
            fragmentCut.cutCount = cut.cutCount;
            fragmentCut.cutMaterial = cut.cutMaterial;
            if (fragmentCut.arrow != null)
            {
                Destroy(fragmentCut.arrow);
            }
        }

        SppatoManager.Register(original);
        SppatoManager.Register(fragment);

        // Colliderを1物理フレームだけ無効化
        StartCoroutine(EnableColliderNextFrame(original));
        StartCoroutine(EnableColliderNextFrame(fragment));

        if (!CheckList[playerId])
        {
            CheckList[playerId] = true;
        }

        if (!isCut)
        {
            if (playerId == NetworkManager.I.myConnectionId)
            {
                int cnt = 0;
                foreach (bool check in CheckList.Values)
                {
                    if (check)
                    {
                        cnt++;
                    }
                }

                point += 5 - cnt;
                if (round >= 4)
                {
                    RoomModel.I.SendScore(point);
                    isScoreSend = true;

                    if (myKnife.attachedToHand != null)
                    {
                        myKnife.attachedToHand.DetachObject(myKnife.gameObject);
                    }

                    Destroy(myKnife.gameObject);
                    return;
                }
                isCut = true;
            }
        }


        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
        {
            foreach (bool check in CheckList.Values)
            {
                if(!check)
                {
                    return;
                }
            }


            if (!sendReset &&!isScoreSend)
            {
                StartCoroutine(ResetFoodsTimer());
                sendReset = true;
            }
        }




    }

    IEnumerator EnableColliderNextFrame(GameObject obj)
    {
        if (obj == null)
            yield break;

        Collider col = obj.GetComponent<Collider>();

        if (col == null)
            yield break;

        col.enabled = false;

        yield return new WaitForFixedUpdate();

        if (col != null)
            col.enabled = true;
    }

    IEnumerator ResetFoodsTimer()
    {
        yield return new WaitForSeconds(2);

        int index = UnityEngine.Random.Range(0, FoodPrefabs.Count);

        RoomModel.I.CreateFood(index);



    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
