using DG.Tweening;
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

    [SerializeField] Transform center;
    SyncObjectManager objectManager;
    GameObject myFood;

    float timer;

    bool isCut;

    Dictionary<Guid, bool> CheckList = new Dictionary<Guid, bool>();

    bool sendReset;
    int point;

    int round;
    bool isScoreSend;

    [SerializeField] GameObject KnifePrefab;

    [SerializeField]MinigameFlowController flowController;

    Interactable myKnife;

    [SerializeField] GameObject HatPrefab;

    Dictionary<Guid, ChefHatManager> HatList = new Dictionary<Guid, ChefHatManager>();

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

        AudioManager.StopBgm();

        int pleIndex = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1;
        if (InRoomPlayerData.I.PlayerList != null)
        {
            foreach(var player in InRoomPlayerData.I.PlayerList)
            {
                PlayerTransform plTrans = player.Value.playerObj.GetComponent<PlayerTransform>();
                CheckList.Add(player.Value.joinedUser.ConnectionId,false);

                GameObject hat = Instantiate(HatPrefab,
                    plTrans.crownParent.position,
                    Quaternion.identity,
                    plTrans.crownParent);
                HatList.Add(player.Key,hat.GetComponent<ChefHatManager>());

                    plTrans.forward = false;
                
                player.Value.playerObj.transform.LookAt(center);
            }
        }
        InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.transform.position =
            PlayerPoses[pleIndex].transform.position;
        setpos = ResetPoses[pleIndex].transform;

        GameObject Knife = Instantiate(KnifePrefab,
            KnifPoses[pleIndex].transform.position, Quaternion.identity);

        Cutter KifeMane = Knife.GetComponent<Cutter>();
        KifeMane.ChengeHandle(pleIndex);
        KifeMane.sppatoManager = this;
        cutter = KifeMane;

        myKnife = Knife.GetComponent<Interactable>();
        
        flowController = GetComponent<MinigameFlowController>();

       

    }

    // Update is called once per frame
    void Update()
    {
        if(flowController.OnMove)
        {
            foreach(var c in HatList.Values)
            {
                Destroy(c.gameObject);
      
            }
            foreach (var player in InRoomPlayerData.I.PlayerList)
            {
                PlayerTransform plTrans = player.Value.playerObj.GetComponent<PlayerTransform>();

                plTrans.forward = true;
            }
            enabled = false;
            return;
        }

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
            AudioManager.ChangeBGM(AudioManager.BGM.Spatto);
        }

       


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



        GameObject food =Å@Instantiate(FoodPrefabs[index],setpos.position,Quaternion.identity);

        switch(InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder)
        {
            case 1:
                break;
                case 2:
                food.transform.Rotate(0,180,0);
                break;
                case 3:
                food.transform.Rotate(0, 270, 0);
                break;
            case 4:
                food.transform.Rotate(0, 90, 0);
                break;
                default:
                break;
        }

 


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

    public void CutFood(Guid playerId, Guid ID, Vector3 planePoint, Vector3 planeNormal,GameObject Target = null)
    {
        GameObject other = Target;
        if (Target == null)
        {
            List<GameObject> foods = new List<GameObject>();

            GameObject.FindGameObjectsWithTag("food", foods);
            other = foods[0];

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

            if (other == null) { return; }

        }
  



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

        AudioManager.PlaySE(AudioManager.SE.Spatto_cut);

        // éüÇ…êÿífÇ≈Ç´ÇÈéûçèÇê›íË 
        float nextTime = Time.time + cut.coolTime;

        Cuttable originalCut = original.GetComponent<Cuttable>();
        if (originalCut != null)
        {
            originalCut.nextCutTime = nextTime;
            originalCut.cutCount = cut.cutCount;
            originalCut.cutMaterial = cut.cutMaterial;
            SyncObject oriSync = original.GetComponent<SyncObject>();
            if (oriSync != null)
            {
                Destroy(oriSync);
            }

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

            SyncObject fraSync = fragment.GetComponent<SyncObject>();
            if (fraSync != null)
            {
                Destroy(fraSync);
            }

            if (fragmentCut.arrow != null)
            {
                Destroy(fragmentCut.arrow);
            }
        }

        SppatoManager.Register(original);
        SppatoManager.Register(fragment);

        // ColliderÇ1ï®óùÉtÉåÅ[ÉÄÇæÇØñ≥å¯âª
        StartCoroutine(EnableColliderNextFrame(original));
        StartCoroutine(EnableColliderNextFrame(fragment));

        if(playerId == Guid.Empty)return;

        if (!CheckList[playerId])
        {
            CheckList[playerId] = true;
        }

      
          
                int cnt = 0;
                foreach (bool check in CheckList.Values)
                {
                    if (check)
                    {
                        cnt++;
                    }
                }

        HatList[playerId].AddHatMid(5 - cnt); 


        if (playerId == NetworkManager.I.myConnectionId)
        {
            if (!isCut)
            {
            
                point += 5 - cnt;
                if (round >= 5)
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
