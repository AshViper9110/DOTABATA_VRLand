using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class NitnitManager : MonoBehaviour
{
    public float MaxPoint = 999;



    [SerializeField] List<GameObject> nitPrefabs = new List<GameObject>();
    [SerializeField] List<Material> materials = new List<Material>();

    [SerializeField] public List<MufflerSetManager> mufflerSets = new List<MufflerSetManager>();

     // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SteamVR_Fade.Start(new Color(0,0,0,0),1.0f);
     

    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void addNit()
    {
       
    }
}
