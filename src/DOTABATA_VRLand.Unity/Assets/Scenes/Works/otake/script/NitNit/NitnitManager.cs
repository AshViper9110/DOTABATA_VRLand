using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class NitnitManager : MonoBehaviour
{
    public float MaxPoint = 999;

    [SerializeField] float maxTimer;
    float timer;


    [SerializeField] List<GameObject> nitPrefabs = new List<GameObject>();
    [SerializeField] List<Material> materials = new List<Material>();

    [SerializeField] public List<MufflerSetManager> mufflerSets = new List<MufflerSetManager>();

   public MinigameFlowController FlowController;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      
        SteamVR_Fade.Start(new Color(0,0,0,0),1.0f);
     timer = maxTimer;

        FlowController = GetComponent<MinigameFlowController>();
       
    }

    // Update is called once per frame
    void Update()
    {
       if(FlowController.isGameStarted)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                FlowController.isGameStarted = false;
            }
        }

       

    }

    public void addNit()
    {
       
    }
}
