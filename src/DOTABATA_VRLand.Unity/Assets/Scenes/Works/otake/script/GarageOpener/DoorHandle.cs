using UnityEngine;
using Valve.VR.InteractionSystem;

public class DoorHandle : Throwable
{
    public GarageManager door;

    protected override void OnAttachedToHand(Hand hand)
    {
        base.OnAttachedToHand(hand);

        door.BeginGrab(hand);
    }

    protected override void OnDetachedFromHand(Hand hand)
    {
        base.OnDetachedFromHand(hand);

        door.EndGrab();

        this.gameObject.transform.position = door.transform.position;
        gameObject.transform.rotation = door.transform.rotation;
    }
}

