
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using static UnityEngine.GraphicsBuffer;

public class GarageOpenerManager : MonoBehaviour
{
    [SerializeField]List<LaneSetManager> LaneSets = new List<LaneSetManager>();
    [SerializeField]MinigameFlowController flowController;
    GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SteamVR_Fade.View(new Color(0, 0, 0, 0), 1.0f);
        LaneSets[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].SetGarage();
        player = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj;
   
    }

    // Update is called once per frame
    void Update()
    {
       
      
            flowController.transform.position = player.transform.position + player.transform.right;
            return;
        
    }
}
