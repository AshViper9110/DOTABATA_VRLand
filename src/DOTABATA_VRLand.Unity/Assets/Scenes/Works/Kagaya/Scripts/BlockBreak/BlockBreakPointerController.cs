using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEngine;

public class BlockBreakPointerController : MonoBehaviour {
    private BlockBreakGameManager gameManager;

    [SerializeField] private SpriteRenderer pointer_1;
    [SerializeField] private SpriteRenderer pointer_2;

    private int playerId;

    private async void Start() {
        gameManager = GameObject.Find("BlockBreakManager").GetComponent<BlockBreakGameManager>();
        SyncObject syncObject = GetComponent<SyncObject>();
        await UniTask.WaitUntil(() => syncObject.Initialized == true);

        playerId = InRoomPlayerData.I.PlayerList[syncObject.CreaterId].joinedUser.JoinOrder;

        switch (playerId) {
            case 1:
                pointer_1.color = Color.red;
                pointer_2.color = Color.red;
                break;
            case 2:
                pointer_1.color = Color.blue;
                pointer_2.color = Color.blue;
                break;
            case 3:
                pointer_1.color = Color.green;
                pointer_2.color = Color.green;
                break;
            case 4:
                pointer_1.color = Color.yellow;
                pointer_2.color = Color.yellow;
                break;
        }

        Hide();

        gameManager.playerHavingObjectList[syncObject.CreaterId].objects["Pointer"] = this.gameObject;
        gameManager.initializedPlayer++;
    }

    /// <summary>
    /// 表示
    /// </summary>
    public void Show() {
        Color color = pointer_1.color;
        color.a = 1f;
        pointer_1.color = color;
        pointer_2.color = color;
    }

    /// <summary>
    /// 非表示
    /// </summary>
    public void Hide() {
        Color color = pointer_1.color;
        color.a = 0.5f;
        pointer_1.color = color;
        pointer_2.color = color;
    }

    /// <summary>
    /// 表示非表示
    /// </summary>
    public void SwitchShowHide(bool isShow) {
        if (isShow) {
            Show();
        }
        else {
            Hide();
        }
    }
}