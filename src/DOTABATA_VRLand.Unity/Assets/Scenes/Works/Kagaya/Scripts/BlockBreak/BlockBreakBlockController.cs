using UnityEngine;

public class BlockBreakBlockController : MonoBehaviour {
    public int blockId;

    private void Start() {
        this.transform.parent = GameObject.Find("Blocks").transform;
    }
}