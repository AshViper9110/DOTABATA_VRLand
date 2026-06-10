using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class NitnitManager : MonoBehaviour
{
    public float MaxPoint = 999;

    [SerializeField] float maxTimer;
    [SerializeField] TextMeshProUGUI timerText;
    float timer;


    [SerializeField] List<GameObject> nitPrefabs = new List<GameObject>();
    [SerializeField] List<Material> materials = new List<Material>();

    [SerializeField] public List<MufflerSetManager> mufflerSets = new List<MufflerSetManager>();
    [SerializeField] List<Text> pointTexts = new List<Text>();

    [SerializeField] List<Transform> startPos = new List<Transform>();

   public MinigameFlowController FlowController;

    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
        SteamVR_Fade.Start(new Color(0,0,0,0),1.0f);
     timer = maxTimer;

        FlowController = GetComponent<MinigameFlowController>();

        AudioManager.StopBgm();
        timerText.text = "";

        foreach (var f in InRoomPlayerData.I.PlayerList.Values)
        {
            if (f.joinedUser.ConnectionId == NetworkManager.I.myConnectionId)
            {
                f.playerObj.transform.position = startPos[f.joinedUser.JoinOrder - 1].position;
                mufflerSets[f.joinedUser.JoinOrder - 1].CreateRod();
            }
           
        }
       
    }

    // Update is called once per frame
    void Update()
    {
       if(FlowController.isGameStarted)
        {
            if(!AudioManager.isStartBgm)
            {
                AudioManager.ChangeBGM(AudioManager.BGM.Nit_Nit);
            }
            timer -= Time.deltaTime;
            timerText.text = ((int)timer).ToString();

            if (timer < 0)
            {
                timerText.text = "フィニッシュ！";
              
                FlowController.isGameStarted = false;
                StartCoroutine(SendScoreCount());
                mufflerSets[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].DeleteRod();
                enabled = false;
                
                AudioManager.StopBgm();
                AudioManager.PlaySE(AudioManager.SE.MiniGame_Finish);

                return;
            }
        }

        for (int i = 0; i < pointTexts.Count; i++)
        {
            pointTexts[i].text = (Mathf.Floor(mufflerSets[i].point*10)/10).ToString();
        }

    }


     public IEnumerator SendScoreCount()
    {
        yield return new WaitForSeconds(2);
        RoomModel.I.SendScore((int)(mufflerSets[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].point * 10));
    }
 
}
