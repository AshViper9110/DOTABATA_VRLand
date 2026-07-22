using UnityEngine;

public class MiniGameCanvasContoroller : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followMoveSpeed = 0.1f;
    [SerializeField] private float followRotateSpeed = 0.02f;
    [SerializeField] private float rotateSpeedThreshold = 0.9f;
    [SerializeField] private float distance = 0.9f;
    [SerializeField] private bool isImmediateMove;
    [SerializeField] private bool isLockX;
    [SerializeField] private bool isLockY;
    [SerializeField] private bool isLockZ;
    [SerializeField] private bool StartSync;
    private Quaternion rot;
    private Quaternion rotDif;

    private void Start()
    {
        if (!target) target = InRoomPlayerData.I.PlayerList[NetworkManager.I.myConnectionId].playerObj.GetComponent<PlayerTransform>().Head; ;
    }

    private void LateUpdate()
    {
  

        if (isImmediateMove) transform.position = target.position;
        else transform.position = Vector3.Lerp(transform.position, target.position, followMoveSpeed);

        rotDif = target.rotation * Quaternion.Inverse(transform.rotation);
        rot = target.rotation;
        if (isLockX) rot.x = 0;
        if (isLockY) rot.y = 0;
        if (isLockZ) rot.z = 0;
        if (rotDif.w < rotateSpeedThreshold) transform.rotation = Quaternion.Lerp(transform.rotation, rot, followRotateSpeed * 4);
        else transform.rotation = Quaternion.Lerp(transform.rotation, rot, followRotateSpeed);

        transform.rotation = new Quaternion(transform.rotation.x,-Camera.main.transform.rotation.y,transform.rotation.z,transform.rotation.w);

        transform.position = target.position + target.forward * distance;
    }

    //‹­§“I‚É“¯Šú‚³‚¹‚½‚¢Žž
    public void ImmediateSync(Transform targetTransform)
    {
        transform.position = targetTransform.position + targetTransform.forward * distance;
        transform.rotation = targetTransform.rotation;
    }
}
