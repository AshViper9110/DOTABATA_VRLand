using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BombDodgeManager : MonoBehaviour
{
    [SerializeField] GameObject EngelRingPrefab;
    [SerializeField] List<Transform> startpos;
    [SerializeField] Transform  center;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SteamVR_Fade.Start(new Color(0, 0, 0, 0), 1.0f);
        int index = 0;
        foreach (var obj in InRoomPlayerData.I.PlayerList.Values)
        {
            if (obj.playerObj.GetComponent<BombDogePlayer>())
            {
                BombDogePlayer dogePlayer = obj.playerObj.AddComponent<BombDogePlayer>();
                dogePlayer.EngelRing = Instantiate(EngelRingPrefab,
                    obj.playerObj.transform.position,
                    Quaternion.identity,
                    obj.playerObj.transform);
            }
            obj.playerObj.transform.position = startpos[index].position;
            obj.playerObj.transform.LookAt(center);
            index++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
