using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;
using static UnityEngine.Rendering.DebugUI.Table;
using Hand = UnityEngine.XR.Hand;

public class MufflerSetManager : MonoBehaviour
{
   [SerializeField] int order;

    [Header("棒関係")]
    [SerializeField] GameObject Rodprefab;
    [SerializeField] Vector3 RightRodPos;
    [SerializeField] Vector3 LeftRodPos;
    [SerializeField] GameObject RightRod;
    [SerializeField] GameObject LeftRod;
    ParticleSystem RightEffect;
    ParticleSystem LeftEffect;
    Interactable RightInteractable;
    Interactable LeftInteractable;
    Vector3 TempRightPos;
    Vector3 TempLeftPos;

    [Header("マフラー関係")]
    [SerializeField] List<GameObject> nitPrefabs = new List<GameObject>();
    [SerializeField] List<Material> materials = new List<Material>();
    int nitIndex;
    int indexVector;
    [SerializeField] Transform nitsParent;
    public float distans;
    public int nitCount;
    public int nitLate = 3;//伸び率

    [Header("SE関係")]
    [SerializeField] AudioSource nitsAudioSource;


    public float point;
    public float tempPoint = 0;


    NitnitManager nitManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        nitsAudioSource = GetComponent<AudioSource>();

        nitCount = 0;

        nitIndex = 0;
        indexVector = 1;


        nitManager = GameObject.Find("GameManager").GetComponent<NitnitManager>();

        tempPoint = 0;
        point = 0;


    }

    // Update is called once per frame
    void Update()
    {
        if(order != InRoomPlayerData.I.PlayerList[RoomModel.I.ConnectionId].joinedUser.JoinOrder)return;
        if (!nitManager.FlowController.isGameStarted)
        {
            RightEffect.Stop();
            LeftEffect.Stop(); 
            nitsAudioSource.Stop();
            return;
        }
   

        if (!RightInteractable.attachedToHand || !LeftInteractable.attachedToHand)
            //|| InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == order)
        {
            RightEffect.Stop();
            LeftEffect.Stop();
            nitsAudioSource.Stop();
            return;
        }
        Vector3 RightVector = (RightRod.transform.position - TempRightPos);
        Vector3 LeftVector = (LeftRod.transform.position - TempLeftPos);



        float sqrtRight = Mathf.Sqrt(Mathf.Abs(RightVector.y));
        sqrtRight = Mathf.Floor(sqrtRight * 10) / 10;
        float sqrtLeft = Mathf.Sqrt(Mathf.Abs(LeftVector.y));
        sqrtLeft = Mathf.Floor(sqrtLeft * 10) / 10;

        float temp = Mathf.Abs(sqrtRight + sqrtLeft);
        temp = Mathf.Floor(temp * 10) / 10;

        temp = temp * nitLate;        //Debug.Log(temp);

        point += temp;

        if (point > nitManager.MaxPoint)
        {
            point = nitManager.MaxPoint;
            RightEffect.Stop();
            LeftEffect.Stop();
            nitsAudioSource.Stop();
        }



        if (Mathf.Floor(point - tempPoint) >= 1)
        {
           
                //サーバーに自身のマフラー追加を送信、ポイント更新
                NetworkManager.I.UpdateNit(NetworkManager.I.myConnectionId,point);
                
            
          

            RightEffect.Play();
            LeftEffect.Play();
            nitsAudioSource.Play();
        }
        else
        {
            RightEffect.Stop();
            LeftEffect.Stop();
            nitsAudioSource.Stop();
        }



        TempRightPos = RightRod.transform.position;
        TempLeftPos = LeftRod.transform.position;
    }

    public void addNit(float point)
    {

  
        for (int i = 0; i < (int)point - this.tempPoint; i++)
        {
           
            GameObject nit = Instantiate(nitPrefabs[nitIndex], nitsParent);
            nit.transform.position = new Vector3(nit.transform.position.x , nit.transform.position.y, nit.transform.position.z - (distans * nitCount));
            if (indexVector == -1)
            {
                nit.transform.Rotate(0, 180, 0);
                nit.GetComponent<MeshRenderer>().material = materials[1];
            }
            else
            {
                nit.GetComponent<MeshRenderer>().material = materials[0];
            }
            //奥に移動させる
            nitsParent.position = new Vector3(nitsParent.transform.position.x , nitsParent.transform.position.y, nitsParent.gameObject.transform.position.z + (distans));
            nitCount++;
            nitIndex += indexVector;


            if (nitIndex >= nitPrefabs.Count)
            {
                indexVector = -indexVector;
                nitIndex = nitPrefabs.Count - 1;
            }
            else if (nitIndex < 0)
            {
                indexVector = -indexVector;
                nitIndex = 0;
            }
            this.point = point;
            tempPoint = point;
        }

    }

    public void CreateRod()
    {
       
        GameObject right = Instantiate(Rodprefab
            ,transform.position + RightRodPos,
            new Quaternion(0, 0, 0, 0),
            transform);
        RightRod = right;
       

        GameObject left = Instantiate(Rodprefab
          , transform.position + LeftRodPos,
          new Quaternion(0,0,0,0),
          transform);
        LeftRod = left;
       

        TempRightPos = RightRod.transform.position;
        TempLeftPos = LeftRod.transform.position;
        RightInteractable = RightRod.GetComponent<Interactable>();
        LeftInteractable = LeftRod.GetComponent<Interactable>();

        RightEffect = RightRod.GetComponentInChildren<ParticleSystem>();
        LeftEffect = LeftRod.GetComponentInChildren<ParticleSystem>();
        RightEffect.Stop();
        LeftEffect.Stop();
    }

    public void DeleteRod()
    {
        if (RightRod != null)
        {
            Interactable rightIntara = RightRod.GetComponent<Interactable>();
            Interactable leftIntara = LeftRod.GetComponent<Interactable>();

            if (rightIntara.attachedToHand != null)
            {
               rightIntara.attachedToHand.DetachObject(RightRod);
            }
            if (leftIntara.attachedToHand != null)
            {
                leftIntara.attachedToHand.DetachObject(LeftRod);
            }




            Destroy(RightRod);
            Destroy(LeftRod);

            enabled = false;
        }
    }
}
