using DOTABATA_VRLand.Shared.Models.Entities;
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

    public async void SetMinigames()
    {
        List<MiniGameInfo> miniGames = await RoomModel.I.GetAllMiniGameAsync();

        foreach (var game in miniGames)
        {
            GameObject minigame = Instantiate(
                FreePlayMinigamePrefabs,
                miniGamesParent.transform);

            FreePlayMiniGame free = minigame.GetComponent<FreePlayMiniGame>();

            free.name = game.SceneName;
            free.Image.sprite = CreateSpriteFromBytes(game.BinaryImg);
        }
    }

    private Sprite CreateSpriteFromBytes(byte[] imageBytes)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            return null;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

        if (!texture.LoadImage(imageBytes))
        {
            Debug.LogError("âÊëúÇÃì«Ç›çûÇ›Ç…é∏îsÇµÇ‹ÇµÇΩÅB");
            Destroy(texture);
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(1f, 1f));
    }

    public void HideRankingBord()
    {
        RankingBord.SetActive(false);
    }
}
