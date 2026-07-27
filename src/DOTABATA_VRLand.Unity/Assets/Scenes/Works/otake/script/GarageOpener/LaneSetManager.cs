using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LaneSetManager : MonoBehaviour
{
    public enum Direction
    {
        Right = 0,
        Up,
        Left,
        
    }

     [SerializeField]public List<Transform> standPos;
     [SerializeField]public List<Transform> GaragePos;
    [SerializeField] GameObject GaragePrefab;

    GameObject player;
    Transform Standtrans;
    int standindex;
    float Timer;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    
        standindex = 0;
        Timer = 0;

  
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            enabled = false;
            return;
        }
        
            player.transform.position = Vector3.MoveTowards(player.transform.position,
                standPos[standindex].position,
                Time.deltaTime * 1f);

            Timer += Time.deltaTime;
        
    }

    public void SetGarage(MinigameFlowController MiniController)
    {
        player = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj;
        player.transform.position = standPos[0].position;
        

        foreach (Transform t in GaragePos)
        {
            GameObject Garage = Instantiate(GaragePrefab,
                t.position,
                Quaternion.identity
                );

           int rand = Random.Range(0,3);
            Debug.Log($"rnd::{rand}");

            GarageManager garageManager = Garage.GetComponentInChildren<GarageManager>();

            garageManager.laneSetManager = this;
            garageManager.controller = MiniController;

            switch (rand)
            {
                case (int)Direction.Right:
                    Garage.transform.rotation = Quaternion.Euler(0,0,0);

                    break;
                case (int)Direction.Up:
                    Garage.transform.rotation = Quaternion.Euler(0, 0, 90);
                    foreach (Transform child in Garage.transform)
                    {
                        child.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                    break;
                case (int)Direction.Left:
                    Garage.transform.rotation = Quaternion.Euler(0, 0, 180);
                    foreach (Transform child in Garage.transform)
                    {
                        child.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                    break;
               
                   
                    default:
                    break;
            }
        }
    }

    public void NextMove()
    {
        standindex++;
        if(standindex >= standPos.Count)
            {
            RoomModel.I.SendScore((int)(10000-Timer)*100);
            enabled = false;
            return;
                }
        
    }
}
