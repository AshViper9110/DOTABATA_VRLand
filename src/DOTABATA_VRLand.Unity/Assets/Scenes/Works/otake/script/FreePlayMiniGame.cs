
using UnityEngine;
using UnityEngine.UI;

public class FreePlayMiniGame : MonoBehaviour
{
    public string name;
    public Image Image;
    GameManager Manager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }


    // Update is called once per frame
    void Update()
    {
        
    }



    public void Select()
    {
        if (InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].joinedUser.JoinOrder == 1)
        {
            RoomModel.I.SelectFreeMinigame(name);
        }
    }
}
