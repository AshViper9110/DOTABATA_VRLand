using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;

public class PlayerTransform : MonoBehaviour {
    [SerializeField] public Transform Head;
    [SerializeField] public Transform LeftHand;
    [SerializeField] public Transform RightHand;
    [SerializeField] public Transform Body;


    [SerializeField] private float duration = 0.2f;

    [SerializeField] public Transform crownParent;
    [SerializeField] float crownsDistance;

    [SerializeField] GameObject SpotLight;
    [SerializeField] float SpotDistans;
    float spotTomer;

    public bool forward = true;

    private void Update()
    {
        if (forward)
        {
            crownParent.position = Head.position + (Vector3.up * crownsDistance);
        }
        SpotLight.transform.position = Head.position + (Vector3.up * SpotDistans);
        if (spotTomer >= 0)
        {
            spotTomer -= Time.deltaTime;

        }
        else
        {
            if(SpotLight.activeSelf)
            {
                SpotLight.SetActive(false);
            }
        }
    }

    /// <summary>
    /// PlayerTransform ==> DTO 
    /// </summary>
    public PlayerTransformDTO ToPlayerTransformDTO() {
        return new PlayerTransformDTO() {
            Head = this.Head.ToSimpleTransform(),
            LeftHand = this.LeftHand.ToSimpleTransform(),
            RightHand = this.RightHand.ToSimpleTransform(),
            Body = this.Body.ToSimpleTransform()
        };
    }

    /// <summary>
    /// DTO ==> PlayerTransform
    /// </summary>
    public void ApplyPlayerTransform (PlayerTransformDTO transformDTO) {
        Head.ApplyTransform(transformDTO.Head, duration);
        LeftHand.ApplyTransform(transformDTO.LeftHand, duration);
        RightHand.ApplyTransform(transformDTO.RightHand, duration);
        Body.ApplyTransform(transformDTO.Body, duration);
    }

    public void StartSpotLight(float time)
    {
        spotTomer = time;
        SpotLight.SetActive(true);
    }
}
