using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BlockBreakBlockObjectsManager : MonoBehaviour {
    [SerializeField] private SerializableDictionary<int, List<GameObject>> objectList = new SerializableDictionary<int, List<GameObject>>();

    [SerializeField] private int newCreateBlockNum = 5;

    [SerializeField] private List<string> jsonNameList = new List<string>();

    [SerializeField] private string saveName;

    [SerializeField] private string previewJsonName;

    /// <summary>
    /// オブジェクトをセット
    /// </summary>
    public async UniTask SetObjects() {
        if (GetCurrentBlockCount() <= newCreateBlockNum) {
            await DestroyCreatedObjects();

            BlockObjects blockObjects = GetBLockObjectsPosLoadJson();

            foreach (var block in blockObjects.blockList) {
                GameObject insObj = objectList[block.id].OrderBy(_=> Random.value).First(); 
                Instantiate(insObj, block.pos, block.rot, this.transform);
            }
        }
        else {
            foreach (GameObject block in GetCreatedObjectList()) {
                if (!block) return;
                await block.GetComponent<SyncObject>().GetOwnership(true);
            }
        }

        ChangeKinematic(false);
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

    /// <summary>
    /// ブロックのキネマティックを変更
    /// </summary>
    public void ChangeKinematic(bool isKinematic) {
        foreach (GameObject obj in GetCreatedObjectList()) {
            obj.GetComponent<Rigidbody>().isKinematic = isKinematic;
        }
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
                rot = child.rotation,
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

    /// <summary>
    /// Jsonファイルを読み込み生成
    /// </summary>
    public void PreviewJsonObject() {
        DestroyObjects();

        string json = System.IO.File.ReadAllText(Application.streamingAssetsPath + "/Json/BlockBreak/" + previewJsonName + ".json");
        BlockObjects blockObjects = JsonConvert.DeserializeObject<BlockObjects>(json);

        foreach (var block in blockObjects.blockList) {
            GameObject insObj = objectList[block.id].OrderBy(_ => Random.value).First();
            GameObject created = Instantiate(insObj, block.pos, block.rot, this.transform);
            Undo.RegisterCreatedObjectUndo(created, "Create Preview Object");
        }
    }

    /// <summary>
    /// オブジェクトを全削除
    /// </summary>
    public void DestroyObjects() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
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
    public Quaternion rot;
}