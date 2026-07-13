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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitBall();
        }

    private void OnDestroy()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        BombTimer -= Time.deltaTime;

        var hand = GetComponentInParent<Hand>();
        if (BombTimer <= 0)
        {
            if (syncObject.IsOwner)
            {
                BombTimer = 0;

                Instantiate(BomberEffectPrefab,
                  transform.position,
                    Quaternion.identity);
                bombDodgeManager.StartCreateBall();
            }


            if (hand != null)
            {
                hand.DetachObject(gameObject);
            }

            Destroy(this.gameObject);
           
            
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

         hand = interactable.attachedToHand;

        if (hand != null)
        {
            if (!syncObject.IsOwner)
            {
                if(InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<BombDogePlayer>().isDead)return;
                syncObject.GetOwnership(true);
                RestartPos = bombDodgeManager.BombStartpos[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].position;
                Debug.Log("君の物だよ");
                AudioManager.PlaySE(AudioManager.SE.Bomb_Catch);
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
               
                RoomModel.I.HitDodgeBall();
            }
        }

    }

    

}
