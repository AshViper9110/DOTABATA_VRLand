using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using UnityEngine;

public class ArcanaGameManager : MonoBehaviour {
    // 自分
    [SerializeField] private GameObject playerPrefab;
    // 他
    [SerializeField] private GameObject otherPlayerPrefab;

    private void Awake() {
        
    }

    private void Start() {
        
    }

    private void OnDisable() {
        if (RoomModel.I != null) {

        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void FixedUpdate() {
        
    }

    private void Update() {
        
    }
}
