using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using UnityEngine;

public class FreePlayManager : MonoBehaviour
{
    public List<string> miniGames = new List<string>();
    [SerializeField] List<Sprite> miniGameTitleImages = new List<Sprite>();

    [SerializeField]public List<TextMeshProUGUI> RankingText = new List<TextMeshProUGUI>();
    [SerializeField]public GameObject RankingBord;

    [SerializeField] GameObject miniGamesParent;
    [SerializeField] GameObject FreePlayMinigamePrefabs;   

    GameManager Manager;

    private void OnEnable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnSelectedFreeMinigame += OnSelected;
    }

    private void OnDisable()
    {
        if (RoomModel.I == null) return;
        RoomModel.I.OnSelectedFreeMinigame -= OnSelected;

    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manager = GetComponent<GameManager>();
        RankingBord.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnSelected(string name)
    {

        Manager.MoveScene(name);

        if(name == "TitleScene")
        {
            RoomModel.I.LeaveRoomAsync();
        }
    }

    public void SetMinigames()
    {
        int index = 0;
        foreach (var game in miniGames)
        {
            GameObject minigame = Instantiate(FreePlayMinigamePrefabs,
                miniGamesParent.transform);

            FreePlayMiniGame free = minigame.GetComponent<FreePlayMiniGame>();
            free.name = game;
            free.Image.sprite = miniGameTitleImages[index];
            index++;

        }
    }

    public void HideRankingBord()
    {
        RankingBord.SetActive(false);
    }
}
