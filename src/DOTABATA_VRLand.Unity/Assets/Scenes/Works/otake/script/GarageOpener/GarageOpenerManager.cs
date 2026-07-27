
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
    bool isstart;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SteamVR_Fade.View(new Color(0, 0, 0, 0), 1.0f);
        LaneSets[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].SetGarage(flowController);
        player = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj;

        AudioManager.StopBgm();
    }

    // Update is called once per frame
    void Update()
    {
        if (flowController.isGameStarted)
        {
            if (!isstart)
            {
                AudioManager.ChangeBGM(AudioManager.BGM.Garage);
                isstart = true; 
            }
        }
      
            flowController.transform.position = player.transform.position + player.transform.right;
            return;
        
    }
}
