using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class NitnitManager : MonoBehaviour
{
    public float MaxPoint = 999;

    [SerializeField] float maxTimer;
    float timer;
    [SerializeField] Text TimerText;
    
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

       

        foreach(var f in InRoomPlayerData.I.PlayerList.Values)
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
        if (FlowController.isGameStarted)
        {
            timer -= Time.deltaTime;
            if(!TimerText.gameObject.activeSelf)
            {
                TimerText.gameObject.SetActive(true);
            }
            TimerText.text = (Mathf.Floor(timer*10)/10).ToString();

            if (timer < 0)
            {
                FlowController.isGameStarted = false;
                RoomModel.I.SendScore((int)(mufflerSets[InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder - 1].point * 10));
                enabled = false;
                TimerText.text = "フィニッシュ！";

                foreach (MufflerSetManager mufflerSet in mufflerSets)
                {
                    mufflerSet.DeleteRod();
                }
                return;
            }
        }
        else
        {
            TimerText.gameObject.SetActive(false);
        }

        for (int i = 0; i < pointTexts.Count; i++)
        {
            pointTexts[i].text = (Mathf.Floor(mufflerSets[i].point*10)/10).ToString();
        }

    }

 
}
