using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;


[RequireComponent(typeof(Interactable))]
public class BombBallManager : MonoBehaviour
{
    [SerializeField] float BombTimerMax;
    float BombTimer;
    [SerializeField] TextMeshProUGUI BombTimerText;
    Interactable interactable;

    public Vector3 RestartPos;

    public SteamVR_Action_Boolean grabAction;

    Rigidbody rb;

    [SerializeField] GameObject BomberEffectPrefab;

    SyncObject syncObject;

    BombDodgeManager bombDodgeManager;

    bool isHaving;
    float haveTimer;
    float notHaveTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitBall();
        haveTimer = 0;
        }

    private void OnDestroy()
    {
       
    }

    // Update is called once per frame
    void Update()
    {


        var hand = GetComponentInParent<Hand>();

        hand = interactable.attachedToHand;

        if (hand != null)
        {
            if (!syncObject.IsOwner)
            {
                if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<BombDogePlayer>().isDead) return;
                syncObject.GetOwnership(true);
                RestartPos = bombDodgeManager.BombStartpos[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].position;
              
                AudioManager.PlaySE(AudioManager.SE.Bomb_Catch);
            }

        }

        BombTimer -= Time.deltaTime;


        if (BombTimer <= 0)
        {

         

            if (syncObject.IsOwner)
            {
                Debug.Log("生成チャレンジ");
                bombDodgeManager.StartCreateBall();

                BombTimer = 0;
                Debug.Log("爆発生成");
                Instantiate(BomberEffectPrefab,
                  transform.position,
                    Quaternion.identity);


                if (hand != null)
                {
                    hand.DetachObject(gameObject);
                }

                bombDodgeManager.Bomb = null;
                Destroy(this.gameObject);

            }
            enabled = false;
            
        }
        
        if(syncObject.IsOwner)
        {
            rb.useGravity = true;
        }
        else
        {
            rb.useGravity = false;

            
        }


        BombTimerText.text = (Mathf.Floor(BombTimer * 10)/10).ToString();

       
        BombTimerText.transform.LookAt(Camera.main.transform);
        BombTimerText.transform.Rotate(0, 180, 0);

        if (interactable.attachedToHand)
        {
            isHaving = true;
            notHaveTimer = 0;
        }
        else
        {
            isHaving = false;
            BombTimerText.gameObject.SetActive(true);
            haveTimer = 0;
        }

        if (isHaving)
        {
            haveTimer += Time.deltaTime;

            if (haveTimer > 2f)
            {
                BombTimerText.gameObject.SetActive(false);
            }
        }
        else
        {
            notHaveTimer += Time.deltaTime;

            if (notHaveTimer > 2f)
            {
                BombTimerText.gameObject.SetActive(false);
            }
        }

            


    }

    public void InitBall()
    {
        BombTimer = BombTimerMax;
        interactable = gameObject.GetComponent<Interactable>();
        rb = gameObject.GetComponent<Rigidbody>();
        syncObject = gameObject.GetComponent<SyncObject>();
        bombDodgeManager = GameObject.Find("GameManager").GetComponent<BombDodgeManager>();
      
        bombDodgeManager.Bomb = this;
    }



    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Ground")
        {
            this.transform.position = RestartPos;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        if (other.transform.parent != null && other.transform.parent.parent != null)
        {

            if (other.transform.parent.transform.parent.gameObject == InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj)
            {
                if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<BombDogePlayer>().isDead) return;

                syncObject.GetOwnership(true);
                Debug.Log("君の物だよ");
                RoomModel.I.HitDodgeBall();
            }
        }

    }

    

}
