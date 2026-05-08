using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Models.Entities;
using System;
using UnityEngine;

public class SyncPlayer : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private float syncInterval = 0.1f;
    private float syncTimer;

    public Guid connectionId;
    public bool isLocalPlayer;

    private void Start()
    {
        RoomModel.I.OnUpdatedUserTransfrom += OnSyncPlayer;
    }

    private void OnDestroy()
    {
        if (RoomModel.I != null)
        {
            RoomModel.I.OnUpdatedUserTransfrom -= OnSyncPlayer;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        syncTimer += Time.deltaTime;

        if (syncTimer >= syncInterval)
        {
            syncTimer = 0f;
            SendTransform().Forget();
        }
    }

    private void OnSyncPlayer(Guid connectionId, PlayerTransform data)
    {
        if (connectionId != this.connectionId)
            return;

        if (isLocalPlayer)
            return;

        data.Head.localScale = head.transform.localScale;
        data.LeftHand.localScale = leftHand.transform.localScale;
        data.RightHand.localScale = rightHand.transform.localScale;

        head.ApplyTransform(data.Head, syncInterval);
        leftHand.ApplyTransform(data.LeftHand, syncInterval);
        rightHand.ApplyTransform(data.RightHand, syncInterval);
    }

    private async UniTaskVoid SendTransform()
    {
        if (!TitleMana.isJoin)
            return;

        PlayerTransform data = new PlayerTransform
        {
            Head = head.ToSimpleTransform(),
            LeftHand = leftHand.ToSimpleTransform(),
            RightHand = rightHand.ToSimpleTransform(),
        };

        await RoomModel.I.UpdateUserTransformAsync(data);
    }
}