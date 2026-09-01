using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class BlockBreakBulletController : MonoBehaviour {
    private SyncObject syncObject;
    private Rigidbody myRb;

    private Guid shotPlayerConId;

    public float maxLife;
    [SerializeField] private float life;

    private void Awake() {
        myRb = this.GetComponent<Rigidbody>();
        syncObject = this.GetComponent<SyncObject>();
        life = maxLife;
    }

    private async void Start() {
        AudioManager.PlaySE(AudioManager.SE.BBShot);
        await UniTask.WaitUntil(() => syncObject.Initialized == true);
        shotPlayerConId = syncObject.CreaterId;
        if (!syncObject.IsOwner) {
            myRb.isKinematic = true;
        }
    }

    private void Update() {
        if (!syncObject.IsOwner) return;

        life -= Time.deltaTime;
        if (life <= 0) {
            Destroy(this.gameObject);
        }
    }


}