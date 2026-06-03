using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static GestureRecognizer;

public class ArcanaGameManager : MonoBehaviour {
    [SerializeField] private GestureRecognizer gestureRecognizer;

    // スポーツ位置
    [SerializeField] private List<Transform> spawnPoints;

    // 魔法のPrefab
    [SerializeField] private GameObject magicBallPrefab;
    // 魔法のVFX
    [SerializeField] private List<GameObject> magicVFXList;

    private ArcanaPlayerController myController;

    private void Awake() {
        gestureRecognizer.CompleteRecognize += CreateMagic;
    }

    private void Start() {
        InRoomPlayerData.I.MySelf.playerObj.AddComponent<PlayerStatus>();
        myController = InRoomPlayerData.I.MySelf.playerObj.AddComponent<ArcanaPlayerController>();

        int index = 0;
        foreach (var playerData in InRoomPlayerData.I.PlayerList) {
            playerData.Value.playerObj.transform.position = spawnPoints[index].position;
            index++;
        }

    }

    private void OnDisable() {
        if (RoomModel.I != null) {

        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Update() {
        
    }

    /// <summary>
    /// 魔法生成
    /// </summary>
    private void CreateMagic(GestureClass gesture, float score) {
        Transform createdTransform = Instantiate(magicBallPrefab, new Vector3(0, 10, 0), Quaternion.identity).transform;
        int rnd = UnityEngine.Random.Range(0, magicVFXList.Count);
        Instantiate(magicVFXList[rnd], createdTransform);

        //myController.SetMagicObj(createdTransform.gameObject);
    }
}
