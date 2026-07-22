using UnityEngine;
using Valve.VR.InteractionSystem;


[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(Rigidbody))]
public class GarageManager : MonoBehaviour
{
    public enum SlideAxis
    {
        X,
        Y,
        Z
    }

    [Header("Slide")]
    public SlideAxis slideAxis = SlideAxis.X;

    public float minPosition = 0f;
    public float maxPosition = 1f;

    Rigidbody rb;

    private Hand currentHand;
    private bool grabbed;

    private Vector3 initialLocalPosition;
    private float grabOffset;

    private float openPosX = -2.0f;
    bool isOpen;


    public LaneSetManager laneSetManager;

    public  MinigameFlowController controller;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
 

        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        initialLocalPosition = transform.localPosition;

        isOpen = false;

        controller = GameObject.Find("Canvas").GetComponent<MinigameFlowController>();

      
    }

    private void Start()
    {
        this.transform.localRotation = Quaternion.Euler(0, 180, 0);
    }

    public void BeginGrab(Hand hand)
    {
        currentHand = hand;
        grabbed = true;

        Vector3 handLocal =
            transform.parent.InverseTransformPoint(hand.transform.position);

        float handValue = GetAxisValue(handLocal);

        float doorValue = GetAxisValue(transform.localPosition);

        grabOffset = handValue - doorValue;
    }

    public void EndGrab()
    {
        grabbed = false;
        currentHand = null;
    }

    void FixedUpdate()
    {
        if (controller == null)
        {
            Debug.Log("フローコントローラーがnullです");
            return;
        }
        if(!controller.isGameStarted)
        {
            return;
        }

        if (isOpen) {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition,
                new Vector3(openPosX, transform.localPosition.y, transform.localPosition.z)
                , Time.deltaTime * 2f);

            if(transform.localPosition.x <= openPosX)
            {
                laneSetManager.NextMove();
                Destroy(gameObject.transform.parent.gameObject);
            }

            return;
        }

        if (!grabbed || currentHand == null)
            return;

        Vector3 handLocal =
            transform.parent.InverseTransformPoint(currentHand.transform.position);

        float value = GetAxisValue(handLocal);

        value -= grabOffset;

        value = Mathf.Clamp(value, minPosition, maxPosition);

        Vector3 target = transform.localPosition;

        switch (slideAxis)
        {
            case SlideAxis.X:
                target.x = value;
                break;

            case SlideAxis.Y:
                target.y = value;
                break;

            case SlideAxis.Z:
                target.z = value;
                break;
        }

        rb.MovePosition(transform.parent.TransformPoint(target));

        
    }

    float GetAxisValue(Vector3 v)
    {
        switch (slideAxis)
        {
            case SlideAxis.X:
                return v.x;

            case SlideAxis.Y:
                return v.y;

            default:
                return v.z;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "OpenLine")
        {
            isOpen = true;
        }
    }
}
