using Cysharp.Threading.Tasks;
using DOTABATA_VRLand.Shared.Models.Entities;
using UnityEngine;

public class SyncPlayer : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerTransform playerTransform;

    [Header("Settings")]
    [SerializeField] private float syncInterval = 0.1f;

    public bool isLocalPlayer = false;

    private float syncTimer;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (playerTransform == null)
            playerTransform = GetComponent<PlayerTransform>();
    }

    private async void Update()
    {
        if (!isLocalPlayer) return;

        syncTimer += Time.deltaTime;
        if (syncTimer >= syncInterval)
        {
            syncTimer = 0f;
            SendTransform().Forget();
        }
    }

    private async UniTaskVoid SendTransform()
    {
        if (!NetworkManager.I.isJoin) return;

        PlayerTransformDTO data = playerTransform.ToPlayerTransformDTO();
        await RoomModel.I.UpdateUserTransformAsync(data);
    }

    public void ApplyTransform(PlayerTransformDTO data)
    {
        data.Head.localScale = playerTransform.Head.localScale;
        data.LeftHand.localScale = playerTransform.LeftHand.localScale;
        data.RightHand.localScale = playerTransform.RightHand.localScale;
        data.Body.localScale = playerTransform.Body.localScale;
        playerTransform.ApplyPlayerTransform(data);
    }
}