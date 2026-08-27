using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

[RequireComponent(typeof(Rigidbody))]
public class Bottle : MonoBehaviour
{
    [SerializeField] private AudioSource bottleAudio;
    [SerializeField] private List<AudioClip> shakeSounds;

    [SerializeField] private float shakeThreshold = 1.5f;
    [SerializeField] private float cooldown = 0.08f;
    [SerializeField] private float targetShake = 10f;
    [SerializeField] private GameObject popbottle;
    private float lastPlayTime;
    private Vector3 lastVelocity;
    public float ShakePower { get; private set; }

    private void FixedUpdate()
    {
        if (this.GetComponent<Interactable>().attachedToHand != null)
        {
            Hand hand = this.GetComponent<Interactable>().attachedToHand;

            Vector3 velocity = hand.GetTrackedObjectVelocity();

            // コントローラーの加速度
            Vector3 acceleration = (velocity - lastVelocity) / Time.fixedDeltaTime;

            if (acceleration.magnitude > shakeThreshold &&
                Time.time - lastPlayTime > cooldown)
            {
                bottleAudio.PlayOneShot(
                    shakeSounds[Random.Range(0, shakeSounds.Count)]);

                lastPlayTime = Time.time;
            }

            lastVelocity = velocity;
        }

        if (this.GetComponent<Interactable>().attachedToHand != null)
        {
            Hand hand = this.GetComponent<Interactable>().attachedToHand;

            Vector3 velocity = hand.GetTrackedObjectVelocity();

            float speedY = Mathf.Abs(velocity.y);

            ShakePower += speedY * Time.fixedDeltaTime;
        }

        if (ShakePower >= targetShake)
        {
            popbottle.SetActive(true);
        }
    }
}