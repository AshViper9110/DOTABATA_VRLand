using UnityEngine;
using Valve.VR.InteractionSystem;

public class DoorHandle : Throwable
{
    public GarageManager door;
    public Transform ResetPos;

    protected override void OnAttachedToHand(Hand hand)
    {
        // DoorHandle ‚ª Destroy ‚³‚ê‚Ä‚¢‚½‚ç‘¦ return
        if (!this || !gameObject || !transform)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }

        // hand ‚ª Destroy ‚³‚ê‚Ä‚¢‚½‚ç‘¦ return
        if (!hand || !hand.transform)
        {
            Destroy(gameObject);
            enabled = false;
            return;
        }

        base.OnAttachedToHand(hand);

        door.BeginGrab(hand);
    }

    protected override void OnDetachedFromHand(Hand hand)
    {



        base.OnDetachedFromHand(hand);

        door.EndGrab();



        if (!this || !gameObject || !transform)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }

        // hand ‚ª Destroy ‚³‚ê‚Ä‚¢‚½‚ç‘¦ return
        if (!hand || !hand.transform)
        {
            Destroy(gameObject);
            return;
        }

        if (!ResetPos || !ResetPos.transform)
        {
            Destroy(gameObject);
            return;
        }


            this.gameObject.transform.position = ResetPos.transform.position;
            gameObject.transform.rotation = ResetPos.transform.rotation;
        
    }
}

