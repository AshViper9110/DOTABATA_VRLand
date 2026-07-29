using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class SkinManager : MonoBehaviour {
    private Player player;

    /*
     * 自分
     */

    private Color headColor = Color.white;
    private string hatName = "None";
    private string accessoriesName = "None";

    // コンテンツの要素(スキン選択ボタン)
    [SerializeField] private GameObject skinSelectBtn;

    // コンテンツの配置場所
    [SerializeField] private Transform contentTransform;

    // スキンデータリスト
    [SerializeField] private List<SkinDataSO> skinDataList = new List<SkinDataSO>();

    public enum SkinCategory {
        HeadColor = 0,
        Hat,
        Accessories,
    }

    private void Awake() {
        if (RoomModel.I != null) {
            RoomModel.I.OnChangedSkin += OnChangedSkin;
        }
    }

    private void OnDisable() {
        if (RoomModel.I != null) {
            RoomModel.I.OnChangedSkin -= OnChangedSkin;
        }
    }

    private void OnDestroy() {
        OnDisable();
    }

    private void Start() {
        player = Player.instance;
    }

    /// <summary>
    /// スキンカテゴリーTabが押されたとき
    /// </summary>
    public void OnClickCategoryTab(int categoryNum) {
        SkinCategory category = (SkinCategory)categoryNum;

        DestroyContentChild();

        List<Skin> skinList = skinDataList.FirstOrDefault(_=>_.skinCategory == category).skinList;
        if (skinList == null) {
            return;
        }

        foreach (Skin skin in skinList) {
            GameObject createdBtn = Instantiate(skinSelectBtn, contentTransform);
            // 名前設定
            createdBtn.GetComponentInChildren<TextMeshProUGUI>().text = skin.name;
            
            if (category == SkinCategory.HeadColor) {
                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().color = skin.color;
                
                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    headColor = skin.color;
                    // 色変更
                    player.head.GetComponent<MeshRenderer>().material.color = skin.color;
                    // 同期
                    await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                });
            }
            else if (category == SkinCategory.Hat){
                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().sprite = skin.spriteImage;

                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    this.hatName = skin.name;

                    // スキン変更
                    Transform hat = player.head.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Hat");
                    DestroySkinObj(hat);

                    if (skin.skinObject) {
                        Instantiate(skin.skinObject, hat);
                    }

                    // 同期
                    await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                });
            }
            else if (category == SkinCategory.Accessories){
                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().sprite = skin.spriteImage;

                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    accessoriesName = skin.name;

                    // スキン変更
                    Transform accessories = player.head.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Accessories");
                    DestroySkinObj(accessories);

                    if (skin.skinObject) {
                        Instantiate(skin.skinObject, accessories);
                    }

                    // 同期
                    await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                });
            }
        }
    }

    /// <summary>
    /// コンテンツの子要素を全削除
    /// </summary>
    public async void DestroyContentChild() {
        foreach (Transform child in contentTransform) {
            Destroy(child.gameObject);
        }

        // 削除されるまで待機
        await UniTask.NextFrame();
        await UniTask.WaitForFixedUpdate();
    }

    /// <summary>
    /// スキンのオブジェクト削除
    /// </summary>
    private async void DestroySkinObj(Transform categoryParent) {
        foreach (Transform child in categoryParent) {
            Destroy(child.gameObject);
        }

        // 削除されるまで待機
        await UniTask.NextFrame();
        await UniTask.WaitForFixedUpdate();
    }

    /// <summary>
    /// [サーバー通知]
    /// スキン変更通知
    /// </summary>
    private void OnChangedSkin(Guid playerConId, Color headColor, string hatName, string accessoriesName) {
        if (!InRoomPlayerData.I.PlayerList.ContainsKey(playerConId)) {
            return;
        }

        // プレイヤーデータ
        PlayerData playerData = InRoomPlayerData.I.PlayerList[playerConId];

        // 色適応
        playerData.playerObj.GetComponentsInChildren<MeshRenderer>()
            .Where(_ => _.gameObject.name == "Head" ||
            _.gameObject.name == "LeftHand" ||
            _.gameObject.name == "RightHand")
            .ToList()
            .ForEach(_=>_.material.color = headColor);

        // 帽子変更
        Transform hat = playerData.playerObj.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Hat");
        DestroySkinObj(hat);
        Instantiate(skinDataList.First(_ => _.skinCategory == SkinCategory.Hat).skinList.First(_=>_.name == hatName).skinObject,
            hat);

        // アクセサリー変更
        Transform accessories = playerData.playerObj.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Accessories");
        DestroySkinObj(accessories);
        Instantiate(skinDataList.First(_ => _.skinCategory == SkinCategory.Accessories).skinList.First(_ => _.name == accessoriesName).skinObject,
            accessories);

    }
}
