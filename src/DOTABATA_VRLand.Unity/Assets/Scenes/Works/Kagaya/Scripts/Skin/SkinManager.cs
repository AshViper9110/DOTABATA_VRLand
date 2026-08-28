using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR.InteractionSystem;

public class SkinManager : MonoBehaviour {
    private Player player;
    private MeshRenderer HeadRenderer;
    private List<MeshRenderer> eyeRendererList;

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

        HeadRenderer = player.head.GetComponent<MeshRenderer>();
        eyeRendererList = new List<MeshRenderer>() {
            player.lEye.GetComponent<MeshRenderer>(),
            player.rEye.GetComponent<MeshRenderer>()
        };
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

        // コンテンツを生成
        foreach (Skin skin in skinList) {
            GameObject createdBtn = Instantiate(skinSelectBtn, contentTransform);

            if (category == SkinCategory.HeadColor) {
                // 名前設定
                //createdBtn.GetComponentInChildren<TextMeshProUGUI>().text = $"Color({skin.color})";

                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().color = skin.color;

                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    headColor = skin.color;
                    // 色変更
                    player.head.GetComponent<MeshRenderer>().material.color = skin.color;
                    // 同期
                    if (RoomModel.I.IsJoinRoom) {
                        await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                    }
                });
            }
            else if (category == SkinCategory.Hat) {
                // 名前設定
                string skinName = "None";

                if (skin.skinObject != null) {
                    skinName = skin.skinObject.name;
                }
                //createdBtn.GetComponentInChildren<TextMeshProUGUI>().text = skinName;

                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().sprite = skin.spriteImage;

                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    this.hatName = skinName;

                    // スキン変更
                    Transform hat = player.head.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Hat");
                    DestroySkinObj(hat);

                    if (skin.skinObject) {
                        Instantiate(skin.skinObject, hat);
                    }

                    // 同期
                    if (RoomModel.I.IsJoinRoom) {
                        await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                    }
                });
            }
            else if (category == SkinCategory.Accessories) {
                // 名前設定
                string skinName = "None";

                if (skin.skinObject != null) {
                    skinName = skin.skinObject.name;
                }
                //createdBtn.GetComponentInChildren<TextMeshProUGUI>().text = skinName;

                // 画像設定
                createdBtn.GetComponentsInChildren<Image>().Last().sprite = skin.spriteImage;

                // イベント設定
                createdBtn.GetComponent<Button>().onClick.AddListener(async () => {
                    // フィールド保持
                    this.accessoriesName = skinName;

                    // 頭と目の表示非表示
                    if (skinName == "Tutankhamun_mask") {
                        HeadRenderer.enabled = false;
                        eyeRendererList.ForEach(_ => _.enabled = false);
                    }
                    else {
                        HeadRenderer.enabled = true;
                        eyeRendererList.ForEach(_ => _.enabled = true);
                    }

                    // スキン変更
                    Transform accessories = player.head.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Accessories");
                    DestroySkinObj(accessories);

                    if (skin.skinObject) {
                        Instantiate(skin.skinObject, accessories);
                    }

                    // 同期
                    if (RoomModel.I.IsJoinRoom) {
                        await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
                    }
                });
            }
        }
    }

    /// <summary>
    /// コンテンツの子要素を全削除
    /// </summary>
    public void DestroyContentChild() {
        foreach (Transform child in contentTransform) {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// スキンのオブジェクト削除
    /// </summary>
    private void DestroySkinObj(Transform categoryParent) {
        foreach (Transform child in categoryParent) {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 他のプレイヤーにスキン情報を送る
    /// </summary>
    public async void SendMySkinData() {
        if (RoomModel.I.IsJoinRoom) {
            await RoomModel.I.ChangeSkinAsync(headColor, hatName, accessoriesName);
        }
    }

    /// <summary>
    /// [サーバー通知]
    /// スキン変更通知
    /// </summary>
    private void OnChangedSkin(Guid playerConId, Color headColor, string hatName, string accessoriesName) {
        if (!InRoomPlayerData.I.PlayerList.ContainsKey(playerConId)) {
            return;
        }

        Debug.Log($"スキン変更通知\n" +
            $"PlayerConId: {playerConId}\n" +
            $"HeadColor: {headColor}\n" +
            $"HatName: {hatName}\n" +
            $"AccessoriesName: {accessoriesName}");

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

        // 元々装備していたオブジェクト削除
        DestroySkinObj(hat);

        // 帽子生成
        if (hatName != "None") {
            Skin skin = skinDataList.First(_ => _.skinCategory == SkinCategory.Hat).skinList.FirstOrDefault(_ => _.skinObject?.name == hatName);
            if (skin?.skinObject) {
                GameObject createdHat = Instantiate(skin.skinObject, hat);
                createdHat.layer = 0;
            }
            else {
                Debug.Log($"帽子が見つかりませんでした。 {hatName}");
            }
        }

        // アクセサリー変更
        Transform accessories = playerData.playerObj.GetComponentsInChildren<Transform>().First(_ => _.gameObject.name == "Accessories");
        
        // 元々装備していたオブジェクト削除
        DestroySkinObj(accessories);

        // アクセサリー生成
        if (accessoriesName != "None") {
            Skin skin = skinDataList.First(_ => _.skinCategory == SkinCategory.Accessories).skinList.FirstOrDefault(_ => _.skinObject?.name == accessoriesName);
            if (skin?.skinObject) {
                GameObject createdAccessories = Instantiate(skin.skinObject, accessories);
                createdAccessories.layer = 0;
            }
            else {
                Debug.Log($"アクセサリーが見つかりませんでした。 {accessoriesName}");
            }
        }

            MeshRenderer otherPlayerHeadRenderer = playerData.playerObj.GetComponentsInChildren<MeshRenderer>().First(_ => _.gameObject.name == "Head");
        List<MeshRenderer> otherPlayerEyeRendererList = new List<MeshRenderer>() {
            playerData.playerObj.GetComponentsInChildren<MeshRenderer>().First(_=>_.gameObject.name == "EyeR"),
            playerData.playerObj.GetComponentsInChildren<MeshRenderer>().First(_=>_.gameObject.name == "EyeL")
        };

        // 頭と目の表示非表示
        if (accessoriesName == "Tutankhamun_mask") {
            otherPlayerHeadRenderer.enabled = false;
            otherPlayerEyeRendererList.ForEach(_ => _.enabled = false);
        }
        else {
            otherPlayerHeadRenderer.enabled = true;
            otherPlayerEyeRendererList.ForEach(_ => _.enabled = true);
        }

    }
}
