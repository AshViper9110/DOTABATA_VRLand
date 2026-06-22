using DG.Tweening;
using NUnit.Framework;
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

    [SerializeField]GameObject BomberObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BombTimer = BombTimerMax;
        interactable = gameObject.GetComponent<Interactable>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        BombTimer -= Time.deltaTime;

        if (BombTimer <= 0)
        {
            BombTimer = 0;
            BomberObj.SetActive(true);
            Destroy(this.gameObject, 3.0f);
            enabled = false;
        }


        BombTimerText.text = (Mathf.Floor(BombTimer * 10)/10).ToString();

       
        BombTimerText.transform.LookAt(Camera.main.transform);
        BombTimerText.transform.Rotate(0, 180, 0);

        

        //if (interactable.isHovering)
        //{
        //    var hand = interactable.hoveringHand;

        //    if (hand != null && interactable.attachedToHand == null && grabAction.GetStateDown(hand.handType))
        //    {
        //        // 手のモデルを非表示（通常の掴みと同じ挙動）
        //        hand.HideController();
        //        hand.otherHand.HideController();

        //        hand.AttachObject(
        //        interactable.gameObject,
        //        GrabTypes.Scripted,
        //        Hand.AttachmentFlags.ParentToHand
        //        | Hand.AttachmentFlags.SnapOnAttach
        //        | Hand.AttachmentFlags.DetachOthers
        //        | Hand.AttachmentFlags.VelocityMovement
        //    );

        //        interactable.attachedToHand = hand;
              
        //    }
        //}

        //// 離されたら手を再表示
        //if (interactable.attachedToHand == null)
        //{
        //    var hand = interactable.hoveringHand;
        //    if (hand.grabGripAction != null)
        //    {
        //        hand.ShowController();
        //        hand.otherHand.ShowController();
        //        hand.DetachObject( interactable.gameObject );
        //    }
        //}

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

            if (other.transform.parent.transform.parent.gameObject.GetComponent<PlayerTransform>())
            {
                Debug.Log("あててんのよ♡");
            }
        }

    }
   
}
