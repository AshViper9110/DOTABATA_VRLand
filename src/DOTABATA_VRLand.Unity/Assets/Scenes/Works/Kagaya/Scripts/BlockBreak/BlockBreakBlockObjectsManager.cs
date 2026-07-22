using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BlockBreakBlockObjectsManager : MonoBehaviour {
    [SerializeField] private List<GameObject> objectList = new List<GameObject>();

    [SerializeField] private int newCreateBlockNum = 5;

    [SerializeField] private List<string> jsonNameList = new List<string>();

    [SerializeField] private string saveName;

    /// <summary>
    /// オブジェクトをセット
    /// </summary>
    public async UniTask SetObjects() {
        if (GetCurrentBlockCount() <= newCreateBlockNum) {
            await DestroyCreatedObjects();

            BlockObjects blockObjects = GetBLockObjectsPosLoadJson();

            foreach (var block in blockObjects.blockList) {
                Instantiate(objectList[block.id - 1], block.pos, Quaternion.identity, this.transform);
            }
        }
        else {
            foreach (GameObject block in GetCreatedObjectList()) {
                await block.GetComponent<SyncObject>().GetOwnership(true);
            }
        }
    }

    /// <summary>
    /// 生成済みオブジェクトのリストを取得
    /// </summary>
    private List<GameObject> GetCreatedObjectList() {
        List<GameObject> objects = new List<GameObject>();
        foreach (Transform child in this.transform) {
            objects.Add(child.gameObject);
        }
        return objects;
    }

    /// <summary>
    /// 現在の生成済みオブジェクトを削除
    /// </summary>
    private async UniTask DestroyCreatedObjects() {
        foreach (Transform child in this.transform) {
            Destroy(child.gameObject);
        }

        await UniTask.NextFrame();
        await UniTask.WaitForFixedUpdate();
    }

    /// <summary>
    /// 現在のブロック数を取得
    /// </summary>
    public int GetCurrentBlockCount() {
        return this.transform.childCount;
    }

    /// ====================================================================================
    
    /// <summary>
    /// Jsonファイルを生成して保存
    /// </summary>
    public void CreateJson() {
        var settings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        BlockObjects blockObjects = new BlockObjects();

        foreach (Transform child in this.transform) {
            BlockBreakBlockController blockCon = child.GetComponent<BlockBreakBlockController>();
            Block block = new Block() {
                id = blockCon.blockId,
                pos = child.position,
            };
            blockObjects.blockList.Add(block);
        }

        string json = JsonConvert.SerializeObject(blockObjects, settings);

        var writer = new StreamWriter(Application.streamingAssetsPath + "/Json/BlockBreak/" + saveName + ".json");
        writer.Write(json);
        writer.Flush();
        writer.Close();

        jsonNameList.Add(saveName);
    }

    /// <summary>
    /// 保存しているJsonファイルを読み込む
    /// </summary>
    private BlockObjects GetBLockObjectsPosLoadJson() {
        int rnd = UnityEngine.Random.Range(0, jsonNameList.Count);
        // ファイルを文字列として読む
        string json = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Json/BlockBreak/" + jsonNameList[rnd] + ".json");
        return JsonConvert.DeserializeObject<BlockObjects>(json);
    }

    /// <summary>
    /// プロジェクトファイルからJsonのファイル名を全て読み込みJsonNameListに保持
    /// </summary>
    public void LoadAllJsonFromProjectsFile() {
        string folderPath = Application.streamingAssetsPath + "/Json/BlockBreak/";
        foreach (string file in Directory.GetFiles(folderPath, "*.json")) {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (!jsonNameList.Contains(fileName)) {
                jsonNameList.Add(fileName);
            }
        }
    }
}

[System.Serializable]
public class BlockObjects {
    public List<Block> blockList = new List<Block>();
}

[System.Serializable]
public class Block {
    public int id;
    public Vector3 pos;
}